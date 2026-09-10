#ifndef __GRAALVM_EE_12_1_H
#define __GRAALVM_EE_12_1_H

#include <graal_isolate.h>


#if defined(__cplusplus)
extern "C" {
#endif

long long int createDocumentBuilder(graal_isolatethread_t*, void *);

long long int j_db_setBaseURI(graal_isolatethread_t*, void *, char*);

long long int j_db_setLineNumbering(graal_isolatethread_t*, void *, int);

long long int j_db_setDTDValidation(graal_isolatethread_t*, void *, int);

int run_main(int argc, char** argv);

long long int createProcessorDataWithArray(graal_isolatethread_t*, void *);

long long int createProcessorData(graal_isolatethread_t*);

long long int createProcessorDataWithCapacity(graal_isolatethread_t*, int);

void addProcessorDataPair(graal_isolatethread_t*, void *, char*, void *);

void addProcessorPropertyPair(graal_isolatethread_t*, void *, char*, char*);

void addProcessorValue(graal_isolatethread_t*, void *, void *);

void clearProcessorData(graal_isolatethread_t*, void *);

long long int createSaxonProcessor(graal_isolatethread_t*, int);

void j_handles_destroy(graal_isolatethread_t*, void *);

long long* j_getLongArray(graal_isolatethread_t*, void *, void *);

long long int j_createStringObject(graal_isolatethread_t*, char*);

long long int createSaxonProcessorWithConfigurationFile(graal_isolatethread_t*, char*);

char* getProductVersion(graal_isolatethread_t*, void *);

void setCatalogFile(graal_isolatethread_t*, void *, char*);

void j_applyToConfiguration(graal_isolatethread_t*, void *, void *);

void setDebugMode(graal_isolatethread_t*, int);

long long int parseXmlFile(graal_isolatethread_t*, void *, char*, char*);

long long int parseXmlFileWithValidator(graal_isolatethread_t*, char*, void *, void *, char*);

long long int parseXmlString(graal_isolatethread_t*, void *, char*);

long long int parseXmlStringWithValidator(graal_isolatethread_t*, void *, void *, char*);

long long int createXdmAtomicItem(graal_isolatethread_t*, char*, char*);

long long int j_main_parseXmlString(graal_isolatethread_t*, char*, void *, void *, void *, char*);

char* getTypeName(graal_isolatethread_t*, char*);

char* j_getStringValue(graal_isolatethread_t*, void *);

long long int j_main_parseXmlFile(graal_isolatethread_t*, char*, void *, void *, void *, char*);

char* j_getErrorMessage(graal_isolatethread_t*, void *);

char* j_getCombinedStaticErrorMessages(graal_isolatethread_t*, void *);

int j_getLineNumber(graal_isolatethread_t*);

char* j_getErrorCode(graal_isolatethread_t*, void *);

char* j_getSystemId(graal_isolatethread_t*, void *);

void j_clearException(graal_isolatethread_t*);

char j_checkForException(graal_isolatethread_t*);

int j_staticErrorCount(graal_isolatethread_t*);

long long int createSaxonCMessageListener(graal_isolatethread_t*, void *, char*, char*);

long long* j_getXslMessages(graal_isolatethread_t*, void *, void *);

long long int createSaxonCResultDocumentHandler(graal_isolatethread_t*, int);

long long int createSchemaValidator(graal_isolatethread_t*);

long long int createSchemaValidatorWithProcessor(graal_isolatethread_t*, void *);

long long int j_getSchemaManager(graal_isolatethread_t*, void *);

long long int j_getValidationReport(graal_isolatethread_t*, void *);

long long int registerSchema(graal_isolatethread_t*, void *, char*, char*, void *);

long long int registerSchemaString(graal_isolatethread_t*, void *, char*, char*, void *);

long long int exportSchema(graal_isolatethread_t*, void *, char*, char*);

long long int j_validate(graal_isolatethread_t*, void *, void *, char*, char*, char*, void *);

long long int j_validateToNode(graal_isolatethread_t*, void *, void *, char*, char*, void *);

long long int createXPathProcessor(graal_isolatethread_t*);

long long int createXPathProcessorWithProcessor(graal_isolatethread_t*, void *);

void j_xp_declareNamespace(graal_isolatethread_t*, void *, char*, char*);

void j_xp_declareVariable(graal_isolatethread_t*, void *, char*);

void j_xp_importSchemaNamespace(graal_isolatethread_t*, void *, char*);

void j_xp_setAllowUndeclaredVariables(graal_isolatethread_t*, void *, int);

void j_xp_setBackwardsCompatible(graal_isolatethread_t*, void *, int);

void j_xp_setBaseURI(graal_isolatethread_t*, void *, char*);

long long* j_evaluate(graal_isolatethread_t*, void *, void *, char*, char*, void *);

long long int j_evaluateSingle(graal_isolatethread_t*, void *, char*, char*, void *);

char j_effectiveBooleanValue(graal_isolatethread_t*, void *, char*, char*, void *);

long long int j_createXQueryEngine(graal_isolatethread_t*);

long long int j_createXQueryEngineWithProcessor(graal_isolatethread_t*, void *);

char* j_executeQueryToString(graal_isolatethread_t*, void *, char*, void *);

long long int j_executeQueryToValue(graal_isolatethread_t*, void *, char*, void *);

long long int j_executeQueryToFile(graal_isolatethread_t*, void *, char*, char*, void *);

char* getBaseURIForXdmNode(graal_isolatethread_t*, void *, void *);

long long int getParentForXdmNode(graal_isolatethread_t*, void *);

char* getStringValueForXdmValue(graal_isolatethread_t*, void *, void *);

char* getStringValueForXdmItem(graal_isolatethread_t*, void *, void *);

char* xdmItemToString(graal_isolatethread_t*, void *, void *);

char* xdmNodeToString(graal_isolatethread_t*, void *, void *);

char* xdmMapToString(graal_isolatethread_t*, void *, void *);

char* j_getPrimitiveTypeName(graal_isolatethread_t*, void *, void *);

long long int j_makeXdmArray(graal_isolatethread_t*, void *);

long long int j_makeXdmArrayFromShort(graal_isolatethread_t*, short*, int);

long long int j_makeXdmArrayFromStrings(graal_isolatethread_t*, char*, int);

long long int j_makeXdmArrayFromInt(graal_isolatethread_t*, int*, int);

long long int j_makeXdmArrayFromLong(graal_isolatethread_t*, long long*, int);

long long int j_makeXdmArrayFromBool(graal_isolatethread_t*, int*, int);

int j_getNodeKind(graal_isolatethread_t*, void *);

char* j_getLocalName(graal_isolatethread_t*, void *, void *);

char* j_getNodeName(graal_isolatethread_t*, void *, void *);

char* xdmValueArrayToString(graal_isolatethread_t*, void *, void *);

char* j_getAttributeValue(graal_isolatethread_t*, void *, char*);

long long* j_getAttributeNodes(graal_isolatethread_t*, void *, void *);

long long* j_getChildren(graal_isolatethread_t*, void *, void *);

int j_getXdmObjectType(graal_isolatethread_t*, void *);

long long int j_getChild(graal_isolatethread_t*, void *, int);

int j_getChildCount(graal_isolatethread_t*, void *);

int j_getAttributeCount(graal_isolatethread_t*, void *);

long long int j_makeStringValue(graal_isolatethread_t*, char*);

long long int j_makeIntegerValue(graal_isolatethread_t*, int);

long long int j_makeDoubleValue(graal_isolatethread_t*, double);

long long int j_makeFloatValue(graal_isolatethread_t*, float);

long long int j_makeLongValue(graal_isolatethread_t*, long long int);

long long int j_makeBooleanValue(graal_isolatethread_t*, int);

long long int j_makeQNameValue(graal_isolatethread_t*, char*);

long long int j_getTypedValue(graal_isolatethread_t*, void *);

char j_getBooleanValue(graal_isolatethread_t*, void *);

double j_getDoubleValue(graal_isolatethread_t*, void *);

long long int j_getLongValue(graal_isolatethread_t*, void *);

int j_getHashCode(graal_isolatethread_t*, void *);

long long int createXslt30Processor(graal_isolatethread_t*);

long long int createXslt30WithProcessor(graal_isolatethread_t*, void *);

long long int j_compileFromFileAndSave(graal_isolatethread_t*, void *, char*, char*, char*);

long long int j_save(graal_isolatethread_t*, char*, void *, char*);

long long int j_compileFromStringAndSave(graal_isolatethread_t*, void *, char*, char*, char*);

long long int j_compileFromXdmNodeAndSave(graal_isolatethread_t*, void *, void *, char*, char*);

long long int j_compileFromAssociatedFile(graal_isolatethread_t*, void *, char*, char*, int, void *);

long long int j_compileFromFile(graal_isolatethread_t*, void *, char*, char*, int, void *);

long long int j_compileFromString(graal_isolatethread_t*, void *, char*, char*, int, void *);

long long int j_compileFromXdmNode(graal_isolatethread_t*, void *, char*, void *, int, void *);

long long int j_applyTemplatesReturningValue(graal_isolatethread_t*, char*, void *, void *, void *);

long long int j_applyTemplatesReturningFile(graal_isolatethread_t*, char*, void *, void *, char*, void *);

char* j_applyTemplatesReturningString(graal_isolatethread_t*, void *, char*, void *, void *, void *);

long long int j_callFunctionReturningFile(graal_isolatethread_t*, char*, void *, char*, char*, void *, void *);

long long int j_callFunctionReturningValue(graal_isolatethread_t*, char*, void *, char*, void *, void *);

char* j_callFunctionReturningString(graal_isolatethread_t*, void *, char*, void *, char*, void *, void *);

long long int j_callTemplateReturningFile(graal_isolatethread_t*, char*, void *, char*, char*, void *);

long long int j_callTemplateReturningValue(graal_isolatethread_t*, char*, void *, char*, void *);

char* j_callTemplateReturningString(graal_isolatethread_t*, void *, char*, void *, char*, void *);

long long int j_transformToFile(graal_isolatethread_t*, char*, void *, void *, char*, char*, char*, void *);

long long int j_transformToValue(graal_isolatethread_t*, char*, void *, void *, char*, char*, void *);

char* j_transformToString(graal_isolatethread_t*, void *, char*, void *, void *, char*, char*, void *);

void c_xsltSaveResultToFile(graal_isolatethread_t*, void *, char*, char*, char*, char*);

char* c_xsltApplyStylesheet(graal_isolatethread_t*, void *, char*, char*, char*, void *);

#if defined(__cplusplus)
}
#endif
#endif
