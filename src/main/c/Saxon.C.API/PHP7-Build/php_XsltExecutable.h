////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2021 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#ifndef PHP_SAXON_H

#ifndef PHP_PHP_SAXON_XSLTEXECUTABLE_H
#define PHP_SAXON_XSLTEXECUTABLE_H
#include "php_saxon.h"

struct xsltExecutable_object {

    XsltExecutable *xsltExecutable;
    zend_object std;
};



    PHP_METHOD(XsltExecutable,  __destruct);
    PHP_METHOD(XsltExecutable, callFunctionReturningValue);
    PHP_METHOD(XsltExecutable, callFunctionReturningString);
    PHP_METHOD(XsltExecutable, callFunctionReturningFile);
    PHP_METHOD(XsltExecutable, callTemplateReturningValue);
    PHP_METHOD(XsltExecutable, callTemplateReturningString);
    PHP_METHOD(XsltExecutable, callTemplateReturningFile);
    PHP_METHOD(XsltExecutable, applyTemplatesReturningValue);
    PHP_METHOD(XsltExecutable, applyTemplatesReturningString);
    PHP_METHOD(XsltExecutable, applyTemplatesReturningFile);
    PHP_METHOD(XsltExecutable,  transformFileToFile);
    PHP_METHOD(XsltExecutable,  transformFileToString);
    PHP_METHOD(XsltExecutable,  transformFileToValue);
    PHP_METHOD(XsltExecutable,  transformToString);
    PHP_METHOD(XsltExecutable,  transformToValue);
    PHP_METHOD(XsltExecutable,  transformToFile);
    PHP_METHOD(XsltExecutable, compileFromFile);
    PHP_METHOD(XsltExecutable, compileFromValue);
    PHP_METHOD(XsltExecutable, compileFromString);
    PHP_METHOD(XsltExecutable, compileFromStringAndSave);
    PHP_METHOD(XsltExecutable, compileFromFileAndSave);
    PHP_METHOD(XsltExecutable, compileFromAssociatedFile);
    PHP_METHOD(XsltExecutable,  setOutputFile);
    PHP_METHOD(XsltExecutable,  setResultAsRawValue);
    PHP_METHOD(XsltExecutable,  setParameter);
    PHP_METHOD(XsltExecutable, setBaseOutputURI);
    PHP_METHOD(XsltExecutable, setInitialTemplateParameters);
    PHP_METHOD(XsltExecutable, setInitialMatchSelection);
    PHP_METHOD(XsltExecutable, setInitialMatchSelectionAsFile);
    PHP_METHOD(XsltExecutable, setGlobalContextItem);
    PHP_METHOD(XsltExecutable, setGlobalContextFromFile);

//new
    PHP_METHOD(XsltExecutable, setcwd);

    PHP_METHOD(XsltExecutable,  setProperty);
    PHP_METHOD(XsltExecutable,  clearParameters);
    PHP_METHOD(XsltExecutable,  clearProperties);
//new
    PHP_METHOD(XsltExecutable,  exportStylesheet);
    PHP_METHOD(XsltExecutable,  exceptionClear);
    PHP_METHOD(XsltExecutable,  exceptionOccurred);
    PHP_METHOD(XsltExecutable,  getErrorCode);
    PHP_METHOD(XsltExecutable,  getErrorMessage);
//new
    PHP_METHOD(XsltExecutable,  setupXslMessage);
//new
    PHP_METHOD(XsltExecutable,  getXslMessages);


#endif /* PHP_SAXON_H */

















