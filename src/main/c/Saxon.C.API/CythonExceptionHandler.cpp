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
  catch (SaxonApiException& e) {
      const char * errorMessage = e.getMessageWithErrorCode();
      if(errorMessage == nullptr) {
        PyErr_SetString(pysaxonapierror, "Unknown exception found");
      } else {
        PyErr_SetString(pysaxonapierror, errorMessage);
      }


   // PyErr_SetString(PyExc_RuntimeError, msg);
  }  catch (const std::exception& e) {
    PyErr_SetString(PyExc_RuntimeError, e.what() );
  }
}
