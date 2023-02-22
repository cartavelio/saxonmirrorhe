using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class ParseOptions {
        private enum Key {
            ADD_COMMENTS_AFTER_VALIDATION_ERRORS,
            APPLICABLE_ACCUMULATORS,
            CHECK_ENTITY_REFERENCES,
            CONTINUE_AFTER_VALIDATION_ERRORS,
            DTD_VALIDATION,
            ERROR_HANDLER,
            ERROR_REPORTER,
            EXPAND_ATTRIBUTE_DEFAULTS,
            FILTERS,
            INVALIDITY_HANDLER,
            LINE_NUMBERING,
            MODEL,
            PLEASE_CLOSE,
            SCHEMA_VALIDATION,
            SPACE_STRIPPING_RULE,
            STABLE,
            TOP_LEVEL_ELEMENT,
            TOP_LEVEL_TYPE,
            TREE_MODEL,
            USE_XSI_SCHEMA_LOCATION,
            VALIDATION_ERROR_LIMIT,
            VALIDATION_PARAMS,
            VALIDATION_STATISTICS_RECIPIENT,
            WRAP_DOCUMENT,
        }
        private readonly System.Collections.Immutable.ImmutableDictionary<Saxon.Hej.lib.ParseOptions.Key,System.Object> properties;
        private System.Collections.Generic.IList<Saxon.Hej.lib.ParseOptions.Key> cacheKeys;
        private System.Collections.Generic.IList<System.Object> cacheValues;
        private System.Collections.Generic.IList<Saxon.Hej.lib.ParseOptions> cacheResults;
        public ParseOptions()  {
            properties = init();
        }
        private ParseOptions(System.Collections.Immutable.ImmutableDictionary<Saxon.Hej.lib.ParseOptions.Key,System.Object> properties)  {
            this.properties = properties;
        }
        private System.Collections.Immutable.ImmutableDictionary<Saxon.Hej.lib.ParseOptions.Key,System.Object> init() {
            return System.Collections.Immutable.ImmutableDictionary<Saxon.Hej.lib.ParseOptions.Key,System.Object>.Empty;
        }
        private Saxon.Hej.lib.ParseOptions searchCache(Saxon.Hej.lib.ParseOptions.Key key, System.Object value) { lock(this) {
            if ((cacheKeys == null)) {
                return null;
            }
            for (int i = 0; (i<cacheKeys.Count); i++) {
                if (((cacheKeys[i] == key) && (cacheValues[i] == value))) {
                    return cacheResults[i];
                }
            }
            return null;
        }}
        private void addToCache(Saxon.Hej.lib.ParseOptions.Key key, System.Object value, Saxon.Hej.lib.ParseOptions result) { lock(this) {
            if ((cacheKeys == null)) {
                cacheKeys = new System.Collections.Generic.List<Saxon.Hej.lib.ParseOptions.Key>(10);
                cacheValues = new System.Collections.Generic.List<System.Object>(10);
                cacheResults = new System.Collections.Generic.List<Saxon.Hej.lib.ParseOptions>(10);
            } else {
                if ((cacheKeys.Count>=10)) {
                    cacheKeys.Clear();
                    cacheValues.Clear();
                    cacheResults.Clear();
                }
            }
            cacheKeys.Add(key);
            cacheValues.Add(value);
            cacheResults.Add(result);
        }}
        private Saxon.Hej.lib.ParseOptions withProperty(Saxon.Hej.lib.ParseOptions.Key key, System.Object value) {
            if ((value == Saxon.Impl.Helpers.MapUtils.Get(properties, key))) {
                return this;
            }
            Saxon.Hej.lib.ParseOptions result = searchCache(key, value);
            if ((result != null)) {
                return result;
            }
            if ((value == null)) {
                result = new Saxon.Hej.lib.ParseOptions(properties.Remove(key));
            } else {
                result = new Saxon.Hej.lib.ParseOptions(properties.SetItem(key, value));
            }
            addToCache(key, value, result);
            return result;
        }
        private System.Object getProperty(Saxon.Hej.lib.ParseOptions.Key key) {
            return Saxon.Impl.Helpers.MapUtils.Get(properties, key);
        }
        private bool hasProperty(Saxon.Hej.lib.ParseOptions.Key key) {
            return (Saxon.Impl.Helpers.MapUtils.Get(properties, key) != null);
        }
        private int getIntegerProperty(Saxon.Hej.lib.ParseOptions.Key key, int defaultValue) {
            System.Object value = Saxon.Impl.Helpers.MapUtils.Get(properties, key);
            if ((value == null)) {
                return defaultValue;
            } else {
                return (int)(value);
            }
        }
        private bool getBooleanProperty(Saxon.Hej.lib.ParseOptions.Key key, bool defaultValue) {
            System.Object value = Saxon.Impl.Helpers.MapUtils.Get(properties, key);
            if ((value == null)) {
                return defaultValue;
            } else {
                return (bool)(value);
            }
        }
        public Saxon.Hej.lib.ParseOptions merge(Saxon.Hej.lib.ParseOptions other) {
            Saxon.Hej.lib.ParseOptions result = this;
            if ((other.getDTDValidationMode() != Saxon.Hej.lib.Validation.DEFAULT)) {
                result = result.withDTDValidationMode(other.getDTDValidationMode());
            }
            if ((other.getSchemaValidationMode() != Saxon.Hej.lib.Validation.DEFAULT)) {
                result = result.withSchemaValidationMode(other.getSchemaValidationMode());
            }
            result = result.withPropertyIfNotNull(Saxon.Hej.lib.ParseOptions.Key.INVALIDITY_HANDLER, other.getInvalidityHandler());
            result = result.withPropertyIfNotNull(Saxon.Hej.lib.ParseOptions.Key.TOP_LEVEL_ELEMENT, other.getTopLevelElement());
            result = result.withPropertyIfNotNull(Saxon.Hej.lib.ParseOptions.Key.TOP_LEVEL_TYPE, other.getTopLevelType());
            result = result.withPropertyIfNotNull(Saxon.Hej.lib.ParseOptions.Key.SPACE_STRIPPING_RULE, other.getSpaceStrippingRule());
            result = result.withPropertyIfNotNull(Saxon.Hej.lib.ParseOptions.Key.TREE_MODEL, other.getTreeModel());
            if (other.hasProperty(Saxon.Hej.lib.ParseOptions.Key.LINE_NUMBERING)) {
                result = result.withLineNumbering(other.isLineNumbering());
            }
            if (other.isPleaseCloseAfterUse()) {
                result = result.withPleaseCloseAfterUse(true);
            }
            if ((other.getFilters() != null)) {
                foreach (Saxon.Hej.@event.FilterFactory ff in other.getFilters()) {
                    result = result.withFilter(ff);
                }
            }
            if (!(other.isExpandAttributeDefaults())) {
                result = result.withExpandAttributeDefaults(false);
            }
            if (!(other.isUseXsiSchemaLocation())) {
                result = result.withUseXsiSchemaLocation(false);
            }
            if (other.isAddCommentsAfterValidationErrors()) {
                result = result.withUseXsiSchemaLocation(true);
            }
            result = result.withValidationErrorLimit(System.Math.Min(this.getValidationErrorLimit(), other.getValidationErrorLimit()));
            return result;
        }
        private Saxon.Hej.lib.ParseOptions withPropertyIfNotNull(Saxon.Hej.lib.ParseOptions.Key key, System.Object value) {
            if ((value != null)) {
                return withProperty(key, value);
            }
            return this;
        }
        public Saxon.Hej.lib.ParseOptions applyDefaults(Saxon.Hej.Configuration config) {
            Saxon.Hej.lib.ParseOptions result = this;
            if ((getDTDValidationMode() == Saxon.Hej.lib.Validation.DEFAULT)) {
                result = result.withDTDValidationMode((config.isValidation() ? Saxon.Hej.lib.Validation.STRICT : Saxon.Hej.lib.Validation.SKIP));
            }
            if ((getSchemaValidationMode() == Saxon.Hej.lib.Validation.DEFAULT)) {
                result = result.withSchemaValidationMode(config.getSchemaValidationMode());
            }
            if ((getModel() == null)) {
                result = result.withModel(Saxon.Hej.om.TreeModel.getTreeModel(config.getTreeModel()));
            }
            if ((getSpaceStrippingRule() == null)) {
                result = result.withSpaceStrippingRule(config.getParseOptions().getSpaceStrippingRule());
            }
            if ((getProperty(Saxon.Hej.lib.ParseOptions.Key.LINE_NUMBERING) == null)) {
                result = result.withProperty(Saxon.Hej.lib.ParseOptions.Key.LINE_NUMBERING, config.isLineNumbering());
            }
            return result;
        }
        public Saxon.Hej.lib.ParseOptions withFilter(Saxon.Hej.@event.FilterFactory filterFactory) {
            System.Collections.Generic.IList<Saxon.Hej.@event.FilterFactory> list = getFilters();
            if ((list == null)) {
                list = new System.Collections.Generic.List<Saxon.Hej.@event.FilterFactory>(2);
            } else {
                list = new System.Collections.Generic.List<Saxon.Hej.@event.FilterFactory>(list);
            }
            list.Add(filterFactory);
            return withProperty(Saxon.Hej.lib.ParseOptions.Key.FILTERS, list);
        }
        public System.Collections.Generic.IList<Saxon.Hej.@event.FilterFactory> getFilters() {
            return (System.Collections.Generic.IList<Saxon.Hej.@event.FilterFactory>)(getProperty(Saxon.Hej.lib.ParseOptions.Key.FILTERS));
        }
        public Saxon.Hej.om.SpaceStrippingRule getSpaceStrippingRule() {
            return (Saxon.Hej.om.SpaceStrippingRule)(getProperty(Saxon.Hej.lib.ParseOptions.Key.SPACE_STRIPPING_RULE));
        }
        public Saxon.Hej.lib.ParseOptions withSpaceStrippingRule(Saxon.Hej.om.SpaceStrippingRule rule) {
            return withProperty(Saxon.Hej.lib.ParseOptions.Key.SPACE_STRIPPING_RULE, rule);
        }
        public Saxon.Hej.lib.ParseOptions withTreeModel(int model) {
            return withProperty(Saxon.Hej.lib.ParseOptions.Key.TREE_MODEL, model);
        }
        public int getTreeModel() {
            Saxon.Hej.om.TreeModel model = getModel();
            if ((model == null)) {
                return Saxon.Hej.@event.Builder.UNSPECIFIED_TREE_MODEL;
            }
            return model.getSymbolicValue();
        }
        public Saxon.Hej.lib.ParseOptions withModel(Saxon.Hej.om.TreeModel model) {
            return withProperty(Saxon.Hej.lib.ParseOptions.Key.MODEL, model);
        }
        public Saxon.Hej.om.TreeModel getModel() {
            Saxon.Hej.om.TreeModel treeModel = (Saxon.Hej.om.TreeModel)(getProperty(Saxon.Hej.lib.ParseOptions.Key.MODEL));
            return ((treeModel == null) ? Saxon.Hej.om.TreeModel.TINY_TREE : treeModel);
        }
        public Saxon.Hej.lib.ParseOptions withSchemaValidationMode(int option) {
            return withProperty(Saxon.Hej.lib.ParseOptions.Key.SCHEMA_VALIDATION, option);
        }
        public int getSchemaValidationMode() {
            return getIntegerProperty(Saxon.Hej.lib.ParseOptions.Key.SCHEMA_VALIDATION, Saxon.Hej.lib.Validation.DEFAULT);
        }
        public Saxon.Hej.lib.ParseOptions withExpandAttributeDefaults(bool expand) {
            return withProperty(Saxon.Hej.lib.ParseOptions.Key.EXPAND_ATTRIBUTE_DEFAULTS, expand);
        }
        public bool isExpandAttributeDefaults() {
            return getBooleanProperty(Saxon.Hej.lib.ParseOptions.Key.EXPAND_ATTRIBUTE_DEFAULTS, true);
        }
        public Saxon.Hej.lib.ParseOptions withTopLevelElement(Saxon.Hej.om.StructuredQName elementName) {
            return withProperty(Saxon.Hej.lib.ParseOptions.Key.TOP_LEVEL_ELEMENT, elementName);
        }
        public Saxon.Hej.om.StructuredQName getTopLevelElement() {
            return (Saxon.Hej.om.StructuredQName)(getProperty(Saxon.Hej.lib.ParseOptions.Key.TOP_LEVEL_ELEMENT));
        }
        public Saxon.Hej.lib.ParseOptions withTopLevelType(Saxon.Hej.type.SchemaType type) {
            return withProperty(Saxon.Hej.lib.ParseOptions.Key.TOP_LEVEL_TYPE, type);
        }
        public Saxon.Hej.type.SchemaType getTopLevelType() {
            return (Saxon.Hej.type.SchemaType)(getProperty(Saxon.Hej.lib.ParseOptions.Key.TOP_LEVEL_TYPE));
        }
        public Saxon.Hej.lib.ParseOptions withUseXsiSchemaLocation(bool use) {
            return withProperty(Saxon.Hej.lib.ParseOptions.Key.USE_XSI_SCHEMA_LOCATION, use);
        }
        public bool isUseXsiSchemaLocation() {
            return getBooleanProperty(Saxon.Hej.lib.ParseOptions.Key.USE_XSI_SCHEMA_LOCATION, true);
        }
        public int getValidationErrorLimit() {
            return getIntegerProperty(Saxon.Hej.lib.ParseOptions.Key.VALIDATION_ERROR_LIMIT, System.Int32.MaxValue);
        }
        public Saxon.Hej.lib.ParseOptions withValidationErrorLimit(int validationErrorLimit) {
            return withProperty(Saxon.Hej.lib.ParseOptions.Key.VALIDATION_ERROR_LIMIT, validationErrorLimit);
        }
        public Saxon.Hej.lib.ParseOptions withDTDValidationMode(int option) {
            return withProperty(Saxon.Hej.lib.ParseOptions.Key.DTD_VALIDATION, option);
        }
        public int getDTDValidationMode() {
            return getIntegerProperty(Saxon.Hej.lib.ParseOptions.Key.DTD_VALIDATION, Saxon.Hej.lib.Validation.SKIP);
        }
        public Saxon.Hej.lib.ParseOptions withValidationStatisticsRecipient(Saxon.Hej.lib.ValidationStatisticsRecipient recipient) {
            return withProperty(Saxon.Hej.lib.ParseOptions.Key.VALIDATION_STATISTICS_RECIPIENT, recipient);
        }
        public Saxon.Hej.lib.ValidationStatisticsRecipient getValidationStatisticsRecipient() {
            return (Saxon.Hej.lib.ValidationStatisticsRecipient)(getProperty(Saxon.Hej.lib.ParseOptions.Key.VALIDATION_STATISTICS_RECIPIENT));
        }
        public Saxon.Hej.lib.ParseOptions withLineNumbering(bool lineNumbering) {
            return withProperty(Saxon.Hej.lib.ParseOptions.Key.LINE_NUMBERING, lineNumbering);
        }
        public bool isLineNumbering() {
            return getBooleanProperty(Saxon.Hej.lib.ParseOptions.Key.LINE_NUMBERING, false);
        }
        public bool isLineNumberingSet() {
            return hasProperty(Saxon.Hej.lib.ParseOptions.Key.LINE_NUMBERING);
        }
        public Saxon.Hej.lib.ParseOptions withXIncludeAware(bool state) {
            if (state) throw new System.NotSupportedException("XInclude is not supported in SaxonCS"); else return this;
        }
        public bool isXIncludeAwareSet() {
            return false;
        }
        public bool isXIncludeAware() {
            return false;
        }
        public Saxon.Hej.lib.ParseOptions withErrorReporter(Saxon.Hej.lib.ErrorReporter reporter) {
            if ((reporter == null)) {
                reporter = new Saxon.Hej.lib.StandardErrorReporter();
            }
            return withProperty(Saxon.Hej.lib.ParseOptions.Key.ERROR_REPORTER, reporter);
        }
        public Saxon.Hej.lib.ErrorReporter getErrorReporter() {
            return (Saxon.Hej.lib.ErrorReporter)(getProperty(Saxon.Hej.lib.ParseOptions.Key.ERROR_REPORTER));
        }
        public Saxon.Hej.lib.ParseOptions withContinueAfterValidationErrors(bool keepGoing) {
            return withProperty(Saxon.Hej.lib.ParseOptions.Key.CONTINUE_AFTER_VALIDATION_ERRORS, keepGoing);
        }
        public bool isContinueAfterValidationErrors() {
            return getBooleanProperty(Saxon.Hej.lib.ParseOptions.Key.CONTINUE_AFTER_VALIDATION_ERRORS, false);
        }
        public Saxon.Hej.lib.ParseOptions withAddCommentsAfterValidationErrors(bool addComments) {
            return withProperty(Saxon.Hej.lib.ParseOptions.Key.ADD_COMMENTS_AFTER_VALIDATION_ERRORS, addComments);
        }
        public bool isAddCommentsAfterValidationErrors() {
            return getBooleanProperty(Saxon.Hej.lib.ParseOptions.Key.ADD_COMMENTS_AFTER_VALIDATION_ERRORS, false);
        }
        public Saxon.Hej.lib.ParseOptions withValidationParams(Saxon.Hej.type.ValidationParams @params) {
            return withProperty(Saxon.Hej.lib.ParseOptions.Key.VALIDATION_PARAMS, @params);
        }
        public Saxon.Hej.type.ValidationParams getValidationParams() {
            return (Saxon.Hej.type.ValidationParams)(getProperty(Saxon.Hej.lib.ParseOptions.Key.VALIDATION_PARAMS));
        }
        public Saxon.Hej.lib.ParseOptions withCheckEntityReferences(bool check) {
            return withProperty(Saxon.Hej.lib.ParseOptions.Key.CHECK_ENTITY_REFERENCES, check);
        }
        public bool isCheckEntityReferences() {
            return getBooleanProperty(Saxon.Hej.lib.ParseOptions.Key.CHECK_ENTITY_REFERENCES, false);
        }
        public bool isStable() {
            return getBooleanProperty(Saxon.Hej.lib.ParseOptions.Key.STABLE, true);
        }
        public Saxon.Hej.lib.ParseOptions withStable(bool stable) {
            return withProperty(Saxon.Hej.lib.ParseOptions.Key.STABLE, stable);
        }
        public Saxon.Hej.lib.InvalidityHandler getInvalidityHandler() {
            return (Saxon.Hej.lib.InvalidityHandler)(getProperty(Saxon.Hej.lib.ParseOptions.Key.INVALIDITY_HANDLER));
        }
        public Saxon.Hej.lib.ParseOptions withInvalidityHandler(Saxon.Hej.lib.InvalidityHandler invalidityHandler) {
            return withProperty(Saxon.Hej.lib.ParseOptions.Key.INVALIDITY_HANDLER, invalidityHandler);
        }
        public Saxon.Hej.lib.ParseOptions withApplicableAccumulators(System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> accumulators) {
            return withProperty(Saxon.Hej.lib.ParseOptions.Key.APPLICABLE_ACCUMULATORS, accumulators);
        }
        public System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> getApplicableAccumulators() {
            return (System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator>)(getProperty(Saxon.Hej.lib.ParseOptions.Key.APPLICABLE_ACCUMULATORS));
        }
        public Saxon.Hej.lib.ParseOptions withPleaseCloseAfterUse(bool close) {
            return withProperty(Saxon.Hej.lib.ParseOptions.Key.PLEASE_CLOSE, close);
        }
        public bool isPleaseCloseAfterUse() {
            return getBooleanProperty(Saxon.Hej.lib.ParseOptions.Key.PLEASE_CLOSE, false);
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
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder();
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.lib.ParseOptions.Key,System.Object> entry in properties) {
                sb.append(entry.Key);
                sb.append('=');
                sb.append(entry.Value);
                sb.append(' ');
            }
            return sb.ToString();
        }
    
    }
}