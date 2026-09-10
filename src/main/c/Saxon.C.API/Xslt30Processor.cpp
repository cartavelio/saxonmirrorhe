// Xslt30Processor.cpp : Defines the exported functions for the DLL application.
//

#include "Xslt30Processor.h"
#include "XdmValue.h"
#include "XdmItem.h"
#include "XdmNode.h"
#include "XdmAtomicValue.h"
#include "XdmFunctionItem.h"
#include "XdmMap.h"
#include "XdmArray.h"
//#include "XsltExecutable.h"
#ifdef DEBUG
#include <typeinfo> //used for testing only
#endif

#ifdef MEM_DEBUG
#define new new(__FILE__, __LINE__)
#endif

Xslt30Processor::Xslt30Processor() {

	SaxonProcessor *p = new SaxonProcessor(false);
	Xslt30Processor(p, "");

}

Xslt30Processor::Xslt30Processor(SaxonProcessor * p, std::string curr) {

	proc = p;
    jitCompilation = false;
    exception = nullptr;
	/*
	 * Look for class.
	 */
	cppClass = lookForClass(SaxonProcessor::sxn_environ->env,
			"net/sf/saxon/option/cpp/Xslt30Processor");

	jobject tempcppXT = createSaxonProcessor2(SaxonProcessor::sxn_environ->env, cppClass,
			"(Lnet/sf/saxon/s9api/Processor;)V", proc->proc);
	if(tempcppXT) {
        cppXT = SaxonProcessor::sxn_environ->env->NewGlobalRef(tempcppXT);
        SaxonProcessor::sxn_environ->env->DeleteLocalRef(tempcppXT);

    } else {
	    createException("Error: Failed to create the Xslt30Processor internal object");

	}

#ifdef DEBUG
	jmethodID debugMID = SaxonProcessor::sxn_environ->env->GetStaticMethodID(cppClass, "setDebugMode", "(Z)V");
	SaxonProcessor::sxn_environ->env->CallStaticVoidMethod(cppClass, debugMID, (jboolean)true);
    
#endif

	if(cppXT == nullptr) {
        createException();
	}

	if(!(proc->cwd.empty()) && curr.empty()){
		cwdXT = proc->cwd;
	} else if(!curr.empty()){
		cwdXT = curr;
	}
}

    Xslt30Processor::~Xslt30Processor(){
	    clearParameters();
	    clearProperties();
	    SaxonProcessor::sxn_environ->env->DeleteGlobalRef(cppXT);
	    cwdXT.erase();
	    exceptionClear();
    }


