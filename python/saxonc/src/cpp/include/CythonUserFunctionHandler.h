//#include "saxonc/UserFunctionHandler.h"
#include "Python.h"
#include "saxonc/XdmValue.h"
#include "saxonc/XdmNode.h"

extern "C" PyObject * CythonUserFunctionHandler;


typedef enum saxonTypeEnum {
  enumNode,
  enumString,
  enumInteger,
  enumDouble,
  enumFloat,
  enumBool,
  enumArrXdmValue
} PRIMITIVE_TYPE;


std::map<std::string, saxonTypeEnum> typeMap = {
{"node", enumNode},
{"Q{http://www.w3.org/2001/XMLSchema}string", enumString},
{"Q{http://www.w3.org/2001/XMLSchema}duration", enumString},
{"Q{http://www.w3.org/2001/XMLSchema}dateTime",enumString},
{"Q{http://www.w3.org/2001/XMLSchema}date", enumString},
{"Q{http://www.w3.org/2001/XMLSchema}gYearMonth", enumString},
{"Q{http://www.w3.org/2001/XMLSchema}gYear",enumString},
{"Q{http://www.w3.org/2001/XMLSchema}gMonthDay",enumString},
{"Q{http://www.w3.org/2001/XMLSchema}time",enumString},
{"Q{http://www.w3.org/2001/XMLSchema}integer",enumInteger},
{"Q{http://www.w3.org/2001/XMLSchema}double",enumDouble},
{"Q{http://www.w3.org/2001/XMLSchema}decimal",enumDouble},
{"Q{http://www.w3.org/2001/XMLSchema}float",enumFloat},
{"Q{http://www.w3.org/2001/XMLSchema}boolean",enumBool},

{"Q{http://www.w3.org/2001/XMLSchema}gDay",enumString},
{"Q{http://www.w3.org/2001/XMLSchema}gMonth",enumString},
{"Q{http://www.w3.org/2001/XMLSchema}hexBinary",enumString},
{"Q{http://www.w3.org/2001/XMLSchema}base64Binary",enumString},
{"Q{http://www.w3.org/2001/XMLSchema}anyURI",enumString},
{"Q{http://www.w3.org/2001/XMLSchema}QName",enumString},
{"Q{http://www.w3.org/2001/XMLSchema}NOTATION",enumString},
{"Q{http://www.w3.org/2001/XMLSchema}numeric",enumInteger},
{"Q{http://www.w3.org/2001/XMLSchema}nonPositiveInteger",enumInteger},
{"Q{http://www.w3.org/2001/XMLSchema}negativeInteger",enumInteger},
{"Q{http://www.w3.org/2001/XMLSchema}long",enumInteger},
{"Q{http://www.w3.org/2001/XMLSchema}int",enumInteger},
{"Q{http://www.w3.org/2001/XMLSchema}short",enumInteger},
{"Q{http://www.w3.org/2001/XMLSchema}byte",enumInteger},
{"Q{http://www.w3.org/2001/XMLSchema}nonNegativeInteger",enumInteger},
{"Q{http://www.w3.org/2001/XMLSchema}positiveInteger",enumInteger},
{"Q{http://www.w3.org/2001/XMLSchema}unsignedLong",enumInteger},
{"Q{http://www.w3.org/2001/XMLSchema}unsignedInt",enumInteger},
{"Q{http://www.w3.org/2001/XMLSchema}unsignedShort",enumInteger},
{"Q{http://www.w3.org/2001/XMLSchema}unsignedByte",enumInteger},
{"Q{http://www.w3.org/2001/XMLSchema}normalizedString",enumString},
{"Q{http://www.w3.org/2001/XMLSchema}token",enumString},
{"Q{http://www.w3.org/2001/XMLSchema}anyType",enumString},
{"Q{http://www.w3.org/2001/XMLSchema}anySimpleType",enumString},
{"[xdmvalue", enumArrXdmValue}

};


