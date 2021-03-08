////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2021 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#ifndef PHP_SAXON_H

#ifndef PHP_SAXON_XSLT30PROCESSOR_H
#define PHP_SAXON_XSLT30PROCESSOR_H


#include "php_saxon.h"

struct xslt30Processor_object {

    Xslt30Processor *xslt30Processor;
    zend_object std;
};



    PHP_METHOD(Xslt30Processor,  __destruct);
    PHP_METHOD(Xslt30Processor,  transformFileToFile);
    PHP_METHOD(Xslt30Processor,  transformFileToString);
    PHP_METHOD(Xslt30Processor,  transformFileToValue);
    PHP_METHOD(Xslt30Processor,  transformToFile);
    PHP_METHOD(Xslt30Processor, compileFromFile);
    PHP_METHOD(Xslt30Processor, compileFromValue);
    PHP_METHOD(Xslt30Processor, compileFromString);
    PHP_METHOD(Xslt30Processor, compileFromStringAndSave);
    PHP_METHOD(Xslt30Processor, compileFromFileAndSave);
    PHP_METHOD(Xslt30Processor, compileFromAssociatedFile);
    PHP_METHOD(Xslt30Processor,  setJustInTimeCompilation);
    PHP_METHOD(Xslt30Processor,  setParameter);
    PHP_METHOD(Xslt30Processor,  clearParameters);
    PHP_METHOD(Xslt30Processor,  exceptionClear);
    PHP_METHOD(Xslt30Processor,  exceptionOccurred);
    PHP_METHOD(Xslt30Processor,  getErrorCode);
    PHP_METHOD(Xslt30Processor,  getErrorMessage);
    PHP_METHOD(Xslt30Processor,  setupXslMessage);
    PHP_METHOD(Xslt30Processor,  setcwd);




#endif /* PHP_SAXON_XSLT30PROCESSOR_H */

















