set jdkdir=..\Saxon.C.API\jni

cl /EHsc "-I%jdkdir%" "-I%jdkdir%\win32" /DEEC Transform.c
cl /EHsc "-I%jdkdir%" "-I%jdkdir%\win32" /DEEC Query.c
cl /EHsc "-I%jdkdir%" "-I%jdkdir%\win32" /DEEC Validate.c