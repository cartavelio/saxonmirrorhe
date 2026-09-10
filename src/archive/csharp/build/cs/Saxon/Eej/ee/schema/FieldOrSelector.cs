using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public abstract class FieldOrSelector : Saxon.Eej.ee.schema.SchemaStructure {
        private string xpath;
        protected internal Saxon.Hej.expr.Expression selectExpression;
        private Saxon.Hej.pattern.Pattern selection;
        private Saxon.Hej.om.NamespaceResolver namespaceContext;
        private Saxon.Hej.om.NamespaceUri xpathDefaultNamespace;
        public string getXPath() {
            return xpath;
        }
        public Saxon.Hej.pattern.Pattern getSelection() {
            return selection;
        }
        public Saxon.Hej.expr.Expression getSelectExpression() {
            return selectExpression;
        }
        public void setXPath(string xpath) {
            this.xpath = xpath;
        }
        public void setSelection(Saxon.Hej.pattern.Pattern selection) {
            this.selection = selection;
        }
        public void setSelectExpression(Saxon.Hej.expr.Expression selectExpression) {
            this.selectExpression = selectExpression;
        }
        public void setNamespaceContext(Saxon.Hej.om.NamespaceResolver resolver) {
            this.namespaceContext = resolver;
        }
        public Saxon.Hej.om.NamespaceResolver getNamespaceContext() {
            return namespaceContext;
        }
        public void setXPathDefaultNamespace(Saxon.Hej.om.NamespaceUri uri) {
            xpathDefaultNamespace = uri;
        }
        public Saxon.Hej.om.NamespaceUri getXPathDefaultNamespace() {
            return xpathDefaultNamespace;
        }
    
    }
}