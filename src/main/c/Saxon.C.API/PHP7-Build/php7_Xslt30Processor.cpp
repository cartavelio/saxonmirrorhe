#include "php_Xslt30Processor.h"



zend_object_handlers xslt30Processor_object_handlers;

zend_class_entry *xslt30Processor_ce;



/*     ============== XSLT30: PHP Interface of  Xslt30Processor class =============== */

void Xslt30Processor_free_storage(zend_object *object)
{

    zend_object_std_dtor(object);

}

void Xslt30Processor_destroy_storage(zend_object *object)
{
    xslt30Processor_object *obj;

    zend_objects_destroy_object(object);
}

zend_object * xslt30Processor_create_handler(zend_class_entry *type)
{


    xslt30Processor_object *obj = (xslt30Processor_object *)ecalloc(1, sizeof(xslt30Processor_object)+ zend_object_properties_size(type));


    zend_object_std_init(&obj->std,type);
    object_properties_init(&obj->std, type);

    obj->std.handlers = &xslt30Processor_object_handlers;

    return &obj->std;
}


PHP_METHOD(Xslt30Processor, __destruct)
{
    zend_object* pobj = Z_OBJ_P(getThis());
    xslt30Processor_object *obj = (xslt30Processor_object *)((char *)pobj - XtOffsetOf(xslt30Processor_object, std));

    Xslt30Processor * xslt30Processor= obj->xslt30Processor;
    if(xslt30Processor != nullptr){
        delete xslt30Processor;
    }
}


PHP_METHOD(Xslt30Processor, transformFileToString)
{
    Xslt30Processor *xslt30Processor;
    char * infilename = nullptr;
    char * styleFileName = nullptr;
    size_t len1, len2;

    if (zend_parse_parameters(ZEND_NUM_ARGS() , "ss", &infilename, &len1, &styleFileName, &len2) == FAILURE) {
        RETURN_NULL();
    }

    zend_object* pobj = Z_OBJ_P(getThis());
    xslt30Processor_object *obj = (xslt30Processor_object *)((char *)pobj - XtOffsetOf(xslt30Processor_object, std));
    xslt30Processor = obj->xslt30Processor;

    if (xslt30Processor != nullptr) {

        const char * result = xslt30Processor->transformFileToString(infilename, styleFileName);
        if(result != nullptr) {
            //char *str = estrdup(result);
            _RETURN_STRING(result);
        } else if(xslt30Processor->exceptionOccurred()){
            //TODO: xslt30Processor->checkException();
            const char * errStr = xslt30Processor->getErrorMessage();
            if(errStr != nullptr) {
                const char * errorCode = xslt30Processor->getErrorCode();
                if(errorCode!=nullptr) {
                    // TODO: throw exception
                }
            }
        }
    }
}

PHP_METHOD(Xslt30Processor, transformFileToValue)
{
    Xslt30Processor *xslt30Processor;
    char * infilename = nullptr;
    char * styleFileName = nullptr;
    size_t len1, len2;

    if (zend_parse_parameters(ZEND_NUM_ARGS() , "ss", &infilename, &len1, &styleFileName, &len2) == FAILURE) {
        RETURN_NULL();
    }

    zend_object* pobj = Z_OBJ_P(getThis());
    xslt30Processor_object *obj = (xslt30Processor_object *)((char *)pobj - XtOffsetOf(xslt30Processor_object, std));
    xslt30Processor = obj->xslt30Processor;

    if (xslt30Processor != nullptr) {

        XdmValue * node = xslt30Processor->transformFileToValue(infilename, styleFileName);
        if(node != nullptr) {
            if (object_init_ex(return_value, xdmValue_ce) != SUCCESS) {
                RETURN_NULL();
            } else {
                //struct xdmValue_object* vobj = (struct xdmValue_object *)Z_OBJ_P(return_value TSRMLS_CC);
                zend_object *ooth =  Z_OBJ_P(return_value);
                xdmValue_object* vobj  = (xdmValue_object *)((char *)ooth - XtOffsetOf(xdmValue_object, std));
                assert (vobj != nullptr);
                vobj->xdmValue = node;
            }
        } else {
            if(obj->xslt30Processor->exceptionOccurred()){
                //TODO
            }
        }
    }else {
        RETURN_NULL();
    }
}



