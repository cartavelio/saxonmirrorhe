#include "XPathProcessor.h"
#include "XdmValue.h"
#include "XdmItem.h"
#include "XdmNode.h"
#include "XdmAtomicValue.h"
#include "XdmFunctionItem.h"
#include "XdmMap.h"
#include "XdmArray.h"

#include <iostream>
#include <fstream>


#ifdef MEM_DEBUG
#define new new(__FILE__, __LINE__)
#endif

XPathProcessor::XPathProcessor() {
	SaxonProcessor *p = new SaxonProcessor(false);
	XPathProcessor(p, "");
}

XPathProcessor::XPathProcessor(SaxonProcessor* p, std::string curr) {
	proc = p;
    //SaxonProcessor::attachCurrentThread();

	/*cppClass = lookForClass(SaxonProcessor::sxn_environ->env,
			"net/sf/saxon/option/cpp/XPathProcessor");
	if ((proc->proc) == nullptr) {
		std::cerr << "Processor is nullptr" << std::endl;
	}

    jobject tempcppXP = createSaxonProcessor2(SaxonProcessor::sxn_environ->env, cppClass,
			"(Lnet/sf/saxon/s9api/Processor;)V", proc->proc);*/


    cppXP = createXPathProcessorWithProcessor(SaxonProcessor::sxn_environ->thread, (void *)p->procRef);


    if(cppXP == -1) {

        std::cerr<<"Error: Failed to create the XPathProcessor internal object"<<std::endl;

        return;
    }



#ifdef DEBUG
	/*jmethodID debugMID = SaxonProcessor::sxn_environ->env->GetStaticMethodID(cppClass, "setDebugMode", "(Z)V");
	SaxonProcessor::sxn_environ->env->CallStaticVoidMethod(cppClass, debugMID, (jboolean)true);*/
#endif    

	exception = nullptr;
	if(!(proc->cwd.empty()) && curr.empty()){
		cwdXP = proc->cwd;
	} else {
		cwdXP = curr;
	}

}


XPathProcessor::XPathProcessor(const XPathProcessor &other) {
    cwdXP = other.cwdXP;
    proc = other.proc; //TODO check thread safety

    cppXP = other.cppXP;
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


XPathProcessor::~XPathProcessor(){
    //SaxonProcessor::attachCurrentThread();
    clearProperties();
    clearParameters();

    cppXP = -1;
    //exceptionClear();

}

XdmValue * XPathProcessor::evaluate(const char * xpathStr) {
    //SaxonProcessor::attachCurrentThread();
  	if (xpathStr == nullptr) {
		std::cerr << "Error:: XPath string cannot be empty or nullptr" << std::endl;
	return nullptr;
    }


	setProperty("resources", proc->getResourcesDirectory());
    allocFn fn = operator new;
    int64_t  processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties);
    long long * results = j_evaluate(SaxonProcessor::sxn_environ->thread, (void *)cppXP, (void *)fn, (char *)cwdXP.c_str(), (char *)xpathStr, (void *)processorDataRef);

	if(!results) {

		//exception = proc->checkAndCreateException(cppClass);

		/*if(exception != nullptr) {
		    std::cout<<exception->getMessage();
		}*/
		return nullptr;
	}

	//int sizex = SaxonProcessor::sxn_environ->env->GetArrayLength(results);


	//if (sizex>0) {

	    //TODO get type from the array
		/*jclass atomicValueClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmAtomicValue");
		jclass nodeClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmNode");
		jclass functionItemClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmFunctionItem");
        jclass mapClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmMap");
        jclass arrayClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmArray");

		XdmValue * value = new XdmValue();
		XdmItem * xdmItem = nullptr;
		for (int p=0; p < sizex; ++p) 
		{
			jobject resulti = SaxonProcessor::sxn_environ->env->GetObjectArrayElement(results, p);

			if(SaxonProcessor::sxn_environ->env->IsInstanceOf(resulti, atomicValueClass)           == JNI_TRUE) {
				xdmItem = new XdmAtomicValue(resulti);

			} else if(SaxonProcessor::sxn_environ->env->IsInstanceOf(resulti, nodeClass)           == JNI_TRUE) {
				xdmItem = new XdmNode(resulti);

			}  else if (SaxonProcessor::sxn_environ->env->IsInstanceOf(resulti, mapClass)           == JNI_TRUE) {
                xdmItem =  new XdmMap(resulti);
            } else if (SaxonProcessor::sxn_environ->env->IsInstanceOf(resulti, arrayClass)           == JNI_TRUE) {
                xdmItem =  new XdmArray(resulti);
            } else if (SaxonProcessor::sxn_environ->env->IsInstanceOf(resulti, functionItemClass)           == JNI_TRUE) {
                xdmItem =  new XdmFunctionItem(resulti);
            } else {
			    continue;
			}
  

			value->addXdmItem(xdmItem);
		}*/

		//return value;
	
//}
return nullptr;

}

