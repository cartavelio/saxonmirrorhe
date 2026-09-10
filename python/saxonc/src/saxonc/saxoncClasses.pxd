# cython: language_level = 3

from libcpp cimport bool
from libcpp.string cimport string
from libcpp.map cimport map

cdef extern from "Python.h" nogil:
    ctypedef struct PyObject

cdef extern from "CythonExceptionHandler.h" namespace "CythonExceptionHandler":
    cdef PyObject* SaxonApiErrorWrapper
    cdef void create_saxonc_api_py_exception()
    cdef void handle_saxonc_api_exceptions()

cdef extern from "saxonc/UserFunctionHandler.h":
  cdef void userFunctionCall(object func)


cdef extern from "saxonc/SaxonProcessor.h":

    cdef cppclass UnprefixedElementMatchingPolicy:
      pass

    cdef cppclass SaxonProcessor:
        SaxonProcessor(bool) except +handle_saxonc_api_exceptions
        SaxonProcessor(const char * configFile) except +handle_saxonc_api_exceptions
        bool license
        char * version()
        void release()

        # set the current working directory
        void setcwd(char* cwd)

        const char* getcwd()

        void registerUserFunction(const char * ns, XdmValue * (*func)(int, XdmValue **), const char * signatureJSON, bool useNativeTypes) except +handle_saxonc_api_exceptions

        void registerUserFunctionWithProcessor(void * proc_ptr, const char * ns, void * func, const char * signatureJSON, bool useNativeTypes) except +handle_saxonc_api_exceptions

        void attachCurrentThread() except +handle_saxonc_api_exceptions

        void detachCurrentThread()  except +handle_saxonc_api_exceptions

        @staticmethod
        void deleteString(const char * data)

        XdmValue ** createXdmValueArray(int len)

        void deleteXdmValueArray(XdmValue ** arr, int len)

        void deleteXdmAtomicValueArray(XdmAtomicValue ** arr, int len)

        XdmAtomicValue ** createXdmAtomicValueArray(int len)

        bool isLicensed()

        const char * getSaxonEdition()

        #SaxonProcessor * getProcessor()

        #Set a configuration property specific to the processor in use. 
        #Properties specified here are common across all the processors.
        void setConfigurationProperty(char * name, char * value) except +handle_saxonc_api_exceptions

        #Clear configuration properties specific to the processor in use. 
        void clearConfigurationProperties()

        bool isSchemaAware()

        const char * EQNameToClarkName(const char * name)

        const char * clarkNameToEQName(const char * name)

        const char * getResourcesDirectory()

        void setResourcesDirectory(const char* dir)

        Xslt30Processor * newXslt30Processor()    except +handle_saxonc_api_exceptions

        XQueryProcessor * newXQueryProcessor()  except +handle_saxonc_api_exceptions

        XPathProcessor * newXPathProcessor()   except +handle_saxonc_api_exceptions

        SchemaValidator * newSchemaValidator() except +handle_saxonc_api_exceptions

        DocumentBuilder * newDocumentBuilder() except +handle_saxonc_api_exceptions

        XdmAtomicValue * makeStringValue(const char* str1, const char* encoding)

        XdmAtomicValue * makeIntegerValue(int i)

        XdmAtomicValue * makeDoubleValue(double d)

        XdmAtomicValue * makeFloatValue(float)

        XdmAtomicValue * makeLongValue(long l)

        XdmAtomicValue * make_boolean_value(bool b)

        XdmAtomicValue * makeBooleanValue(bool b)

        XdmAtomicValue * makeQNameValue(const char* str)

        XdmAtomicValue * makeAtomicValue(const char* type, const char* value)


        XdmArray *makeArray(const char **input, int length)

        XdmArray *makeArray(short *input, int length)


        XdmArray *makeArray(int *input, int length)

        XdmArray *makeArray(long *input, int length)

        XdmArray *makeArray(bool *input, int length)


        XdmArray *makeArray(XdmValue ** values, int length)


        XdmMap *makeMap2(map[string, XdmValue *] dataMap)

        XdmMap *makeMap3(XdmAtomicValue ** keys, XdmValue ** values, int len_)

        void setCatalog(const char * filename)  except +handle_saxonc_api_exceptions

        void setCatalogFiles(const char ** filenames, int _len)  except +handle_saxonc_api_exceptions

        const char * getStringValue(XdmItem * item)

        XdmNode * parseXmlFromString(const char* source, const char* encoding, SchemaValidator * validator) except +handle_saxonc_api_exceptions

        XdmNode * parseXmlFromString(const char* source) except +handle_saxonc_api_exceptions

        XdmNode * parseXmlFromFile(const char* source, SchemaValidator * validator)  except +handle_saxonc_api_exceptions

        XdmNode * parseXmlFromUri(const char* source, SchemaValidator * validator)   except +handle_saxonc_api_exceptions

        XdmValue * parseJsonFromString(const char* source, const char* encoding) except +handle_saxonc_api_exceptions

        XdmValue * parseJsonFromFile(const char* source)  except +handle_saxonc_api_exceptions

        bool isSchemaAwareProcessor()

        bool exceptionOccurred()

        void exceptionClear()

        const char * getErrorMessage()


    cdef cppclass DocumentBuilder:
        DocumentBuilder() except +handle_saxonc_api_exceptions

        void setLineNumbering(bool option)

        bool isLineNumbering()

        void setSchemaValidator(SchemaValidator * validator)

        SchemaValidator * getSchemaValidator()


        void setDTDValidation(bool option)  except +handle_saxonc_api_exceptions

        bool isDTDValidation()

        void setBaseUri(const char* uri)  except +handle_saxonc_api_exceptions

        const char * getBaseUri()

        XdmNode* parseXmlFromString(const char * content, const char * encoding) except +handle_saxonc_api_exceptions

        XdmNode* parseXmlFromFile(const char * filename)  except +handle_saxonc_api_exceptions

        XdmNode * parseXmlFromUri(const char* source)  except +handle_saxonc_api_exceptions




    cdef cppclass Xslt30Processor:
        Xslt30Processor() except +handle_saxonc_api_exceptions
        # set the current working directory
        void setcwd(const char* cwd)

        const char* getcwd()

        # Set the output file of where the transformation result is sent
        void setOutputFile(const char* outfile)

        # Say whether just-in-time compilation of template rules should be used.
        void setJustInTimeCompilation(bool jit)

        # Get just-in-time compilation of template rules flag.
        void getJustInTimeCompilation()

        void setXsltLanguageVersion(const char* version)

        void setFastCompilation(bool fast)

        void setTargetEdition(const char* edition)

        void setRelocatable(bool relocatable)

        void setResultAsRawValue(bool option)

        # Set the base output URI.
        void setBaseOutputURI(const char * baseURI)

        # Set the value of a stylesheet parameter
        void setParameter(const char* name, XdmValue*value)

        # Get a parameter value by name
        XdmValue* getParameter(const char* name)

        # Remove a parameter (name, value) pair from a stylesheet
        bool removeParameter(const char* name)

        XdmValue ** createXdmValueArray(int len)

        void deleteXdmValueArray(XdmValue** arr, int len)

        # Clear parameter values set
        void clearParameters(bool deleteValues=false)

        void importPackage(const char *packageFile)


        # Perform a one shot transformation. The result is returned as a string
        const char * transformFileToString(const char* sourcefile, const char* stylesheetfile)    except +handle_saxonc_api_exceptions

        # Perform a one shot transformation. The result are saved to file
        void transformFileToFile(const char* sourcefile, const char* stylesheetfile, const char* outputfile)   except +handle_saxonc_api_exceptions

        # Perform a one shot transformation. The result is returned as an XdmValue
        XdmValue * transformFileToValue(const char* sourcefile, const char* stylesheetfile)   except +handle_saxonc_api_exceptions

        # compile a stylesheet file.
        XsltExecutable * compileFromFile(const char* stylesheet)  except +handle_saxonc_api_exceptions

        # compile a stylesheet received as a string.
        XsltExecutable * compileFromString(const char* stylesheet, const char * encoding)   except +handle_saxonc_api_exceptions

        # Get the stylesheet associated via the xml-stylesheet processing instruction
        XsltExecutable * compileFromAssociatedFile(const char* sourceFile)    except +handle_saxonc_api_exceptions

        # Compile a stylesheet received as a string and save to an exported file (SEF).
        void compileFromStringAndSave(const char* stylesheet, const char* filename, const char * encoding)  except +handle_saxonc_api_exceptions

        # Compile a stylesheet received as a file and save to an exported file (SEF).
        void compileFromFileAndSave(const char* xslFilename, const char* filename)  except +handle_saxonc_api_exceptions

        # Compile a stylesheet received as an XdmNode. The compiled stylesheet is cached
        # and available for execution later.
        void compileFromXdmNodeAndSave(XdmNode * node, const char* filename)    except +handle_saxonc_api_exceptions

        # compile a stylesheet received as an XdmNode.
        XsltExecutable * compileFromXdmNode(XdmNode * node)   except +handle_saxonc_api_exceptions


        # Checks for pending exceptions without creating a local reference to the exception object
        bool exceptionOccurred()

        # Check for exception thrown.
        const char* checkException()

        # Clear any exception thrown
        void exceptionClear()

        # Get number of errors reported during execution or evaluate of stylesheet
        int exceptionCount()

        # Get the error message if there are any error
        const char * getErrorMessage()

        # Get the ith error code if there are any error
        const char * getErrorCode()




    cdef cppclass XsltExecutable:
        XsltExecutable() except +handle_saxonc_api_exceptions
        # set the current working directory
        void setcwd(const char* cwd)

        const char* getcwd()

        # Set the base output URI.
        void setBaseOutputURI(const char * baseURI)

        void setGlobalContextItem(XdmItem * value)

        # Set the source from file for the transformation.
        void setGlobalContextFromFile(const char * filename)

        void setInitialMode(const char * modeName)  except +handle_saxonc_api_exceptions

        void setCaptureResultDocuments(bool flag, bool rawResult)

        # The initial value to which templates are to be applied (equivalent to the <code>select</code> attribute of <code>xsl:pply-templates</code>)
        void setInitialMatchSelection(XdmValue * selection)     except +handle_saxonc_api_exceptions

        # The initial filename to which templates are to be applied (equivalent to the <code>select</code> attribute of <code>xsl:apply-templates</code>).
        void setInitialMatchSelectionAsFile(const char * filename)   except +handle_saxonc_api_exceptions

        # Set the output file of where the transformation result is sent
        void setOutputFile(const char* outfile)


        void setResultAsRawValue(bool option)

        map[string,XdmValue*]& getResultDocuments()  except +handle_saxonc_api_exceptions

        XsltExecutable *clone()

        # Produce a representation of the compiled stylesheet, in XML form, suitable for
        # distribution and reloading.
        void exportStylesheet(const char * filename)  except +handle_saxonc_api_exceptions

        # Set the value of a stylesheet parameter
        void setParameter(const char* name, XdmValue*value)

        # Get a parameter value by name
        XdmValue* getParameter(const char* name)

        # Remove a parameter (name, value) pair from a stylesheet
        bool removeParameter(const char* name)

        # Set a property specific to the processor in use.
        void setProperty(const char* name, const char* value)

        void setInitialTemplateParameters(map[string,XdmValue*] parameters, bool tunnel)

        XdmValue ** createXdmValueArray(int len)

        void deleteXdmValueArray(XdmValue** arr, int len)

        # Get a property value by name
        const char* getProperty(const char* name)

        # Get all parameters as a std::map
        map[string,XdmValue*]& getParameters()

        # Get all properties as a std::map
        map[string,string]& getProperties()

        # Clear parameter values set
        void clearParameters(bool deleteValues=false)

        # Clear property values set
        void clearProperties()

        # Get the messages written using the <code>xsl:message</code> instruction
        void setSaveXslMessage(bool show, const char* filename)  except +handle_saxonc_api_exceptions

        # Get the messages written using the xsl:message instruction
        XdmValue * getXslMessages()  except +handle_saxonc_api_exceptions

        # Clear xsl:messages held in the PyXsltExecutable
        XdmValue * clearXslMessages()  except +handle_saxonc_api_exceptions

        # Perform a one shot transformation. The result is stored in the supplied outputfile.
        void transformFileToFile(const char* sourcefile, const char* outputfile)  except +handle_saxonc_api_exceptions

        # Perform a one shot transformation. The result is returned as a string
        const char * transformFileToString(const char* sourcefile)   except +handle_saxonc_api_exceptions

        # Perform a one shot transformation. The result is returned as an XdmValue
        XdmValue * transformFileToValue(const char* sourcefile)   except +handle_saxonc_api_exceptions

        # Invoke the stylesheet by applying templates to a supplied input sequence, Saving the results to file.
        void applyTemplatesReturningFile(const char* outfile)    except +handle_saxonc_api_exceptions

        # Invoke the stylesheet by applying templates to a supplied input sequence, Saving the results as serialized string.
        const char* applyTemplatesReturningString()  except +handle_saxonc_api_exceptions

        # Invoke the stylesheet by applying templates to a supplied input sequence, Saving the results as an XdmValue.
        XdmValue * applyTemplatesReturningValue()  except +handle_saxonc_api_exceptions

        # Invoke a transformation by calling a named template and save result to file.
        void callTemplateReturningFile(const char* templateName, const char* outfile)  except +handle_saxonc_api_exceptions

        # Invoke a transformation by calling a named template and return result as a string.
        const char* callTemplateReturningString(const char* templateName)   except +handle_saxonc_api_exceptions

        # Invoke a transformation by calling a named template and return result as an XdmValue.
        XdmValue* callTemplateReturningValue(const char* templateName)  except +handle_saxonc_api_exceptions

        # Call a public user-defined function in the stylesheet
        # Here we wrap the result in an XML document, and sending this document to a specified file
        void callFunctionReturningFile(const char* functionName, XdmValue ** arguments, int argument_length, const char* outfile)  except +handle_saxonc_api_exceptions

        # Call a public user-defined function in the stylesheet
        # Here we wrap the result in an XML document, and serialized this document to string value
        const char * callFunctionReturningString(const char* functionName, XdmValue ** arguments, int argument_length)   except +handle_saxonc_api_exceptions

        # Call a public user-defined function in the stylesheet
        # Here we wrap the result in an XML document, and return the document as an XdmVale
        XdmValue * callFunctionReturningValue(const char* functionName, XdmValue ** arguments, int argument_length)   except +handle_saxonc_api_exceptions

        # Execute transformation to string. Properties supplied in advance.
        const char * transformToString(XdmNode * source)   except +handle_saxonc_api_exceptions

        # Execute transformation to Xdm Value. Properties supplied in advance.
        XdmValue * transformToValue(XdmNode * source)    except +handle_saxonc_api_exceptions

        # Execute transformation to file. Properties supplied in advance.
        void transformToFile(XdmNode * source)   except +handle_saxonc_api_exceptions

        # Checks for pending exceptions without creating a local reference to the exception object
        bool exceptionOccurred()

        # Check for exception thrown.
        SaxonApiException* getException()

        # Get the error message if there are any error
        const char * getErrorMessage()

        # Clear any exception thrown
        void exceptionClear()

        # Get number of errors reported during execution or evaluate of stylesheet
        int exceptionCount()


    cdef cppclass SaxonApiException:
        SaxonApiException() except +handle_saxonc_api_exceptions


    cdef cppclass SchemaValidator:
        SchemaValidator() except +handle_saxonc_api_exceptions

        void setcwd(const char* cwd)

        const char* getcwd()

        void registerSchemaFromNode(XdmNode * node)    except +handle_saxonc_api_exceptions

        void registerSchemaFromFile(const char * xsd)   except +handle_saxonc_api_exceptions

        void registerSchemaFromString(const char * schemaStr)  except +handle_saxonc_api_exceptions

        void exportSchema(const char * fileName)  except +handle_saxonc_api_exceptions

        void setOutputFile(const char * outputFile)

        const char* getOutputFile();

        void validate(const char * sourceFile) except +handle_saxonc_api_exceptions
   
        XdmNode * validateToNode(const char * sourceFile) except +handle_saxonc_api_exceptions

        void setSourceNode(XdmNode * source)

        XdmNode* getValidationReport() except +handle_saxonc_api_exceptions

        void setParameter(const char * name, XdmValue*value)

        bool removeParameter(const char * name)

        void setProperty(const char * name, const char * value)

        void clearParameters()

        void clearProperties()

        bool exceptionOccurred()

        const char* checkException()

        void exceptionClear()

        int exceptionCount()

    
        const char * getErrorMessage()
     
        const char * getErrorCode()

        void setLax(bool l)

    cdef cppclass XPathProcessor:

        XPathProcessor() except +handle_saxonc_api_exceptions

        void setBaseURI(const char * uriStr)

        const char * getBaseURI()

        XdmValue * evaluate(const char * xpathStr, const char* encoding)  except +handle_saxonc_api_exceptions
   
        XdmItem * evaluateSingle(const char * xpathStr, const char* encoding)  except +handle_saxonc_api_exceptions

        void setUnprefixedElementMatchingPolicy(UnprefixedElementMatchingPolicy policy);

        UnprefixedElementMatchingPolicy getUnprefixedElementMatchingPolicy();

        UnprefixedElementMatchingPolicy convertEnumPolicy(int n);

        void setContextItem(XdmItem * item)
        
        void setcwd(const char* cwd)

        const char* getcwd()

        void setContextFile(const char * filename)

        void setLanguageVersion(const char * version)

        bool effectiveBooleanValue(const char * xpathStr, const char* encoding) except +handle_saxonc_api_exceptions

        void setParameter(const char * name, XdmValue*value)

        bool removeParameter(const char * name)

        void setProperty(const char * name, const char * value)

        const char * getProperty(const char * name)

        void declareNamespace(const char *prefix, const char * uri)  except +handle_saxonc_api_exceptions

        void declareVariable(const char * name)

        void setBackwardsCompatible(bool option)

        void setCaching(bool caching)

        void importSchemaNamespace(const char * uri)

        void clearParameters()

        void clearProperties()

        bool exceptionOccurred()

        void exceptionClear()

        int exceptionCount()

        const char * getErrorMessage()

        const char * getErrorCode()

    cdef cppclass XQueryProcessor:
        XQueryProcessor() except +handle_saxonc_api_exceptions

        void setContextItem(XdmItem * value) except +handle_saxonc_api_exceptions

        void setOutputFile(const char* outfile)

        void setContextItemFromFile(const char * filename)

        void setLanguageVersion(const char * version)

        void setParameter(const char * name, XdmValue*value)

        bool removeParameter(const char * name)

        void setProperty(const char * name, const char * value)

        void clearParameters()

        void clearProperties()

        void setUpdating(bool updating)

        void setStreaming(bool option)

        void isStreaming()

        XdmValue * runQueryToValue() except +handle_saxonc_api_exceptions
        const char * runQueryToString() except +handle_saxonc_api_exceptions

        void executeQueryToFile(const char *infilename,const char *ofilename,const char *query, const char* encoding) except +handle_saxonc_api_exceptions

        XdmValue *executeQueryToValue(const char *infilename, const char *query, const char* encoding) except +handle_saxonc_api_exceptions

        const char *executeQueryToString(const char *infilename, const char *query, const char* encoding) except +handle_saxonc_api_exceptions

        void runQueryToFile() except +handle_saxonc_api_exceptions

        void declareNamespace(const char *prefix, const char * uri) except +handle_saxonc_api_exceptions

        void setQueryFile(const char* filename)

        void setQueryContent(const char* content)

        void setQueryBaseURI(const char * baseURI)

        void setcwd(const char* cwd)

        const char* getcwd()

        const char* checkException()

        bool exceptionOccurred()

        void exceptionClear()

        int exceptionCount()

        const char * getErrorMessage()

        const char * getErrorCode()


