// XsltExecutable.cpp : Defines the exported functions for the DLL application.
//

#include "XsltExecutable.h"
#include "XdmValue.h"
#include "XdmItem.h"
#include "XdmNode.h"
#include "XdmFunctionItem.h"
#include "XdmMap.h"
#include "XdmArray.h"
#include "XdmAtomicValue.h"

#ifdef DEBUG
#include <typeinfo> //used for testing only
#endif

#ifdef MEM_DEBUG
#define new new(__FILE__, __LINE__)
#endif

XsltExecutable::XsltExecutable(int64_t procRefi, int64_t exRef, std::string curr, std::string resources) {




#ifdef DEBUG


#endif
    tunnel = false;
    rawResultsFlag = false;
    selectionRef = -1;
    saxonMessageListenerRef = -1;
    saxonResultDocRef = -1;
    executableRef = exRef;
    procRef = procRefi;
    cwdXE = curr;
    resources_dir = resources;
    exceptionFound = false;
}

XsltExecutable::~XsltExecutable() {
    clearProperties();
    clearParameters();

    if(saxonMessageListenerRef != -1){
        saxonMessageListenerRef = -1;
    }
    if(saxonResultDocRef != -1) {
        saxonResultDocRef = -1;
    }

}


XsltExecutable::XsltExecutable(const XsltExecutable &other) {

    executableRef = other.executableRef;
    procRef = other.procRef;
    cwdXE = other.cwdXE;
    resources_dir = other.resources_dir;
    selectionRef = other.selectionRef;
    tunnel = other.tunnel;
    rawResultsFlag = other.rawResultsFlag;
    exceptionFound = other.exceptionFound;

    std::map<std::string, XdmValue *>::const_iterator paramIter = other.parameters.begin();
    while (paramIter != other.parameters.end()) {

        XdmValue *valuei = paramIter->second;
        if (valuei == nullptr) {
            std::cerr << "Error in XsltExecutable copy constructor - parameter value is NULL" << std::endl;
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


bool XsltExecutable::exceptionOccurred() {
    return exceptionFound; //return SaxonProcessor::sxn_environ->env->ExceptionCheck();
}


void XsltExecutable::setGlobalContextItem(XdmItem *value) {
    if (value != nullptr) {
        value->incrementRefCount();
        parameters["node"] = value;
    }
}

void XsltExecutable::setGlobalContextFromFile(const char *ifile) {
    if (ifile != nullptr) {
        setProperty("s", ifile);
    }
}

void XsltExecutable::setInitialMatchSelection(XdmValue *_selection) {
    
    if(_selection == nullptr) {
        selectionRef = -1;
    } else {
        selectionRef = _selection->getUnderlyingValue();
    }

}


void XsltExecutable::setInitialMatchSelectionAsFile(const char *filename) {
    
    //TODO remove selectRef from objectHandles pool in Java
    selectionRef = -1;

    if (filename != nullptr) {
        selectionRef = j_createStringObject(SaxonProcessor::sxn_environ->thread, (char *)filename);
        if(selectionRef == -2) {
            exceptionFound = true;
        }
    }   else {
       selectionRef = -1;
    }
}

void XsltExecutable::setOutputFile(const char *ofile) {
    setProperty("o", ofile);
}

void XsltExecutable::setBaseOutputURI(const char *baseURI) {
    if (baseURI != nullptr) {
        setProperty("baseoutput", baseURI);
    }
}

void XsltExecutable::setInitialMode(const char * modeName){
    if(modeName != nullptr) {
        setProperty("im", modeName);
    }

}


void XsltExecutable::setParameter(const char *name, XdmValue *value) {
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

void XsltExecutable::setInitialTemplateParameters(std::map<std::string, XdmValue *> _itparameters, bool _tunnel) {
    for (std::map<std::string, XdmValue *>::iterator itr = _itparameters.begin(); itr != _itparameters.end(); itr++) {
        parameters["itparam:" + std::string(itr->first)] = itr->second;
        itr->second->incrementRefCount();
    }
    tunnel = _tunnel;
    if (tunnel) {
        setProperty("tunnel", "true");
    }
}

void XsltExecutable::setCaptureResultDocuments(bool flag, bool rawResults) {
    
    if(flag) {
        resultDocumentMap.clear();
        if(saxonResultDocRef != -1) {
            saxonResultDocRef = -1;
        }


        rawResultsFlag = rawResults || rawResultsFlag;


        saxonResultDocRef = createSaxonCResultDocumentHandler(SaxonProcessor::sxn_environ->thread, (int)rawResultsFlag);

        if(saxonResultDocRef == -1) {
            std::cerr << "Error: Failed to create SaxonCResultDocumentHandler" <<  std::endl;
        }
    } else {
        setProperty("rd", "off");
        
        if(saxonResultDocRef != -1) {
            //TODO delete SaxonResultDoc from ObjectHandles pool
            saxonResultDocRef = -1;
        }


          while (!resultDocumentMap.empty())
          {
            delete resultDocumentMap.begin()->second;
            resultDocumentMap.erase(resultDocumentMap.begin());
          }
    }


}



std::map<std::string,XdmValue*>& XsltExecutable::getResultDocuments(){
        
        /*if(saxonResultDocRef) {
            static jmethodID getresultID = (jmethodID) SaxonProcessor::sxn_environ->env->GetMethodID(resultDocumentClass,
                                                                                                      "getResultDocuments",
                                                                                                      "()[Lnet/sf/saxon/s9api/XdmValue;");

            static jmethodID getresult_uriID = (jmethodID) SaxonProcessor::sxn_environ->env->GetMethodID(resultDocumentClass,
                                                                                                      "getResultDocumentURIs",
                                                                                                      "()[Ljava/lang/String;");
            if (!getresultID) {
                std::cerr << "Error: " << getDllname() << ".getResultDocuments" << " not found\n"
                          << std::endl;
                SaxonProcessor::sxn_environ->env->ExceptionClear();
            } else {
                jobjectArray results = (jobjectArray) (
                        SaxonProcessor::sxn_environ->env->CallObjectMethod(saxonResultDocRef, getresultID));
                jobjectArray resultUris = (jobjectArray) (
                        SaxonProcessor::sxn_environ->env->CallObjectMethod(saxonResultDocRef, getresult_uriID));
                if(!results || !resultUris ) {
                    return resultDocumentMap;
                }

                int sizex = SaxonProcessor::sxn_environ->env->GetArrayLength(results);
                jclass atomicValueClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmAtomicValue");
                jclass nodeClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmNode");
                jclass functionItemClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmFunctionItem");
                jclass mapItemClass = lookForClass(SaxonProcessor::sxn_environ->env,
                                                   "net/sf/saxon/s9api/XdmMap");
                jclass arrayItemClass = lookForClass(SaxonProcessor::sxn_environ->env,
                                                     "net/sf/saxon/s9api/XdmArray");

                if (sizex > 0) {
                    for (int p = 0; p < sizex; ++p) {
                        jobject resulti = SaxonProcessor::sxn_environ->env->GetObjectArrayElement(results, p);
                        jobject resultURIsi = SaxonProcessor::sxn_environ->env->GetObjectArrayElement(resultUris, p);
                        jstring sresultURI = (jstring)resultURIsi;
                        const char * str = SaxonProcessor::sxn_environ->env->GetStringUTFChars(sresultURI,
                                        NULL);


                        XdmValue * value= nullptr;
                        XdmItem * xdmItem = nullptr;
                        if(rawResultsFlag) {
                            value = new XdmValue();

                            if(SaxonProcessor::sxn_environ->env->IsInstanceOf(resulti, atomicValueClass)           == JNI_TRUE) {
                                xdmItem = new XdmAtomicValue(resulti);
                                value->addXdmItem(xdmItem);


                            } else if(SaxonProcessor::sxn_environ->env->IsInstanceOf(resulti, nodeClass)           == JNI_TRUE) {
                                xdmItem = new XdmNode(resulti);
                                value->addXdmItem(xdmItem);


                            } else if (SaxonProcessor::sxn_environ->env->IsInstanceOf(resulti, mapItemClass) == JNI_TRUE) {
                                xdmItem = new XdmMap(resulti);
                                value->addXdmItem(xdmItem);

                            } else if (SaxonProcessor::sxn_environ->env->IsInstanceOf(resulti, arrayItemClass) == JNI_TRUE) {
                                xdmItem = new XdmArray(resulti);
                                value->addXdmItem(xdmItem);

                            } else if (SaxonProcessor::sxn_environ->env->IsInstanceOf(resulti, functionItemClass)           == JNI_TRUE) {
                                xdmItem = new XdmFunctionItem(resulti);
                                value->addXdmItem(xdmItem);

                            } else {
                                value = new XdmValue(resulti, true);

                            }

                        } else {
                            value = new XdmValue();
                            value->addXdmItem(new XdmNode(resulti));
                            SaxonProcessor::sxn_environ->env->DeleteLocalRef(resulti);

                        }
                        resultDocumentMap[str] = value;
                        SaxonProcessor::sxn_environ->env->DeleteLocalRef(resulti);
                        SaxonProcessor::sxn_environ->env->DeleteLocalRef(resultURIsi);

                    }
                    SaxonProcessor::sxn_environ->env->DeleteLocalRef(results);

                }
            }
        } */
       return resultDocumentMap;
}


XdmValue *XsltExecutable::getParameter(const char *name) {
    std::map<std::string, XdmValue *>::iterator it;
    it = parameters.find("param:" + std::string(name));
    if (it != parameters.end())
        return it->second;
    else {
        it = parameters.find("sparam:" + std::string(name));
        if (it != parameters.end())
            return it->second;
    }
    return nullptr;
}

bool XsltExecutable::removeParameter(const char *name) {
    return (bool) (parameters.erase("param:" + std::string(name)));
}


void XsltExecutable::setResultAsRawValue(bool option) {
    if (option) {
        setProperty("outvalue", "yes");
        rawResultsFlag = true;
    } else {
        removeProperty("outvalue");
        rawResultsFlag = false;
    }
}

XsltExecutable * XsltExecutable::clone() {
    XsltExecutable * executable = new XsltExecutable(*this);
    return executable;

}

void XsltExecutable::setProperty(const char *name, const char *value) {
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



bool XsltExecutable::removeProperty(const char *name) {
    return (bool) (properties.erase(std::string(name)));
}

const char *XsltExecutable::getProperty(const char *name) {
    std::map<std::string, std::string>::iterator it;
    it = properties.find(std::string(name));
    if (it != properties.end())
        return it->second.c_str();
    return nullptr;
}

void XsltExecutable::clearParameters(bool delValues) {
    if (delValues) {
        for (std::map<std::string, XdmValue *>::iterator itr = parameters.begin(); itr != parameters.end(); itr++) {

            XdmValue *value = itr->second;
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

void XsltExecutable::clearProperties() {
    
    properties.clear();
    if(selectionRef != -1) {
            selectionRef = -1;
    }

}


std::map<std::string, XdmValue *> &XsltExecutable::getParameters() {
    std::map<std::string, XdmValue *> &ptr = parameters;
    return ptr;
}

std::map<std::string, std::string> &XsltExecutable::getProperties() {
    std::map<std::string, std::string> &ptr = properties;
    return ptr;
}

void XsltExecutable::exceptionClear() {
    j_clearException(SaxonProcessor::sxn_environ->thread);
}

void XsltExecutable::setcwd(const char *dir) {
    if (dir != nullptr) {
        cwdXE = std::string(dir);
    }
}


void XsltExecutable::exportStylesheet(const char *filename) {
    long status = j_save(SaxonProcessor::sxn_environ->thread, (char *)cwdXE.c_str(), (void *)executableRef, (char *)filename);
    if(status == -2) {
        exceptionFound = true;
    }

}

void XsltExecutable::applyTemplatesReturningFile(const char *output_filename) {
    if (selectionRef == -1) {
        std::cerr
                << "Error: The initial match selection has not been set. Please set it using setInitialMatchSelection or setInitialMatchSelectionFile."
                << std::endl;
        return;
    }



    	setProperty("resources", resources_dir.c_str());

        int additions = 0;
        int iadd = 0;
        /*if(saxonMessageListenerRef != -1 ) {
            additions++;
        }
        if(saxonResultDocRef != -1) {
            additions++;
        }*/
        int64_t processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties, additions);

    std::string optionStr;
    //TODO change map to set values as int64_t then we can add saxonMessageListenerRef directly
    if(saxonMessageListenerRef != -1) {
        optionStr = "m";
        addProcessorDataPair(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef, (char *)optionStr.c_str(), (void *)saxonMessageListenerRef);

    }
    if(saxonResultDocRef != -1) {
        optionStr = "rd";
        addProcessorDataPair(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef, (char *)optionStr.c_str(), (void *)saxonResultDocRef);
    }

    long status = j_applyTemplatesReturningFile(SaxonProcessor::sxn_environ->thread, (char *)cwdXE.c_str(), (void *)executableRef, (void *)(selectionRef == -1 ? (void *) nullptr : (void *)selectionRef), (char *)output_filename, (void *)(processorDataRef == -1 ? (void *) NULL: (void *)processorDataRef));

    if(status == -2) {
        exceptionFound = true;
    }

    return;

}

const char *XsltExecutable::applyTemplatesReturningString() {
    

    if (selectionRef == -1) {
        std::cerr
                << "Error: The initial match selection has not been set. Please set it using setInitialMatchSelection or setInitialMatchSelectionFile."
                << std::endl;
        return nullptr;
    }


    setProperty("resources", resources_dir.c_str());

    int additions = 0;
    int iadd = 0;
    /*if(saxonMessageListenerRef != -1 ) {
        additions++;
    }
    if(saxonResultDocRef != -1) {
        additions++;
    }*/
    int64_t processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties, additions);

    std::string optionStr;
    //TODO change map to set values as int64_t then we can add saxonMessageListenerRef directly
    if(saxonMessageListenerRef != -1) {
        optionStr = "m";
        addProcessorDataPair(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef, (char *)optionStr.c_str(), (void *)saxonMessageListenerRef);

    }
    if(saxonResultDocRef != -1) {
        optionStr = "rd";
        addProcessorDataPair(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef, (char *)optionStr.c_str(), (void *)saxonResultDocRef);
    }
    allocFn fn = operator new;
    char * result = j_applyTemplatesReturningString(SaxonProcessor::sxn_environ->thread, (void *)fn, (char *)cwdXE.c_str(), (void *)executableRef, (selectionRef == -1 ? (void *)NULL : (void *)selectionRef), (processorDataRef == -1 ? (void *) NULL: (void *)processorDataRef));
    if(result == nullptr) {
        exceptionFound = true;
    }
    return result;

}

XdmValue *XsltExecutable::applyTemplatesReturningValue() {
    
    setProperty("resources", resources_dir.c_str());

    int additions = 0;
    int iadd = 0;
    /*if(saxonMessageListenerRef != -1 ) {
        additions++;
    }
    if(saxonResultDocRef != -1) {
        additions++;
    }*/
    int64_t processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties, additions);

    //TODO change map to set values as int64_t then we can add saxonMessageListenerRef directly
    std::string optionStr;
    if(saxonMessageListenerRef != -1) {
        optionStr = "m";
        addProcessorDataPair(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef, (char *)optionStr.c_str(), (void *)saxonMessageListenerRef);

    }
    if(saxonResultDocRef != -1) {
        optionStr = "rd";
        addProcessorDataPair(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef, (char *)optionStr.c_str(), (void *)saxonResultDocRef);
    }
    std::cerr<<"selectionRef ="<<(long)selectionRef<<std::endl;
    int64_t result = j_applyTemplatesReturningValue(SaxonProcessor::sxn_environ->thread, (char *)cwdXE.c_str(), (void *)executableRef, (void *)(selectionRef == -1 ? (void *)NULL : (void *)selectionRef), (void *)(processorDataRef == -1 ? (void *) NULL: (void *)processorDataRef));
    if (result > 0) {

        int  typeRef = j_getXdmObjectType(SaxonProcessor::sxn_environ->thread, (void *)result);
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
    }else if(result==-2){
        exceptionFound = true;



    }
    return nullptr;

}


void XsltExecutable::callFunctionReturningFile(const char *functionName, XdmValue **arguments, int argument_length,
                                               const char *outfile) {

    


    	setProperty("resources", resources_dir.c_str());

        int additions = 0;
        int iadd = 0;
        /*if(saxonMessageListenerRef != -1 ) {
            additions++;
        }
        if(saxonResultDocRef != -1) {
            additions++;
        }*/
        int64_t processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties, additions);

        std::string optionStr;
        //TODO change map to set values as int64_t then we can add saxonMessageListenerRef directly
        if(saxonMessageListenerRef != -1) {
            optionStr = "m";
            addProcessorDataPair(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef, (char *)optionStr.c_str(), (void *)saxonMessageListenerRef);

        }
        if(saxonResultDocRef != -1) {
            optionStr = "rd";
            addProcessorDataPair(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef, (char *)optionStr.c_str(), (void *)saxonResultDocRef);
        }

        int64_t argumentValuesRef = createProcessorDataWithCapacity(SaxonProcessor::sxn_environ->thread, argument_length);

        for(int i= 0; i<argument_length; i++) {
            addProcessorValue(SaxonProcessor::sxn_environ->thread, (void *)argumentValuesRef,  (void *)arguments[i]->getUnderlyingValue());
        }

        j_callFunctionReturningFile(SaxonProcessor::sxn_environ->thread, (char *)cwdXE.c_str(), (void *)executableRef, (char *)functionName, (char *)outfile, (argumentValuesRef == -1? (void *) NULL :(void *)argumentValuesRef), (processorDataRef == -1 ? (void *) NULL: (void *)processorDataRef));

        return;

}

const char *
XsltExecutable::callFunctionReturningString(const char *functionName, XdmValue **arguments, int argument_length) {

    

    setProperty("resources", resources_dir.c_str());

    int additions = 0;
    int iadd = 0;
    /*if(saxonMessageListenerRef != -1 ) {
        additions++;
    }
    if(saxonResultDocRef != -1) {
        additions++;
    }*/
    int64_t processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties, additions);

    std::string optionStr;
    //TODO change map to set values as int64_t then we can add saxonMessageListenerRef directly
    if(saxonMessageListenerRef != -1) {
        optionStr = "m";
        addProcessorDataPair(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef, (char *)optionStr.c_str(), (void *)saxonMessageListenerRef);

    }
    if(saxonResultDocRef != -1) {
        optionStr = "rd";
        addProcessorDataPair(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef, (char *)optionStr.c_str(), (void *)saxonResultDocRef);
    }

    int64_t argumentValuesRef = createProcessorDataWithCapacity(SaxonProcessor::sxn_environ->thread, argument_length);

    for(int i= 0; i<argument_length; i++) {
        addProcessorValue(SaxonProcessor::sxn_environ->thread, (void *)argumentValuesRef,  (void *)arguments[i]->getUnderlyingValue());
    }
    allocFn fn = operator new;
    char * result = j_callFunctionReturningString(SaxonProcessor::sxn_environ->thread, (void *)fn, (char *)cwdXE.c_str(), (void *)executableRef, (char *)functionName,  (void *)(argumentValuesRef == -1 ? (void *)nullptr : (void *)argumentValuesRef), (void *)(processorDataRef == -1 ? (void *) nullptr: (void *)processorDataRef));

    return result;


}


XdmValue *
XsltExecutable::callFunctionReturningValue(const char *functionName, XdmValue **arguments, int argument_length) {

    
    setProperty("resources", resources_dir.c_str());

    int additions = 0;
    int iadd = 0;
    /*if(saxonMessageListenerRef != -1 ) {
        additions++;
    }
    if(saxonResultDocRef != -1) {
        additions++;
    }*/
    int64_t processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties, additions);

    std::string optionStr;
    //TODO change map to set values as int64_t then we can add saxonMessageListenerRef directly
    if(saxonMessageListenerRef != -1) {
        optionStr = "m";
        addProcessorDataPair(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef, (char *)optionStr.c_str(), (void *)saxonMessageListenerRef);

    }
    if(saxonResultDocRef != -1) {
        optionStr = "rd";
        addProcessorDataPair(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef, (char *)optionStr.c_str(), (void *)saxonResultDocRef);
    }

    int64_t argumentValuesRef = createProcessorDataWithCapacity(SaxonProcessor::sxn_environ->thread, argument_length);

    for(int i= 0; i<argument_length; i++) {
        addProcessorValue(SaxonProcessor::sxn_environ->thread, (void *)argumentValuesRef,  (void *)arguments[i]->getUnderlyingValue());
    }

    int64_t  result = j_callFunctionReturningValue(SaxonProcessor::sxn_environ->thread, (char *)cwdXE.c_str(), (void *)executableRef, (char *)functionName, (void *)argumentValuesRef,  (void *)(processorDataRef == -1 ? (void *) NULL: (void *)processorDataRef));
    if (result > 0) {
        int  typeRef = j_getXdmObjectType(SaxonProcessor::sxn_environ->thread, (void *)result);
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
        xdmItem = new XdmItem(result);
        value->addXdmItem(xdmItem);
        return value;
    }else if(result== -2){

        //TODO create exception


    }
    return nullptr;
}


void XsltExecutable::callTemplateReturningFile(const char *templateName, const char *outfile) {

    
    setProperty("resources", resources_dir.c_str());

    int additions = 0;
    int iadd = 0;
    if(saxonMessageListenerRef != -1 ) {
        additions++;
    }
    if(saxonResultDocRef != -1) {
        additions++;
    }
    int64_t processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties, additions);

    //TODO change map to set values as int64_t then we can add saxonMessageListenerRef directly
    std::string optionStr;
    if(saxonMessageListenerRef != -1) {
        optionStr = "m";
        addProcessorDataPair(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef, (char *)optionStr.c_str(), (void *)saxonMessageListenerRef);

    }
    if(saxonResultDocRef != -1) {
        optionStr = "rd";
        addProcessorDataPair(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef, (char *)optionStr.c_str(), (void *)saxonResultDocRef);
    }

    j_callTemplateReturningFile(SaxonProcessor::sxn_environ->thread, (char *)cwdXE.c_str(), (void *)executableRef, (char *)templateName, (char *)outfile, (void *)(processorDataRef == -1 ? (void *) NULL: (void *)processorDataRef));

}


const char * XsltExecutable::callTemplateReturningString(const char *templateName) {
    
    setProperty("resources", resources_dir.c_str());

    int additions = 0;
    int iadd = 0;
    /*if(saxonMessageListenerRef != -1 ) {
        additions++;
    }
    if(saxonResultDocRef != -1) {
        additions++;
    } */
    std::string optionStr;
    int64_t processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties, additions);

    //TODO change map to set values as int64_t then we can add saxonMessageListenerRef directly
    if(saxonMessageListenerRef != -1) {
        optionStr = "m";
        addProcessorDataPair(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef, (char *)optionStr.c_str(), (void *)saxonMessageListenerRef);

    }
    if(saxonResultDocRef != -1) {
        optionStr = "rd";
        addProcessorDataPair(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef, (char *)optionStr.c_str(), (void *)saxonResultDocRef);
    }
    allocFn fn = operator new;
    char * result = j_callTemplateReturningString(SaxonProcessor::sxn_environ->thread, (void*)fn, (char *)cwdXE.c_str(), (void *)executableRef, (char *)templateName, (void *)(processorDataRef == -1 ? (void *) NULL: (void *)processorDataRef));

    return result;

}

