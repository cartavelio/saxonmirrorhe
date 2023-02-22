#include "CythonExceptionHandler.h"
#include "Python.h"
#include <exception>
#include <string>
#include "SaxonApiException.h"

extern PyObject *pysaxonapierror;

void raise_py_error()
{
  try {
    throw;
  } 
  catch (SaxonApiException* e) {
          const char * errorCode = e->getErrorCode();
          string errorMessage = NULL;
          if(errorCode != NULL) {
                errorMessage = errorCode  ": " +  e->getMessage();
          } else {
                errorMessage = e->getMessage();
          }


    PyErr_SetString(pysaxonapierror, errorMessage.c_str());

   // PyErr_SetString(PyExc_RuntimeError, msg);
  }  catch (const std::exception& e) {
    PyErr_SetString(PyExc_RuntimeError, e.what() );
  }
}