PHP_METHOD(Xslt30Processor, compileFromFile)
{
    Xslt30Processor *xslt30Processor;
    char * name = nullptr;
    size_t len1;
    if (zend_parse_parameters(ZEND_NUM_ARGS(), "s", &name, &len1) == FAILURE) {
        RETURN_NULL();
    }
    zend_object* pobj = Z_OBJ_P(getThis());
    xslt30Processor_object *obj = (xslt30Processor_object *)((char *)pobj - XtOffsetOf(xslt30Processor_object, std));
    xslt30Processor = obj->xslt30Processor;
    if (xslt30Processor != nullptr) {
        xslt30Processor->compileFromFile(name);
    }
}


PHP_METHOD(Xslt30Processor, compileFromAssociatedFile)
{
    Xslt30Processor *xslt30Processor;
    char * name = nullptr;
    size_t len1;
    if (zend_parse_parameters(ZEND_NUM_ARGS(), "s", &name, &len1) == FAILURE) {
        RETURN_NULL();
    }
    zend_object* pobj = Z_OBJ_P(getThis());
    xslt30Processor_object *obj = (xslt30Processor_object *)((char *)pobj - XtOffsetOf(xslt30Processor_object, std));
    xslt30Processor = obj->xslt30Processor;
    if (xslt30Processor != nullptr) {
        xslt30Processor->compileFromAssociatedFile(name);
    }
}


PHP_METHOD(Xslt30Processor, compileFromString)
{
    Xslt30Processor *xslt30Processor;
    char * stylesheetStr;
    size_t len1, myint;
    if (zend_parse_parameters(ZEND_NUM_ARGS() , "s", &stylesheetStr, &len1) == FAILURE) {
        RETURN_NULL();
    }
    zend_object* pobj = Z_OBJ_P(getThis());
    xslt30Processor_object *obj = (xslt30Processor_object *)((char *)pobj - XtOffsetOf(xslt30Processor_object, std));
    xslt30Processor = obj->xslt30Processor;
    if (xslt30Processor != nullptr) {
        xslt30Processor->compileFromString(stylesheetStr);
    }
}

PHP_METHOD(Xslt30Processor, compileFromStringAndSave)
{
    Xslt30Processor *xslt30Processor;
    char * stylesheetStr;
    char * filename = nullptr;
    size_t len1, len2, myint;
    if (zend_parse_parameters(ZEND_NUM_ARGS() , "ss", &stylesheetStr, filename, &len1, &len2) == FAILURE) {
        RETURN_NULL();
    }
    zend_object* pobj = Z_OBJ_P(getThis());
    xslt30Processor_object *obj = (xslt30Processor_object *)((char *)pobj - XtOffsetOf(xslt30Processor_object, std));
    xslt30Processor = obj->xslt30Processor;
    if (xslt30Processor != nullptr) {
        xslt30Processor->compileFromStringAndSave(stylesheetStr, filename);
    }
}

PHP_METHOD(Xslt30Processor, compileFromFileAndSave)
{
    Xslt30Processor *xslt30Processor;
    char * stylesheetFilename = nullptr;
    char * filename = nullptr;
    size_t len1, len2, myint;
    if (zend_parse_parameters(ZEND_NUM_ARGS() , "ss", &stylesheetFilename, filename, &len1, &len2) == FAILURE) {
        RETURN_NULL();
    }
    zend_object* pobj = Z_OBJ_P(getThis());
    xslt30Processor_object *obj = (xslt30Processor_object *)((char *)pobj - XtOffsetOf(xslt30Processor_object, std));
    xslt30Processor = obj->xslt30Processor;
    if (xslt30Processor != nullptr) {
        xslt30Processor->compileFromStringAndSave(stylesheetFilename, filename);
    }
}



