zend_object_handlers xsltProcessor_object_handlers;
zend_class_entry *xsltProcessor_ce;

/*     ============== XSLT10/20/30/31: PHP Interface of   XsltProcessor class =============== */

void XsltProcessor_free_storage(zend_object *object)
{

    zend_object_std_dtor(object);
}

void XsltProcessor_destroy_storage(zend_object *object)
{
    xsltProcessor_object *obj;

    zend_objects_destroy_object(object);
}

zend_object * xsltProcessor_create_handler(zend_class_entry *type)
{


    xsltProcessor_object *obj = (xsltProcessor_object *)ecalloc(1, sizeof(xsltProcessor_object)+ zend_object_properties_size(type));


   zend_object_std_init(&obj->std,type);
    object_properties_init(&obj->std, type);

    obj->std.handlers = &xsltProcessor_object_handlers;

    return &obj->std;
}



PHP_METHOD(XsltProcessor, __destruct)
{

    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));


    XsltProcessor * xsltProcessor= obj->xsltProcessor;
    if(xsltProcessor != nullptr){
    	delete xsltProcessor;
     }


}

PHP_METHOD(XsltProcessor, transformFileToFile)
{
    XsltProcessor *xsltProcessor;
    char * outfileName;
    char * infilename;
    char * styleFileName;
    size_t len1, len2, len3;

    if (zend_parse_parameters(ZEND_NUM_ARGS() , "sss", &infilename, &len1, &styleFileName, &len2, &outfileName, &len3) == FAILURE) {
        RETURN_NULL();
    }

    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    xsltProcessor = obj->xsltProcessor;

    if (xsltProcessor != nullptr) {

        xsltProcessor->transformFileToFile(infilename, styleFileName, outfileName);
        if(xsltProcessor->exceptionOccurred()) {
     	  // TODO: throw exception
        }
    }
}

PHP_METHOD(XsltProcessor, transformFileToValue)
{
    XsltProcessor *xsltProcessor;
    char * infilename;
    char * styleFileName;
   size_t len1, len2;

    if (zend_parse_parameters(ZEND_NUM_ARGS() , "ss", &infilename, &len1, &styleFileName, &len2) == FAILURE) {
        RETURN_NULL();
    }

    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    xsltProcessor = obj->xsltProcessor;

    if (xsltProcessor != nullptr) {

        XdmValue * node = xsltProcessor->transformFileToValue(infilename, styleFileName);
        if(node != nullptr) {
            if (object_init_ex(return_value, xdmValue_ce) != SUCCESS) {
                RETURN_NULL();
            } else {
		zend_object *ooth =  Z_OBJ_P(return_value);
                //struct xdmValue_object* vobj = (struct xdmValue_object *)Z_OBJ_P(return_value TSRMLS_CC);
		xdmValue_object* vobj  = (xdmValue_object *)((char *)ooth - XtOffsetOf(xdmValue_object, std));
                assert (vobj != nullptr);
                vobj->xdmValue = node;
            }
        } else {
            if(obj->xsltProcessor->exceptionOccurred()){
  		//TODO
	    }
        }
    }else {
        RETURN_NULL();
    }
}


PHP_METHOD(XsltProcessor, transformFileToString)
{
    XsltProcessor *xsltProcessor;
    char * infilename;
    char * styleFileName;
    size_t len1, len2;

    if (zend_parse_parameters(ZEND_NUM_ARGS() , "ss", &infilename, &len1, &styleFileName, &len2) == FAILURE) {
        RETURN_NULL();
    }

    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    xsltProcessor = obj->xsltProcessor;

    if (xsltProcessor != nullptr) {

        const char * result = xsltProcessor->transformFileToString(infilename, styleFileName);
	if(result != nullptr) {
            //char *str = estrdup(result);
            _RETURN_STRING(result);
        } else if(xsltProcessor->exceptionOccurred()){
            //TODO: xsltProcessor->checkException();
            const char * errStr = xsltProcessor->getErrorMessage();
            if(errStr != nullptr) {
                const char * errorCode = xsltProcessor->getErrorCode();
                if(errorCode!=nullptr) {
                    // TODO: throw exception
                }
            }
        }
    }
}