XdmValue *XsltExecutable::callTemplateReturningValue(const char *templateName) {
    
    setProperty("resources", resources_dir.c_str());

    int additions = 0;
    int iadd = 0;
    /*if(saxonMessageListenerRef != -1 ) {
        additions++;
    }
    if(saxonResultDocRef != -1) {
        additions++;
    }*/
    int64_t processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties, additions);

    std::string optionStr;
    //TODO change map to set values as int64_t then we can add saxonMessageListenerRef directly
    if(saxonMessageListenerRef != -1) {
        optionStr = "m";
        addProcessorDataPair(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef, (char *)optionStr.c_str(), (void *)saxonMessageListenerRef);

    }
    if(saxonResultDocRef != -1) {
        optionStr = "rd";
        addProcessorDataPair(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef, (char *)optionStr.c_str(), (void *)saxonResultDocRef);
    }

    int64_t result = j_callTemplateReturningValue(SaxonProcessor::sxn_environ->thread, (char *)cwdXE.c_str(), (void *)executableRef, (char *)templateName, (void *)(processorDataRef == -1 ? (void *) NULL: (void *)processorDataRef));
    if (result > 0) {
        int  typeRef = j_getXdmObjectType(SaxonProcessor::sxn_environ->thread,(void *)result);

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
        xdmItem = new XdmItem(result);
        value->addXdmItem(xdmItem);
        return value;
    }else if(result == -2) {

        //createException();


    }

    return nullptr;
}


