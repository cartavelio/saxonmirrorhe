set jdkdir=../Saxon.C.API/graalvm

cl /EHsc "-I%jdkdir%" "-I%jdkdir%\win32" /DEEC Transform.c /link ..\..\libs\win\libsaxon-@@edition@@c-@@VERSION@@.lib
cl /EHsc "-I%jdkdir%" "-I%jdkdir%\win32" /DEEC Query.c  /link ..\..\libs\win\libsaxon-@@edition@@c-@@VERSION@@.lib
cl /EHsc "-I%jdkdir%" "-I%jdkdir%\win32" /DEEC Validate.c  /link ..\..\libs\win\libsaxon-@@edition@@c-@@VERSION@@.lib