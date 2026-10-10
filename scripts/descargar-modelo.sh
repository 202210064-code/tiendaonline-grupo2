#!/usr/bin/env bash
# Descarga el modelo de embeddings (búsqueda semántica) en ml-models/ y verifica su SHA-256.
# Uso (desde la raíz del repo):  bash scripts/descargar-modelo.sh
set -euo pipefail
REPO="sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2"
COMMIT="e8f8c211226b894fcb81acc59f3b34ba3efd5f42"   # versión fijada para que todos usen el mismo modelo
DEST="$(dirname "$0")/../ml-models/paraphrase-multilingual-MiniLM-L12-v2"
mkdir -p "$DEST"
get() { # remoto local sha256
  local out="$DEST/$2"
  if [ -f "$out" ] && echo "$3  $out" | sha256sum -c --status 2>/dev/null; then echo "OK (ya descargado): $2"; return; fi
  echo "Descargando $1 ..."
  curl -fL --retry 3 -o "$out" "https://huggingface.co/$REPO/resolve/$COMMIT/$1"
  echo "$3  $out" | sha256sum -c --status || { rm -f "$out"; echo "SHA-256 no coincide para $2. Archivo eliminado." >&2; exit 1; }
  echo "OK: $2"
}
get onnx/model_quint8_avx2.onnx model.onnx              98a01d88b7de996cdea58c32ca71208c09968d143798814b2ea09d3439dc334f
get sentencepiece.bpe.model     sentencepiece.bpe.model cfc8146abe2a0488e9e2a0c56de7952f7c11ab059eca145a0a727afce0db2865
echo "Modelo listo en $DEST"