XdmValue *XsltExecutable::transformFileToValue(const char *sourcefile) {
    
    if (sourcefile == nullptr) {

        return nullptr;
    }


    setProperty("resources", resources_dir.c_str());

    int additions = 0;
    int iadd = 0;
    /*if(saxonMessageListenerRef != -1 ) {
        additions++;
    }
    if(saxonResultDocRef != -1) {
        additions++;
    }*/
    int64_t processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties, additions);

    std::string optionStr;
    //TODO change map to set values as int64_t then we can add saxonMessageListenerRef directly
    if(saxonMessageListenerRef != -1) {
        optionStr = "m";
        addProcessorDataPair(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef, (char *)optionStr.c_str(), (void *)saxonMessageListenerRef);

    }
    if(saxonResultDocRef != -1) {
        optionStr = "rd";
        addProcessorDataPair(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef, (char *)optionStr.c_str(), (void *)saxonResultDocRef);
    }
    char * cstylesheet = nullptr;
    int64_t result = j_transformToValue(SaxonProcessor::sxn_environ->thread, (char *)cwdXE.c_str(), (void *) nullptr, (void *)executableRef, (char *)sourcefile, cstylesheet,  (void *)(processorDataRef == -1 ? (void *) NULL: (void *)processorDataRef));
    if (result > 0) {
        int  typeRef = j_getXdmObjectType(SaxonProcessor::sxn_environ->thread,(void *)result);

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
        xdmItem = new XdmItem(result);
        value->addXdmItem(xdmItem);
        return value;
    }else if(result == -2){

        //createException();


    }



    return nullptr;

}


