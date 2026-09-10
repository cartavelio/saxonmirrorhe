//#include "saxonc/UserFunctionHandler.h"
#include "Python.h"
//#include "saxonc/XdmValue.h"

extern "C" PyObject * CythonUserFunctionHandler;

void * userFunctionPythonNativeCall(void * proc, void * func, int useNativeTypes, int numOfArguments,  void ** argumentsi){
    //PyObject_CallNoArgs((PyObject *)func);
    XdmValue ** arguments;
    PyObject* argsList = nullptr;
    PyObject* arg = nullptr;
    int err;

    if (getenv("SAXONC_DEBUG_FLAG")) {
          fprintf(stdout, "We are in a **Cython** function. calling Python logic \n");
          if(proc == NULL) {
            fprintf(stdout, "Python proc is NULL");
          }
    }

    fprintf(stdout, "user function python pointer= %p func=%p\n", (void *)proc, (void *)func);



    if(proc != nullptr && numOfArguments>0) {

        arguments = new XdmValue*[numOfArguments];

        for(int i =0; i< numOfArguments; i++) {
            if (getenv("SAXONC_DEBUG_FLAG")) {
                fprintf(stdout, "Making XdmValue YYYIII ref=%ld.\n", (long)argumentsi[i]);
            }
            arguments[i] = SaxonProcessor::makeXdmValueFromRef((int64_t)argumentsi[i]);

            if(arguments[i] == NULL) {
                        std::cerr << "Failed to construct list for arguments argument "<<i<< " is NULL" << std::endl;
                        return nullptr;
                    }
        }
        fprintf(stderr, "We are in a CythonUserFunctionHandler proc is not null\n");
        //arg = ((PyObject *)proc)->wrap_atomic_value(arguments[0]);
        //arg = proc_wrap_atomic_value((PyObject *)proc, arguments[0]);

        //Py_ssize_t size = numOfArguments;

        argsList = PyList_New(numOfArguments);

        if(argsList == nullptr) {
            std::cerr << "Failed to construct list for arguments" << std::endl;
            return nullptr;

        }

        std::cerr << "arguments count="<< numOfArguments << std::endl;

        //args = new PyObject[numOfArguments];
        PyObject * argi = nullptr;

        for(int i = 0; i< numOfArguments; i++) {
        std::cerr << "arguments loop cp0: "<<i<< std::endl;
        if(arguments[i] == NULL) {
            std::cerr << "Failed to construct list for arguments argument "<<i<< " is NULL" << std::endl;
            return nullptr;
        }

        // Check both indices are valid
            /*PyObject *a = (PyObject*)proc;
            if (!a) return NULL;

            PyObject *b = (PyObject*)arguments[i];
            if (!b) return NULL;

             if (PyDict_Check(a) || PyDict_Check(b)) return NULL;*/

           fprintf(stdout, "user function xdm pointer[%i]= %p \n", i, (void *)arguments[i]);


           /*argi = PyObject_CallMethodOneArg((PyObject *) proc, PyUnicode_FromString("__convert_c_obj"),
                                              (PyObject *) arguments[i]); */

            argi = PyObject_CallMethodObjArgs((PyObject *) proc, PyUnicode_FromString("__convert_c_obj"),
                                              (PyObject *) arguments[i], NULL);
            //Py_INCREF(argi);
            std::cerr << "arguments loop cp1"<< std::endl;
            if(argi == nullptr) {
                return nullptr;

            }
            err = PyList_SetItem(argsList, i, argi);
            if (err < 0) {
                //std::cerr << "Failed construction of Argument for list i="<<i << std::endl;
                //std::cerr << "Error code"<<err << std::endl;
                return nullptr;
            }


        }
    }

     if (getenv("SAXONC_DEBUG_FLAG")) {
        fprintf(stdout, "checkpoint 1\n");
     }
    //fflush(stdout);

    //arg = PyLong_FromLong(5);

    //PyObject* result = PyObject_CallOneArg((PyObject *)func, arg);


    //PyObject* result = PyObject_CallObject((PyObject *)func, argsList);

    PyObject* result = PyObject_CallFunctionObjArgs((PyObject *)func, (PyObject *) proc, argsList, NULL);

    if(argsList != nullptr){
        //TODO decrement ref counts
        Py_DECREF(argsList);
    }

    fprintf(stderr, "We are in a CythonUserFunctionHandler cp0\n");
    if(result == Py_None ||  result == nullptr) {
        fprintf(stderr, "We are in a CythonUserFunctionHandler cp0a\n");
        return nullptr;
    }

    fprintf(stderr, "We are in a CythonUserFunctionHandler cp1\n");

    PyTypeObject * typeObj = Py_TYPE(result);

    if(typeObj != nullptr) {
        //PyObject * typeObji = PyType_GetName(typeObj);
        const char* c_typeObjStr = Py_TYPE(result)->tp_name; //PyUnicode_AsUTF8(typeObji);

        std::string typeObjStr = std::string(c_typeObjStr);

        std::string prefix("PyXdm");
        fprintf(stderr, "We are in a CythonUserFunctionHandler type= %s\n", c_typeObjStr);
        fflush(stdout);

        if(typeObjStr.find(prefix) == std::string::npos) {
            //TODO try to handle Python primitive types
            return nullptr;
        }


        //std::cerr<< "Type of userFunction result = "<<c_typeObjStr<<std::endl;

    }
    fprintf(stderr, "We are in a CythonUserFunctionHandler cp2\n");

    long resulti = PyLong_AsLong(PyObject_CallMethodNoArgs(result, PyUnicode_FromString("__get_underlying_int_value")));

    //fflush(stdout);

    return  (void *)resulti;
}

void setNativePythonFunction(){
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    j_setPythonNative(thread);
    nativePythonFn fn1 = userFunctionPythonNativeCall;

    j_setIndirectFunctionForPythonNativeCall(thread, (void *)fn1);


}
