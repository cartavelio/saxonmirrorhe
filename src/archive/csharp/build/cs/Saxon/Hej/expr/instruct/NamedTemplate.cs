using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class NamedTemplate : Saxon.Hej.expr.instruct.Actor, Saxon.Hej.trace.TraceableComponent {
        private Saxon.Hej.om.StructuredQName templateName;
        private bool _hasRequiredParams;
        private bool bodyIsTailCallReturner;
        private Saxon.Hej.value.SequenceType requiredType;
        private Saxon.Hej.type.ItemType requiredContextItemType = Saxon.Hej.type.AnyItemType.getInstance();
        private bool mayOmitContextItem = true;
        private bool absentFocus = false;
        private System.Collections.Generic.IList<Saxon.Hej.expr.instruct.NamedTemplate.LocalParamInfo> localParamDetails = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.NamedTemplate.LocalParamInfo>(4);
        private Saxon.Hej.expr.elab.PushEvaluator bodyEvaluator;
        public NamedTemplate(Saxon.Hej.om.StructuredQName templateName, Saxon.Hej.Configuration config)  {
            setTemplateName(templateName);
        }
        public void setTemplateName(Saxon.Hej.om.StructuredQName templateName) {
            this.templateName = templateName;
        }
        public void setContextItemRequirements(Saxon.Hej.type.ItemType type, bool mayBeOmitted, bool absentFocus) {
            requiredContextItemType = type;
            mayOmitContextItem = mayBeOmitted;
            this.absentFocus = absentFocus;
        }
        public override Saxon.Hej.trans.SymbolicName getSymbolicName() {
            if ((getTemplateName() == null)) {
                return null;
            } else {
                return new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_TEMPLATE, getTemplateName());
            }
        }
        public override string getTracingTag() {
            return "xsl:template";
        }
        public void gatherProperties(Saxon.Ejava.util.function.BiConsumer<string,System.Object> consumer) {
            consumer("name", getTemplateName());
        }
        public override void setBody(Saxon.Hej.expr.Expression body) {
            base.setBody(body);
        }
        public Saxon.Hej.om.StructuredQName getTemplateName() {
            return templateName;
        }
        public Saxon.Hej.om.StructuredQName getObjectName() {
            return templateName;
        }
        public void setHasRequiredParams(bool has) {
            _hasRequiredParams = has;
        }
        public bool hasRequiredParams() {
            return _hasRequiredParams;
        }
        public void setRequiredType(Saxon.Hej.value.SequenceType type) {
            requiredType = type;
        }
        public Saxon.Hej.value.SequenceType getRequiredType() {
            if ((requiredType == null)) {
                return Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
            } else {
                return requiredType;
            }
        }
        public Saxon.Hej.type.ItemType getRequiredContextItemType() {
            return requiredContextItemType;
        }
        public bool isMayOmitContextItem() {
            return mayOmitContextItem;
        }
        public bool isAbsentFocus() {
            return absentFocus;
        }
        public Saxon.Hej.expr.instruct.NamedTemplate.LocalParamInfo getLocalParamInfo(Saxon.Hej.om.StructuredQName id) {
            System.Collections.Generic.IList<Saxon.Hej.expr.instruct.NamedTemplate.LocalParamInfo> @params = getLocalParamDetails();
            foreach (Saxon.Hej.expr.instruct.NamedTemplate.LocalParamInfo lp in @params) {
                if (lp.name.Equals(id)) {
                    return lp;
                }
            }
            return null;
        }
        public Saxon.Hej.expr.instruct.TailCall expand(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Item contextItem = context.getContextItem();
            if ((contextItem == null)) {
                if (!(mayOmitContextItem)) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("The template requires a context item, but none has been supplied", "XTTE3090");
                    err.setLocation(getLocation());
                    err.setIsTypeError(true);
                    throw err;
                }
            } else {
                Saxon.Hej.type.TypeHierarchy th = context.getConfiguration().getTypeHierarchy();
                if (((requiredContextItemType != Saxon.Hej.type.AnyItemType.getInstance()) && !(requiredContextItemType.matches(contextItem, th)))) {
                    Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.MISC, "context item for the named template", 0);
                    string message = role.composeErrorMessage(requiredContextItemType, contextItem, th);
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message, "XTTE0590");
                    err.setLocation(getLocation());
                    err.setIsTypeError(true);
                    throw err;
                }
                if (absentFocus) {
                    context = context.newMinorContext();
                    context.setCurrentIterator(null);
                }
            }
            lock (this) {
                if ((bodyEvaluator == null)) {
                    bodyEvaluator = getBody().makeElaborator().elaborateForPush();
                }
            }
            return /*Delegate.processLeavingTail()*/bodyEvaluator(output, context);
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("template");
            presenter.emitAttribute("name", getTemplateName());
            explainProperties(presenter);
            presenter.emitAttribute("slots", (""+getStackFrameMap().getNumberOfVariables()));
            if ((getBody() != null)) {
                presenter.setChildRole("body");
                getBody().export(presenter);
            }
            presenter.endElement();
        }
        public void explainProperties(Saxon.Hej.trace.ExpressionPresenter presenter) {
            if ((getRequiredContextItemType() != Saxon.Hej.type.AnyItemType.getInstance())) {
                Saxon.Hej.value.SequenceType st = Saxon.Hej.value.SequenceType.makeSequenceType(getRequiredContextItemType(), Saxon.Hej.expr.StaticProperty.EXACTLY_ONE);
                presenter.emitAttribute("cxt", st.toAlphaCode());
            }
            string flags = "";
            if (mayOmitContextItem) {
                flags = "o";
            }
            if (!(absentFocus)) {
                flags += "s";
            }
            presenter.emitAttribute("flags", flags);
            if ((getRequiredType() != Saxon.Hej.value.SequenceType.ANY_SEQUENCE)) {
                presenter.emitAttribute("as", getRequiredType().toAlphaCode());
            }
            presenter.emitAttribute("line", (getLineNumber()+""));
            presenter.emitAttribute("module", getSystemId());
        }
        public void setLocalParamDetails(System.Collections.Generic.IList<Saxon.Hej.expr.instruct.NamedTemplate.LocalParamInfo> details) {
            localParamDetails = details;
        }
        public System.Collections.Generic.IList<Saxon.Hej.expr.instruct.NamedTemplate.LocalParamInfo> getLocalParamDetails() {
            return localParamDetails;
        }
        public class LocalParamInfo {
            public Saxon.Hej.om.StructuredQName name;
            public Saxon.Hej.value.SequenceType requiredType;
            public bool isRequired;
            public bool isTunnel;
        
        }
    
    }
}