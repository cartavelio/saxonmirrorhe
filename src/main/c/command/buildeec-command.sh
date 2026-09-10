#!/bin/sh

#Build file for SaxonC on C++

gcc -m64 -fPIC -I../Saxon.C.API/graalvm  -c ../Saxon.C.API/SaxonCGlue.c -o SaxonCGlue.o $1 $2

gcc -m64 -fPIC -I../Saxon.C.API/graalvm  Transform.c  -o transform -ldl -lc -DEEC SaxonCGlue.o -lsaxon-eec-12.1 $1 $2

#gcc  Query.c -o query -ldl -lc -DEEC -I../../Saxon.C.API/graalvm -lsaxon-eec-12.1 $1 $2

#gcc  Validate.c -o validate -ldl -lc -DEEC -I../../Saxon.C.API/graalvm -lsaxon-eec-12.1 $1 $2

