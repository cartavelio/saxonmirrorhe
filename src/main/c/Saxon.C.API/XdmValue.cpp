#include "XdmValue.h"
#include "XdmItem.h"
#include "XdmAtomicValue.h"
#include "XdmNode.h"
#include "XdmFunctionItem.h"
#include "XdmMap.h"
#include "XdmArray.h"


#ifdef MEM_DEBUG
#define new new(__FILE__, __LINE__)
#endif


XdmValue::XdmValue(const XdmValue &other) {
	//SaxonProcessor *proc = other.proc; //TODO
	valueType = other.valueType;
	refCount = 0;
	xdmSize = other.xdmSize;
	jValues = other.jValues;
	toStringValue = other.toStringValue;
	values.resize(0);//TODO memory issue might occur here
	for (int i = 0; i < xdmSize; i++) {
		addXdmItem(other.values[i]);
	}
	
}

const char * XdmValue::toString() {
    int count = size();
    if(count == 0) {
        return nullptr;
    }
    if (toStringValue == nullptr) {
        allocFn fn = operator new;
        if(count == 1) {
            toStringValue = xdmItemToString(SaxonProcessor::sxn_environ->thread, (void *) fn, (void *) (values[0]->getUnderlyingValue()));
        } else {
            if (jValues == -1) {
                jValues = getUnderlyingValue();

            }

            toStringValue = xdmValueArrayToString(SaxonProcessor::sxn_environ->thread, (void *) fn, (void *) jValues);
        }

    }
    return toStringValue;



}


int XdmValue::size() {
	return xdmSize;
}

XdmValue::XdmValue(int64_t valRef) {
	XdmItem * value = new XdmItem(valRef);
	values.resize(0);//TODO memory issue might occur here. Need to delete XdmItems
	values.push_back(value);
	xdmSize++;
	jValues = -1;
	valueType = nullptr;
}

//TODO handle the construction of arrays
XdmValue::XdmValue(int64_t val, bool arr){
	xdmSize = 0;
	values.resize(0);
	jValues = -1;
	valueType = nullptr;
	/*jclass xdmValueForcppClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/option/cpp/XdmValueForCpp");
	jmethodID xvfMID = SaxonProcessor::sxn_environ->env->GetStaticMethodID(xdmValueForcppClass, "makeArrayFromXdmValue", "(Lnet/sf/saxon/s9api/XdmValue;)[Lnet/sf/saxon/s9api/XdmItem;");

	if(!xvfMID){

		std::cerr << "Error: SaxonDll." << "makeArrayFromXdmValue"
				<< " not found\n" << std::endl;
			return ;
	}
	
	jobjectArray results = (jobjectArray) SaxonProcessor::sxn_environ->env->CallStaticObjectMethod(xdmValueForcppClass, xvfMID, val);
	if(results){
	int sizex = SaxonProcessor::sxn_environ->env->GetArrayLength(results);
	if (sizex>0) {
		jclass atomicValueClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmAtomicValue");
		jclass nodeClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmNode");
		jclass functionItemClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmFunctionItem");
        jclass mapItemClass = lookForClass(SaxonProcessor::sxn_environ->env,
                                           "net/sf/saxon/s9api/XdmMap");
        jclass arrayItemClass = lookForClass(SaxonProcessor::sxn_environ->env,
                                             "net/sf/saxon/s9api/XdmArray");

		//XdmValue * value = new XdmValue();
		//value->setProcessor(proc);
		XdmItem * xdmItem = nullptr;
		for (int p=0; p < sizex; ++p) 
		{
			jobject resulti = SaxonProcessor::sxn_environ->env->GetObjectArrayElement(results, p);
			//value->addUnderlyingValue(resulti);

			if(SaxonProcessor::sxn_environ->env->IsInstanceOf(resulti, atomicValueClass)           == JNI_TRUE) {
				xdmItem = new XdmAtomicValue(SaxonProcessor::sxn_environ->env->NewGlobalRef(resulti));
				

			} else if(SaxonProcessor::sxn_environ->env->IsInstanceOf(resulti, nodeClass)           == JNI_TRUE) {
				xdmItem = new XdmNode(SaxonProcessor::sxn_environ->env->NewGlobalRef(resulti));


			} else if (SaxonProcessor::sxn_environ->env->IsInstanceOf(resulti, functionItemClass)           == JNI_TRUE) {

				xdmItem = new XdmFunctionItem(SaxonProcessor::sxn_environ->env->NewGlobalRef(resulti));

			}  else if (SaxonProcessor::sxn_environ->env->IsInstanceOf(resulti, mapItemClass)           == JNI_TRUE) {

                xdmItem = new XdmMap(SaxonProcessor::sxn_environ->env->NewGlobalRef(resulti));

            }  else if (SaxonProcessor::sxn_environ->env->IsInstanceOf(resulti, arrayItemClass)           == JNI_TRUE) {

                xdmItem = new XdmArray(SaxonProcessor::sxn_environ->env->NewGlobalRef(resulti));

            }

			addXdmItem(xdmItem);
			SaxonProcessor::sxn_environ->env->DeleteLocalRef(resulti);
		}
	}
		SaxonProcessor::sxn_environ->env->DeleteLocalRef(results);
	}	*/
}


