set graalvmdir=..\Saxon.C.API\graalvm

cl /EHsc "-I%graalvmdir%" /DEEC Transform.c /link ..\libs\win\libsaxon-@@edition@@c-@@VERSION@@.lib
cl /EHsc "-I%graalvmdir%" /DEEC Query.c  /link ..\libs\win\libsaxon-@@edition@@c-@@VERSION@@.lib
cl /EHsc "-I%graalvmdir%"  /DEEC Validate.c  /link ..\libs\win\libsaxon-@@edition@@c-@@VERSION@@.lib