Xslt30Processor::Xslt30Processor(const Xslt30Processor &other) {
	proc = other.proc;

	cppXT = other.cppXT;
    cwdXT = other.cwdXT;
	std::map<std::string, XdmValue*>::const_iterator paramIter = other.parameters.begin();
    while(paramIter != other.parameters.end())
    {

       XdmValue * valuei = paramIter->second;
       if(valuei == nullptr) {
    	 	//std::cerr<<"Error in Xslt30Processor copy constructor"<<std::endl;
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

	jitCompilation = other.jitCompilation;

}

void Xslt30Processor::setProperty(const char *name, const char *value) {
    if (name != nullptr) {
        properties.erase(name);
        std::string skey = std::string(name);
        properties[skey] = std::string((value == nullptr ? "" : value));
    }
}

void Xslt30Processor::clearProperties() {
    properties.clear();
}




bool Xslt30Processor::exceptionOccurred() {
	return proc->exceptionOccurred() || exception != nullptr;
}

const char * Xslt30Processor::getErrorCode() {
 if(exception == nullptr) {return nullptr;}
 return exception->getErrorCode();
 }

void Xslt30Processor::setBaseOutputURI(const char *baseURI) {
     if (baseURI != nullptr) {
         setProperty("baseoutput", baseURI);
     }
}


void Xslt30Processor::setParameter(const char* name, XdmValue * value) {
    if(value != nullptr && name != nullptr) {
        std::string skey = ("sparam:" + std::string(name));
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

XdmValue* Xslt30Processor::getParameter(const char* name) {
        std::map<std::string, XdmValue*>::iterator it;
        it = parameters.find("sparam:"+std::string(name));
        if (it != parameters.end())
          return it->second;
	    return nullptr;
}

bool Xslt30Processor::removeParameter(const char* name) {
	return (bool)(parameters.erase("param:"+std::string(name)));
}

void Xslt30Processor::setJustInTimeCompilation(bool jit){
    jitCompilation = jit;
}


void Xslt30Processor::clearParameters(bool delValues) {
	if(delValues){
        for(std::map<std::string, XdmValue*>::iterator itr = parameters.begin(); itr != parameters.end(); itr++){
            XdmValue * value = itr->second;
            if(value != nullptr) {
#ifdef DEBUG
                std::cout<<"clearParameter() - XdmValue refCount="<<value->getRefCount()<<std::endl;
#endif
                    delete value;
                    value = nullptr;

            }
        }

	}
	parameters.clear();
	
}



std::map<std::string,XdmValue*>& Xslt30Processor::getParameters(){
	std::map<std::string,XdmValue*>& ptr = parameters;
	return ptr;
}


void Xslt30Processor::exceptionClear(){
 if(exception != nullptr) {
 	delete exception;
 	exception = nullptr;
	SaxonProcessor::sxn_environ->env->ExceptionClear();
 }
  
 }

   void Xslt30Processor::setcwd(const char* dir){
    if (dir!= nullptr) {
        cwdXT = std::string(dir);
    }
   }

SaxonApiException* Xslt30Processor::getException() {
    return exception;
}

void Xslt30Processor::createException(const char * message) {
    if(exception != nullptr) {
        delete exception;
        exception = nullptr;
    }
    if(message == nullptr) {
        exception = proc->checkAndCreateException(cppClass);
    } else {
        exception = new SaxonApiException(message);
    }

}



void Xslt30Processor::compileFromXdmNodeAndSave(XdmNode * node, const char* filename) {
    j_compileFromXdmNodeAndSave(SaxonProcessor::sxn_environ->thread, cppXT, cwdXT, node->getUnderlyingValue(), filename);

}

    void Xslt30Processor::compileFromStringAndSave(const char* stylesheetStr, const char* filename){
        j_compileFromStringAndSave(SaxonProcessor::sxn_environ->thread, cppXT, cwdXT, stylesheetStr, filename);
}



    void Xslt30Processor::compileFromFileAndSave(const char* xslFilename, const char* filename){
        j_compileFromFileAndSave(SaxonProcessor::sxn_environ->thread, cppXT, cwdXT, xslFilename, filename);
}

XsltExecutable * Xslt30Processor::compileFromString(const char* stylesheetStr) {
	    int64_t  objRef = j_compileFromString(SaxonProcessor::sxn_environ->thread, cppXT, cwdXT, stylesheetStr);
        XsltExecutable * executable = new XsltExecutable(objRef, cwdXT, proc->getResourcesDirectory());
		return executable;
	}

}

XsltExecutable * Xslt30Processor::compileFromXdmNode(XdmNode * node) {
    int64_t  objRef = j_compileFromXdmNode(SaxonProcessor::sxn_environ->thread, cppXT, cwdXT, node->getUnderlyingValue());
    XsltExecutable * executable = new XsltExecutable(objRef, cwdXT, proc->getResourcesDirectory());
    return executable;
	}

}

XsltExecutable * Xslt30Processor::compileFromAssociatedFile(const char* source) {
    int64_t  objRef = j_compileFromAssociatedFile(SaxonProcessor::sxn_environ->thread, cppXT, cwdXT, source);
    XsltExecutable * executable = new XsltExecutable(objRef, cwdXT, proc->getResourcesDirectory());
    return executable;
}


XsltExecutable * Xslt30Processor::compileFromFile(const char* stylesheet) {
    int64_t  objRef = j_compileFromFile(SaxonProcessor::sxn_environ->thread, cppXT, cwdXT, stylesheet);
    XsltExecutable * executable = new XsltExecutable(objRef, cwdXT, proc->getResourcesDirectory());
    return executable;
}


XdmValue * Xslt30Processor::transformFileToValue(const char* sourcefile,
		const char* stylesheetfile) {



	if(sourcefile == nullptr && stylesheetfile == nullptr){
	
		return nullptr;
	}
    int64_t processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties);
    int64_t  result = j_transformToValue(SaxonProcessor::sxn_environ->thread,cwdXT, cppXT, -1, sourcefile, stylesheetfile, processorDataRef);


    if (result > -1) {
                int  typeRef = j_getXdmObjectType(SaxonProcessor::sxn_environ->thread,result);

			    XdmValue * value = new XdmValue();
          		XdmItem * xdmItem = nullptr;


          			if(typeRef == 1) {
          				xdmItem = new XdmAtomicValue(result);
                        value->addXdmItem(xdmItem);
                        return value;

          			} else if(typeRef == 2) {
          				xdmItem = new XdmNode(result);
                        value->addXdmItem(xdmItem);
                        return value;

          			}  else if (typeRef == 4) {
                        xdmItem =  new XdmMap(result);
                        value->addXdmItem(xdmItem);
                        return value;

                    } else if (typeRef == 3) {
                        xdmItem =  new XdmArray(result);
                        value->addXdmItem(xdmItem);
                        return value;

                    } else if (typeRef == 5) {
                        xdmItem =  new XdmFunctionItem(result);
                        value->addXdmItem(xdmItem);
                        return value;
                    }
				    value = new XdmValue(result, false);
          		    return value;
    }else {

			createException();


	}
	return nullptr;

}


void Xslt30Processor::transformFileToFile(const char* source,
		const char* stylesheet, const char* outputfile) {

	if(exceptionOccurred()) {
		//Possible error detected in the compile phase. Processor not in a clean state.
		//Require clearing exception.
        std::cerr<<"Error: Xslt30Processor not in a clean state. - Exception found"<<std::endl;
		return;	
	}
	if(stylesheet==nullptr){
		std::cerr<< "Error: stylesheet has not been set."<<std::endl;
		return;
	}
    int64_t processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties);
    j_transformToFile(SaxonProcessor::sxn_environ->thread,cwdXT, cppXT, -1, source, stylesheet, outputfile, processorDataRef);
}


const char * Xslt30Processor::transformFileToString(const char* source,
		const char* stylesheet) {
	if(exceptionOccurred()) {
		//Possible error detected in the compile phase. Processor not in a clean state.
		//Require clearing exception.
        std::cerr<<"Error: Xslt30Processor not in a clean state. - Exception found"<<std::endl;
		return nullptr;
	}
	if(source == nullptr && stylesheet == nullptr){
		std::cerr<< "Error: nullptr file name found in transformFiletoString."<<std::endl;
		return nullptr;
	}

    int64_t processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties);
    char * result = j_transformToString(SaxonProcessor::sxn_environ->thread,cwdXT, cppXT, -1, source, stylesheet, processorDataRef);

    return result;
}



const char * Xslt30Processor::getErrorMessage(){
 	if(exception == nullptr) {return nullptr;}
 	return exception->getMessage();
 }

