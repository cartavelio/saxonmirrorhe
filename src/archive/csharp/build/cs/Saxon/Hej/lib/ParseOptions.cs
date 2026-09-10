using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class ParseOptions {
        private int schemaValidation = Saxon.Hej.lib.Validation.DEFAULT;
        private int dtdValidation = Saxon.Hej.lib.Validation.DEFAULT;
        private Saxon.Hej.om.StructuredQName topLevelElement;
        private Saxon.Hej.type.SchemaType topLevelType;
        private Saxon.Ejava.util.Optional<bool> wrapDocument = Saxon.Ejava.util.Optional<bool>.empty();
        private Saxon.Hej.om.TreeModel treeModel = null;
        private Saxon.Hej.om.SpaceStrippingRule spaceStrippingRule = null;
        private Saxon.Ejava.util.Optional<bool> lineNumbering = Saxon.Ejava.util.Optional<bool>.empty();
        private bool pleaseClose = false;
        private Saxon.Hej.lib.ErrorReporter errorReporter = null;
        private System.Collections.Generic.IList<Saxon.Hej.@event.FilterFactory> filters = null;
        private bool continueAfterValidationErrors = false;
        private bool addCommentsAfterValidationErrors = false;
        private bool expandAttributeDefaults = true;
        private bool useXsiSchemaLocation = true;
        private bool checkEntityReferences = false;
        private bool stable = true;
        private int validationErrorLimit = System.Int32.MaxValue;
        private Saxon.Hej.type.ValidationParams validationParams = null;
        private Saxon.Hej.lib.ValidationStatisticsRecipient validationStatisticsRecipient = null;
        private System.Collections.Generic.IDictionary<string,bool> parserFeatures = null;
        private System.Collections.Generic.IDictionary<string,System.Object> parserProperties = null;
        private Saxon.Hej.lib.InvalidityHandler invalidityHandler = null;
        private System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> applicableAccumulators = null;
        public ParseOptions()  {
        }
        public ParseOptions(Saxon.Hej.lib.ParseOptions p)  {
            schemaValidation = p.schemaValidation;
            validationParams = p.validationParams;
            setDTDValidationMode(p.dtdValidation);
            topLevelElement = p.topLevelElement;
            topLevelType = p.topLevelType;
            wrapDocument = p.wrapDocument;
            treeModel = p.treeModel;
            spaceStrippingRule = p.spaceStrippingRule;
            lineNumbering = p.lineNumbering;
            pleaseClose = p.pleaseClose;
            errorReporter = p.errorReporter;
            invalidityHandler = p.invalidityHandler;
            stable = p.stable;
            if ((p.filters != null)) {
                filters = new System.Collections.Generic.List<Saxon.Hej.@event.FilterFactory>(p.filters);
            }
            setExpandAttributeDefaults(p.expandAttributeDefaults);
            useXsiSchemaLocation = p.useXsiSchemaLocation;
            validationErrorLimit = p.validationErrorLimit;
            continueAfterValidationErrors = p.continueAfterValidationErrors;
            addCommentsAfterValidationErrors = p.addCommentsAfterValidationErrors;
            if ((p.parserFeatures != null)) {
                parserFeatures = new System.Collections.Generic.Dictionary<string, bool>(p.parserFeatures);
            }
            if ((p.parserProperties != null)) {
                parserProperties = new System.Collections.Generic.Dictionary<string, System.Object>(p.parserProperties);
            }
            applicableAccumulators = p.applicableAccumulators;
            checkEntityReferences = p.checkEntityReferences;
            validationStatisticsRecipient = p.validationStatisticsRecipient;
        }
        public void merge(Saxon.Hej.lib.ParseOptions options) {
            if ((options.dtdValidation != Saxon.Hej.lib.Validation.DEFAULT)) {
                setDTDValidationMode(options.dtdValidation);
            }
            if ((options.schemaValidation != Saxon.Hej.lib.Validation.DEFAULT)) {
                schemaValidation = options.schemaValidation;
            }
            if ((options.invalidityHandler != null)) {
                invalidityHandler = options.invalidityHandler;
            }
            if ((options.topLevelElement != null)) {
                topLevelElement = options.topLevelElement;
            }
            if ((options.topLevelType != null)) {
                topLevelType = options.topLevelType;
            }
            if (options.wrapDocument.isPresent()) {
                wrapDocument = options.wrapDocument;
            }
            if ((options.treeModel != null)) {
                treeModel = options.treeModel;
            }
            if ((options.spaceStrippingRule != null)) {
                spaceStrippingRule = options.spaceStrippingRule;
            }
            if (options.lineNumbering.isPresent()) {
                lineNumbering = options.lineNumbering;
            }
            if (options.pleaseClose) {
                pleaseClose = true;
            }
            if ((options.filters != null)) {
                if ((filters == null)) {
                    filters = new System.Collections.Generic.List<Saxon.Hej.@event.FilterFactory>();
                }
                Saxon.Impl.Helpers.ListUtils.AddAll(filters, options.filters);
            }
            if ((options.parserFeatures != null)) {
                if ((parserFeatures == null)) {
                    parserFeatures = new System.Collections.Generic.Dictionary<string, bool>();
                }
                Saxon.Impl.Helpers.MapUtils.PutAll(parserFeatures, options.parserFeatures);
            }
            if ((options.parserProperties != null)) {
                if ((parserProperties == null)) {
                    parserProperties = new System.Collections.Generic.Dictionary<string, System.Object>();
                }
                Saxon.Impl.Helpers.MapUtils.PutAll(parserProperties, options.parserProperties);
            }
            if (!(options.expandAttributeDefaults)) {
                setExpandAttributeDefaults(false);
            }
            if (!(options.useXsiSchemaLocation)) {
                useXsiSchemaLocation = false;
            }
            if (options.addCommentsAfterValidationErrors) {
                addCommentsAfterValidationErrors = true;
            }
            validationErrorLimit = System.Math.Min(validationErrorLimit, options.validationErrorLimit);
        }
        public void applyDefaults(Saxon.Hej.Configuration config) {
            if ((dtdValidation == Saxon.Hej.lib.Validation.DEFAULT)) {
                setDTDValidationMode((config.isValidation() ? Saxon.Hej.lib.Validation.STRICT : Saxon.Hej.lib.Validation.SKIP));
            }
            if ((schemaValidation == Saxon.Hej.lib.Validation.DEFAULT)) {
                schemaValidation = config.getSchemaValidationMode();
            }
            if ((treeModel == null)) {
                treeModel = Saxon.Hej.om.TreeModel.getTreeModel(config.getTreeModel());
            }
            if ((spaceStrippingRule == null)) {
                spaceStrippingRule = config.getParseOptions().getSpaceStrippingRule();
            }
            if (!(lineNumbering.isPresent())) {
                lineNumbering = Saxon.Ejava.util.Optional<bool>.of(config.isLineNumbering());
            }
        }
        public void addFilter(Saxon.Hej.@event.FilterFactory filterFactory) {
            if ((filters == null)) {
                filters = new System.Collections.Generic.List<Saxon.Hej.@event.FilterFactory>(5);
            }
            filters.Add(filterFactory);
        }
        public System.Collections.Generic.IList<Saxon.Hej.@event.FilterFactory> getFilters() {
            return filters;
        }
        public Saxon.Hej.om.SpaceStrippingRule getSpaceStrippingRule() {
            return spaceStrippingRule;
        }
        public void setSpaceStrippingRule(Saxon.Hej.om.SpaceStrippingRule rule) {
            spaceStrippingRule = rule;
        }
        public void setTreeModel(int model) {
            treeModel = Saxon.Hej.om.TreeModel.getTreeModel(model);
        }
        public void addParserFeature(string uri, bool value) {
            if ((parserFeatures == null)) {
                parserFeatures = new System.Collections.Generic.Dictionary<string, bool>();
            }
            Saxon.Impl.Helpers.MapUtils.Put(parserFeatures, uri, value);
        }
        public void addParserProperties(string uri, System.Object value) {
            if ((parserProperties == null)) {
                parserProperties = new System.Collections.Generic.Dictionary<string, System.Object>();
            }
            Saxon.Impl.Helpers.MapUtils.Put(parserProperties, uri, value);
        }
        public bool getParserFeature(string uri) {
            return Saxon.Impl.Helpers.MapUtils.Get(parserFeatures, uri);
        }
        public System.Object getParserProperty(string name) {
            return Saxon.Impl.Helpers.MapUtils.Get(parserProperties, name);
        }
        public System.Collections.Generic.IDictionary<string,bool> getParserFeatures() {
            return parserFeatures;
        }
        public System.Collections.Generic.IDictionary<string,System.Object> getParserProperties() {
            return parserProperties;
        }
        public int getTreeModel() {
            if ((treeModel == null)) {
                return Saxon.Hej.@event.Builder.UNSPECIFIED_TREE_MODEL;
            }
            return treeModel.getSymbolicValue();
        }
        public void setModel(Saxon.Hej.om.TreeModel model) {
            treeModel = model;
        }
        public Saxon.Hej.om.TreeModel getModel() {
            return ((treeModel == null) ? Saxon.Hej.om.TreeModel.TINY_TREE : treeModel);
        }
        public void setSchemaValidationMode(int option) {
            schemaValidation = option;
        }
        public int getSchemaValidationMode() {
            return schemaValidation;
        }
        public void setExpandAttributeDefaults(bool expand) {
            this.expandAttributeDefaults = expand;
        }
        public bool isExpandAttributeDefaults() {
            return expandAttributeDefaults;
        }
        public void setTopLevelElement(Saxon.Hej.om.StructuredQName elementName) {
            topLevelElement = elementName;
        }
        public Saxon.Hej.om.StructuredQName getTopLevelElement() {
            return topLevelElement;
        }
        public void setTopLevelType(Saxon.Hej.type.SchemaType type) {
            topLevelType = type;
        }
        public Saxon.Hej.type.SchemaType getTopLevelType() {
            return topLevelType;
        }
        public void setUseXsiSchemaLocation(bool use) {
            useXsiSchemaLocation = use;
        }
        public bool isUseXsiSchemaLocation() {
            return useXsiSchemaLocation;
        }
        public int getValidationErrorLimit() {
            return validationErrorLimit;
        }
        public void setValidationErrorLimit(int validationErrorLimit) {
            this.validationErrorLimit = validationErrorLimit;
        }
        public void setDTDValidationMode(int option) {
            dtdValidation = option;
            addParserFeature("http://xml.org/sax/features/validation", ((option == Saxon.Hej.lib.Validation.STRICT) || (option == Saxon.Hej.lib.Validation.LAX)));
        }
        public int getDTDValidationMode() {
            return dtdValidation;
        }
        public void setValidationStatisticsRecipient(Saxon.Hej.lib.ValidationStatisticsRecipient recipient) {
            validationStatisticsRecipient = recipient;
        }
        public Saxon.Hej.lib.ValidationStatisticsRecipient getValidationStatisticsRecipient() {
            return validationStatisticsRecipient;
        }
        public void setLineNumbering(bool lineNumbering) {
            this.lineNumbering = Saxon.Ejava.util.Optional<bool>.of(lineNumbering);
        }
        public bool isLineNumbering() {
            return (lineNumbering.isPresent() && lineNumbering.get());
        }
        public bool isLineNumberingSet() {
            return lineNumbering.isPresent();
        }
        public void setWrapDocument(Saxon.Ejava.util.Optional<bool> wrap) {
            wrapDocument = wrap;
        }
        public Saxon.Ejava.util.Optional<bool> getWrapDocument() {
            return wrapDocument;
        }
        public void setXIncludeAware(bool state) {
            addParserFeature("http://apache.org/xml/features/xinclude", state);
        }
        public bool isXIncludeAwareSet() {
            return ((parserFeatures != null) && parserFeatures.ContainsKey("http://apache.org/xml/features/xinclude"));
        }
        public bool isXIncludeAware() {
            return ((parserFeatures != null) && Saxon.Impl.Helpers.MapUtils.GetOrDefault(parserFeatures, "http://apache.org/xml/features/xinclude", false));
        }
        public void setErrorReporter(Saxon.Hej.lib.ErrorReporter reporter) {
            if ((reporter == null)) {
                reporter = new Saxon.Hej.lib.StandardErrorReporter();
            }
            errorReporter = reporter;
        }
        public Saxon.Hej.lib.ErrorReporter getErrorReporter() {
            return errorReporter;
        }
        public void setContinueAfterValidationErrors(bool keepGoing) {
            continueAfterValidationErrors = keepGoing;
        }
        public bool isContinueAfterValidationErrors() {
            return continueAfterValidationErrors;
        }
        public void setAddCommentsAfterValidationErrors(bool keepGoing) {
            addCommentsAfterValidationErrors = keepGoing;
        }
        public bool isAddCommentsAfterValidationErrors() {
            return addCommentsAfterValidationErrors;
        }
        public void setValidationParams(Saxon.Hej.type.ValidationParams @params) {
            validationParams = @params;
        }
        public Saxon.Hej.type.ValidationParams getValidationParams() {
            return validationParams;
        }
        public void setCheckEntityReferences(bool check) {
            this.checkEntityReferences = check;
        }
        public bool isCheckEntityReferences() {
            return this.checkEntityReferences;
        }
        public bool isStable() {
            return stable;
        }
        public void setStable(bool stable) {
            this.stable = stable;
        }
        public Saxon.Hej.lib.InvalidityHandler getInvalidityHandler() {
            return invalidityHandler;
        }
        public void setInvalidityHandler(Saxon.Hej.lib.InvalidityHandler invalidityHandler) {
            this.invalidityHandler = invalidityHandler;
        }
        public void setApplicableAccumulators(System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> accumulators) {
            this.applicableAccumulators = accumulators;
        }
        public System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> getApplicableAccumulators() {
            return applicableAccumulators;
        }
        public void setPleaseCloseAfterUse(bool close) {
            pleaseClose = close;
        }
        public bool isPleaseCloseAfterUse() {
            return pleaseClose;
        }
        public static void close(Saxon.Ejavax.xml.transform.Source source) {
            try {
                if ((source is Saxon.Ejavax.xml.transform.stream.StreamSource)) {
                    Saxon.Ejavax.xml.transform.stream.StreamSource ss = (Saxon.Ejavax.xml.transform.stream.StreamSource)(source);
                    if ((ss.getInputStream() != null)) {
                        ss.getInputStream().Close();
                    }
                    if ((ss.getReader() != null)) {
                        ss.getReader().Close();
                    }
                } else {
                    if ((source is Saxon.Hej.lib.AugmentedSource)) {
                        ((Saxon.Hej.lib.AugmentedSource)(source)).close();
                    }
                }
            } catch (System.IO.IOException err) {
            }
        }
    
    }
}