PHP_METHOD(XsltProcessor, transformToString)
{
    XsltProcessor *xsltProcessor;

    if (ZEND_NUM_ARGS()>0) {
        WRONG_PARAM_COUNT;
    }


    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    xsltProcessor = obj->xsltProcessor;
    if (xsltProcessor != nullptr) {

        const char * result = xsltProcessor->transformToString();
        if(result != nullptr) {
            _RETURN_STRING(result);
        } else if(xsltProcessor->exceptionOccurred()){
            xsltProcessor->checkException();
            const char * errStr = xsltProcessor->getErrorMessage();
            if(errStr != nullptr) {
                const char * errorCode = xsltProcessor->getErrorCode();
                if(errorCode!=nullptr) {
                    // TODO: throw exception
                }
            }
        }
    }
    RETURN_NULL();
}

PHP_METHOD(XsltProcessor, transformToValue)
{
    XsltProcessor *xsltProcessor;

    if (ZEND_NUM_ARGS()>0) {
        WRONG_PARAM_COUNT;
    }

    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    xsltProcessor = obj->xsltProcessor;

    if (xsltProcessor != nullptr) {

	XdmValue * node = xsltProcessor->transformToValue();
        if(node != nullptr) {
            if (object_init_ex(return_value, xdmValue_ce) != SUCCESS) {
                RETURN_NULL();
            } else {
                //struct xdmValue_object* vobj = (struct xdmValue_object *)Z_OBJ_P(return_value);
		zend_object *vvobj =  Z_OBJ_P(return_value);
		xdmValue_object* vobj  = (xdmValue_object *)((char *)vvobj - XtOffsetOf(xdmValue_object, std));
                assert (vobj != nullptr);
                vobj->xdmValue = node;
            }
        } else if(xsltProcessor->exceptionOccurred()){
            xsltProcessor->checkException();
	    RETURN_NULL();
        }
    } else {
        RETURN_NULL();
    }
}

PHP_METHOD(XsltProcessor, transformToFile)
{
    XsltProcessor *xsltProcessor;

    if (ZEND_NUM_ARGS()>0) {
        WRONG_PARAM_COUNT;
    }

    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    xsltProcessor = obj->xsltProcessor;

    if (xsltProcessor != nullptr) {

	xsltProcessor->transformToFile();
        if(xsltProcessor->exceptionOccurred()) {
           //TODO
            const char * exStr = xsltProcessor->checkException();
        }
    } else {
        RETURN_NULL();
    }
}

PHP_METHOD(XsltProcessor, compileFromFile)
{
    XsltProcessor *xsltProcessor;
    char * name;
    size_t len1;
    if (zend_parse_parameters(ZEND_NUM_ARGS(), "s", &name, &len1) == FAILURE) {
        RETURN_NULL();
    }
    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    xsltProcessor = obj->xsltProcessor;
    if (xsltProcessor != nullptr) {
        xsltProcessor->compileFromFile(name);
    }
}

PHP_METHOD(XsltProcessor, compileFromString)
{
    XsltProcessor *xsltProcessor;
    char * stylesheetStr;
    size_t len1, myint;
    if (zend_parse_parameters(ZEND_NUM_ARGS() , "s", &stylesheetStr, &len1) == FAILURE) {
        RETURN_NULL();
    }
    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    xsltProcessor = obj->xsltProcessor;
    if (xsltProcessor != nullptr) {
        xsltProcessor->compileFromString(stylesheetStr);
    }
}

PHP_METHOD(XsltProcessor, compileFromStringAndSave)
{
    XsltProcessor *xsltProcessor;
    char * stylesheetStr;
    char * filename;
    size_t len1, len2, myint;
    if (zend_parse_parameters(ZEND_NUM_ARGS() , "ss", &stylesheetStr, &filename, &len1, &len2) == FAILURE) {
        RETURN_NULL();
    }
    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    xsltProcessor = obj->xsltProcessor;
    if (xsltProcessor != nullptr) {
        xsltProcessor->compileFromStringAndSave(stylesheetStr, filename);
    }
}

PHP_METHOD(XsltProcessor, compileFromFileAndSave)
{
    XsltProcessor *xsltProcessor;
    char * stylesheetFilename;
    char * filename;
    size_t len1, len2, myint;
    if (zend_parse_parameters(ZEND_NUM_ARGS() , "ss", &stylesheetFilename, &filename, &len1, &len2) == FAILURE) {
        RETURN_NULL();
    }
    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    xsltProcessor = obj->xsltProcessor;
    if (xsltProcessor != nullptr) {
        xsltProcessor->compileFromStringAndSave(stylesheetFilename, filename);
    }
}



