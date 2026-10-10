# Descarga el modelo de embeddings (búsqueda semántica) en ml-models/ y verifica su SHA-256.
# Uso (desde la raíz del repo):  powershell -ExecutionPolicy Bypass -File scripts/descargar-modelo.ps1
$ErrorActionPreference = "Stop"
$repo   = "sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2"
$commit = "e8f8c211226b894fcb81acc59f3b34ba3efd5f42"   # versión fijada para que todos usen el mismo modelo
$dest   = Join-Path $PSScriptRoot "..\ml-models\paraphrase-multilingual-MiniLM-L12-v2"
$files  = @(
  @{ Remote = "onnx/model_quint8_avx2.onnx"; Local = "model.onnx";              Sha = "98a01d88b7de996cdea58c32ca71208c09968d143798814b2ea09d3439dc334f" },
  @{ Remote = "sentencepiece.bpe.model";     Local = "sentencepiece.bpe.model"; Sha = "cfc8146abe2a0488e9e2a0c56de7952f7c11ab059eca145a0a727afce0db2865" }
)
New-Item -ItemType Directory -Force -Path $dest | Out-Null
foreach ($f in $files) {
  $out = Join-Path $dest $f.Local
  if ((Test-Path $out) -and ((Get-FileHash $out -Algorithm SHA256).Hash.ToLower() -eq $f.Sha)) {
    Write-Host "OK (ya descargado): $($f.Local)"; continue
  }
  $url = "https://huggingface.co/$repo/resolve/$commit/$($f.Remote)"
  Write-Host "Descargando $($f.Remote) ..."
  Invoke-WebRequest -Uri $url -OutFile $out -UseBasicParsing
  $hash = (Get-FileHash $out -Algorithm SHA256).Hash.ToLower()
  if ($hash -ne $f.Sha) { Remove-Item $out; throw "SHA-256 no coincide para $($f.Local). Archivo eliminado." }
  Write-Host "OK: $($f.Local)"
}
Write-Host "Modelo listo en $((Resolve-Path $dest).Path)"
