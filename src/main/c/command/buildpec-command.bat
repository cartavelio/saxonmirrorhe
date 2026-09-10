set jdkdir=..\Saxon.C.API\jni

cl /EHsc "-I%jdkdir%" "-I%jdkdir%\win32"  /DPEC Transform.c
cl /EHsc "-I%jdkdir%" "-I%jdkdir%\win32" /DPEC Query.c