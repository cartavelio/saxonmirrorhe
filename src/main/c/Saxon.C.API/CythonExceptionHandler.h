////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2022 - 2023 Saxonica Liimited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef CYTHON_EXCEPTION_HANDLER_H
#define CYTHON_EXCEPTION_HANDLER_H

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


#endif /* CYTHON_EXCEPTION_HANDLER_H */
