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
    if(proc == nullptr) {

        return;
    }
    jitCompilation = false;
    exception = nullptr;
    cppXT = createXslt30WithProcessor(SaxonProcessor::sxn_environ->thread, (void *)proc->procRef);


#ifdef DEBUG
	//jmethodID debugMID = SaxonProcessor::sxn_environ->env->GetStaticMethodID(cppClass, "setDebugMode", "(Z)V");
	//SaxonProcessor::sxn_environ->env->CallStaticVoidMethod(cppClass, debugMID, (jboolean)true);
    
#endif

	if(cppXT < 0) {
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
	    cppXT = -1;
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
	return exception != nullptr;
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

 }
      j_clearException(SaxonProcessor::sxn_environ->thread);
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
      exception = new SaxonApiException();
    } else {
        exception = new SaxonApiException(message);
    }

}



void Xslt30Processor::compileFromXdmNodeAndSave(XdmNode * node, const char* filename) {
    long status = j_compileFromXdmNodeAndSave(SaxonProcessor::sxn_environ->thread, (void *)cppXT,  (void *)node->getUnderlyingValue(),  (char *)cwdXT.c_str(), (char *)filename);
    if(status == -2) {
        createException();
    }

}

    void Xslt30Processor::compileFromStringAndSave(const char* stylesheetStr, const char* filename){
        long status = j_compileFromStringAndSave(SaxonProcessor::sxn_environ->thread, (void *)cppXT, (char *)cwdXT.c_str(), (char *)stylesheetStr, (char *)filename);
        if(status == -2) {
            createException();
        }
}



    void Xslt30Processor::compileFromFileAndSave(const char* xslFilename, const char* filename){
        if(xslFilename == nullptr) {
            createException("XSL filename is null");
            return;
        }
        if(filename == nullptr) {
            createException("Output filename is null");
            return;
        }
        long status = j_compileFromFileAndSave(SaxonProcessor::sxn_environ->thread, (void *)cppXT, (char *)cwdXT.c_str(), (char *)xslFilename, (char *)filename);
        if(status == -2) {
            createException();
        }
}

XsltExecutable * Xslt30Processor::compileFromString(const char* stylesheetStr) {
    if(stylesheetStr == nullptr) {
        createException("XSL string is null");
        return nullptr;
    }
        int64_t processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties);

	    int64_t  objRef = j_compileFromString(SaxonProcessor::sxn_environ->thread, (void *)cppXT, (char *)cwdXT.c_str(), (char *)stylesheetStr, (int)jitCompilation, (void *)(processorDataRef == -1 ? (void *) NULL: (void *)processorDataRef));
        if(objRef == -2) {
            createException();
            return nullptr;
        }
	    XsltExecutable * executable = new XsltExecutable(proc->procRef, objRef, cwdXT, proc->getResourcesDirectory());
        if(processorDataRef > 0) {
            j_handles_destroy(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef);
            processorDataRef = -1;
        }
		return executable;


}

XsltExecutable * Xslt30Processor::compileFromXdmNode(XdmNode * node) {
    if(node == nullptr) {
        createException("XSL node is null");
        return nullptr;
    }
    int64_t processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties);
    int64_t  objRef = j_compileFromXdmNode(SaxonProcessor::sxn_environ->thread, (void *)cppXT, (char *)cwdXT.c_str(), (void *)node->getUnderlyingValue(), (int)jitCompilation, (void *)(processorDataRef == -1 ? (void *) NULL: (void *)processorDataRef));
    if(objRef == -2) {
        createException();
        return nullptr;
    }
    XsltExecutable * executable = new XsltExecutable(proc->procRef, objRef, cwdXT, proc->getResourcesDirectory());
    if(processorDataRef > 0) {
        j_handles_destroy(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef);
        processorDataRef = -1;
    }
    return executable;


}

