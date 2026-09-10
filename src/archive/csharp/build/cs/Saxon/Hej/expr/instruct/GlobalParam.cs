using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public sealed class GlobalParam : Saxon.Hej.expr.instruct.GlobalVariable {
        private bool implicitlyRequired;
        public GlobalParam()  {
        }
        public void setImplicitlyRequiredParam(bool requiredParam) {
            this.implicitlyRequired = requiredParam;
        }
        public bool isImplicitlyRequiredParam() {
            return this.implicitlyRequired;
        }
        public override string getTracingTag() {
            return "xsl:param";
        }
        public override Saxon.Hej.om.GroundedValue evaluateVariable(Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.Component target) {
            Saxon.Hej.Controller controller = context.getController();
            System.Diagnostics.Debug.Assert((controller != null));
            Saxon.Hej.expr.instruct.Bindery b = controller.getBindery(getPackageData());
            Saxon.Hej.om.GroundedValue val = b.getGlobalVariableValue(this);
            if ((val != null)) {
                if ((val is Saxon.Hej.expr.instruct.Bindery.FailureValue)) {
                    throw ((Saxon.Hej.expr.instruct.Bindery.FailureValue)(val)).getObject();
                }
                return val;
            }
            val = controller.getConvertedParameter(getVariableQName(), getRequiredType(), context);
            if ((val != null)) {
                return b.saveGlobalVariableValue(this, val);
            }
            if (isRequiredParam()) {
                Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException(("No value supplied for required parameter $"+getVariableQName().getDisplayName()));
                e.setXPathContext(context);
                e.setLocator(this);
                e.setErrorCode((getPackageData().isXSLT() ? "XTDE0050" : "XPDY0002"));
                throw e;
            } else {
                if (isImplicitlyRequiredParam()) {
                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException((("A value must be supplied for parameter $"+getVariableQName().getDisplayName())+" because there is no default value for the required type"));
                    e.setXPathContext(context);
                    e.setLocator(this);
                    e.setErrorCode("XTDE0700");
                    throw e;
                }
            }
            return actuallyEvaluate(context, target);
        }
        public override Saxon.Hej.om.GroundedValue evaluateVariable(Saxon.Hej.expr.XPathContext context) {
            return evaluateVariable(context, null);
        }
        protected internal override string getFlags() {
            string f = base.getFlags();
            if (isImplicitlyRequiredParam()) {
                f += "i";
            }
            return f;
        }
    
    }
}