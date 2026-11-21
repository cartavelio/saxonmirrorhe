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