void XsltExecutable::transformFileToFile(const char *source, const char *outputfile) {

    
    setProperty("resources", resources_dir.c_str());

    int additions = 0;
    int iadd = 0;
    /*if(saxonMessageListenerRef != -1 ) {
        additions++;
    }
    if(saxonResultDocRef != -1) {
        additions++;
    }*/
    int64_t processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties, additions);

    std::string optionStr;
    //TODO change map to set values as int64_t then we can add saxonMessageListenerRef directly
    if(saxonMessageListenerRef != -1) {
        optionStr = "m";
        addProcessorDataPair(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef, (char *)optionStr.c_str(), (void *)saxonMessageListenerRef);

    }
    if(saxonResultDocRef != -1) {
        optionStr = "rd";
        addProcessorDataPair(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef, (char *)optionStr.c_str(), (void *)saxonResultDocRef);
    }
    char * cstylesheet = nullptr;
    long status = j_transformToFile(SaxonProcessor::sxn_environ->thread, (char *)cwdXE.c_str(), (void *) nullptr, (void *)executableRef,  (char *)source, cstylesheet, (char *)outputfile, (void *)(processorDataRef == -1 ? (void *) NULL: (void *)processorDataRef));
    if(status == -2) {
        exceptionFound = true;
    }
}