XdmValue::~XdmValue() {

	if(values.size() > 0) {
        for (size_t i = 0; i < values.size(); i++) {
            if (values[i] != nullptr && values[i]->getRefCount()<1) {
                delete values[i];
            }
        }
        values.clear();
    }
	if (valueType != nullptr) { delete valueType; }
	jValues = -1;
	xdmSize = 0;

    if(toStringValue != nullptr) {
        toStringValue = nullptr;
    }

}

void XdmValue::addXdmItem(XdmItem* val) {
	if (val != nullptr) {
		values.push_back(val);
        val->incrementRefCount();
		xdmSize++;
        if (jValues) {
            jValues = -1;
        }
        if(toStringValue != nullptr) {
            delete toStringValue;
            toStringValue = nullptr;
        }

	}
}


void XdmValue::addUnderlyingValue(int64_t val) {
	XdmItem * valuei = new XdmItem(val);
	valuei->incrementRefCount();
	values.push_back(valuei);
	xdmSize++;
	jValues = -1; //TODO clear Graalvm reference from ObjectHandles pool

}

void XdmValue::incrementRefCount() {
		refCount++;
		//std::cerr<<"refCount-inc-xdmVal="<<refCount<<" ob ref="<<(this)<<std::endl;
	}

void XdmValue::decrementRefCount() {
		if (refCount > 0)
			refCount--;
		//std::cerr<<"refCount-dec-xdmVal="<<refCount<<" ob ref="<<(this)<<std::endl;
	}




XdmItem * XdmValue::getHead() {
	if (values.size() > 0) {
		return values[0];
	}
	else {
		return nullptr;
	}
}

int64_t XdmValue::getUnderlyingValue() {
#ifdef DEBUG
	std::cerr<<std::endl<<"XdmValue-getUnderlyingValue:"<<std::endl;
#endif
	if (jValues == -1) {
		int i;

		int count = values.size();
		if (count == 0) {
			return -1;
		}
        jValues = createProcessorDataWithCapacity(SaxonProcessor::sxn_environ->thread, count);

          #ifdef DEBUG
          	std::cerr<<std::endl<<"XdmValue-getUnderlyingValue cp0"<<std::endl;
          #endif
		for (i = 0; i < count; i++) {
		    int64_t ivalue = values[i]->getUnderlyingValue();
            addProcessorValue(SaxonProcessor::sxn_environ->thread, (void *)jValues, (void *)ivalue);
		}


	} 
	return jValues;
}

void XdmValue::releaseXdmValue() {
    for (size_t i = 0; i < values.size(); i++) {
        if (values[i] != nullptr) {
            delete values[i];
        }
    }

    jValues = -1;


}

XdmItem * XdmValue::itemAt(int n) {
	if (n >= 0 && (unsigned int)n < values.size()) {
		return values[n];
	}
	return nullptr;
}

/**
* Get the type of the object
*/
XDM_TYPE XdmValue::getType() {
	return XDM_VALUE;
}




