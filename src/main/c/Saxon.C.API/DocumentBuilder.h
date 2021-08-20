////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2021 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef DOCUMENT_BUILDER_H
#define DOCUMENT_BUILDER_H


#include "SaxonProcessor.h"
//#include "XdmValue.h"
#include <string>

class SaxonProcessor;
class SaxonApiException;
class XdmNode;
class SchemaValidator;

/**
 * A document builder holds properties controlling how a Saxon document tree should be built, and
 * provides methods to invoke the tree construction.
 * <p>This class has no public constructor.  To construct a DocumentBuilder,
 * use the factory method SaxonProcessor.newDocumentBuilder().</p>
 * <p>All documents used in a single Saxon query, transformation, or validation episode must
 * be built with the same Configuration. However, there is no requirement that they
 * should use the same <code>DocumentBuilder</code>.</p>
 * <p>Sharing of a <code>DocumentBuilder</code> across multiple threads is not recommended. However,
 * in the current implementation sharing a <code>DocumentBuilder</code> (once initialized) will only
 * cause problems if a <code>SchemaValidator</code> is used.</p>
 */
class DocumentBuilder {

    friend class SaxonProcessor;


    public:


    /**
     * Say whether line and column numbering and is to be enabled for documents constructed using this DocumentBuilder.
     * This has the effect that the line and column number in the original source document is maintained in the constructed
     * tree, for each element node (and only for elements). The line and column number in question are generally the position
     * at which the closing "&gt;" of the element start tag appears.
     * <p>By default, line and column numbers are not maintained.</p>
     * <p>Errors relating to document parsing and validation will generally contain line numbers whether or not
     * this option is set, because such errors are detected during document construction.</p>
     * <p>Line numbering is not available for all kinds of source: for example,
     * it is not available when loading from an existing DOM Document.</p>
     *
     * @param option true if line numbers are to be maintained, false otherwise.
     */

    void setLineNumbering(bool option);

    /**
     * Ask whether line and column numbering is enabled for documents loaded using this
     * <code>DocumentBuilder</code>.
     * <p>By default, line and column numbering is disabled.</p>
     * <p>Line numbering is not available for all kinds of source: in particular,
     * it is not available when loading from an existing DOM Document.</p>
     * @return true if line numbering is enabled
     */

    bool isLineNumbering();

    /**
     * Set the schemaValidator to be used. This determines whether schema validation is applied to an input
     * document and whether type annotations in a supplied document are retained. If no schemaValidator
     * is supplied, then schema validation does not take place.
     * <p>This option requires the schema-aware version of the Saxon product (Saxon-EE).</p>
     * <p>Since a <code>SchemaValidator</code> is serially reusable but not thread-safe, using this
     * method is not appropriate when the <code>DocumentBuilder</code> is shared between threads.</p>
     *
     * @param validator the SchemaValidator to be used
     */

    void setSchemaValidator(SchemaValidator * validator);

    /**
     * Get the SchemaValidator used to validate documents loaded using this
     * <code>DocumentBuilder</code>.
     *
     * @return the SchemaValidator if one has been set; otherwise null.
     */
    SchemaValidator * getSchemaValidator();

    /**
     * Set whether DTD validation should be applied to documents loaded using this
     * <code>DocumentBuilder</code>.
     * <p>By default, no DTD validation takes place.</p>
     *
     * @param option true if DTD validation is to be applied to the document
     */

    void setDTDValidation(bool option);


    /**
     * Ask whether DTD validation is to be applied to documents loaded using this <code>DocumentBuilder</code>
     *
     * @return true if DTD validation is to be applied
     */

    bool isDTDValidation();


    /**
     * Set the base URI of a document loaded using this <code>DocumentBuilder</code>.
     * <p>This is used for resolving any relative URIs appearing
     * within the document, for example in references to DTDs and external entities.</p>
     * <p>This information is required when the document is loaded from a source that does not
     * provide an intrinsic URI, notably when loading from a Stream or a DOMSource. The value is
     * ignored when loading from a source that does have an intrinsic base URI.</p>
     *
     * @param uri the base URI of documents loaded using this <code>DocumentBuilder</code>. This
     *            must be an absolute URI.
     * @throws IllegalArgumentException if the baseURI supplied is not an absolute URI
     */

    void setBaseURI(const char* uri);

    /**
     * Get the base URI of documents loaded using this DocumentBuilder when no other URI is available.
     *
     * @return the base URI to be used, or null if no value has been set.
     */

    const char * getBaseURI();


    /**
     * Load an XML document, to create a tree representation of the document in memory.
     *
     * @param source A JAXP Source object identifying the source of the document. This can always be
     *               a {@link javax.xml.transform.stream.StreamSource} or a {@link javax.xml.transform.sax.SAXSource}.
     *               Some kinds of Source are consumed by this method, and should only be used once.
     *               <p>If a SAXSource is supplied, the XMLReader held within the SAXSource may be modified (by setting
     *               features and properties) to reflect the options selected on this DocumentBuilder.</p>
     *               <p>If the source is an instance of {@link net.sf.saxon.om.NodeInfo} then the subtree rooted at this node
     *               will be copied (applying schema validation if requested) to create a new tree.</p>
     *               <p>Saxon also accepts an instance of {@link javax.xml.transform.stax.StAXSource} or
     *               {@link net.sf.saxon.pull.PullSource}, which can be used to supply a document that is to be parsed
     *               using a StAX parser.</p>
     *               <p>(9.8.0.5) This method now (once again) accepts an instance of {@link net.sf.saxon.lib.AugmentedSource}.
     *               If an {@code AugmentedSource} is supplied, the properties of the {@code AugmentedSource} take
     *               precedence over any properties set on this {@code DocumentBuilder}, which in turn take precedence
     *               over properties set at the {@link Processor} or {@link Configuration} level. The concept of
     *               "taking precedence" is explained more fully at {@link ParseOptions#merge(ParseOptions)}</p>
     *
     * @return An <code>XdmNode</code>. This will be
     *         the document node at the root of the tree of the resulting in-memory document.
     * @throws NullPointerException     if the source argument is null
     * @throws IllegalArgumentException if the kind of source is not recognized
     * @throws SaxonApiException        if any other failure occurs building the document, for example
     *                                  a parsing error
     */

    XdmNode* parseXmlFromString(const char * content);

    /**
     * Build a document from a supplied XML file
     *
     * @param file the supplied file
     * @return the XdmNode representing the root of the document tree
     * @throws SaxonApiException if any failure occurs retrieving or parsing the document
     */

   XdmNode* parseXmlFromFile(const char * filename);


    private:


       /**
         * Create a DocumentBuilder. This is a protected constructor. Users should construct a DocumentBuilder
         * by calling the factory method {@link net.sf.saxon.s9api.Processor#newDocumentBuilder()}.
         *
         * @param config the Saxon configuration
         */

        DocumentBuilder() ;



        //! Constructor with the SaxonProcessor supplied.
        /*!
          @param proc - Supplied pointer to the SaxonProcessor object
          cwd - The current working directory
        */
        DocumentBuilder(jobject docBuilderObject, std::string cwd);

    	/**
    	 * Xslt30Processor copy constructor.
    	 * @param other - Xslt30Processor
    	 */
        DocumentBuilder(const DocumentBuilder &other);

        
        bool lineNumbering, dtdVal;
        SchemaValidator * schemaValidator;
        jobject docBuilderObject;
        jclass  docBuilderClass;
        std::string cwdDB, baseURI; /*!< current working directory */
} ;

#endif /* DOCUMENT_BUILDER_H */