
//

#include "XdmFunctionItem.h"

#ifdef MEM_DEBUG
#define new new(__FILE__, __LINE__)
#endif

    XdmFunctionItem::XdmFunctionItem():XdmItem(), arity(-1){}


    XdmFunctionItem::XdmFunctionItem(const XdmFunctionItem &aVal): XdmItem(aVal){
        arity = aVal.arity;
    }

   
	

    XdmFunctionItem::XdmFunctionItem(jobject obj):XdmItem(obj), arity(-1){
    }

    const char* XdmFunctionItem::getName(){
          if(fname.empty()) {
             jclass xdmUtilsClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/option/cpp/XdmUtils");
             		static jmethodID xmID = (jmethodID) SaxonProcessor::sxn_environ->env->GetStaticMethodID(xdmUtilsClass,"getFunctionName",
             					"(Lnet/sf/saxon/s9api/XdmFunctionItem;)Ljava/lang/String;");
             		if (!xmID) {
             			std::cerr << "Error: SaxonC." << "getFunctionName"
             				<< " not found\n" << std::endl;
             			return nullptr;
             		} else {
             			jstring result = (jstring)(SaxonProcessor::sxn_environ->env->CallStaticObjectMethod(xdmUtilsClass, xmID, value));
             			if(result) {
                        			const char * stri = SaxonProcessor::sxn_environ->env->GetStringUTFChars(result, nullptr);

                        		    SaxonProcessor::sxn_environ->env->DeleteLocalRef(result);
                        			fname = std::string(stri);
                        			return stri;
                        }
                        return nullptr;
             		}

          } else {
            return fname.c_str();
          }

    }

    int XdmFunctionItem::getArity(){
          if(arity == -1) {
             jclass xdmFunctionClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmFunctionItem");
             		static jmethodID bfmID = (jmethodID) SaxonProcessor::sxn_environ->env->GetMethodID(xdmFunctionClass,
             					"getArity",
             					"()I");
             		if (!bfmID) {
             			std::cerr << "Error: SaxonC." << "getArity"
             				<< " not found\n" << std::endl;
             			return 0;
             		} else {
             			jint result = (jint)(SaxonProcessor::sxn_environ->env->CallIntMethod(value, bfmID));
             			return (int)result;
             		}

          } else {
            return arity;
          }

    }

    XdmFunctionItem * XdmFunctionItem::getSystemFunction(SaxonProcessor * processor, const char * name, int arity){
        if(processor == nullptr || name == nullptr) {
            std::cerr << "Error in getSystemFunction. Please make sure processor and name are not nullptr." << std::endl;
             return nullptr;
        }
             jclass xdmUtilsClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/option/cpp/XdmUtils");
             static jmethodID xsfmID = (jmethodID) SaxonProcessor::sxn_environ->env->GetStaticMethodID(xdmUtilsClass,"getSystemFunction",
             "(Lnet/sf/saxon/s9api/Processor;Ljava/lang/String;I)Lnet/sf/saxon/s9api/XdmFunctionItem;");
             if (!xsfmID) {
                       std::cerr << "Error: SaxonC." << "getSystemFunction" << " not found\n" << std::endl;
                         			return nullptr;
                         		} else {
                         			jobject result = (jobject)(SaxonProcessor::sxn_environ->env->CallStaticObjectMethod(xdmUtilsClass, xsfmID, processor->proc, SaxonProcessor::sxn_environ->env->NewStringUTF(name), arity));
                         			if(result) {
                         			    auto functionItem = new XdmFunctionItem(result);
                         			    return functionItem;

                         			} else {
                         			    return nullptr;

                         			}

                         		}



    }

    XdmValue * XdmFunctionItem::call(XdmValue ** arguments, int argument_length) {
          if(argument_length > 0 && arguments == nullptr) {
                      std::cerr << "Error in XdmFunctionItem.call.  nullptr arguments found." << std::endl;
                      return nullptr;
          }

                       jclass xdmFunctionClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmFunctionItem");
                       static jmethodID xffmID = (jmethodID) SaxonProcessor::sxn_environ->env->GetMethodID(xdmFunctionClass,"call",
                       "(Lnet/sf/saxon/s9api/Processor;[Lnet/sf/saxon/s9api/XdmValue)Lnet/sf/saxon/s9api/XdmValue");
                       if (!xffmID) {
                                 std::cerr << "Error: SaxonC." << "getSystemFunction" << " not found\n" << std::endl;
                                 return nullptr;
                       } else {

                                 jobjectArray argumentJArray = SaxonProcessor::createJArray(arguments, argument_length);
                                 jobject result = (jobject)(SaxonProcessor::sxn_environ->env->CallStaticObjectMethod(xdmFunctionClass, xffmID, cpp_processor->proc, argumentJArray));
                                 if(argumentJArray != nullptr) {
                                    	SaxonProcessor::sxn_environ->env->DeleteLocalRef(argumentJArray);


                                 }
                                 if(result) {
                                        jclass atomicValueClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmAtomicValue");
                                        jclass nodeClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmNode");
                                        jclass functionItemClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmFunctionItem");
                                        XdmValue * value = nullptr;
                                        XdmItem * xdmItem = nullptr;


                                        if(SaxonProcessor::sxn_environ->env->IsInstanceOf(result, atomicValueClass)           == JNI_TRUE) {
                                                xdmItem =  new XdmAtomicValue(result);
                                                SaxonProcessor::sxn_environ->env->DeleteLocalRef(result);
                                                return xdmItem;

                                        } else if(SaxonProcessor::sxn_environ->env->IsInstanceOf(result, nodeClass)           == JNI_TRUE) {
                                                xdmItem =  new XdmNode(result);
                                                SaxonProcessor::sxn_environ->env->DeleteLocalRef(result);
                                                return xdmItem;
                                        } else if (SaxonProcessor::sxn_environ->env->IsInstanceOf(result, functionItemClass)           == JNI_TRUE) {
                                                xdmItem =  new XdmFunctionItem(result);
                                                SaxonProcessor::sxn_environ->env->DeleteLocalRef(result);
                                                return xdmItem;
                                        } else {
                                                value = new XdmValue(result, true);
                                                SaxonProcessor::sxn_environ->env->DeleteLocalRef(result);
                                                return value;
                                        }

                                 } else {
                                 	    return nullptr;

                                 }

                       }

    }