void XsltExecutable::setSaveXslMessage(bool create, const char * filename) {
    
    if (create) {

        if (filename == nullptr) {
            std::string optionStr = "-:on";
            //saxonMessageListenerRef = createSaxonCMessageListener(SaxonProcessor::sxn_environ->thread, (void *)procRef, (char *)cwdXE.c_str(), (char *)optionStr.c_str());

            //setProperty("m", "on");
        } else {
            //saxonMessageListenerRef = createSaxonCMessageListener(SaxonProcessor::sxn_environ->thread, (void *)procRef, (char *)cwdXE.c_str(), (char *)filename);

        }

    } else {
        //TODO remove saxonMessageListener from the objectHandle's pool
        saxonMessageListenerRef = -1;
        setProperty("m", "off");

    }


}


XdmValue * XsltExecutable::getXslMessages(){

    if(saxonMessageListenerRef > 0) {
       /* static jmethodID getmessageID = (jmethodID) SaxonProcessor::sxn_environ->env->GetMethodID(messageListenerClass,
                                                                                                  "getXslMessages",
                                                                                                  "()[Lnet/sf/saxon/s9api/XdmNode;");
        if (!getmessageID) {
            std::cerr << "Error: " << getDllname() << ".getXslMessages" << " not found\n"
                      << std::endl;
            SaxonProcessor::sxn_environ->env->ExceptionClear();
        } else {
            jobjectArray results = (jobjectArray) (
                    SaxonProcessor::sxn_environ->env->CallObjectMethod(saxonMessageListenerRef, getmessageID));
            int sizex = SaxonProcessor::sxn_environ->env->GetArrayLength(results);

            if (sizex > 0) {
                XdmValue *value = new XdmValue();

                for (int p = 0; p < sizex; ++p) {
                    jobject resulti = SaxonProcessor::sxn_environ->env->GetObjectArrayElement(results, p);
                    value->addUnderlyingValue(resulti);
                }
                SaxonProcessor::sxn_environ->env->DeleteLocalRef(results);
                return value;
            }
        }*/
    }
    return nullptr;


}


