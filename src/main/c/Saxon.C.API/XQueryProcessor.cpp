#include "XQueryProcessor.h"
#include "XdmValue.h"
#include "XdmItem.h"
#include "XdmNode.h"
#include "XdmAtomicValue.h"
#include "XdmFunctionItem.h"
#include "XdmMap.h"
#include "XdmArray.h"


#ifdef MEM_DEBUG
#define new new(__FILE__, __LINE__)
#endif

    XQueryProcessor::XQueryProcessor() {
	SaxonProcessor *p = new SaxonProcessor(false);
	XQueryProcessor(p, "");
     }


    XQueryProcessor::~XQueryProcessor(){
        SaxonProcessor::attachCurrentThread();
        clearProperties();
        clearParameters();
        cppXQ = -1;
        cwdXQ.erase();
        exceptionClear();
    }

    XQueryProcessor::XQueryProcessor(SaxonProcessor *p, std::string curr) {
        SaxonProcessor::attachCurrentThread();
    proc = p;
        exception = nullptr;
        cppXQ = j_createXQueryEngineWithProcessor(SaxonProcessor::sxn_environ->thread, (void *)p->procRef);


        if(cppXQ == -1) {

            std::cerr<<"Error: Failed to create the XPathProcessor internal object"<<std::endl;

            return;
        }
    
#ifdef DEBUG
	//jmethodID debugMID = SaxonProcessor::sxn_environ->env->GetStaticMethodID(cppClass, "setDebugMode", "(Z)V");
	//SaxonProcessor::sxn_environ->env->CallStaticVoidMethod(cppClass, debugMID, (jboolean)true);
#endif


   // outputfile1 = "";
	if(!(proc->cwd.empty()) && curr.empty()){
		cwdXQ = proc->cwd;
	} else {
		cwdXQ = curr;
	}
}



XQueryProcessor::XQueryProcessor(const XQueryProcessor &other) {
    cwdXQ = other.cwdXQ;
	proc = other.proc; //TODO check thread safety
	
	cppXQ = other.cppXQ;
    std::map<std::string, XdmValue*>::const_iterator paramIter = other.parameters.begin();
    while(paramIter != other.parameters.end())
    {

        XdmValue * valuei = paramIter->second;
        if(valuei == nullptr) {

        } else {
            parameters[paramIter->first] = new XdmValue(*(valuei));
        }
        paramIter++;
    }

    std::map<std::string, std::string>::const_iterator propIter = other.properties.begin();
    while (propIter != other.properties.end()) {
        properties[propIter->first] = propIter->second;
        propIter++;
    }


}


XQueryProcessor * XQueryProcessor::clone() {
      XQueryProcessor * proc = new XQueryProcessor(*this);
      return proc;


}


std::map<std::string,XdmValue*>& XQueryProcessor::getParameters(){
	std::map<std::string,XdmValue*>& ptr = parameters;
	return ptr;
}

