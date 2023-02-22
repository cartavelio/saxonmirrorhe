using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class AugmentedSource : Saxon.Ejavax.xml.transform.Source {
        private readonly Saxon.Ejavax.xml.transform.Source source;
        private Saxon.Hej.lib.ParseOptions options = new Saxon.Hej.lib.ParseOptions();
        private string systemID;
        private AugmentedSource(Saxon.Ejavax.xml.transform.Source source)  {
            if ((source is Saxon.Hej.lib.AugmentedSource)) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Contained source must not be an AugmentedSource");
            }
            this.source = source;
        }
        public AugmentedSource(Saxon.Ejavax.xml.transform.Source source, Saxon.Hej.lib.ParseOptions options)  {
            if ((source is Saxon.Hej.lib.AugmentedSource)) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Contained source must not be an AugmentedSource");
            }
            this.source = source;
            this.options = options;
        }
        public static Saxon.Hej.lib.AugmentedSource makeAugmentedSource(Saxon.Ejavax.xml.transform.Source source) {
            if ((source is Saxon.Hej.lib.AugmentedSource)) {
                return (Saxon.Hej.lib.AugmentedSource)(source);
            }
            return new Saxon.Hej.lib.AugmentedSource(source);
        }
        public void addFilter(Saxon.Hej.@event.FilterFactory filter) {
            options.addFilter(filter);
        }
        public System.Collections.Generic.IList<Saxon.Hej.@event.FilterFactory> getFilters() {
            return options.getFilters();
        }
        public Saxon.Ejavax.xml.transform.Source getContainedSource() {
            return source;
        }
        public Saxon.Hej.lib.ParseOptions getParseOptions() {
            return options;
        }
        public void setModel(Saxon.Hej.om.TreeModel model) {
            options.setModel(model);
        }
        public Saxon.Hej.om.TreeModel getModel() {
            return options.getModel();
        }
        public void setSchemaValidationMode(int option) {
            options.setSchemaValidationMode(option);
        }
        public int getSchemaValidation() {
            return options.getSchemaValidationMode();
        }
        public void setTopLevelElement(Saxon.Hej.om.StructuredQName elementName) {
            options.setTopLevelElement(elementName);
        }
        public Saxon.Hej.om.StructuredQName getTopLevelElement() {
            return options.getTopLevelElement();
        }
        public void setTopLevelType(Saxon.Hej.type.SchemaType type) {
            options.setTopLevelType(type);
        }
        public Saxon.Hej.type.SchemaType getTopLevelType() {
            return options.getTopLevelType();
        }
        public void setDTDValidationMode(int option) {
            options.setDTDValidationMode(option);
        }
        public int getDTDValidation() {
            return options.getDTDValidationMode();
        }
        public void setLineNumbering(bool lineNumbering) {
            options.setLineNumbering(lineNumbering);
        }
        public bool isLineNumbering() {
            return options.isLineNumbering();
        }
        public bool isLineNumberingSet() {
            return options.isLineNumberingSet();
        }
        public void setWrapDocument(Saxon.Ejava.util.Optional<bool> wrap) {
            options.setWrapDocument(wrap);
        }
        public Saxon.Ejava.util.Optional<bool> getWrapDocument() {
            return options.getWrapDocument();
        }
        public void setSystemId(string id) {
            systemID = id;
        }
        public string getSystemId() {
            return ((systemID != null) ? systemID : source.getSystemId());
        }
        public void setXIncludeAware(bool state) {
            options.setXIncludeAware(state);
        }
        public bool isXIncludeAwareSet() {
            return options.isXIncludeAwareSet();
        }
        public bool isXIncludeAware() {
            return options.isXIncludeAware();
        }
        public void setErrorReporter(Saxon.Hej.lib.ErrorReporter listener) {
            options.setErrorReporter(listener);
        }
        public Saxon.Hej.lib.ErrorReporter getErrorReporter() {
            return options.getErrorReporter();
        }
        public void setPleaseCloseAfterUse(bool close) {
            options.setPleaseCloseAfterUse(close);
        }
        public bool isPleaseCloseAfterUse() {
            return options.isPleaseCloseAfterUse();
        }
        public void close() {
            Saxon.Hej.lib.ParseOptions.close(source);
        }
    
    }
}