using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    internal class DocumentBuilder {
        private readonly Saxon.Hej.Configuration config;
        private Saxon.Hej.s9api.SchemaValidator schemaValidator;
        private bool dtdValidation;
        private bool lineNumbering;
        private Saxon.Hej.om.TreeModel treeModel = Saxon.Hej.om.TreeModel.TINY_TREE;
        private Saxon.Api.WhitespacePolicy whitespacePolicy = Saxon.Api.WhitespacePolicy.Unspecified;
        private System.Uri baseURI;
        private Saxon.Hej.s9api.XQueryExecutable projectionQuery;
        protected internal DocumentBuilder(Saxon.Hej.Configuration config)  {
            this.config = config;
        }
        public void setTreeModel(Saxon.Hej.om.TreeModel model) {
            this.treeModel = model;
        }
        public Saxon.Hej.om.TreeModel getTreeModel() {
            return treeModel;
        }
        public void setLineNumbering(bool option) {
            lineNumbering = option;
        }
        public bool isLineNumbering() {
            return lineNumbering;
        }
        public void setSchemaValidator(Saxon.Hej.s9api.SchemaValidator validator) {
            schemaValidator = validator;
        }
        public Saxon.Hej.s9api.SchemaValidator getSchemaValidator() {
            return schemaValidator;
        }
        public void setDTDValidation(bool option) {
            dtdValidation = option;
        }
        public bool isDTDValidation() {
            return dtdValidation;
        }
        public void setWhitespaceStrippingPolicy(Saxon.Api.WhitespacePolicy policy) {
            whitespacePolicy = policy;
        }
        public Saxon.Api.WhitespacePolicy getWhitespaceStrippingPolicy() {
            return whitespacePolicy;
        }
        public void setBaseURI(System.Uri uri) {
            if (!(uri.IsAbsoluteUri)) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Supplied base URI must be absolute");
            }
            baseURI = uri;
        }
        public System.Uri getBaseURI() {
            return baseURI;
        }
        public void setDocumentProjectionQuery(Saxon.Hej.s9api.XQueryExecutable query) {
            this.projectionQuery = query;
        }
        public Saxon.Hej.s9api.XQueryExecutable getDocumentProjectionQuery() {
            return this.projectionQuery;
        }
        public Saxon.Api.XdmNode build(Saxon.Ejavax.xml.transform.Source source) {
            Saxon.Ejava.util.Objects.requireNonNull(source, "source");
            Saxon.Hej.lib.ParseOptions options = getParseOptions(source);
            try {
                Saxon.Hej.om.TreeInfo doc = config.buildDocumentTree(source, options);
                return new Saxon.Api.XdmNode(doc.getRootNode());
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        private Saxon.Hej.lib.ParseOptions getParseOptions(Saxon.Ejavax.xml.transform.Source source) {
            if (!(((((whitespacePolicy == Saxon.Api.WhitespacePolicy.Unspecified) || (whitespacePolicy == Saxon.Api.WhitespacePolicy.StripIgnorable)) || (whitespacePolicy.Ordinal() == Saxon.Hej.value.Whitespace.XSLT))))) {
                if (dtdValidation) {
                    throw new Saxon.Api.SaxonApiException("When DTD validation is used, the whitespace stripping policy must be IGNORABLE");
                }
                if ((schemaValidator != null)) {
                    throw new Saxon.Api.SaxonApiException("When schema validation is used, the whitespace stripping policy must be IGNORABLE");
                }
            }
            Saxon.Hej.lib.ParseOptions options = config.getParseOptions().withDTDValidationMode((dtdValidation ? Saxon.Hej.lib.Validation.STRICT : Saxon.Hej.lib.Validation.STRIP));
            if ((schemaValidator != null)) {
                options = options.withSchemaValidationMode((schemaValidator.isLax() ? Saxon.Hej.lib.Validation.LAX : Saxon.Hej.lib.Validation.STRICT));
                if ((schemaValidator.getDocumentElementName() != null)) {
                    Saxon.Api.QName qn = schemaValidator.getDocumentElementName();
                    options = options.withTopLevelElement(qn.GetStructuredQName());
                }
                if ((schemaValidator.getDocumentElementType() != null)) {
                    options = options.withTopLevelType(schemaValidator.getDocumentElementType());
                }
                options = options.withExpandAttributeDefaults(schemaValidator.isExpandAttributeDefaults());
                options = options.withUseXsiSchemaLocation(schemaValidator.isUseXsiSchemaLocation());
                options = options.withValidationParams(schemaValidator.getValidationParameters());
                options = options.withInvalidityHandler(schemaValidator.getInvalidityHandler());
            }
            if ((treeModel != null)) {
                options = options.withModel(treeModel);
            }
            if (((whitespacePolicy != null) && (whitespacePolicy != Saxon.Api.WhitespacePolicy.Unspecified))) {
                int option = whitespacePolicy.Ordinal();
                if ((option == Saxon.Hej.value.Whitespace.XSLT)) {
                    options = options.withSpaceStrippingRule(Saxon.Hej.om.NoElementsSpaceStrippingRule.getInstance());
                    options = options.withFilter((next => (new Saxon.Hej.@event.Stripper(whitespacePolicy.Implementation, next))));
                } else {
                    options = options.withSpaceStrippingRule(whitespacePolicy.Implementation);
                }
            }
            options = options.withLineNumbering(lineNumbering);
            if (((source.getSystemId() == null) && (baseURI != null))) {
                source.setSystemId(baseURI.ToString());
            }
            if ((projectionQuery != null)) {
                Saxon.Hej.query.XQueryExpression exp = projectionQuery.getUnderlyingCompiledQuery();
                Saxon.Hej.@event.FilterFactory ff = config.makeDocumentProjector(exp);
                if ((ff != null)) {
                    options = options.withFilter(ff);
                }
            }
            if ((source is Saxon.Hej.lib.AugmentedSource)) {
                options = options.merge(((Saxon.Hej.lib.AugmentedSource)(source)).getParseOptions());
            }
            return options;
        }
        public Saxon.Api.XdmNode build(Saxon.Ejava.io.File file) {
            return build(new Saxon.Ejavax.xml.transform.stream.StreamSource(file));
        }
        public void parse(Saxon.Ejavax.xml.transform.Source source, Saxon.Hej.s9api.Destination destination) {
            try {
                Saxon.Hej.lib.ParseOptions options = getParseOptions(source);
                Saxon.Hej.@event.PipelineConfiguration pipe = config.makePipelineConfiguration();
                Saxon.Hej.@event.Sender.send(source, destination.getReceiver(pipe, new Saxon.Hej.serialize.SerializationProperties()), options);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public void parse(Saxon.Ejava.io.File file, Saxon.Hej.s9api.Destination destination) {
            parse(new Saxon.Ejavax.xml.transform.stream.StreamSource(file), destination);
        }
    
    }
}