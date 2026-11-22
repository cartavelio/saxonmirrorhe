#!/usr/bin/env bash
set -euo pipefail

fixture_dir=$(cd -- "$(dirname -- "$0")/../../../src/test/testdata/compiled-stylesheet" && pwd)
runtime=$(realpath "${1:?Pass the staged SaxonJ-HE runtime directory}")
jars=("$runtime"/saxon-he-*.jar)
[[ ${#jars[@]} == 1 && -f ${jars[0]} ]]
scratch=$(mktemp -d)
trap 'rm -rf -- "$scratch"' EXIT

check_input() {
    local xml=$1 expected=$2 result stylesheet
    printf '%s\n' "$xml" > "$scratch/input.xml"
    printf '%s' "$expected" > "$scratch/expected"
    for stylesheet in labels.xsl labels.sef; do
        result="$scratch/$3-$stylesheet.out"
        java -jar "${jars[0]}" "-s:$scratch/input.xml" \
            "-xsl:$fixture_dir/$stylesheet" "-o:$result"
        cmp "$scratch/expected" "$result"
    done
}

check_input '<record name="Ada"/>' '[ADA]' ascii
check_input '<record name="Río &amp; Elm"/>' '[RÍO & ELM]' unicode

mkdir -p "$scratch/source/module" "$scratch/deployment" "$scratch/disabled"
cat > "$scratch/source/main.xsl" <<'XSL'
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="module/render.xsl"/>
</xsl:stylesheet>
XSL
cat > "$scratch/source/module/render.xsl" <<'XSL'
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="text"/>
  <xsl:template match="/">
    <xsl:value-of select="doc('runtime.xml')/value"/>
  </xsl:template>
</xsl:stylesheet>
XSL
printf '%s' '<value>original runtime file</value>' > "$scratch/source/module/runtime.xml"
printf '%s' '<value>deployed runtime file</value>' > "$scratch/deployment/runtime.xml"
printf '%s' '<input/>' > "$scratch/input.xml"

java -jar "${jars[0]}" "-xsl:$scratch/source/main.xsl" -nogo "-export:$scratch/deployment/ordinary.sef"
java -jar "${jars[0]}" "-xsl:$scratch/source/main.xsl" -nogo -relocate:on "-export:$scratch/deployment/relocatable.sef"
mv "$scratch/source/main.xsl" "$scratch/source/module/render.xsl" "$scratch/disabled"

printf '%s' 'original runtime file' > "$scratch/expected"
java -jar "${jars[0]}" "-s:$scratch/input.xml" "-xsl:$scratch/deployment/ordinary.sef" "-o:$scratch/actual"
cmp "$scratch/expected" "$scratch/actual"

printf '%s' 'deployed runtime file' > "$scratch/expected"
java -jar "${jars[0]}" "-s:$scratch/input.xml" "-xsl:$scratch/deployment/relocatable.sef" "-o:$scratch/actual"
cmp "$scratch/expected" "$scratch/actual"
