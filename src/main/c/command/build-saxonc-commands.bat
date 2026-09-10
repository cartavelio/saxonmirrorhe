set graalvmdir=..\Saxon.C.API\graalvm

cl /EHsc "-I%graalvmdir%" /DEEC Transform.c ../Saxon.C.API/SaxonCGlue.c /link ..\libs\win\libsaxon-@@edition@@c-@@VERSION@@.lib

cl /EHsc "-I%graalvmdir%" /DEEC Query.c ../Saxon.C.API/SaxonCGlue.c /link ..\libs\win\libsaxon-@@edition@@c-@@VERSION@@.lib

if exist Validate.c cl /EHsc "-I%graalvmdir%"  /DEEC Validate.c ../Saxon.C.API/SaxonCGlue.c /link ..\libs\win\libsaxon-@@edition@@c-@@VERSION@@.lib