cdef extern from "saxonc/XdmValue.h":
    cdef cppclass XdmValue:
        XdmValue() except +handle_saxonc_api_exceptions

        void addXdmItem(XdmItem *val)
        #void releaseXdmValue()

        XdmItem * getHead()

        XdmItem * itemAt(int)

        int size()

        long getUnderlyingValue()

        const char * toString()  except +handle_saxonc_api_exceptions

        void incrementRefCount()

        void decrementRefCount()

        void resetRelinquishedItems()

        void incrementRefCountForRelinquishedValue(int i)

        int getRefCount()

        int getType()

cdef extern from "saxonc/XdmItem.h":
    cdef cppclass XdmItem(XdmValue):
        XdmItem() except +handle_saxonc_api_exceptions
        const char * getStringValue()
        bool isAtomic()

        bool isFunction()

        bool isNode()

        bool isMap()

        bool isArray()


cdef extern from "saxonc/XdmNode.h":

    cdef cppclass EnumXdmAxis:
      pass

    cdef cppclass XdmNode(XdmItem):
        bool isAtomic()

        int getNodeKind()

        const char * getNodeName()

        const char * getLocalName()

        bool equals(XdmNode * other)  except +handle_saxonc_api_exceptions

        XdmValue * getTypedValue()

        int getLineNumber()

        int getColumnNumber()

        void resetRelinquishedChildren()

        void incrementRefCountForRelinquishedChildren()

        bool hasRelinquishedChildren()

        const char* getBaseUri()

        XdmNode* getParent()

        XdmNode *getChild(int i, bool cache=False);

        const char* getAttributeValue(const char *str)

        int getAttributeCount()

        XdmNode** getAttributeNodes()

        int axisNodeCount()

        XdmNode **axisNodes(EnumXdmAxis axis)

        EnumXdmAxis convertEnumXdmAxis(int n)

        XdmNode** getChildren()

        int getChildCount()

        @staticmethod
        void deleteChildrenArray(XdmNode** fetchedChildren)



