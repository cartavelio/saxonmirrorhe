#ifndef SAXONCPROC_H
#define SAXONCPROC_H

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2022 - 2023 Saxonica Liimited.
// This Source Code Form is subject to the terms of the Mozilla Public License,
// v. 2.0. If a copy of the MPL was not distributed with this file, You can
// obtain one at http://mozilla.org/MPL/2.0/. This Source Code Form is
// "Incompatible With Secondary Licenses", as defined by the Mozilla Public
// License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SaxonCGlue.h"

/*! <code>sxnc_xquery</code>. This struct is used to capture the Java processor
 * objects created in C for XQuery <p/>
 */
typedef struct {
  int64_t xqueryProc;
} sxnc_xquery;

/*! <code>sxnc_processor</code>. This struct is used to capture the Java
 * processor objects created in C for XSLT <p/>
 */
typedef struct {
  int64_t xsltProc;
} sxnc_xslt;

/*! <code>sxnc_processor</code>. This struct is used to capture the Java
 * processor objects created in C for XPath <p/>
 */
typedef struct {
  int64_t xpathProc;
} sxnc_xpath;

EXTERN_SAXONC

/*
 * Get the Saxon version
 */
const char *version(sxnc_environment *environi, sxnc_processor *proc);

/*
 * Get the Saxon version
 */
const char *getProductVariantAndVersion(sxnc_environment *environi,
                                        sxnc_processor *proc);

/**
 * Initialise the SaxonC Processor, along with the parameters and the properties pointers.
 * This utility function is used to allocate memory on the heap therefore is required to be called before execution of stylesheets or queries
 *
 * @param environi -  Graalvm environment held as the struct sxnc_environment. This function calls malloc on the pointer
 * @param proc - Pointer to the Saxon Processor which is represented as the struct sxnc_processor
 *              and is used for creating the XSLT and XQuery processors. This function calls malloc on the pointer
 * @param parameters - Pointer to the sxnc_parameter struct used for the mapping (string, value). This function calls calloc on the pointer
 * @param properties - Pointer to the sxnc_property struct used for the the mapping (string, string).  This function calls calloc on the pointer
 * @param parLen - The length of the parameters array
 * @param propLen - The length of the properties array
 */
void initSaxonc(sxnc_environment **environi, sxnc_processor **proc,
                sxnc_parameter **param, sxnc_property **prop, int cap,
                int propCap);

/**
 * Free associated memory on the heap created by the SaxonC Processors, properties and parameters
 *
 * @param environi -  Graalvm environment held as the struct sxnc_environment.
 * @param proc - Pointer to the Saxon Processor which is represented as the struct sxnc_processor.
 * @param parameters - Pointer to the sxnc_parameter struct used for the mapping (string, value).
 * @param properties - Pointer to the sxnc_property struct used for the the mapping (string, string).
 */
void freeSaxonc(sxnc_environment **environi, sxnc_processor **proc,
                sxnc_parameter **param, sxnc_property **prop);


/**
 * Transform the XSLT stylesheet with the result saved to file.
 * The compile and execution is done in the same call of this method
 *
 * @param environi -  Graalvm environment
 * @param proc - Pointer to the Saxon Processor used for creating the XSLT executable
 * @param cwd - Current working directory and used for base URI for input and output files
 * @param source - The file name of the source document
 * @param stylesheet - The file name of the stylesheet document.
 * @param outputfile - The file name of where result will be stored
 * @param parameters - Array of parameters used for the query as the mapping (string, value)
 * @param properties - Array of properties used for the query as the mapping (string, string)
 * Available Xslt Processor properties: set serialization properties (names start with '!' i.e. name
 * "!method" -> "xml") 'o':outfile name, 'it': initial template, 'im': initial
 * mode, 's': source as file name 'm': switch on message listener for xsl:message instructions
 * @param parLen - The length of the parameters array
 * @param propLen - The length of the properties array
 */
void xsltSaveResultToFile(sxnc_environment *environi, sxnc_processor *proc,
                          char *cwd, char *source, char *stylesheet,
                          char *outputfile, sxnc_parameter *parameters,
                          sxnc_property *properties, int parLen, int propLen);

/**
 * Transform the XSLT stylesheet with the result returned as string value (char pointer array).
 * The compile and execution is done in the same call of this method
 *
 * @param environi -  Graalvm environment
 * @param proc - Pointer to the Saxon Processor used for creating the XSLT executable
 * @param cwd - Current working directory and used for base URI for input and output files
 * @param source - The file name of the source document
 * @param stylesheet - The file name of the stylesheet document.
 * @param parameters - Array of parameters used for the query as the mapping (string, value)
 * @param properties - Array of properties used for the query as the mapping (string, string)
 * Available Xslt Processor properties: set serialization properties (names start with '!' i.e. name
 * "!method" -> "xml") 'o':outfile name, 'it': initial template, 'im': initial
 * mode, 's': source as file name 'm': switch on message listener for xsl:message instructions
 * @param parLen - The length of the parameters array
 * @param propLen - The length of the properties array
 * @return char pointer array - The result of the transformation
 */
const char *xsltApplyStylesheet(sxnc_environment *environi,
                                sxnc_processor *proc, char *cwd, char *source,
                                char *stylesheet, sxnc_parameter *parameters,
                                sxnc_property *properties, int parLen,
                                int propLen);
/**
 * Perform the Query to file.
 *
 * @param environi -  Graalvm environment
 * @param proc - Pointer to the Saxon Processor used for creating the XQuery executable
 * @param cwd - Current working directory and used for base URI for input and output files
 * @param outputfile - The file name of where result will be stored
 * @param parameters - Array of parameters used for the query as the mapping (string, value)
 * @param properties - Array of properties used for the query as the mapping (string, string)
  * Available XQuery Processor properties: set serialization properties (names start with '!' i.e.
  * name "!method" -> "xml") 'o':outfile name, 's': source as file name 'q':
  * query file name, 'q': current by name, 'qs': string form of the query,
  * 'base': set the base URI of the query, 'dtd': set DTD validation 'on' or 'off'
 * @param parLen - The length of the parameters array
 * @param propLen - The length of the properties array
 */
void executeQueryToFile(sxnc_environment *environi, sxnc_processor *proc,
                        char *cwd, char *outputfile, sxnc_parameter *parameters,
                        sxnc_property *properties, int parLen, int propLen);


/**
 * Perform the Query and return the result as tring value (char pointer array)
 *
 * @param environi -  Graalvm environment
 * @param proc - Pointer to the Saxon Processor used for creating the XQuery executable
 * @param cwd - Current working directory and used for base URI for input and output files
 * @param parameters - Array of parameters used for the query as the mapping (string, value)
 * @param properties - Array of properties used for the query as the mapping (string, string).
 * Available XQuery Processor properties: set serialization properties (names start with '!' i.e.
 * name "!method" -> "xml") 'o':outfile name, 's': source as file name 'q':
 * query file name, 'q': current by name, 'qs': string form of the query,
 * 'base': set the base URI of the query, 'dtd': set DTD validation 'on' or 'off'
 * @param parLen - The length of the parameters array
 * @param propLen - The length of the properties array
 */
const char *executeQueryToString(sxnc_environment *environi,
                                 sxnc_processor *proc, char *cwd,
                                 sxnc_parameter *parameters,
                                 sxnc_property *properties, int parLen,
                                 int propLen);

const char *c_getErrorMessage(sxnc_environment *environi);

EXTERN_SAXONC_END

#endif