std::map<std::string,std::string>& XQueryProcessor::getProperties(){
	std::map<std::string,std::string> &ptr = properties;
	return ptr;
}


    /**
     * Set the source document for the query
    */
    void XQueryProcessor::setContextItem(XdmItem * value){
    	if(value != nullptr){
	 value->incrementRefCount();
     	 parameters["node"] = (XdmValue *)value;
    	}
    }


     void XQueryProcessor::declareNamespace(const char *prefix, const char * uri){
        if (prefix == nullptr || uri == nullptr) {
		    return;
        }  else {
            //setProperty("ns-prefix", uri);
             int s = properties.size();
             std::string skey = std::string("ns-prefix:") + prefix;
             properties.insert(std::pair<std::string, std::string>(skey, std::string(uri)));

             if(s == properties.size()) {
                 std::map<std::string, std::string>::iterator it;
                 it = properties.find(skey);
                 if (it != properties.end()) {
                       properties.erase(skey);
                       properties[skey] = std::string(uri);
                 }
             }

        }
}


    /**
     * Set the source document for the query
    */
    void XQueryProcessor::setContextItemFromFile(const char * ifile){
	setProperty("s", ifile);
    }

    /**
     * Set the output file where the result is sent
    */
    void XQueryProcessor::setOutputFile(const char* ofile){
      // outputfile1 = std::string(ofile); 
       setProperty("o", ofile);
    }

    /**
     * Set a parameter value used in the query
     *
     * @param name  of the parameter, as a string
     * @param value of the query parameter, or nullptr to clear a previously set value
     */
    void XQueryProcessor::setParameter(const char * name, XdmValue*value){
        if(value != nullptr && name != nullptr) {
            std::string skey = ("param:" + std::string(name));
            std::map<std::string, XdmValue *>::iterator it = parameters.find(skey);
            if (it != parameters.end()) {
                XdmValue *valuei = it->second;
                if (valuei != nullptr) {
                    valuei->decrementRefCount();
                    parameters.erase(skey);
                }
            }
            parameters[skey] = value;
        }
    }


    /**
     * Remove a parameter (name, value) pair
     *
     * @param namespacei currently not used
     * @param name  of the parameter
     * @return bool - outcome of the romoval
     */
    bool XQueryProcessor::removeParameter(const char * name){
	return (bool)(parameters.erase("param:"+std::string(name)));
    }
    /**
     * Set a property.
     *
     * @param name of the property
     * @param value of the property
     */
    void XQueryProcessor::setProperty(const char * name, const char * value){
#ifdef DEBUG	
	if(value == nullptr) {
		std::cerr<<"XQueryProc setProperty is nullptr"<<std::endl;
	}
#endif
        if (name != nullptr) {
            properties.erase(name);
            std::string skey = std::string(name);
            properties[skey] = std::string((value == nullptr ? "" : value));
        }

    }

    void XQueryProcessor::clearParameters(bool delVal){
	if(delVal){
       		for(std::map<std::string, XdmValue*>::iterator itr = parameters.begin(); itr != parameters.end(); itr++) {
                XdmValue *value = itr->second;
                if (value != nullptr) {
#ifdef DEBUG
                    std::cerr<<"XQueryProc.clearParameter() - XdmValue refCount="<<value->getRefCount()<<std::endl;
#endif
                        delete value;
                        value = nullptr;
                }
            }
	}
	parameters.clear();
    }

   void XQueryProcessor::clearProperties(){
	properties.clear();
        //outputfile1.clear();
   }


   void XQueryProcessor::setcwd(const char* dir){
    cwdXQ = std::string(dir);
   }

    void XQueryProcessor::setQueryBaseURI(const char * baseURI){
	setProperty("base", baseURI);
    }


    void XQueryProcessor::setUpdating(bool updating){
        SaxonProcessor::attachCurrentThread();
    	if (updating) {
            setProperty("updating", "on");
    	} else {
            setProperty("updating", "off");
    	}

    }

    void XQueryProcessor::executeQueryToFile(const char * infilename, const char * ofilename, const char * query){
        SaxonProcessor::attachCurrentThread();


        setProperty("resources", proc->getResourcesDirectory());
        if(infilename != nullptr) {
            setProperty("s", infilename);
        }
        if(query != nullptr) {
            setProperty("qs", query);
        }
        int64_t  processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties);
        j_executeQueryToFile(SaxonProcessor::sxn_environ->thread, (void *)cppXQ, (char *)cwdXQ.c_str(), (char *)ofilename, (void *)processorDataRef);


	  if(proc->exceptionOccurred()) {
          createException();
      }
	 


   }


    XdmValue * XQueryProcessor::executeQueryToValue(const char * infilename, const char * query){
        SaxonProcessor::attachCurrentThread();
        SaxonProcessor::attachCurrentThread();


        setProperty("resources", proc->getResourcesDirectory());
        if(infilename != nullptr) {
            setProperty("s", infilename);
        }
        if(query != nullptr) {
            setProperty("qs", query);
        }
        int64_t  processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties);
        int64_t result = j_executeQueryToValue(SaxonProcessor::sxn_environ->thread, (void *)cppXQ, (char *)cwdXQ.c_str(),(void *)processorDataRef);


        if(proc->exceptionOccurred()) {
            createException();
        }

       /*
        * TODO check type of the value returned
        * if(result) {
		jclass atomicValueClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmAtomicValue");
		jclass nodeClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmNode");
		jclass functionItemClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmFunctionItem");
        jclass mapItemClass = lookForClass(SaxonProcessor::sxn_environ->env,
                                           "net/sf/saxon/s9api/XdmMap");
        jclass arrayItemClass = lookForClass(SaxonProcessor::sxn_environ->env,
                                             "net/sf/saxon/s9api/XdmArray");
		XdmValue * value = new XdmValue();
		XdmItem * xdmItem = nullptr;

		if(SaxonProcessor::sxn_environ->env->IsInstanceOf(result, atomicValueClass)           == JNI_TRUE) {
				xdmItem = new XdmAtomicValue(result);
				SaxonProcessor::sxn_environ->env->DeleteLocalRef(result);
                value->addXdmItem(xdmItem);
                return value;

			} else if(SaxonProcessor::sxn_environ->env->IsInstanceOf(result, nodeClass)           == JNI_TRUE) {
				xdmItem = new XdmNode(result);
				SaxonProcessor::sxn_environ->env->DeleteLocalRef(result);
                value->addXdmItem(xdmItem);
                return value;

			 } else if (SaxonProcessor::sxn_environ->env->IsInstanceOf(result, mapItemClass) == JNI_TRUE) {
                xdmItem = new XdmMap(result);
                SaxonProcessor::sxn_environ->env->DeleteLocalRef(result);
                value->addXdmItem(xdmItem);
                return value;
            } else if (SaxonProcessor::sxn_environ->env->IsInstanceOf(result, arrayItemClass) == JNI_TRUE) {
                xdmItem = new XdmArray(result);
                SaxonProcessor::sxn_environ->env->DeleteLocalRef(result);
                value->addXdmItem(xdmItem);
                return value;
            } else if (SaxonProcessor::sxn_environ->env->IsInstanceOf(result, functionItemClass)           == JNI_TRUE) {
                xdmItem = new XdmFunctionItem(result);
                SaxonProcessor::sxn_environ->env->DeleteLocalRef(result);
                value->addXdmItem(xdmItem);
                return value;
        } else {
				value = new XdmValue(result, true);
				return value;
			}
     } */
	   
	// exception = proc->checkAndCreateException(cppClass);


  return nullptr;

}

    const char * XQueryProcessor::executeQueryToString(const char * infilename, const char * query){
        SaxonProcessor::attachCurrentThread();
        SaxonProcessor::attachCurrentThread();


        setProperty("resources", proc->getResourcesDirectory());
        if(infilename != nullptr) {
            setProperty("s", infilename);
        }
        if(query != nullptr) {
            setProperty("qs", query);
        }
        int64_t  processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties);
        char *result = j_executeQueryToString(SaxonProcessor::sxn_environ->thread, (void *)cppXQ, (char *)cwdXQ.c_str(), (void *)processorDataRef);


        return (const char *)result;


    }


    const char * XQueryProcessor::runQueryToString(){
	return executeQueryToString(nullptr, nullptr);

    }


    XdmValue * XQueryProcessor::runQueryToValue(){
	return executeQueryToValue(nullptr, nullptr);
   }

    void XQueryProcessor::runQueryToFile(){
	executeQueryToFile(nullptr, nullptr, nullptr);
   }

    void XQueryProcessor::setQueryFile(const char * ofile){
	   //outputfile1 = std::string(ofile); 
	   setProperty("q", ofile);
    }

   void XQueryProcessor::setQueryContent(const char* content){
	  // outputfile1 = std::string(content); 
	   setProperty("qs", content);
  }



void XQueryProcessor::exceptionClear(){
    SaxonProcessor::attachCurrentThread();
    cppXQ = -1;
	if(exception != nullptr) {
		delete exception;
		exception = nullptr;

	}


   
 
}

bool XQueryProcessor::exceptionOccurred(){
    return proc->exceptionOccurred() || exception != nullptr;
}


const char * XQueryProcessor::getErrorCode() {
	if(exception == nullptr) {return nullptr;}
	return exception->getErrorCode();
}

const char * XQueryProcessor::getErrorMessage(){
	if(exception == nullptr) {return nullptr;}
	return exception->getMessage();
}

const char* XQueryProcessor::checkException(){
	/*if(proc->exception == nullptr) {
		proc->exception = proc->checkForException(SaxonProcessor::sxn_environ->env, cppClass, cppXQ);
	}
        return proc->exception;*/
	return proc->checkException();
}



SaxonApiException* XQueryProcessor::getException() {
    return exception;
}

void XQueryProcessor::createException(const char * message) {
    if(exception != nullptr) {
        delete exception;
        exception = nullptr;
    }
    if(message == nullptr) {
        //exception = proc->checkAndCreateException(cppClass);
    } else {
        exception = new SaxonApiException(message);
    }

}