cdef extern from "saxonc/XdmAtomicValue.h":
    cdef cppclass XdmAtomicValue(XdmItem):
        XdmAtomicValue() except +handle_saxonc_api_exceptions

        const char * getPrimitiveTypeName()

        bool getBooleanValue()

        double getDoubleValue()

        long getLongValue()

        int getHashCode()


        

cdef extern from "saxonc/XdmFunctionItem.h":
    cdef cppclass XdmFunctionItem(XdmItem):
        const char* getName()

        int getArity()

        @staticmethod
        XdmFunctionItem * getSystemFunction(SaxonProcessor * processor, const char * name, int arity)

        XdmValue * call(SaxonProcessor * processor, XdmValue ** arguments, int argument_length) except +handle_saxonc_api_exceptions

        XdmValue ** createXdmValueArray(int len)




cdef extern from "saxonc/XdmMap.h":
    cdef cppclass XdmMap(XdmFunctionItem):
        int mapSize()

        XdmValue * get(XdmAtomicValue* key)

        XdmValue * get(const char * key)

        XdmValue * get(int key)

        XdmValue * get(double key)

        XdmValue *  get(long key)

        XdmMap * put(XdmAtomicValue * key, XdmValue * value)

        XdmMap * remove(XdmAtomicValue* key)

        list[XdmAtomicValue*] keySet()

        XdmAtomicValue** keys()

        map[string, XdmValue*]& asMap()

        bool isEmpty()

        bool containsKey(XdmAtomicValue* key)

        XdmValue ** createXdmValueArray(int len)

        list[XdmValue*] valuesAsList()

        XdmValue** values()



cdef extern from "saxonc/XdmArray.h":
    cdef cppclass XdmArray(XdmFunctionItem):

        int arrayLength()

        XdmValue* get(int n)

        XdmArray* put(int n, XdmValue * value)

        XdmArray* addMember(XdmValue* value)

        XdmArray* concat(XdmArray * value)

        XdmValue ** createXdmValueArray(int len)

        list[XdmValue *] asList()

        XdmValue ** values()

        int getArity()