XdmItem * XPathProcessor::evaluateSingle(const char * xpathStr) {


    if (xpathStr == nullptr) {
        std::cerr << "Error:: XPath string cannot be empty or nullptr" << std::endl;
        return nullptr;
    }


    setProperty("resources", proc->getResourcesDirectory());
    int64_t  processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties);
    int64_t results = j_evaluateSingle(SaxonProcessor::sxn_environ->thread, (void *)cppXP, (char *)cwdXP.c_str(), (char *)xpathStr, (void *)processorDataRef);

/*
	if (result) {
		jclass atomicValueClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmAtomicValue");
		jclass nodeClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmNode");
		jclass functionItemClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmFunctionItem");
        jclass mapClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmMap");
        jclass arrayClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmArray");
		XdmItem * xdmItem = nullptr;
		if(SaxonProcessor::sxn_environ->env->IsInstanceOf(result, atomicValueClass)           == JNI_TRUE) {
			xdmItem = new XdmAtomicValue(result);

		} else if(SaxonProcessor::sxn_environ->env->IsInstanceOf(result, nodeClass)           == JNI_TRUE) {

			xdmItem = new XdmNode(result);

		} else if (SaxonProcessor::sxn_environ->env->IsInstanceOf(result, mapClass)           == JNI_TRUE) {
            xdmItem =  new XdmMap(result);
        } else if (SaxonProcessor::sxn_environ->env->IsInstanceOf(result, arrayClass)           == JNI_TRUE) {
            xdmItem =  new XdmArray(result);
        } else if (SaxonProcessor::sxn_environ->env->IsInstanceOf(result, functionItemClass)           == JNI_TRUE) {
		    xdmItem = new XdmFunctionItem(result);

		}
		SaxonProcessor::sxn_environ->env->DeleteLocalRef(result);
		return xdmItem;
	} else  {
			exception = proc->checkAndCreateException(cppClass);

     		}*/


return nullptr;

}

void XPathProcessor::setContextItem(XdmItem * item) {
    if(item == nullptr) {
       std::map<std::string, XdmValue*>::iterator it;
       it = parameters.find("node");
       if (it != parameters.end()) {
            XdmValue * valuei = it->second;
            if(valuei != nullptr) {
                valuei->decrementRefCount();
                parameters.erase("node");
            }
       }
    } else {
        setParameter("node", item, false);
    }
}

void XPathProcessor::setContextFile(const char * filename) {

	if (filename != nullptr) {
		setProperty("s", filename);
	}  else {
	    properties.erase("s");
	}
}


void XPathProcessor::declareNamespace(const char *prefix, const char * uri){
    //SaxonProcessor::attachCurrentThread();
        if (uri == nullptr) {
		return;
        }
	j_xp_declareNamespace(SaxonProcessor::sxn_environ->thread, (void *)cppXP, (char *)prefix, (char *)uri);

}


void XPathProcessor::declareVariable(const char * name){
    SaxonProcessor::attachCurrentThread();
    if (name == nullptr) {
		return;
    }
    j_xp_declareVariable(SaxonProcessor::sxn_environ->thread, (void *)cppXP, (char *)name);

}