PHP_METHOD(XsltProcessor, compileFromValue)
{
    XsltProcessor *xsltProcessor;
   zval* oth;

    if (zend_parse_parameters(ZEND_NUM_ARGS() , "O", &oth, xdmValue_ce) == FAILURE) {
        RETURN_NULL();
    }
    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    xsltProcessor = obj->xsltProcessor;
    if (xsltProcessor != nullptr) {
	zend_object* vvobj = Z_OBJ_P(oth);
	xdmValue_object* ooth = (xdmValue_object *)((char *)vvobj - XtOffsetOf(xdmValue_object, std));
        if(ooth != nullptr) {
            XdmValue * value = ooth->xdmValue;
            if(value != nullptr && value->size() == 1 && (value->getHead())->getType() == 3) {
        	xsltProcessor->compileFromXdmNode((XdmNode*)(value->getHead()));
	    }
	}
    }
}




PHP_METHOD(XsltProcessor, setSourceFromXdmValue)
{
    XsltProcessor *xsltProcessor;
    zval* oth = nullptr;

    if (zend_parse_parameters(ZEND_NUM_ARGS() , "z", &oth) == FAILURE) {
        RETURN_NULL();
    }

    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    xsltProcessor = obj->xsltProcessor;
    if (xsltProcessor != nullptr) {

    if(!oth) {
	php_error(E_WARNING, "Error setting source value");
	return;
    } else {
	if(Z_TYPE_P(oth) ==IS_NULL){
		php_error(E_WARNING, "Error setting source value");
		return;
	}

      const char * objName =ZSTR_VAL(Z_OBJCE_P(oth)->name);
      //std::cout<<"test type:"<<(Z_OBJCE_P(oth)->name)<<std::endl;

      if(strcmp(objName, "Saxon\\XdmNode")==0) {
	zend_object * nobj = Z_OBJ_P(oth);

	xdmNode_object* ooth = (xdmNode_object *)((char *)nobj - XtOffsetOf(xdmNode_object, std));//(xdmNode_object*)Z_OBJ_P(oth);
        if(ooth != nullptr) {
            XdmNode * value = ooth->xdmNode;
            if(value != nullptr) {

	        xsltProcessor->setSourceFromXdmNode(value);

            }
        }
      } /*else if(strcmp(objName, "Saxon\\XdmValue")==0) {
	zend_object* vvobj = Z_OBJ_P(oth);
	xdmValue_object* ooth = (xdmValue_object *)((char *)vvobj - XtOffsetOf(xdmValue_object, std));
        if(ooth != nullptr) {
            XdmValue * value = ooth->xdmValue;
            if(value != nullptr && value) {
	        xsltProcessor->setSourceFromXdmNode((XdmNode*)value);
            }
        }
      }  */


    }
  }
}


PHP_METHOD(XsltProcessor, setJustInTimeCompilation)
{
    XsltProcessor *xsltProcessor;
    bool jit = false;
    size_t len1;

    if (zend_parse_parameters(ZEND_NUM_ARGS() , "b", &jit) == FAILURE) {
        RETURN_NULL();
    }

    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    xsltProcessor = obj->xsltProcessor;
    if (xsltProcessor != nullptr) {
	 xsltProcessor->setJustInTimeCompilation(jit);
    }
}

PHP_METHOD(XsltProcessor, setOutputFile)
{
    XsltProcessor *xsltProcessor;
    char * outputFilename;
    size_t len1;

    if (zend_parse_parameters(ZEND_NUM_ARGS() , "s", &outputFilename, &len1) == FAILURE) {
        RETURN_NULL();
    }

    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    xsltProcessor = obj->xsltProcessor;
    if (xsltProcessor != nullptr && outputFilename != nullptr) {

	 xsltProcessor->setOutputFile(outputFilename);


    }
}


PHP_METHOD(XsltProcessor, setBaseOutputURI)
{
    XsltProcessor *xsltProcessor;
    char * baseOutputURI = nullptr;
    size_t len1;

    if (zend_parse_parameters(ZEND_NUM_ARGS() , "s", &baseOutputURI, &len1) == FAILURE) {
        RETURN_NULL();
    }

    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    xsltProcessor = obj->xsltProcessor;
    if (xsltProcessor != nullptr && baseOutputURI != nullptr) {

	 xsltProcessor->setBaseOutputURI(baseOutputURI);


    }
}


