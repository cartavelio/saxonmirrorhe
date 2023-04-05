#include "Python.h"
#include "CythonExceptionHandler.h"

#include <exception>
#include <string>
#include "SaxonApiException.h"

#ifndef _WIN64
extern PyObject *pysaxonapierror;
#endif

void raise_py_error()
{
  try {
    throw;
  }
  catch (SaxonApiException& e) {

#ifndef _WIN64
      const char * errorMessage = e.getMessageWithErrorCode();
      if(errorMessage == nullptr) {
        PyErr_SetString(pysaxonapierror, "Unknown exception found");
      } else {
        PyErr_SetString(pysaxonapierror, errorMessage);
      }
#else
      const char * errorMessage = e.getMessageWithErrorCode();
      if(errorMessage == nullptr) {
        PyErr_SetString(PyExc_RuntimeError, "Unknown exception found");
      } else {
        PyErr_SetString(PyExc_RuntimeError, errorMessage);
      }
#endif

  }  catch (const std::exception& e) {
    PyErr_SetString(PyExc_RuntimeError, e.what() );
  }
}