PHP_METHOD(Xslt30Processor, compileFromValue)
{
    Xslt30Processor *xslt30Processor;
    zval* oth;

    if (zend_parse_parameters(ZEND_NUM_ARGS() , "O", &oth, xdmValue_ce) == FAILURE) {
        RETURN_NULL();
    }
    zend_object* pobj = Z_OBJ_P(getThis());
    xslt30Processor_object *obj = (xslt30Processor_object *)((char *)pobj - XtOffsetOf(xslt30Processor_object, std));
    xslt30Processor = obj->xslt30Processor;
    if (xslt30Processor != nullptr) {
        zend_object* vvobj = Z_OBJ_P(oth);
        xdmValue_object* ooth = (xdmValue_object *)((char *)vvobj - XtOffsetOf(xdmValue_object, std));
        if(ooth != nullptr) {
            XdmValue * value = ooth->xdmValue;
            if(value != nullptr && value->size() == 1 && (value->getHead())->getType() == 3) {
                xslt30Processor->compileFromXdmNode((XdmNode*)(value->getHead()));
            }
        }
    }
}

PHP_METHOD(Xslt30Processor, setJustInTimeCompilation)
{
    Xslt30Processor *xslt30Processor;
    bool jit = false;
    size_t len1;

    if (zend_parse_parameters(ZEND_NUM_ARGS() , "b", &jit) == FAILURE) {
        RETURN_NULL();
    }

    zend_object* pobj = Z_OBJ_P(getThis());
    xslt30Processor_object *obj = (xslt30Processor_object *)((char *)pobj - XtOffsetOf(xslt30Processor_object, std));
    xslt30Processor = obj->xslt30Processor;
    if (xslt30Processor != nullptr) {
        xslt30Processor->setJustInTimeCompilation(jit);
    }
}

PHP_METHOD(Xslt30Processor, setcwd)
{
    Xslt30Processor *xslt30Processor;
    char * cwdStr;
    size_t len;

    if (zend_parse_parameters(ZEND_NUM_ARGS() , "s", &cwdStr, &len) == FAILURE) {
        RETURN_NULL();
    }

    zend_object* pobj = Z_OBJ_P(getThis());
    xslt30Processor_object * obj = (xslt30Processor_object *)((char *)pobj - XtOffsetOf(xslt30Processor_object, std));
    xslt30Processor = obj->xslt30Processor;
    if (xslt30Processor != nullptr) {

        if(cwdStr != nullptr) {
            xslt30Processor->setcwd(cwdStr);
        }
    }
}


zend_function_entry Xslt30Processor_methods[] = {
        PHP_ME(Xslt30Processor,  __destruct,     nullptr, ZEND_ACC_PUBLIC | ZEND_ACC_DTOR)
        PHP_ME(Xslt30Processor,  transformFileToFile, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(Xslt30Processor,  transformFileToString, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(Xslt30Processor,  transformFileToValue, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(Xslt30Processor, compileFromFile, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(Xslt30Processor, compileFromValue, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(Xslt30Processor, compileFromString, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(Xslt30Processor, compileFromStringAndSave, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(Xslt30Processor, compileFromFileAndSave, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(Xslt30Processor, compileFromAssociatedFile, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(Xslt30Processor,  setJustInTimeCompilation, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(Xslt30Processor,  setParameter, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(Xslt30Processor,  clearParameters, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(Xslt30Processor,  setupXslMessage, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(Xslt30Processor,  exceptionOccurred, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(Xslt30Processor,  exceptionClear, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(Xslt30Processor,  getErrorCode, nullptr, ZEND_ACC_PUBLIC)
        PHP_ME(Xslt30Processor,  getErrorMessage, nullptr, ZEND_ACC_PUBLIC)
        {nullptr, nullptr, nullptr}
};