int size_t2int(size_t val) {
    return (val <= INT_MAX) ? (int)((ssize_t)val) : -1;
}

PHP_METHOD(XsltProcessor, setSourceFromFile)
{
    XsltProcessor *xsltProcessor;
    char * inFilename;
    size_t len1;
    if (zend_parse_parameters(ZEND_NUM_ARGS(), "s", &inFilename, &len1) == FAILURE) {
        RETURN_NULL();
    }

    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    xsltProcessor = obj->xsltProcessor;
    if (xsltProcessor != nullptr && inFilename != nullptr) {
	 xsltProcessor->setSourceFromFile(inFilename);


    }
}


PHP_METHOD(XsltProcessor, setProperty)
{
    XsltProcessor *xsltProcessor;
    char * name;
    char * value;
    size_t len1, len2, myint;

    if (zend_parse_parameters(ZEND_NUM_ARGS() , "ss", &name, &len1, &value, &len2) == FAILURE) {
        RETURN_NULL();
    }
    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    xsltProcessor = obj->xsltProcessor;
    if (xsltProcessor != nullptr) {
        xsltProcessor->setProperty(name, value);
    }
}

PHP_METHOD(XsltProcessor, setParameter)
{

   XsltProcessor *xsltProcessor;
   char * name;
   zval* oth;
   size_t len1, len2, myint;
    if (zend_parse_parameters(ZEND_NUM_ARGS() , "sz", &name, &len2, &oth) == FAILURE) {
        RETURN_NULL();
    }
    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    xsltProcessor = obj->xsltProcessor;
    if (xsltProcessor != nullptr) {

      if(!oth) {
    	php_error(E_WARNING, "Error in setParameter - value is nullptr");
    	return;
        } else {

        if(Z_TYPE_P(oth) ==IS_NULL){
		    php_error(E_WARNING, "Error in setParameter - value is nullptr");
		    return;
	    }

      const char * objName =ZSTR_VAL(Z_OBJCE_P(oth)->name);
      //std::cout<<"test type:"<<(Z_OBJCE_P(oth)->name)<<std::endl;
      if(objName == nullptr) {
        php_error(E_WARNING, "Error in setParameter value is not the right type");
        return;
      }


      if(strcmp(objName, "Saxon\\XdmNode")==0) {
	zend_object* ooth = Z_OBJ_P(oth);
	xdmNode_object * nobj = (xdmNode_object *)((char *)ooth - XtOffsetOf(xdmNode_object, std));
        if(nobj != nullptr) {
            XdmNode * value = nobj->xdmNode;
            value->incrementRefCount();
            if(value != nullptr) {
	        xsltProcessor->setParameter(name, (XdmValue *)value);

            }
        }
      } else if(strcmp(objName, "Saxon\\XdmValue")==0){
	zend_object* ooth = Z_OBJ_P(oth);
	xdmValue_object * vobj = (xdmValue_object *)((char *)ooth - XtOffsetOf(xdmValue_object, std));
        if(vobj != nullptr) {
            XdmValue * value = vobj->xdmValue;
            value->incrementRefCount();
            if(value != nullptr) {

                xsltProcessor->setParameter(name, value);
            }
        }



      } else if(strcmp(objName, "Saxon\\XdmAtomicValue")==0){
	zend_object* ooth = Z_OBJ_P(oth);
	xdmAtomicValue_object * aobj = (xdmAtomicValue_object *)((char *)ooth - XtOffsetOf(xdmAtomicValue_object, std));
        if(aobj != nullptr) {
            XdmAtomicValue * value = aobj->xdmAtomicValue;
            value->incrementRefCount();
            if(value != nullptr) {
                xsltProcessor->setParameter(name, (XdmValue *)value);
            }
        }



      }

    }
    }
}

PHP_METHOD(XsltProcessor, clearParameters)
{
    XsltProcessor *xsltProcessor;
    if (ZEND_NUM_ARGS()>0) {
        WRONG_PARAM_COUNT;
    }

    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    xsltProcessor = obj->xsltProcessor;
    if (xsltProcessor != nullptr) {
        xsltProcessor->clearParameters();
    }
}

