set jdkdir=..\Saxon.C.API\jni

cl /EHsc "-I%jdkdir%" "-I%jdkdir%\win32" /DHEC Transform.c
cl /EHsc "-I%jdkdir%" "-I%jdkdir%\win32" /DHEC Query.c