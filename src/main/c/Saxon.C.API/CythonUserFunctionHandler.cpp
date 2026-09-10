#include "CythonUserFunctionHandler.h"
#include "XdmValue.h"

extern "C" PyObject * CythonUserFunctionHandler;

void * python_nativeFunctionCall(char * functionName, void * proc, void * func, int numOfArguments,  XdmValue ** arguments){
    //PyObject_CallNoArgs((PyObject *)func);
    PyObject* argsList = nullptr;
    PyObject* arg = nullptr;
    int err;



    if(proc != nullptr && numOfArguments>0) {
        //fprintf(stderr, "We are in a CythonUserFunctionHandler proc is not null\n");
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
            argi = PyObject_CallMethodObjArgs((PyObject *) proc, PyUnicode_FromString("__convert_c_obj"),
                                              (PyObject *) arguments[i], NULL);
            //Py_INCREF(argi);

            if(argi == nullptr) {
                std::cerr << "Argument is null "<< std::endl;
                return nullptr;

            }
            err = PyList_SetItem(argsList, i, argi);
            if (err < 0) {
                std::cerr << "Failed construction of Argument for list i="<<i << std::endl;
                std::cerr << "Error code"<<err << std::endl;
                return nullptr;
            }

            std::cerr << "CthonUserFunction cp0" << std::endl;

        }
    }
    //fflush(stdout);

    //arg = PyLong_FromLong(5);

    //PyObject* result = PyObject_CallOneArg((PyObject *)func, arg);
    

    //PyObject* result = PyObject_CallObject((PyObject *)func, argsList);

    PyObject* result = PyObject_CallFunctionObjArgs((PyObject *)func, argsList, NULL);

    if(argsList != nullptr){
        //TODO decrement ref counts
        Py_DECREF(argsList);
    }


    if(Py_IsNone(result) || result == nullptr) {
        return nullptr;
    }

    PyTypeObject * typeObj = Py_TYPE(result);

    if(typeObj != nullptr) {
        PyObject * typeObji = PyType_GetName(typeObj);
        const char* c_typeObjStr = PyUnicode_AsUTF8(typeObji);

        std::string typeObjStr = std::string(c_typeObjStr);

        std::string prefix("PyXdm");

        if(typeObjStr.compare(0,prefix.size(),prefix)) {
            //TODO try to handle Python primitive types
            return nullptr;
        }



        std::cerr<< "Type of userFunction result = "<<c_typeObjStr<<std::endl;



    }



    long resulti = PyLong_AsLong(PyObject_CallMethodNoArgs(result, PyUnicode_FromString("__get_underlying_int_value")));



    fprintf(stderr, "We are end of CythonUserFunctionHandler cp2\n");

    fflush(stdout);

    return  (void *)resulti;





}
