#!/bin/sh

#Build file for SaxonC on C++
parent_path=$( cd "$(dirname "$0")" ; pwd -P )
current_dir=${1-$(pwd -P)}
echo "CWD for L set to '$current_dir'"
cd "$parent_path"
gcc -m64 -fPIC -I../Saxon.C.API/graalvm  -c ../Saxon.C.API/SaxonCGlue.c -o SaxonCGlue.o $2

gcc -m64 -fPIC -I../Saxon.C.API/graalvm  Transform.c  -o transform -ldl -lc SaxonCGlue.o -L$current_dir -lsaxon-eec-@@VERSION@@ $2

gcc  -m64 -fPIC -I../Saxon.C.API/graalvm Query.c -o query -ldl -lc SaxonCGlue.o -L$current_dir -lsaxon-eec-@@VERSION@@ $2

gcc  -m64 -fPIC -I../Saxon.C.API/graalvm Validate.c -o validate -ldl -lc SaxonCGlue.o -L$current_dir -lsaxon-eec-@@VERSION@@ $2