const char *XsltExecutable::transformFileToString(const char *source) {

    

    setProperty("resources", resources_dir.c_str());

    int additions = 0;

    /*if(saxonMessageListenerRef != -1 ) {
        additions++;
    }
    if(saxonResultDocRef != -1) {
        additions++;
    }*/
    int64_t processorDataRef = SaxonProcessor::createParameterJArray(parameters, properties, additions);

    //TODO change map to set values as int64_t then we can add saxonMessageListenerRef directly
    std::string optionStr;
    if(saxonMessageListenerRef != -1) {
        optionStr = "m";
        addProcessorDataPair(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef, (char *)optionStr.c_str(), (void *)saxonMessageListenerRef);

    }
    if(saxonResultDocRef != -1) {
        optionStr = "rd";
        addProcessorDataPair(SaxonProcessor::sxn_environ->thread, (char *)processorDataRef, (char *)optionStr.c_str(), (void *)saxonResultDocRef);
    }
    char * cstylesheet = nullptr;
    allocFn fn = operator new;

    char *  result = j_transformToString(SaxonProcessor::sxn_environ->thread, (void *)fn,(char *)cwdXE.c_str(), (void *)nullptr, (void *)executableRef, (char *)source, cstylesheet, (void *)(processorDataRef == -1 ? (void *) NULL: (void *)processorDataRef));
    if(result == nullptr) {
        exceptionFound = true;
    }
    return result;
}


const char *XsltExecutable::transformToString(XdmNode *source) {

    if (source != nullptr) {
        //source->incrementRefCount();
        parameters["node"] = source;
    }
    return transformFileToString(nullptr);
}


XdmValue *XsltExecutable::transformToValue(XdmNode *source) {

    if (source != nullptr) {
        //source->incrementRefCount();
        parameters["node"] = source;
    }
    return transformFileToValue(nullptr);
}

void XsltExecutable::transformToFile(XdmNode *source) {

    if (source != nullptr) {
        //source->incrementRefCount();
        parameters["node"] = source;
    }
    transformFileToFile(nullptr, nullptr);
}


SaxonApiException *XsltExecutable::getException() {
    
    SaxonApiException * exception = new SaxonApiException();
    return exception;
}


const char * XsltExecutable::getErrorMessage() {
    SaxonApiException * exception = getException();
    if(exception != nullptr) {
        std::string str = std::string(exception->getMessage());
        if(str.length() > 0) {
            char * cstr = new char [str.length()+1];
            strcpy (cstr, str.c_str());
            delete exception;
            return cstr;
        }
    }

    return nullptr;
}
