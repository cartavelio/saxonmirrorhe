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
        std::cerr << "arguments count="<< numOfArguments << std::endl;
        fprintf(stdout, "user function python proc pointer= %p func=%p\n", (void *)proc, (void *)func);
    }


    if(proc != nullptr && numOfArguments>0) {

        graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);

        argsList = PyList_New(numOfArguments);

        PyObject * argi = nullptr;


        PyObject *pArgs;

        /*if(PyCallable_Check((PyObject *)proc)) {
             fprintf(stdout, "Processor is not callable in Python");
             return nullptr;
        } */

        XdmValue * tempValue = nullptr;

        for(int i =0; i< numOfArguments; i++) {
            if (getenv("SAXONC_DEBUG_FLAG")) {
                fprintf(stdout, "Making XdmValue ref=%ld.\n", (long)argumentsi[i]);
            }

            long argxi = (long)j_copy_handles(thread, (void *)argumentsi[i]);
            //arguments[i] = SaxonProcessor::makeXdmValueFromRef((int64_t)argxi);//, (void *)argumentsi[i]));


           /*argi = PyObject_CallMethodOneArg((PyObject *) proc, PyUnicode_FromString("convert_c_obj"),
                                              (PyObject *) arguments[i]); */

            argi = PyObject_CallMethodOneArg((PyObject *) proc, PyUnicode_FromString("convert_c_obj"), PyLong_FromLong(argxi));

            if(argi == Py_None ||  argi == nullptr) {
                //PyErr_Print();

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

    PyObject* result = PyObject_CallFunctionObjArgs((PyObject *)func, (PyObject *) proc, argsList, NULL);

    if(argsList != nullptr){
        //TODO decrement ref counts
        Py_DECREF(argsList);
    }

    if(result == Py_None ||  result == nullptr) {
        return nullptr;
    }

    //fprintf(stderr, "We are in a CythonUserFunctionHandler cp1\n");

    PyTypeObject * typeObj = Py_TYPE(result);

    if(typeObj != nullptr) {
        //TODO special case Xdm types. The primitive types need to be handled too
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

    }

    long resulti = PyLong_AsLong(PyObject_CallMethodNoArgs(result, PyUnicode_FromString("_get_underlying_int_value")));

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