PyObject *makePythonValue(void * value){
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    int typeRef = j_getXdmObjectType(thread, value);

    allocFn fn = operator new;

    if (typeRef == SXN_EXCEPTION) {

                //throw SaxonApiException();

        //zend_throw_exception(zend_ce_exception, "Error found in converting arguments", 0);
        return nullptr;
    }

    XDM_TYPE xdm_type = static_cast<XDM_TYPE>(typeRef);

    if (xdm_type == XDM_ATOMIC_VALUE) {

        char * primType = j_getPrimitiveTypeName(thread,(void *)fn, value);

                //XdmAtomicValue * aitem = (XdmAtomicValue *) (value->getHead());
                //const char * primType = aitem->getPrimitiveTypeName();

                if (getenv("SAXONC_DEBUG_FLAG")) {
                  std::cerr<<"Primitive type = "<< primType<<std::endl;
                }

                saxonTypeEnum typeValue;

                if(primType == NULL) {
                    typeValue = typeMap["Q{http://www.w3.org/2001/XMLSchema}anySimpleType"];
                } else {
                    typeValue = typeMap[primType];
                    operator delete(primType);
                }

        long bvalue;
        long lnumber;
        double dvalue;
        char * astri = nullptr;
        PyObject * unicodeStr;

        switch (typeValue) {
            case saxonTypeEnum::enumBool:
              bvalue = (long)j_getBooleanValue(thread, value);
              return PyBool_FromLong(bvalue);
              break;
            case saxonTypeEnum::enumInteger:
              lnumber = j_getLongValue(thread, (void *)value);//aitem->getLongValue();
              return PyLong_FromLong(lnumber);
              break;
            case saxonTypeEnum::enumString:
              astri = getStringValueForXdmItem(thread,(void *)fn, value, NULL);
              unicodeStr = PyUnicode_FromString(astri);
              operator delete(astri);
              return unicodeStr;
              break;
            case saxonTypeEnum::enumDouble:
            case saxonTypeEnum::enumFloat:
              dvalue = j_getDoubleValue(thread, value);
              return PyFloat_FromDouble(dvalue);
              break;

            default:
                if (getenv("SAXONC_DEBUG_FLAG")) {
                  std::cerr<<"Python internal: type not found"<< std::endl;
                }
                astri = getStringValueForXdmItem(thread,(void *)fn, value, NULL);//aitem->getStringValue();
                unicodeStr = PyUnicode_FromString(astri);
                operator delete(astri);
                return unicodeStr;
                break;

        }
    } else if (xdm_type == XDM_NODE) {

        //TODO shift creation of XdmNode to the python code
        int64_t copiedValue = (int64_t)j_copy_handles(SaxonProcessor::sxn_environ->thread, value);
        XdmNode * nxdmItem = new XdmNode(copiedValue);

        return nullptr;


      }  else if (xdm_type == XDM_MAP) {

         graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
        int count = j_xdmMap_size(thread, (void *)value);
        //int count = mitem->mapSize();

        if (getenv("SAXONC_DEBUG_FLAG")) {
          std::cerr<<"PythonValue: XDMMap size="<<count<<std::endl;
        }

        ///// creating Python dict array
        //zend_array *ret = zend_new_array(count);
        //zend_ulong idx = 0;
        //XdmAtomicValue ** aKeys = mitem->keys();

        //graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
        long long *rawKeys = j_xdmMap_keys(thread,(void *)fn, (void *)value);

        long long * rawMapValues = j_xdmMap_values(thread,(void *)fn, (void *)value);

        char * ss = nullptr;

          if (getenv("SAXONC_DEBUG_FLAG")) {
            std::cerr<<"PHPValue: XDMMap before loop call"<<std::endl;
          }

        for (int j = 0; j < count; j++) {
          //XdmValue * avaluej =mitem->get(aKeys[j]);
          //todo add encoding instead of NULL
          ss = getStringValueForXdmItem(thread,(void *)fn, (void *)rawKeys[j], NULL);
          //const char * ss = aKeys[j]->getStringValue();
        if (getenv("SAXONC_DEBUG_FLAG")) {
          std::cerr<<"PythonValue: XDMMap key str="<<ss<<std::endl;
        }
         // zend_string * keyzvalj = zend_string_init(ss, strlen(ss), 0);


          PyObject * retvalj = makePythonValue((void *)rawMapValues[j]);

          //TODO add value to the python dict
          //delete avaluej;

          operator delete(ss);

        }
        //delete [] aKeys;


        for(int j = 0; j < count; j++) {
          j_handles_destroy(thread, (void *)rawKeys[j]);
          j_handles_destroy(thread, (void *)rawMapValues[j]);
        }
        operator delete(rawKeys);
        operator delete(rawMapValues);
        /*delete mitem;
        delete value;*/

        //value->resetRelinquishedItems();
        //delete value;

        //TODO XdmMap objectHandle pool not getting cleared.. Clear Keys and values

        return nullptr;//retval;

        } else if (xdm_type == XDM_ARRAY) {

        graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
        int count = j_xdmArray_arrayLength(thread,(void *)value);

        if (getenv("SAXONC_DEBUG_FLAG")) {
          std::cerr<<"PythonValue: XDMarray size="<<count<<std::endl;
        }

        ///// creating Python array
        //TODO create python array

        long idx = 0;
        int64_t avaluej =-1;
        PyObject *  retvali = nullptr;
        for (int j = 0; j < count; j++) {

          avaluej =
              j_xdmArray_get(thread, (void *)value, j);
          //XdmValue * avaluej =aitem->get(j);
          //itemi->incrementRefCount();

          retvali = makePythonValue((void *)avaluej);

          j_handles_destroy(thread, (void *)avaluej);

          //TODO add item to the array

          idx++;
        }
        return nullptr;

}
}

