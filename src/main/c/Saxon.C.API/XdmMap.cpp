#include "XdmMap.h"
#include "XdmAtomicValue.h"
#include <map>

#ifdef MEM_DEBUG
#define new new(__FILE__, __LINE__)
#endif


XdmMap::XdmMap(): XdmFunctionItem() {
    map_size = 1;
}

XdmMap::XdmMap(const XdmMap &d) {
      arity = d.arity;
      data = d.data; //TODO check if copy works
      map_size = d.map_size;
    }

XdmMap::XdmMap(int64_t obj): XdmFunctionItem(obj) {
     map_size = -1;

}

const char * XdmMap::toString(){
  allocFn fn = operator new;
  const char * str = xdmMapToString(SaxonProcessor::sxn_environ->thread, (void *)fn, (void *)value);

  return str;
}



    int XdmMap::mapSize(){
      if(map_size == -1) {
        return j_xdmMap_size(SaxonProcessor::sxn_environ->thread, (void *)value);
      }
      return map_size;
    }

    XdmValue * XdmMap::get(XdmAtomicValue* key) {
        int64_t result = j_xdmMap_get(SaxonProcessor::sxn_environ->thread, (void *)value, (void *)key->getUnderlyingValue());

        return getXdmValueSubClass(result);
    }

    XdmValue * XdmMap::get(const char * key) {
        if(key == nullptr) {
            return nullptr;
        }
       int64_t result = j_xdmMap_get_with_key_as_string(SaxonProcessor::sxn_environ->thread, (void *)value, (char *)key);
       return getXdmValueSubClass(result);

    }

    XdmValue * XdmMap::get(int key) {
        if(key<0) {
            return nullptr;
        }
       int64_t result = j_xdmMap_get_with_key_as_int(SaxonProcessor::sxn_environ->thread, (void *)value, key);
       return getXdmValueSubClass(result);


    }

    XdmValue *  XdmMap::get(double key) {
        if(key<0) {
            return nullptr;
        }
       int64_t result = j_xdmMap_get_with_key_as_double(SaxonProcessor::sxn_environ->thread, (void *)value, key);
       return getXdmValueSubClass(result);


    }

    XdmValue * XdmMap::get(long key) {
        if(key<0) {
            return nullptr;
        }
       int64_t result = j_xdmMap_get_with_key_as_long(SaxonProcessor::sxn_environ->thread, (void *)value, key);
       return getXdmValueSubClass(result);

    }

