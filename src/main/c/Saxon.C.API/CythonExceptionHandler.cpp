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
          const char * msg = e->getMessage();

    PyErr_SetString(pysaxonapierror, msg);

   // PyErr_SetString(PyExc_RuntimeError, msg);
  }  catch (const std::exception& e) {
    PyErr_SetString(PyExc_RuntimeError, e.what() );
  }
}
