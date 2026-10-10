using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TiendaOnline.Data;
namespace TiendaOnline.Services;

// Guarda y consulta los embeddings de productos en PostgreSQL con la extensión pgvector.
public class ProductVectorStore(ShopDbContext db) {
 public async Task<bool> EnsureSchemaAsync(CancellationToken ct = default) {
  var conn = await OpenAsync(ct);
  await using (var check = new NpgsqlCommand("SELECT to_regclass('public.\"Products\"') IS NOT NULL", conn))
   if (!(bool)(await check.ExecuteScalarAsync(ct))!) return false; // la tabla Products aún no existe
  await using var cmd = new NpgsqlCommand($"""
   CREATE EXTENSION IF NOT EXISTS vector;
   CREATE TABLE IF NOT EXISTS "ProductEmbeddings" (
    "ProductId"  integer PRIMARY KEY REFERENCES "Products"("Id") ON DELETE CASCADE,
    "Embedding"  vector({EmbeddingService.Dimensions}) NOT NULL,
    "SourceHash" varchar(64) NOT NULL,
    "UpdatedUtc" timestamptz NOT NULL DEFAULT now());
   """, conn);
  await cmd.ExecuteNonQueryAsync(ct);
  return true;
 }

 public async Task<Dictionary<int, string>> GetHashesAsync(CancellationToken ct = default) {
  var conn = await OpenAsync(ct);
  await using var cmd = new NpgsqlCommand("SELECT \"ProductId\", \"SourceHash\" FROM \"ProductEmbeddings\"", conn);
  await using var r = await cmd.ExecuteReaderAsync(ct);
  var hashes = new Dictionary<int, string>();
  while (await r.ReadAsync(ct)) hashes[r.GetInt32(0)] = r.GetString(1);
  return hashes;
 }

 public async Task UpsertAsync(int productId, float[] embedding, string sourceHash, CancellationToken ct = default) {
  var conn = await OpenAsync(ct);
  await using var cmd = new NpgsqlCommand("""
   INSERT INTO "ProductEmbeddings" ("ProductId","Embedding","SourceHash","UpdatedUtc")
   VALUES (@id, @v::vector, @h, now())
   ON CONFLICT ("ProductId") DO UPDATE SET "Embedding"=EXCLUDED."Embedding","SourceHash"=EXCLUDED."SourceHash","UpdatedUtc"=now();
   """, conn);
  cmd.Parameters.AddWithValue("id", productId);
  cmd.Parameters.AddWithValue("v", ToLiteral(embedding));
  cmd.Parameters.AddWithValue("h", sourceHash);
  await cmd.ExecuteNonQueryAsync(ct);
 }

 // Productos más parecidos al vector de búsqueda (similitud coseno = 1 - distancia <=>).
 public async Task<List<(int ProductId, double Similarity)>> SearchAsync(float[] query, int limit, double minSimilarity,
   IReadOnlyCollection<int> exclude, CancellationToken ct = default) {
  var conn = await OpenAsync(ct);
  await using var cmd = new NpgsqlCommand("""
   SELECT "ProductId", 1 - ("Embedding" <=> @q::vector) AS similarity
   FROM "ProductEmbeddings"
   WHERE NOT ("ProductId" = ANY(@exclude)) AND 1 - ("Embedding" <=> @q::vector) >= @min
   ORDER BY "Embedding" <=> @q::vector
   LIMIT @limit;
   """, conn);
  cmd.Parameters.AddWithValue("q", ToLiteral(query));
  cmd.Parameters.AddWithValue("exclude", exclude.ToArray());
  cmd.Parameters.AddWithValue("min", minSimilarity);
  cmd.Parameters.AddWithValue("limit", limit);
  await using var r = await cmd.ExecuteReaderAsync(ct);
  var found = new List<(int, double)>();
  while (await r.ReadAsync(ct)) found.Add((r.GetInt32(0), r.GetDouble(1)));
  return found;
 }

 private async Task<NpgsqlConnection> OpenAsync(CancellationToken ct) {
  var conn = (NpgsqlConnection)db.Database.GetDbConnection();
  if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync(ct);
  return conn;
 }

 private static string ToLiteral(float[] v) =>
  "[" + string.Join(",", v.Select(x => x.ToString("R", CultureInfo.InvariantCulture))) + "]";
}