XsltExecutable * Xslt30Processor::compileFromAssociatedFile(const char* source) {
    if(source == nullptr) {
        createException("XSL filename is null");
        return nullptr;
    }
    int64_t processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties);
    int64_t  objRef = j_compileFromAssociatedFile(SaxonProcessor::sxn_environ->thread, (void *)cppXT, (char *)cwdXT.c_str(), (char *)source, (int)jitCompilation , (void *)(processorDataRef == -1 ? (void *) NULL: (void *)processorDataRef));
    if(objRef == -2) {
        createException();
        return nullptr;
    }
    XsltExecutable * executable = new XsltExecutable(proc->procRef, objRef, cwdXT, proc->getResourcesDirectory());
    return executable;
}


XsltExecutable * Xslt30Processor::compileFromFile(const char* stylesheet) {
    if(stylesheet == nullptr) {
        createException("XSL filename is null");
        return nullptr;
    }
    int64_t processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties);
    int64_t  objRef = j_compileFromFile(SaxonProcessor::sxn_environ->thread, (void *)cppXT, (char *)cwdXT.c_str(), (char *)stylesheet, (int)jitCompilation, (void *)(processorDataRef == -1 ? (void *) NULL: (void *)processorDataRef));
    if(objRef == -2) {
        createException();
        return nullptr;
    }
    XsltExecutable * executable = new XsltExecutable(proc->procRef,objRef, cwdXT, proc->getResourcesDirectory());
    if(processorDataRef > 0) {
        j_handles_destroy(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef);
        processorDataRef = -1;
    }
    return executable;
}


XdmValue * Xslt30Processor::transformFileToValue(const char* sourcefile,const char* stylesheetfile) {



	if(sourcefile == nullptr && stylesheetfile == nullptr){
        createException("Found null file name");
		return nullptr;
	}
    int64_t processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties);

    int64_t  result = j_transformToValue(SaxonProcessor::sxn_environ->thread,(char *)cwdXT.c_str(), (void *)cppXT, (void *) nullptr, (char *)sourcefile, (char *)stylesheetfile, (void *)(processorDataRef == -1 ? (void *) NULL: (void *)processorDataRef));
    if(processorDataRef > 0) {
        j_handles_destroy(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef);
        processorDataRef = -1;
    }
    if (result > 0) {
         return SaxonProcessor::makeXdmValueFromRef(result);
    }else if(result == -2) {

			createException();


	}
	return nullptr;

}


void Xslt30Processor::transformFileToFile(const char* source,
		const char* stylesheet, const char* outputfile) {

	if(exceptionOccurred()) {
		//Possible error detected in the compile phase. Processor not in a clean state.
		//Require clearing exception.
		return;	
	}
	if(stylesheet==nullptr){
		std::cerr<< "Error: stylesheet has not been set."<<std::endl;
		return;
	}
    int64_t processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties);
    long status = j_transformToFile(SaxonProcessor::sxn_environ->thread,(char *)cwdXT.c_str(), (void *)cppXT, (void *) nullptr, (char *)source, (char *)stylesheet, (char *)outputfile, (void *)(processorDataRef == -1 ? (void *) NULL: (void *)processorDataRef));
    if(status == -2) {
        createException();
    }
    if(processorDataRef > 0) {
        j_handles_destroy(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef);
        processorDataRef = -1;
    }
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
    allocFn fn = operator new;
    int64_t processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties);
    char * result = j_transformToString(SaxonProcessor::sxn_environ->thread,(void *)fn, (char *)cwdXT.c_str(), (void *)cppXT, (void *) nullptr, (char *)source, (char *)stylesheet, (void *)(processorDataRef == -1 ? (void *) NULL: (void *)processorDataRef));
    if(result == nullptr) {
        createException();
    }
    if(processorDataRef > 0) {
        j_handles_destroy(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef);
        processorDataRef = -1;
    }
    return result;
}



const char * Xslt30Processor::getErrorMessage(){
 	if(exception == nullptr) {return nullptr;}
 	return exception->getMessage();
 }

