#!/bin/sh

#Build file for SaxonC on C++
parent_path=$( cd "$(dirname "$0")" ; pwd -P )
library_dir=${1-$(pwd -P)}/../@@libdir@@
#cd "$parent_path"
gcc -m64 -fPIC -I../Saxon.C.API/graalvm  -c ../Saxon.C.API/SaxonCGlue.c -o SaxonCGlue.o $2

gcc -m64 -fPIC -I../Saxon.C.API/graalvm  Transform.c  -o transform -ldl -lc SaxonCGlue.o -L$library_dir -lsaxon-@@edition@@c-@@VERSION@@ $2

gcc  -m64 -fPIC -I../Saxon.C.API/graalvm Query.c -o query -ldl -lc SaxonCGlue.o -L$library_dir -lsaxon-@@edition@@c-@@VERSION@@ $2

gcc  -m64 -fPIC -I../Saxon.C.API/graalvm Validate.c -o validate -ldl -lc SaxonCGlue.o -L$library_dir -lsaxon-@@edition@@c-@@VERSION@@ $2

