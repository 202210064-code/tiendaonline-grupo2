using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;
namespace TiendaOnline.Services;

// Genera embeddings de texto con un modelo local (paraphrase-multilingual-MiniLM-L12-v2, ONNX).
// Se descarga con scripts/descargar-modelo.ps1; si falta, la búsqueda semántica queda desactivada.
public sealed class EmbeddingService : IDisposable {
 public const int Dimensions = 384;
 private const int MaxTokens = 128;
 // Ids especiales de XLM-R (convención fairseq): el resto de ids = id SentencePiece + 1.
 private const long Bos = 0, Eos = 2, Unk = 3;
 private readonly InferenceSession? _session;
 private readonly SentencePieceTokenizer? _tokenizer;
 private readonly bool _usesTokenTypes;

 public bool IsAvailable => _session != null;
 public string? UnavailableReason { get; }

 public EmbeddingService(IConfiguration config, IHostEnvironment env, ILogger<EmbeddingService> log) {
  var dir = config["Embeddings:ModelPath"] ?? "ml-models/paraphrase-multilingual-MiniLM-L12-v2";
  if (!Path.IsPathRooted(dir)) dir = Path.Combine(env.ContentRootPath, dir);
  var model = Path.Combine(dir, "model.onnx");
  var vocab = Path.Combine(dir, "sentencepiece.bpe.model");
  if (!File.Exists(model) || !File.Exists(vocab)) {
   UnavailableReason = $"No se encontró el modelo de embeddings en {dir}. Ejecuta scripts/descargar-modelo.ps1.";
   log.LogWarning("{Reason}", UnavailableReason);
   return;
  }
  using (var stream = File.OpenRead(vocab))
   _tokenizer = SentencePieceTokenizer.Create(stream, addBeginningOfSentence: false, addEndOfSentence: false);
  _session = new InferenceSession(model);
  _usesTokenTypes = _session.InputMetadata.ContainsKey("token_type_ids");
  log.LogInformation("Modelo de embeddings cargado desde {Dir}", dir);
 }

 public float[] Embed(string text) {
  if (_session == null || _tokenizer == null) throw new InvalidOperationException(UnavailableReason);
  var pieces = _tokenizer.EncodeToIds(text, MaxTokens - 2, out _, out _);
  var ids = new long[pieces.Count + 2];
  ids[0] = Bos;
  for (int i = 0; i < pieces.Count; i++) ids[i + 1] = pieces[i] == 0 ? Unk : pieces[i] + 1;
  ids[^1] = Eos;
  int n = ids.Length;
  var shape = new[] { 1, n };
  var inputs = new List<NamedOnnxValue> {
   NamedOnnxValue.CreateFromTensor("input_ids", new DenseTensor<long>(ids, shape)),
   NamedOnnxValue.CreateFromTensor("attention_mask", new DenseTensor<long>(Enumerable.Repeat(1L, n).ToArray(), shape))
  };
  if (_usesTokenTypes) inputs.Add(NamedOnnxValue.CreateFromTensor("token_type_ids", new DenseTensor<long>(new long[n], shape)));

  using var results = _session.Run(inputs);
  var hidden = results.First().AsTensor<float>(); // [1, n, 384]
  // Mean pooling sobre los tokens (todos tienen máscara 1) y normalización L2.
  var vector = new float[Dimensions];
  for (int t = 0; t < n; t++)
   for (int d = 0; d < Dimensions; d++) vector[d] += hidden[0, t, d];
  double norm = 0;
  for (int d = 0; d < Dimensions; d++) { vector[d] /= n; norm += vector[d] * vector[d]; }
  norm = Math.Sqrt(norm);
  if (norm > 0) for (int d = 0; d < Dimensions; d++) vector[d] = (float)(vector[d] / norm);
  return vector;
 }

 public void Dispose() => _session?.Dispose();
}