//TODO test this method
    XdmMap * XdmMap::put(XdmAtomicValue* key, XdmValue * valuei) {
         if(key == nullptr || valuei == nullptr) {
             return nullptr;
         }
        int64_t result = j_xdmMap_put(SaxonProcessor::sxn_environ->thread, (void *)value, (void *)key->getUnderlyingValue(), (void *)valuei->getUnderlyingValue());
        if(result <= 0) {
            return nullptr;
        }
        return new XdmMap(result);

    }

    XdmMap * XdmMap::remove(XdmAtomicValue* key){
              /*  jclass xdmMapClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmMap");
                static jmethodID removemapID = nullptr;

                if(removemapID == nullptr) {
                    removemapID = (jmethodID) SaxonProcessor::sxn_environ->env->GetMethodID(xdmMapClass, "remove",
                					"(Lnet/sf/saxon/s9api/XdmAtomicValue;)Lnet/sf/saxon/s9api/XdmMap;");

                }
                if (!removemapID) {
                			std::cerr << "Error: Saxonc.XdmMap." << "remove"
                				<< " not found\n" << std::endl;
                			return nullptr;
                } else {
                		jobject valueObj = (SaxonProcessor::sxn_environ->env->CallObjectMethod(value, removemapID, ((XdmItem *)key)->getUnderlyingValue()));
                		if(valueObj) {
                			XdmMap * newValue = new XdmMap(valueObj);
               				return newValue;
               			}
               			return nullptr;
                }*/

        return nullptr;



    }

    std::set<XdmAtomicValue*> XdmMap::keySet(){
       std::set<XdmAtomicValue *> myset;

        /*jclass xdmUtilsClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/option/cpp/XdmUtils");
        static jmethodID keySetmID = nullptr;

        if(keySetmID == nullptr) {
            keySetmID = (jmethodID) SaxonProcessor::sxn_environ->env->GetStaticMethodID(xdmUtilsClass,"getXdmMapKeys",
                        					"(Lnet/sf/saxon/s9api/XdmMap;)[Lnet/sf/saxon/s9api/XdmAtomicValue;");
            }
            if (!keySetmID) {
                std::cerr << "Error: SaxonDll." << "makeXdmMap"<< " not found\n" << std::endl;
                return myset;
            }


            jobjectArray results = (jobjectArray)(SaxonProcessor::sxn_environ->env->CallStaticObjectMethod(xdmUtilsClass, keySetmID,value));
            if(results) {
            	   int sizex = SaxonProcessor::sxn_environ->env->GetArrayLength(results);


                   	XdmAtomicValue * tempValue = nullptr;
                   		for (int p=0; p < sizex; ++p){
                   			jobject resulti = SaxonProcessor::sxn_environ->env->GetObjectArrayElement(results, p);
                   			tempValue = new XdmAtomicValue();
                   			tempValue->addUnderlyingValue(resulti);
                   			myset.insert(tempValue);
                   		}
        }*/



         return myset;

    }



        XdmAtomicValue ** XdmMap::keys(){
            XdmAtomicValue ** keysArr = nullptr;

            /*jclass xdmUtilsClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/option/cpp/XdmUtils");
            static jmethodID keySetmID = nullptr;

            if(keySetmID == nullptr) {
                keySetmID = (jmethodID) SaxonProcessor::sxn_environ->env->GetStaticMethodID(xdmUtilsClass,"getXdmMapKeys",
                            					"(Lnet/sf/saxon/s9api/XdmMap;)[Lnet/sf/saxon/s9api/XdmAtomicValue;");
                }
                if (!keySetmID) {
                    std::cerr << "Error: SaxonDll." << "makeXdmMap"<< " not found\n" << std::endl;
                    return nullptr;
                }


                jobjectArray results = (jobjectArray)(SaxonProcessor::sxn_environ->env->CallStaticObjectMethod(xdmUtilsClass, keySetmID,value));
                if(results) {
                	   int sizex = SaxonProcessor::sxn_environ->env->GetArrayLength(results);


                       	XdmAtomicValue * tempValue = nullptr;
                       	keysArr = new XdmAtomicValue*[sizex];
                       	for (int p=0; p < sizex; ++p){
                       			jobject resulti = SaxonProcessor::sxn_environ->env->GetObjectArrayElement(results, p);
                       			tempValue = new XdmAtomicValue(resulti);
                       			keysArr[p] =tempValue;
                       	}
            }*/



             return keysArr;
        }

    //std::map<XdmAtomicValue*, XdmValue*> XdmMap::asMap();

    bool XdmMap::isEmpty() {
               /* jclass xdmMapClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmMap");
                static jmethodID isEmptyMapID = nullptr;

                if(isEmptyMapID == nullptr) {
                    isEmptyMapID = (jmethodID) SaxonProcessor::sxn_environ->env->GetMethodID(xdmMapClass, "isEmpty",
                					"()Z");

                }
                if (!isEmptyMapID) {
                			std::cerr << "Error: Saxonc.XdmMap." << "isEmpty"
                				<< " not found\n" << std::endl;
                			return false;
                } else {
                		jboolean result = (SaxonProcessor::sxn_environ->env->CallBooleanMethod(value, isEmptyMapID));
                		return result;
                }*/
               return false;
    }

    bool XdmMap::containsKey(XdmAtomicValue * key) {
               /* jclass xdmMapClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmMap");
                static jmethodID containsMapID = nullptr;

                if(containsMapID == nullptr) {
                    containsMapID = (jmethodID) SaxonProcessor::sxn_environ->env->GetMethodID(xdmMapClass, "containsKey",
                					"(Lnet/sf/saxon/s9api/XdmAtomicValue;)Z");

                }
                if (!containsMapID) {
                			std::cerr << "Error: Saxonc.XdmMap." << "containsKey"
                				<< " not found\n" << std::endl;
                			return false;
                } else {
                		jboolean result = (SaxonProcessor::sxn_environ->env->CallBooleanMethod(value, containsMapID, ((XdmItem *)key)->getUnderlyingValue()));
                		return result;
                }*/

        return false;

    }

    std::list<XdmValue *> XdmMap::valuesAsList() {
         std::list<XdmValue *> mylist;

         /*   jclass xdmUtilsClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/option/cpp/XdmUtils");
            jmethodID xmID = (jmethodID) SaxonProcessor::sxn_environ->env->GetStaticMethodID(xdmUtilsClass,"getXdmMapValues",
                        					"(Lnet/sf/saxon/s9api/XdmMap;)[Lnet/sf/saxon/s9api/XdmValue;");
            if (!xmID) {
                std::cerr << "Error: SaxonDll." << "makeXdmMap"<< " not found\n" << std::endl;
                return mylist;
            }


            jobjectArray results = (jobjectArray)(SaxonProcessor::sxn_environ->env->CallStaticObjectMethod(xdmUtilsClass, xmID,value));
            if(results) {
            	   int sizex = SaxonProcessor::sxn_environ->env->GetArrayLength(results);


                   	XdmValue * tempValue = nullptr;
                   	for (int p=0; p < sizex; ++p){
                   			jobject resulti = SaxonProcessor::sxn_environ->env->GetObjectArrayElement(results, p);
                   			tempValue = new XdmValue();
                   			tempValue->addUnderlyingValue(resulti);
                   			mylist.push_back(tempValue);

                   		}
                    SaxonProcessor::sxn_environ->env->DeleteLocalRef(results);
            }

*/
         return mylist;
    }




        XdmValue ** XdmMap::values() {
        //TODO: We need a strategy of the memory management of the values returned
             XdmValue ** valuesArr = nullptr;

              /* jclass xdmUtilsClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/option/cpp/XdmUtils");
                jmethodID xmID = (jmethodID) SaxonProcessor::sxn_environ->env->GetStaticMethodID(xdmUtilsClass,"getXdmMapValues",
                            					"(Lnet/sf/saxon/s9api/XdmMap;)[Lnet/sf/saxon/s9api/XdmValue;");
                if (!xmID) {
                    std::cerr << "Error: SaxonDll." << "values()"<< " not found\n" << std::endl;
                    return valuesArr;
                }


                jobjectArray results = (jobjectArray)(SaxonProcessor::sxn_environ->env->CallStaticObjectMethod(xdmUtilsClass, xmID,value));
                if(results) {
                    jclass atomicValueClass = lookForClass(SaxonProcessor::sxn_environ->env,
                                                           "net/sf/saxon/s9api/XdmAtomicValue");
                    jclass nodeClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/s9api/XdmNode");
                    jclass functionItemClass = lookForClass(SaxonProcessor::sxn_environ->env,
                                                            "net/sf/saxon/s9api/XdmFunctionItem");
                    jclass mapItemClass = lookForClass(SaxonProcessor::sxn_environ->env,
                                                            "net/sf/saxon/s9api/XdmMap");
                    jclass arrayItemClass = lookForClass(SaxonProcessor::sxn_environ->env,
                                                       "net/sf/saxon/s9api/XdmArray");
                	   int sizex = SaxonProcessor::sxn_environ->env->GetArrayLength(results);
                        valuesArr = new XdmValue*[sizex];

                       	XdmValue * tempValue = nullptr;
                       	XdmItem *xdmItem = nullptr;
                       	for (int p=0; p < sizex; ++p){
                       			jobject resulti = SaxonProcessor::sxn_environ->env->GetObjectArrayElement(results, p);
                       		tempValue = new XdmValue();

                            if (SaxonProcessor::sxn_environ->env->IsInstanceOf(resulti, atomicValueClass) == JNI_TRUE) {
                            xdmItem = new XdmAtomicValue(resulti);
                            SaxonProcessor::sxn_environ->env->DeleteLocalRef(resulti);
                            tempValue->addXdmItem(xdmItem);
                            valuesArr[p] = tempValue;

                        } else if (SaxonProcessor::sxn_environ->env->IsInstanceOf(resulti, nodeClass) == JNI_TRUE) {
                            xdmItem = new XdmNode(resulti);
                            SaxonProcessor::sxn_environ->env->DeleteLocalRef(resulti);
                            tempValue->addXdmItem(xdmItem);
                            valuesArr[p] = tempValue;
                        } else if (SaxonProcessor::sxn_environ->env->IsInstanceOf(resulti, functionItemClass) == JNI_TRUE) {
                            xdmItem = new XdmFunctionItem(resulti);
                            SaxonProcessor::sxn_environ->env->DeleteLocalRef(resulti);
                            tempValue->addXdmItem(xdmItem);
                            valuesArr[p] = tempValue;

                        } else if (SaxonProcessor::sxn_environ->env->IsInstanceOf(resulti, mapItemClass) == JNI_TRUE) {
                            xdmItem = new XdmMap(resulti);
                            SaxonProcessor::sxn_environ->env->DeleteLocalRef(resulti);
                            tempValue->addXdmItem(xdmItem);
                            valuesArr[p] = tempValue;
                        } else if (SaxonProcessor::sxn_environ->env->IsInstanceOf(resulti, arrayItemClass) == JNI_TRUE) {
                            xdmItem = new XdmArray(resulti);
                            SaxonProcessor::sxn_environ->env->DeleteLocalRef(resulti);
                            tempValue->addXdmItem(xdmItem);
                            valuesArr[p] = tempValue;
                        } else {
                            tempValue = new XdmValue(resulti, true);
                            SaxonProcessor::sxn_environ->env->DeleteLocalRef(resulti);
                            valuesArr[p] = tempValue;
                        }



                    }
                }*/


             return valuesArr;
        }