void * userFunctionPythonNativeCall(void * proc, void * func, int useProc, int useNativeTypes, int numOfArguments,  void ** argumentsi){
    //PyObject_CallNoArgs((PyObject *)func);
    XdmValue ** arguments;
    //PyObject* argsList = nullptr;
    PyObject* arg = nullptr;
    PyObject *args = nullptr;
    int err;

    allocFn fn = operator new;


    if (getenv("SAXONC_DEBUG_FLAG")) {
          fprintf(stdout, "We are in a **Cython** function. calling Python logic \n");
          if(proc == NULL) {
            fprintf(stdout, "Python proc is NULL");
          }
        std::cerr << "arguments count="<< numOfArguments << std::endl;
        fprintf(stdout, "user function python proc pointer= %p func=%p\n", (void *)proc, (void *)func);
    }


    if(numOfArguments>0) {

        graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);

        //argsList = PyList_New(numOfArguments);

        if(useProc) {
            numOfArguments++;
        }

        args = PyTuple_New(numOfArguments);   // n = number of arguments

        PyObject * argi = nullptr;


        PyObject *pArgs;

        /*if(PyCallable_Check((PyObject *)proc)) {
             fprintf(stdout, "Processor is not callable in Python");
             return nullptr;
        } */

        XdmValue * tempValue = nullptr;
        Py_ssize_t i = 0;

        if(useProc) {
            if (getenv("SAXONC_DEBUG_FLAG")) {
                fprintf(stdout, "proc found !!!!.\n");
            }
            Py_INCREF((PyObject *) proc);
            PyTuple_SET_ITEM(args, i, (PyObject *) proc);
            i++;
        }


        for(Py_ssize_t j = 0; i < numOfArguments; i++, j++) {
            if (getenv("SAXONC_DEBUG_FLAG")) {
                fprintf(stdout, "Making XdmValue ref=%ld.\n", (long)argumentsi[j]);
            }

            long argxi = (long)argumentsi[j];//j_copy_handles(thread, (void *)argumentsi[i]);
            //arguments[i] = SaxonProcessor::makeXdmValueFromRef((int64_t)argxi);//, (void *)argumentsi[i]));

            if(useNativeTypes) {
                argi = makePythonValue(argumentsi[j]);
            } else {
                //TODO how to we handle a emptySequence??
                argi = PyObject_CallMethodOneArg((PyObject *) proc, PyUnicode_FromString("convert_c_obj"), PyLong_FromLong(argxi));
            }

            if(argi == Py_None ||  argi == nullptr) {
                if (getenv("SAXONC_DEBUG_FLAG")) {
                    PyErr_Print();
                }

                return nullptr;
            }

            Py_INCREF(argi);        // Ensure ownership
            PyTuple_SET_ITEM(args, i, argi);  // steals reference
        }

    }
    //PyErr_Print();

    PyObject* result = PyObject_Call((PyObject *)func, args, NULL); //PyObject_CallMethodObjArgs((PyObject *)func, (PyObject *) proc, args, NULL);
    //PyErr_Print();

    if(args != nullptr){
        //TODO decrement ref counts
        Py_DECREF(args);
    }

    if(func != nullptr) {
        Py_DECREF((PyObject *)func);
    }

    if(result == Py_None ||  result == nullptr) {
        if (getenv("SAXONC_DEBUG_FLAG")) {
            fprintf(stdout, "user function result is NULL\n");
        }
        return nullptr;
    }

    PyTypeObject * typeObj = Py_TYPE(result);

    graal_isolatethread_t *thread1 = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);

    if(typeObj != nullptr) {

        if (PyLong_Check(result)) {
            long valueL = PyLong_AsLong(result);
            int64_t longValueRef = j_makeLongValue(thread1, valueL);
            return (void *)longValueRef;
        } else if (PyFloat_Check(result)) {
            double valueD = PyFloat_AsDouble(result);
            int64_t doubleValueRef = j_makeDoubleValue(thread1, valueD);
            return (void *)doubleValueRef;
        } else if(PyBool_Check(result)) {
            int truth = PyObject_IsTrue(result);
            if (truth < 0) {
                return nullptr;
            }
            int64_t boolValueRef = j_makeBooleanValue(thread1, truth);
            return (void *)boolValueRef;

        } else if(PyUnicode_Check(result)) {

            const char* ss = PyUnicode_AsUTF8(result);
            if (ss == NULL) {
                return nullptr;
            }
            int64_t stringValueRef = j_makeStringValue(thread1, (char *)ss, nullptr);

            return (void *)stringValueRef;

        }

        //TODO handle other python types - e.g Set, Dict, list



        //PyObject * typeObji = PyType_GetName(typeObj);
        const char* c_typeObjStr = Py_TYPE(result)->tp_name; //PyUnicode_AsUTF8(typeObji);

        std::string typeObjStr = std::string(c_typeObjStr);

        std::string prefix("PyXdm");
        if (getenv("SAXONC_DEBUG_FLAG")) {
            fprintf(stderr, "We are in a CythonUserFunctionHandler type= %s\n", c_typeObjStr);
            fflush(stdout);
        }

        if(typeObjStr.find(prefix) == std::string::npos) {
            //TODO try to handle Python primitive types
            return nullptr;
        }

    }

    long resulti = PyLong_AsLong(PyObject_CallMethodNoArgs(result, PyUnicode_FromString("_get_underlying_int_value")));

    /*if(result != nullptr){
        Py_DECREF(result);
    }*/

    if(resulti < 0) {
        return nullptr;
    }

    return  (void *)resulti;
}

void setNativePythonFunction(){
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    j_setPythonNative(thread);
    nativePythonFn fn1 = userFunctionPythonNativeCall;

    j_setIndirectFunctionForPythonNativeCall(thread, (void *)fn1);


}