#if CVERSION_API_NO >= 121


void XPathProcessor::setBackwardsCompatible(bool option) {
    if (option) {
        		setProperty("backwardsCom:", "true");
    } else {
       properties.erase("backwardsCom:");
    }
}

void XPathProcessor::setCaching(bool caching){
    if (caching) {
        	setProperty("caching:", "true");
    } else {
       properties.erase("caching:");
    }

}

void XPathProcessor::importSchemaNamespace(const char* uri){
      if (uri != nullptr) {
       		setProperty("importSN", uri);
      }
}


#endif


void XPathProcessor::setBaseURI(const char * uriStr) {
    j_xp_setBaseURI(SaxonProcessor::sxn_environ->thread, (void *)cppXP, (char *)uriStr);


}

bool XPathProcessor::effectiveBooleanValue(const char * xpathStr) {

    setProperty("resources", proc->getResourcesDirectory());
    int64_t  processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties);
    //bool resultsi = (bool)j_effectiveBooleanValue(SaxonProcessor::sxn_environ->thread, (void *)cppXP, (char *)cwdXP.c_str(), (char *)xpathStr, (void *)processorDataRef);
    return false;//resultsi;
}


void XPathProcessor::setParameter(const char * name, XdmValue*value, bool withParam) {
    if(value != nullptr && name != nullptr) {
        std::string skey = (withParam ? "param:" : "")+std::string(name);
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



void XPathProcessor::setParameter(const char * name, XdmValue* value) {
    setParameter(name, value, true);
}

bool XPathProcessor::removeParameter(const char * name) {
	return (bool)(parameters.erase("param:"+std::string(name)));
}

void XPathProcessor::setProperty(const char * name, const char * value) {
	if(name != nullptr) {
	    int s = properties.size();
		std::string skey = std::string(name);
		properties.insert(std::pair<std::string, std::string>(skey, std::string((value == nullptr ? "" : value))));

		if(s == properties.size()) {
            std::map<std::string, std::string>::iterator it;
            it = properties.find(skey);
            if (it != properties.end()) {
                properties.erase(skey);
                properties[skey] = std::string((value == nullptr ? "" : value));
            }
		}
	}
}

void XPathProcessor::clearParameters(bool delVal) {
//std::cerr<<"XPath clearParameter"<<std::endl;
	if(delVal) {
        for (std::map<std::string, XdmValue *>::iterator itr = parameters.begin(); itr != parameters.end(); itr++) {
            XdmValue *value = itr->second;
            if (value != nullptr) {
#ifdef DEBUG
                std::cerr<<"XPathProc.clearParameter() - XdmValue refCount="<<value->getRefCount()<<std::endl;
#endif
                    delete value;
            }
        }
    }
	parameters.clear();

}

void XPathProcessor::clearProperties() {
	properties.clear();
}


   void XPathProcessor::setcwd(const char* dir){
    cwdXP = std::string(dir);
   }

std::map<std::string,XdmValue*>& XPathProcessor::getParameters(){
	std::map<std::string,XdmValue*>& ptr = parameters;
	return ptr;
}

std::map<std::string,std::string>& XPathProcessor::getProperties(){
	std::map<std::string,std::string> &ptr = properties;
	return ptr;
}

void XPathProcessor::exceptionClear(){
    SaxonProcessor::attachCurrentThread();

    if(exception != nullptr) {
        delete exception;
        exception = nullptr;

    }

}

const char * XPathProcessor::getErrorCode() {
	if(exception == nullptr) {return nullptr;}
	return exception->getErrorCode();
}

const char * XPathProcessor::getErrorMessage(){
	if(exception == nullptr) {return nullptr;}
	return exception->getMessage();
}

    SaxonApiException * XPathProcessor::getException() {
        return exception;

}

bool XPathProcessor::exceptionOccurred() {
    return proc->exceptionOccurred() || exception != nullptr;
}



void XPathProcessor::createException(const char * message) {
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



