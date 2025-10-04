#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR=$(cd "$(dirname "$0")" && pwd)
PDF_DIR="$ROOT_DIR/pdf"
TMP_DIR="$PDF_DIR/tmp"
DIAG_DIR="$PDF_DIR/diagrams"
STYLE="$ROOT_DIR/styles/print.css"

ensure_tools() {
  if ! command -v npx >/dev/null 2>&1; then
    echo "npx not found. Please install Node.js (includes npm)." >&2
    exit 1
  fi
  cd "$ROOT_DIR"
  if [ ! -f package.json ]; then
    cat > package.json <<'JSON'
{
  "name": "deliveryapp-docs",
  "private": true,
  "scripts": {
    "build": "bash ./compile-pdf.sh"
  },
  "dependencies": {
    "@mermaid-js/mermaid-cli": "^10.9.1",
    "md-to-pdf": "^5.2.4"
  }
}
JSON
  fi
  npm install --silent
}

prepare_dirs() {
  mkdir -p "$PDF_DIR" "$DIAG_DIR" "$TMP_DIR"
}

process_file() {
  local input_md="$1"
  local base
  base=$(basename "$input_md" .md)
  local processed="$TMP_DIR/${base}.processed.md"
  local mmd_list="$TMP_DIR/${base}.mmd.list"
  : > "$mmd_list"

  # Extract mermaid code blocks into .mmd files and replace with image links
  awk -v diagDirAbs="$DIAG_DIR" -v diagDirRel="diagrams" -v base="$base" -v listFile="$mmd_list" '
    BEGIN { inblk=0; count=0; list="" }
    $0=="```mermaid" { inblk=1; count++; fname=sprintf("%s/%s-diagram-%03d.mmd", diagDirAbs, base, count); list = list "\n" fname; next }
    inblk==1 && $0=="```" { inblk=0; svgrel=sprintf("%s/%s-diagram-%03d.svg", diagDirRel, base, count); print "![diagram](" svgrel ")"; next }
    inblk==1 { print $0 >> fname; next }
    { print }
    END { if (listFile!="") { print list > listFile } }
  ' "$input_md" > "$processed"

  # Render all .mmd to .svg
  if [ -s "$mmd_list" ]; then
    while IFS= read -r mmd; do
      [ -z "$mmd" ] && continue
      svg="${mmd%.mmd}.svg"
      echo "Rendering Mermaid: $(basename \"$mmd\") -> $(basename \"$svg\")" >&2
      npx --yes @mermaid-js/mermaid-cli -i "$mmd" -o "$svg" -b transparent --scale 1 -p "$ROOT_DIR/styles/puppeteer.json" >/dev/null 2>&1 || {
        echo "Warning: Mermaid render failed for $mmd" >&2
      }
    done < "$mmd_list"
  fi

  echo "$processed"
}

export_pdf() {
  local processed_md="$1"
  local body_class="$2" # "" or "rtl"
  local out_pdf_name="$3"
  local out_pdf="$PDF_DIR/$out_pdf_name"

  local extra_args=()
  if [ -n "$body_class" ]; then
    extra_args+=("--body-class=$body_class")
  fi

  echo "Exporting PDF: $(basename "$out_pdf")"
  # md-to-pdf writes alongside the source by default; run from output dir with a temp copy
  local tmp_for_pdf
  if [ "$(dirname "$processed_md")" = "$TMP_DIR" ]; then
    tmp_for_pdf="$processed_md"
  else
    tmp_for_pdf="$TMP_DIR/$(basename "$processed_md")"
    cp "$processed_md" "$tmp_for_pdf"
  fi
  pushd "$PDF_DIR" >/dev/null
  npx --yes md-to-pdf "$(realpath --relative-to="$PDF_DIR" "$tmp_for_pdf")" \
    --stylesheet "$STYLE" \
    --pdf-options '{"format":"A4","printBackground":true}' \
    --launch-options '{"args":["--no-sandbox","--disable-setuid-sandbox"]}' \
    "${extra_args[@]}" \
    || { echo "md-to-pdf failed" >&2; popd >/dev/null; exit 1; }
  # Move the produced PDF to the desired name
  local produced_pdf="${tmp_for_pdf%.md}.pdf"
  mv -f "$produced_pdf" "$out_pdf"
  popd >/dev/null
}

main() {
  ensure_tools
  prepare_dirs

  declare -a files=(
    "$ROOT_DIR/developer-guide.md"
    "$ROOT_DIR/investor-overview.md"
    "$ROOT_DIR/system-specification.en.md"
    "$ROOT_DIR/developer-guide.ar.md"
    "$ROOT_DIR/investor-overview.ar.md"
    "$ROOT_DIR/system-specification.ar.md"
  )

  for f in "${files[@]}"; do
    if [ ! -f "$f" ]; then
      echo "Missing doc: $f" >&2
      exit 1
    fi
  done

  # English
  eng_dev=$(process_file "$ROOT_DIR/developer-guide.md")
  export_pdf "$eng_dev" "" "developer-guide.en.pdf"

  eng_inv=$(process_file "$ROOT_DIR/investor-overview.md")
  export_pdf "$eng_inv" "" "investor-overview.en.pdf"

  eng_spec=$(process_file "$ROOT_DIR/system-specification.en.md")
  export_pdf "$eng_spec" "" "system-specification.en.pdf"

  # Arabic (RTL)
  ar_dev=$(process_file "$ROOT_DIR/developer-guide.ar.md")
  export_pdf "$ar_dev" "rtl" "developer-guide.ar.pdf"

  ar_inv=$(process_file "$ROOT_DIR/investor-overview.ar.md")
  export_pdf "$ar_inv" "rtl" "investor-overview.ar.pdf"

  ar_spec=$(process_file "$ROOT_DIR/system-specification.ar.md")
  export_pdf "$ar_spec" "rtl" "system-specification.ar.pdf"

  echo "PDFs written to: $PDF_DIR"
}

main "$@"
