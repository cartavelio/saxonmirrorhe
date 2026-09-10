using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    internal class XPathExecutable {
        private readonly Saxon.Hej.sxpath.XPathExpression exp;
        private readonly Saxon.Hej.s9api.Processor processor;
        private readonly Saxon.Hej.sxpath.IndependentContext env;
        protected internal XPathExecutable(Saxon.Hej.sxpath.XPathExpression exp, Saxon.Hej.s9api.Processor processor, Saxon.Hej.sxpath.IndependentContext env)  {
            this.exp = exp;
            this.processor = processor;
            this.env = env;
        }
        public Saxon.Hej.s9api.XPathSelector load() {
            System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.sxpath.XPathVariable> declaredVariables = new Saxon.Impl.Helpers.OrderedDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.sxpath.XPathVariable>();
            foreach (Saxon.Hej.sxpath.XPathVariable var in env.getExternalVariables()) {
                Saxon.Impl.Helpers.MapUtils.Put(declaredVariables, var.getVariableQName(), var);
            }
            return new Saxon.Hej.s9api.XPathSelector(exp, declaredVariables);
        }
        public Saxon.Api.XdmItemType getResultItemType() {
            Saxon.Hej.type.ItemType it = exp.getInternalExpression().getItemType();
            return new Saxon.Hej.s9api.ConstructedItemType(it, processor.getUnderlyingConfiguration());
        }
        public Saxon.Hej.s9api.OccurrenceIndicator getResultCardinality() {
            int card = exp.getInternalExpression().getCardinality();
            return Saxon.Hej.s9api.OccurrenceIndicator.getOccurrenceIndicator(card);
        }
        public System.Collections.Generic.IEnumerator<Saxon.Api.QName> iterateExternalVariables() {
            System.Collections.Generic.IList<Saxon.Api.QName> list = new System.Collections.Generic.List<Saxon.Api.QName>();
            foreach (Saxon.Hej.sxpath.XPathVariable var in env.getExternalVariables()) {
                list.Add(new Saxon.Api.QName(var.getVariableQName()));
            }
            return list.GetEnumerator();
        }
        public Saxon.Api.XdmItemType getRequiredItemTypeForVariable(Saxon.Api.QName variableName) {
            Saxon.Hej.sxpath.XPathVariable var = env.getExternalVariable(variableName.GetStructuredQName());
            if ((var == null)) {
                return null;
            } else {
                return new Saxon.Hej.s9api.ConstructedItemType(var.getRequiredType().getPrimaryType(), processor.getUnderlyingConfiguration());
            }
        }
        public Saxon.Hej.s9api.OccurrenceIndicator getRequiredCardinalityForVariable(Saxon.Api.QName variableName) {
            Saxon.Hej.sxpath.XPathVariable var = env.getExternalVariable(variableName.GetStructuredQName());
            if ((var == null)) {
                return null;
            } else {
                return Saxon.Hej.s9api.OccurrenceIndicator.getOccurrenceIndicator(var.getRequiredType().getCardinality());
            }
        }
        public Saxon.Hej.sxpath.XPathExpression getUnderlyingExpression() {
            return exp;
        }
        public Saxon.Hej.expr.StaticContext getUnderlyingStaticContext() {
            return env;
        }
    
    }
}