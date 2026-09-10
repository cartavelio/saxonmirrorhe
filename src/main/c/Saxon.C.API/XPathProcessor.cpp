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
	exception = new SaxonApiException("Error:: XPath string cannot be empty or nullptr");
	return nullptr;
    }


	setProperty("resources", proc->getResourcesDirectory());
    allocFn fn = operator new;
    int64_t  processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties,0);
    std::cerr<<"evaluate ======"<<std::endl;
    long long * results = j_evaluate(SaxonProcessor::sxn_environ->thread, (void *)fn, (void *)cppXP, (char *)cwdXP.c_str(), (char *)xpathStr, (void *)(processorDataRef == -1 ? (void *) NULL: (void *)processorDataRef));

	if(results != nullptr) {
	    XdmValue * value = new XdmValue();
	    int size = results[0];
        std::cerr<<"size="<<size<<std::endl;
    for(int i = 1; i<size; i++){
        if(results[i]=='\0') {
            break;
        }
        std::cerr<<"i"<<i<<" result"<<results[i]<<std::endl;
        int  typeRef = j_getXdmObjectType(SaxonProcessor::sxn_environ->thread, (void *)results[i]);
        XdmItem * xdmItem = nullptr;
        std::cerr<<"i"<<i<<" Type="<<typeRef<<std::endl;

        if(typeRef == 1) {
            xdmItem = new XdmAtomicValue(results[i]);
            value->addXdmItem(xdmItem);

        } else if(typeRef == 2) {
            xdmItem = new XdmNode(results[i]);
            value->addXdmItem(xdmItem);

        }  else if (typeRef == 4) {
            xdmItem =  new XdmMap(results[i]);
            value->addXdmItem(xdmItem);

        } else if (typeRef == 3) {
            xdmItem =  new XdmArray(results[i]);
            value->addXdmItem(xdmItem);

        } else if (typeRef == 5) {
            xdmItem =  new XdmFunctionItem(results[i]);
            value->addXdmItem(xdmItem);
        } else {
            xdmItem = new XdmItem(results[i]);
            value->addXdmItem(xdmItem);
        }

    }
		return value;
	} else if(results == nullptr){
	    exception = new SaxonApiException();
	}


    return nullptr;

}

XdmItem * XPathProcessor::evaluateSingle(const char * xpathStr) {


    if (xpathStr == nullptr) {
        exception = new SaxonApiException("Error:: XPath string cannot be empty or nullptr");
        return nullptr;
    }


    setProperty("resources", proc->getResourcesDirectory());
    int64_t  processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties);
    int64_t result = j_evaluateSingle(SaxonProcessor::sxn_environ->thread, (void *)cppXP, (char *)cwdXP.c_str(), (char *)xpathStr, (void *)(processorDataRef == -1 ? (void *) NULL: (void *)processorDataRef));

    if (result > 0) {
        int  typeRef = j_getXdmObjectType(SaxonProcessor::sxn_environ->thread, (void *)result);
        XdmItem * xdmItem = nullptr;


        if(typeRef == 1) {
            xdmItem = new XdmAtomicValue(result);
            return xdmItem;

        } else if(typeRef == 2) {
            xdmItem = new XdmNode(result);
            return xdmItem;

        }  else if (typeRef == 4) {
            xdmItem =  new XdmMap(result);
            return xdmItem;

        } else if (typeRef == 3) {
            xdmItem =  new XdmArray(result);
            return xdmItem;

        } else if (typeRef == 5) {
            xdmItem =  new XdmFunctionItem(result);
            return xdmItem;
        }
        xdmItem = new XdmItem(result);
        return xdmItem;
    }else if(result== -2){

        exception = new SaxonApiException();


    }
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
    if (prefix == nullptr) {
        exception = new SaxonApiException("Null prefix supplied to declareNamespace()");
        return;
    }
    if (uri == nullptr) {
        exception = new SaxonApiException("Null namespace URI supplied to declareNamespace()");
        return;
    }
	j_xp_declareNamespace(SaxonProcessor::sxn_environ->thread, (void *)cppXP, (char *)prefix, (char *)uri);

}


void XPathProcessor::declareVariable(const char * name){
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
    bool resultsi = (bool)j_effectiveBooleanValue(SaxonProcessor::sxn_environ->thread, (void *)cppXP, (char *)cwdXP.c_str(), (char *)xpathStr, (void *)(processorDataRef == -1 ? (void *) NULL: (void *)processorDataRef));
    return resultsi;
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