PHP_METHOD(XsltProcessor, clearProperties)
{
    XsltProcessor *xsltProcessor;
    if (ZEND_NUM_ARGS()>0) {
        WRONG_PARAM_COUNT;
    }

    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    xsltProcessor = obj->xsltProcessor;
    if (xsltProcessor != nullptr) {
        xsltProcessor->clearProperties();
    }
}



PHP_METHOD(XsltProcessor, setSaveXslMessage)
{
    XsltProcessor *xsltProcessor;
    bool show = false;
    char * filename = nullptr;
    size_t len1;
    if (zend_parse_parameters(ZEND_NUM_ARGS() , "bs", &show, &filename,  &len1) == FAILURE) {
        RETURN_NULL();
    }

    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    xsltProcessor = obj->xsltProcessor;

    if (xsltProcessor != nullptr) {

	    xsltProcessor->setSaveXslMessage(show, filename);
	}

}



PHP_METHOD(XsltProcessor, exceptionOccurred)
{
    XsltProcessor *xsltProcessor;
    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    if (ZEND_NUM_ARGS()>0) {
        WRONG_PARAM_COUNT;
    }

    xsltProcessor = obj->xsltProcessor;
    if (xsltProcessor != nullptr) {
        bool result = xsltProcessor->exceptionOccurred();
        RETURN_BOOL(result);
    }
    RETURN_BOOL(false);
}



PHP_METHOD(XsltProcessor, getErrorCode)
{
    XsltProcessor *xsltProcessor;
    if (ZEND_NUM_ARGS()>0) {
        WRONG_PARAM_COUNT;
    }
    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    xsltProcessor = obj->xsltProcessor;
    if (xsltProcessor != nullptr) {
        const char * errCode = xsltProcessor->getErrorCode();
        if(errCode != nullptr) {
            //char *str = estrdup(errCode);
            _RETURN_STRING(errCode);
        }
    }
    RETURN_NULL();
}

PHP_METHOD(XsltProcessor, getErrorMessage)
{
    XsltProcessor *xsltProcessor;
    long index;
    if (zend_parse_parameters(ZEND_NUM_ARGS(), "l", &index) == FAILURE) {
        RETURN_NULL();
    }
    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    xsltProcessor = obj->xsltProcessor;
    if (xsltProcessor != nullptr) {
        const char * errStr = xsltProcessor->getErrorMessage();
        if(errStr != nullptr) {
            //char *str = estrdup(errStr);
            _RETURN_STRING(errStr);
        }
    }
    RETURN_NULL();
}
PHP_METHOD(XsltProcessor, exceptionClear)
{
    XsltProcessor *xsltProcessor;
    zend_object* pobj = Z_OBJ_P(getThis());
    xsltProcessor_object *obj = (xsltProcessor_object *)((char *)pobj - XtOffsetOf(xsltProcessor_object, std));
    xsltProcessor = obj->xsltProcessor;
    if (xsltProcessor != nullptr) {
        xsltProcessor->exceptionClear();
    }
}


zend_function_entry XsltProcessor_methods[] = {
        PHP_ME(XsltProcessor,  __destruct,     nullptr, ZEND_ACC_PUBLIC | ZEND_ACC_DTOR)
        PHP_ME(XsltProcessor,  transformFileToFile, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(XsltProcessor,  transformFileToString, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(XsltProcessor,  transformFileToValue, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(XsltProcessor,  transformToString, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(XsltProcessor,  transformToValue, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(XsltProcessor,  transformToFile, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(XsltProcessor, compileFromFile, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(XsltProcessor, compileFromValue, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(XsltProcessor, compileFromString, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(XsltProcessor, compileFromStringAndSave, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(XsltProcessor, compileFromFileAndSave, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(XsltProcessor,  setOutputFile, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(XsltProcessor,  setSourceFromFile, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(XsltProcessor,  setSourceFromXdmValue, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(XsltProcessor,  setJustInTimeCompilation, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(XsltProcessor,  setParameter, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(XsltProcessor,  setProperty, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(XsltProcessor,  clearParameters, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(XsltProcessor,  clearProperties, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(XsltProcessor,  setSaveXslMessage, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(XsltProcessor,  exceptionOccurred, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(XsltProcessor,  exceptionClear, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(XsltProcessor,  getErrorCode, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(XsltProcessor,  getErrorMessage, nullptr, ZEND_ACC_PUBLIC)
        {nullptr, nullptr, nullptr}
};

