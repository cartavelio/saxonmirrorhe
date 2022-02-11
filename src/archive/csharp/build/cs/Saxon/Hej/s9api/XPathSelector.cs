using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    internal class XPathSelector : System.Collections.Generic.IEnumerable<Saxon.Api.XdmItem> {
        private readonly Saxon.Hej.sxpath.XPathExpression exp;
        private readonly Saxon.Hej.sxpath.XPathDynamicContext dynamicContext;
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.sxpath.XPathVariable> declaredVariables;
        protected internal XPathSelector(Saxon.Hej.sxpath.XPathExpression exp, System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.sxpath.XPathVariable> declaredVariables)  {
            this.exp = exp;
            this.declaredVariables = declaredVariables;
            dynamicContext = exp.createDynamicContext();
        }
        public void setContextItem(Saxon.Api.XdmItem item) {
            if ((item == null)) {
                throw new Saxon.Ejava.lang.NullPointerException("contextItem");
            }
            if (!(exp.getInternalExpression().getPackageData().isSchemaAware())) {
                Saxon.Hej.om.Item it = ((Saxon.Hej.om.Item)item.Unwrap()).head();
                if (((it is Saxon.Hej.om.NodeInfo) && ((Saxon.Hej.om.NodeInfo)(it)).getTreeInfo().isTyped())) {
                    throw new Saxon.Api.SaxonApiException("The supplied node has been schema-validated, but the XPath expression was compiled without schema-awareness");
                }
            }
            try {
                dynamicContext.setContextItem(((Saxon.Hej.om.Item)item.Unwrap()));
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Api.XdmItem getContextItem() {
            return Saxon.Api.XdmItem.WrapItem(dynamicContext.getContextItem());
        }
        public void setVariable(Saxon.Api.QName name, Saxon.Api.XdmValue value) {
            Saxon.Ejava.util.Objects.requireNonNull(name, "name");
            Saxon.Ejava.util.Objects.requireNonNull(value, "value");
            Saxon.Hej.om.StructuredQName qn = name.GetStructuredQName();
            Saxon.Hej.sxpath.XPathVariable var = Saxon.Impl.Helpers.MapUtils.Get(declaredVariables, qn);
            if ((var == null)) {
                throw new Saxon.Api.SaxonApiException(new Saxon.Hej.trans.XPathException(("Variable has not been declared: "+name)));
            }
            try {
                dynamicContext.setVariable(var, ((Saxon.Hej.om.GroundedValue)value.Unwrap()));
            } catch (Exception e) when (e is Saxon.Hej.trans.XPathException || e is Saxon.Hej.trans.UncheckedXPathException) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public void setURIResolver(Saxon.Ejavax.xml.transform.URIResolver resolver) {
            dynamicContext.setURIResolver(resolver);
        }
        public Saxon.Ejavax.xml.transform.URIResolver getURIResolver() {
            return dynamicContext.getURIResolver();
        }
        public void setUnparsedTextResolver(Saxon.Hej.lib.UnparsedTextURIResolver resolver) {
            dynamicContext.setUnparsedTextURIResolver(resolver);
        }
        public Saxon.Hej.lib.UnparsedTextURIResolver getUnparsedTextURIResolver() {
            return dynamicContext.getUnparsedTextURIResolver();
        }
        public void setErrorReporter(Saxon.Hej.lib.ErrorReporter reporter) {
            dynamicContext.setErrorReporter(reporter);
        }
        public Saxon.Api.XdmValue evaluate() {
            Saxon.Hej.om.Sequence value;
            try {
                value = Saxon.Hej.om.SequenceTool.toGroundedValue(exp.iterate(dynamicContext));
            } catch (Saxon.Hej.trans.UncheckedXPathException uxe) {
                throw new Saxon.Api.SaxonApiException(uxe);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
            return Saxon.Api.XdmValue.Wrap(value);
        }
        public Saxon.Api.XdmItem evaluateSingle() {
            try {
                Saxon.Hej.om.Item i = exp.evaluateSingle(dynamicContext);
                if ((i == null)) {
                    return null;
                }
                return (Saxon.Api.XdmItem)(Saxon.Api.XdmValue.Wrap(i));
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public System.Collections.Generic.IEnumerator<Saxon.Api.XdmItem> GetEnumerator() {
            try {
                return new Saxon.Hej.s9api.XdmSequenceIterator<Saxon.Api.XdmItem>(exp.iterate(dynamicContext));
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        // Extra method to make the class IEnumerable as well as IEnumerable<T>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        public bool effectiveBooleanValue() {
            try {
                return exp.effectiveBooleanValue(dynamicContext);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Hej.sxpath.XPathDynamicContext getUnderlyingXPathContext() {
            return dynamicContext;
        }
    
    }
}