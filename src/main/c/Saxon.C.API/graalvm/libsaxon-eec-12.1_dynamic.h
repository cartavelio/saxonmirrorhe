#ifndef __LIBSAXON_EEC_12_1_H
#define __LIBSAXON_EEC_12_1_H

#include <graal_isolate_dynamic.h>


#if defined(__cplusplus)
extern "C" {
#endif

typedef long long int (*createDocumentBuilder_fn_t)(graal_isolatethread_t*, void *);

typedef long long int (*j_db_setBaseURI_fn_t)(graal_isolatethread_t*, void *, char*);

typedef long long int (*j_db_setLineNumbering_fn_t)(graal_isolatethread_t*, void *, int);

typedef long long int (*j_db_setDTDValidation_fn_t)(graal_isolatethread_t*, void *, int);

typedef int (*run_main_fn_t)(int argc, char** argv);

typedef long long int (*createProcessorDataWithArray_fn_t)(graal_isolatethread_t*, void *);

typedef long long int (*createProcessorData_fn_t)(graal_isolatethread_t*);

typedef long long int (*createProcessorDataWithCapacity_fn_t)(graal_isolatethread_t*, int);

typedef void (*addProcessorDataPair_fn_t)(graal_isolatethread_t*, void *, char*, void *);

typedef void (*addProcessorPropertyPair_fn_t)(graal_isolatethread_t*, void *, char*, char*);

typedef void (*addProcessorValue_fn_t)(graal_isolatethread_t*, void *, void *);

typedef void (*clearProcessorData_fn_t)(graal_isolatethread_t*, void *);

typedef long long int (*createSaxonProcessor_fn_t)(graal_isolatethread_t*, int);

typedef long long int (*j_run_transform_fn_t)(graal_isolatethread_t*, int, char*);

typedef void (*j_handles_destroy_fn_t)(graal_isolatethread_t*, void *);

typedef long long* (*j_getLongArray_fn_t)(graal_isolatethread_t*, void *, void *);

typedef long long int (*j_createStringObject_fn_t)(graal_isolatethread_t*, char*);

typedef long long int (*createSaxonProcessorWithConfigurationFile_fn_t)(graal_isolatethread_t*, char*);

typedef char* (*getProductVersion_fn_t)(graal_isolatethread_t*, void *);

typedef void (*setCatalogFile_fn_t)(graal_isolatethread_t*, void *, char*);

typedef void (*j_applyToConfiguration_fn_t)(graal_isolatethread_t*, void *, void *);

typedef void (*setDebugMode_fn_t)(graal_isolatethread_t*, int);

typedef long long int (*parseXmlFile_fn_t)(graal_isolatethread_t*, void *, char*, char*);

typedef long long int (*parseXmlFileWithValidator_fn_t)(graal_isolatethread_t*, char*, void *, void *, char*);

typedef long long int (*parseXmlString_fn_t)(graal_isolatethread_t*, void *, char*);

typedef long long int (*parseXmlStringWithValidator_fn_t)(graal_isolatethread_t*, void *, void *, char*);

typedef long long int (*createXdmAtomicItem_fn_t)(graal_isolatethread_t*, char*, char*);

typedef long long int (*j_main_parseXmlString_fn_t)(graal_isolatethread_t*, char*, void *, void *, void *, char*);

typedef char* (*getTypeName_fn_t)(graal_isolatethread_t*, char*);

typedef char* (*j_getStringValue_fn_t)(graal_isolatethread_t*, void *);

typedef long long int (*j_main_parseXmlFile_fn_t)(graal_isolatethread_t*, char*, void *, void *, void *, char*);

typedef char* (*j_getErrorMessage_fn_t)(graal_isolatethread_t*, void *);

typedef char* (*j_getCombinedStaticErrorMessages_fn_t)(graal_isolatethread_t*, void *);

typedef int (*j_getLineNumber_fn_t)(graal_isolatethread_t*);

typedef char* (*j_getErrorCode_fn_t)(graal_isolatethread_t*, void *);

typedef char* (*j_getSystemId_fn_t)(graal_isolatethread_t*, void *);

typedef void (*j_clearException_fn_t)(graal_isolatethread_t*);

typedef char (*j_checkForException_fn_t)(graal_isolatethread_t*);

typedef int (*j_staticErrorCount_fn_t)(graal_isolatethread_t*);

typedef long long int (*createSaxonCMessageListener_fn_t)(graal_isolatethread_t*, void *, char*, char*);

typedef long long* (*j_getXslMessages_fn_t)(graal_isolatethread_t*, void *, void *);

typedef long long int (*createSaxonCResultDocumentHandler_fn_t)(graal_isolatethread_t*, int);

typedef long long int (*createSchemaValidator_fn_t)(graal_isolatethread_t*);

typedef long long int (*createSchemaValidatorWithProcessor_fn_t)(graal_isolatethread_t*, void *);

typedef long long int (*j_getSchemaManager_fn_t)(graal_isolatethread_t*, void *);

typedef long long int (*j_getValidationReport_fn_t)(graal_isolatethread_t*, void *);

typedef long long int (*j_registerSchema_fn_t)(graal_isolatethread_t*, void *, char*, char*, void *);

typedef long long int (*j_registerSchemaString_fn_t)(graal_isolatethread_t*, void *, char*, char*, void *);

typedef long long int (*j_exportSchema_fn_t)(graal_isolatethread_t*, void *, char*, char*);

typedef long long int (*j_validate_fn_t)(graal_isolatethread_t*, void *, void *, char*, char*, char*, void *);

typedef long long int (*j_validateToNode_fn_t)(graal_isolatethread_t*, void *, void *, char*, char*, void *);

typedef long long int (*createXPathProcessor_fn_t)(graal_isolatethread_t*);

typedef long long int (*createXPathProcessorWithProcessor_fn_t)(graal_isolatethread_t*, void *);

typedef void (*j_xp_declareNamespace_fn_t)(graal_isolatethread_t*, void *, char*, char*);

typedef void (*j_xp_declareVariable_fn_t)(graal_isolatethread_t*, void *, char*);

typedef void (*j_xp_importSchemaNamespace_fn_t)(graal_isolatethread_t*, void *, char*);

typedef void (*j_xp_setAllowUndeclaredVariables_fn_t)(graal_isolatethread_t*, void *, int);

typedef void (*j_xp_setBackwardsCompatible_fn_t)(graal_isolatethread_t*, void *, int);

typedef void (*j_xp_setBaseURI_fn_t)(graal_isolatethread_t*, void *, char*);

typedef long long int (*j_evaluate_fn_t)(graal_isolatethread_t*, void *, void *, char*, char*, void *);

typedef long long int (*j_evaluateSingle_fn_t)(graal_isolatethread_t*, void *, char*, char*, void *);

typedef char (*j_effectiveBooleanValue_fn_t)(graal_isolatethread_t*, void *, char*, char*, void *);

typedef long long int (*j_createXQueryEngine_fn_t)(graal_isolatethread_t*);

typedef long long int (*j_createXQueryEngineWithProcessor_fn_t)(graal_isolatethread_t*, void *);

typedef char* (*j_executeQueryToString_fn_t)(graal_isolatethread_t*, void *, void *, char*, void *);

typedef long long int (*j_executeQueryToValue_fn_t)(graal_isolatethread_t*, void *, char*, void *);

typedef long long int (*j_executeQueryToFile_fn_t)(graal_isolatethread_t*, void *, char*, char*, void *);

typedef char* (*getBaseURIForXdmNode_fn_t)(graal_isolatethread_t*, void *, void *);

typedef long long int (*getParentForXdmNode_fn_t)(graal_isolatethread_t*, void *);

typedef char* (*getStringValueForXdmValue_fn_t)(graal_isolatethread_t*, void *, void *);

typedef char* (*getStringValueForXdmItem_fn_t)(graal_isolatethread_t*, void *, void *);

typedef char* (*xdmItemToString_fn_t)(graal_isolatethread_t*, void *, void *);

typedef char* (*xdmNodeToString_fn_t)(graal_isolatethread_t*, void *, void *);

typedef char* (*xdmMapToString_fn_t)(graal_isolatethread_t*, void *, void *);

typedef char* (*j_getPrimitiveTypeName_fn_t)(graal_isolatethread_t*, void *, void *);

typedef long long int (*j_makeXdmArray_fn_t)(graal_isolatethread_t*, void *);

typedef long long int (*j_makeXdmArrayFromShort_fn_t)(graal_isolatethread_t*, short*, int);

typedef long long int (*j_makeXdmArrayFromStrings_fn_t)(graal_isolatethread_t*, char*, int);

typedef long long int (*j_makeXdmArrayFromInt_fn_t)(graal_isolatethread_t*, int*, int);

typedef long long int (*j_makeXdmArrayFromLong_fn_t)(graal_isolatethread_t*, long long*, int);

typedef long long int (*j_makeXdmArrayFromBool_fn_t)(graal_isolatethread_t*, int*, int);

typedef int (*j_getNodeKind_fn_t)(graal_isolatethread_t*, void *);

typedef char* (*j_getLocalName_fn_t)(graal_isolatethread_t*, void *, void *);

typedef char* (*j_getNodeName_fn_t)(graal_isolatethread_t*, void *, void *);

typedef char* (*xdmValueArrayToString_fn_t)(graal_isolatethread_t*, void *, void *);

typedef char* (*j_getAttributeValue_fn_t)(graal_isolatethread_t*, void *, char*);

typedef long long* (*j_getAttributeNodes_fn_t)(graal_isolatethread_t*, void *, void *);

typedef long long* (*j_getChildren_fn_t)(graal_isolatethread_t*, void *, void *);

typedef int (*j_getXdmObjectType_fn_t)(graal_isolatethread_t*, void *);

typedef long long int (*j_getChild_fn_t)(graal_isolatethread_t*, void *, int);

typedef int (*j_getChildCount_fn_t)(graal_isolatethread_t*, void *);

typedef int (*j_getAttributeCount_fn_t)(graal_isolatethread_t*, void *);

typedef long long int (*j_makeStringValue_fn_t)(graal_isolatethread_t*, char*);

typedef long long int (*j_makeIntegerValue_fn_t)(graal_isolatethread_t*, int);

typedef long long int (*j_makeDoubleValue_fn_t)(graal_isolatethread_t*, double);

typedef long long int (*j_makeFloatValue_fn_t)(graal_isolatethread_t*, float);

typedef long long int (*j_makeLongValue_fn_t)(graal_isolatethread_t*, long long int);

typedef long long int (*j_makeBooleanValue_fn_t)(graal_isolatethread_t*, int);

typedef long long int (*j_makeQNameValue_fn_t)(graal_isolatethread_t*, char*);

typedef long long* (*makeArrayFromXdmValue2_fn_t)(graal_isolatethread_t*, void *, void *);

typedef long long int (*j_getTypedValue_fn_t)(graal_isolatethread_t*, void *);

typedef char (*j_getBooleanValue_fn_t)(graal_isolatethread_t*, void *);

typedef double (*j_getDoubleValue_fn_t)(graal_isolatethread_t*, void *);

typedef long long int (*j_getLongValue_fn_t)(graal_isolatethread_t*, void *);

typedef int (*j_getHashCode_fn_t)(graal_isolatethread_t*, void *);

typedef long long int (*createXslt30Processor_fn_t)(graal_isolatethread_t*);

typedef long long int (*createXslt30WithProcessor_fn_t)(graal_isolatethread_t*, void *);

typedef long long int (*j_compileFromFileAndSave_fn_t)(graal_isolatethread_t*, void *, char*, char*, char*);

typedef long long int (*j_save_fn_t)(graal_isolatethread_t*, char*, void *, char*);

typedef long long int (*j_compileFromStringAndSave_fn_t)(graal_isolatethread_t*, void *, char*, char*, char*);

typedef long long int (*j_compileFromXdmNodeAndSave_fn_t)(graal_isolatethread_t*, void *, void *, char*, char*);

typedef long long int (*j_compileFromAssociatedFile_fn_t)(graal_isolatethread_t*, void *, char*, char*, int, void *);

typedef long long int (*j_compileFromFile_fn_t)(graal_isolatethread_t*, void *, char*, char*, int, void *);

typedef long long int (*j_compileFromString_fn_t)(graal_isolatethread_t*, void *, char*, char*, int, void *);

typedef long long int (*j_compileFromXdmNode_fn_t)(graal_isolatethread_t*, void *, char*, void *, int, void *);

typedef long long int (*j_applyTemplatesReturningValue_fn_t)(graal_isolatethread_t*, char*, void *, void *, void *);

typedef long long int (*j_applyTemplatesReturningFile_fn_t)(graal_isolatethread_t*, char*, void *, void *, char*, void *);

typedef char* (*j_applyTemplatesReturningString_fn_t)(graal_isolatethread_t*, void *, char*, void *, void *, void *);

typedef long long int (*j_callFunctionReturningFile_fn_t)(graal_isolatethread_t*, char*, void *, char*, char*, void *, void *);

typedef long long int (*j_callFunctionReturningValue_fn_t)(graal_isolatethread_t*, char*, void *, char*, void *, void *);

typedef char* (*j_callFunctionReturningString_fn_t)(graal_isolatethread_t*, void *, char*, void *, char*, void *, void *);

typedef long long int (*j_callTemplateReturningFile_fn_t)(graal_isolatethread_t*, char*, void *, char*, char*, void *);

typedef long long int (*j_callTemplateReturningValue_fn_t)(graal_isolatethread_t*, char*, void *, char*, void *);

typedef char* (*j_callTemplateReturningString_fn_t)(graal_isolatethread_t*, void *, char*, void *, char*, void *);

typedef long long int (*j_transformToFile_fn_t)(graal_isolatethread_t*, char*, void *, void *, char*, char*, char*, void *);

typedef long long int (*j_transformToValue_fn_t)(graal_isolatethread_t*, char*, void *, void *, char*, char*, void *);

typedef char* (*j_transformToString_fn_t)(graal_isolatethread_t*, void *, char*, void *, void *, char*, char*, void *);

typedef void (*c_xsltSaveResultToFile_fn_t)(graal_isolatethread_t*, void *, char*, char*, char*, char*);

typedef char* (*c_xsltApplyStylesheet_fn_t)(graal_isolatethread_t*, void *, char*, char*, char*, void *);

#if defined(__cplusplus)
}
#endif
#endif
