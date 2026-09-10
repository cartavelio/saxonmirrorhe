using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class TemplateRule : Saxon.Hej.trans.rules.RuleTarget, Saxon.Hej.s9api.Location, Saxon.Hej.expr.ExpressionOwner, Saxon.Hej.trace.TraceableComponent {
        protected internal Saxon.Hej.expr.Expression body;
        protected internal Saxon.Hej.expr.elab.PushEvaluator bodyEvaluator;
        protected internal Saxon.Hej.pattern.Pattern matchPattern;
        private bool _hasRequiredParams;
        private Saxon.Hej.value.SequenceType requiredType;
        private bool declaredStreamable;
        private Saxon.Hej.type.ItemType requiredContextItemType = Saxon.Hej.type.AnyItemType.getInstance();
        private bool absentFocus;
        private Saxon.Hej.expr.instruct.SlotManager stackFrameMap;
        private Saxon.Hej.expr.PackageData packageData;
        private string systemId;
        private int lineNumber;
        private int columnNumber;
        private readonly System.Collections.Generic.IList<Saxon.Hej.trans.rules.Rule> rules = new System.Collections.Generic.List<Saxon.Hej.trans.rules.Rule>();
        protected internal System.Collections.Generic.IList<Saxon.Hej.expr.instruct.TemplateRule> slaveCopies = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.TemplateRule>();
        public TemplateRule()  {
        }
        public void setMatchPattern(Saxon.Hej.pattern.Pattern pattern) {
            matchPattern = pattern;
        }
        public Saxon.Hej.expr.Expression getBody() {
            return body;
        }
        public Saxon.Hej.expr.Expression getChildExpression() {
            return body;
        }
        public Saxon.Hej.s9api.Location getLocation() {
            return this;
        }
        public void gatherProperties(Saxon.Ejava.util.function.BiConsumer<string,System.Object> consumer) {
            consumer("match", getMatchPattern().toShortString());
        }
        public void setContextItemRequirements(Saxon.Hej.type.ItemType type, bool absentFocus) {
            requiredContextItemType = type;
            this.absentFocus = absentFocus;
        }
        public int getComponentKind() {
            return Saxon.Hej.om.StandardNames.XSL_TEMPLATE;
        }
        public Saxon.Hej.pattern.Pattern getMatchPattern() {
            return matchPattern;
        }
        public void setBody(Saxon.Hej.expr.Expression body) {
            this.body = body;
        }
        public void setStackFrameMap(Saxon.Hej.expr.instruct.SlotManager map) {
            stackFrameMap = map;
        }
        public Saxon.Hej.expr.instruct.SlotManager getStackFrameMap() {
            return stackFrameMap;
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
        public void registerRule(Saxon.Hej.trans.rules.Rule rule) {
            rules.Add(rule);
        }
        public System.Collections.Generic.IList<Saxon.Hej.trans.rules.Rule> getRules() {
            return rules;
        }
        public int getContainerGranularity() {
            return 0;
        }
        public Saxon.Hej.expr.PackageData getPackageData() {
            return packageData;
        }
        public void setPackageData(Saxon.Hej.expr.PackageData data) {
            this.packageData = data;
        }
        public string getPublicId() {
            return null;
        }
        public string getSystemId() {
            return systemId;
        }
        public void setSystemId(string id) {
            this.systemId = id;
        }
        public int getLineNumber() {
            return lineNumber;
        }
        public void setLineNumber(int line) {
            this.lineNumber = line;
        }
        public void setColumnNumber(int col) {
            this.columnNumber = col;
        }
        public int getColumnNumber() {
            return columnNumber;
        }
        public Saxon.Hej.s9api.Location saveLocation() {
            return this;
        }
        public Saxon.Hej.type.ItemType getRequiredContextItemType() {
            return requiredContextItemType;
        }
        public bool isAbsentFocus() {
            return absentFocus;
        }
        public System.Collections.Generic.IList<Saxon.Hej.expr.instruct.LocalParam> getLocalParams() {
            System.Collections.Generic.IList<Saxon.Hej.expr.instruct.LocalParam> result = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.LocalParam>();
            gatherLocalParams(getBody(), result);
            return result;
        }
        private static void gatherLocalParams(Saxon.Hej.expr.Expression exp, System.Collections.Generic.IList<Saxon.Hej.expr.instruct.LocalParam> result) {
            if ((exp is Saxon.Hej.expr.instruct.LocalParam)) {
                result.Add((Saxon.Hej.expr.instruct.LocalParam)(exp));
            } else {
                foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                    gatherLocalParams(o.getChildExpression(), result);
                }
            }
        }
        public virtual void prepareInitializer(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.om.StructuredQName modeName) {
        }
        public virtual void initialize() {
        }
        public void apply(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContextMajor context) {
            Saxon.Hej.expr.instruct.TailCall tc = applyLeavingTail(output, context);
            while ((tc != null)) {
                tc = tc.processLeavingTail();
            }
        }
        public Saxon.Hej.expr.instruct.TailCall applyLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.type.TypeHierarchy th = context.getConfiguration().getTypeHierarchy();
            if (((requiredContextItemType != Saxon.Hej.type.AnyItemType.getInstance()) && !(requiredContextItemType.matches(context.getContextItem(), th)))) {
                Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.MISC, "context item for the template rule", 0);
                string message = role.composeErrorMessage(requiredContextItemType, context.getContextItem(), th);
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message, "XTTE0590");
                err.setLocation(this);
                err.setIsTypeError(true);
                throw err;
            }
            if (absentFocus) {
                context = context.newMinorContext();
                context.setCurrentIterator(null);
            }
            try {
                ensureBodyEvaluatorExists();
                return /*Delegate.processLeavingTail()*/bodyEvaluator(output, context);
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                Saxon.Hej.trans.XPathException xe = e.getXPathException();
                xe.maybeSetLocation(this);
                xe.maybeSetContext(context);
                throw xe;
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetLocation(this);
                e.maybeSetContext(context);
                throw;
            } catch (System.Exception e2) {
                string message = (("Internal error evaluating template rule "+(((getLineNumber()>0) ? (" at line "+getLineNumber()) : "")))+(((getSystemId() != null) ? (" in module "+getSystemId()) : "")));
                System.Console.Error.WriteLine(e2);
                throw new System.Exception(message, e2);
            }
        }
        private void ensureBodyEvaluatorExists() {
            ensureBodyEvaluatorExistsCS();
        }
        private void ensureBodyEvaluatorExistsCS() { lock(this) {
            if ((bodyEvaluator == null)) {
                bodyEvaluator = body.makeElaborator().elaborateForPush();
            }
        }}
        public void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        public virtual void setDeclaredStreamable(bool streamable) {
        }
        public virtual bool isDeclaredStreamable() {
            return false;
        }
        public void explainProperties(Saxon.Hej.trace.ExpressionPresenter presenter) {
            if ((getRequiredContextItemType() != Saxon.Hej.type.AnyItemType.getInstance())) {
                Saxon.Hej.value.SequenceType st = Saxon.Hej.value.SequenceType.makeSequenceType(getRequiredContextItemType(), Saxon.Hej.expr.StaticProperty.EXACTLY_ONE);
                presenter.emitAttribute("cxt", st.toAlphaCode());
            }
            string flags = "";
            if (!(absentFocus)) {
                flags += "s";
            }
            presenter.emitAttribute("flags", flags);
            if ((getRequiredType() != Saxon.Hej.value.SequenceType.ANY_SEQUENCE)) {
                presenter.emitAttribute("as", getRequiredType().toAlphaCode());
            }
            presenter.emitAttribute("line", (getLineNumber()+""));
            presenter.emitAttribute("module", getSystemId());
            if (isDeclaredStreamable()) {
                presenter.emitAttribute("streamable", "1");
            }
        }
        public virtual Saxon.Hej.expr.instruct.TemplateRule copy() {
            Saxon.Hej.expr.instruct.TemplateRule tr = new Saxon.Hej.expr.instruct.TemplateRule();
            if (((body == null) || (matchPattern == null))) {
                slaveCopies.Add(tr);
            } else {
                copyTo(tr);
            }
            return tr;
        }
        public void updateSlaveCopies() {
            foreach (Saxon.Hej.expr.instruct.TemplateRule tr in slaveCopies) {
                copyTo(tr);
            }
        }
        protected internal void copyTo(Saxon.Hej.expr.instruct.TemplateRule tr) {
            if ((body != null)) {
                tr.body = body.copy(new Saxon.Hej.expr.parser.RebindingMap());
            }
            if ((matchPattern != null)) {
                tr.matchPattern = matchPattern.copy(new Saxon.Hej.expr.parser.RebindingMap());
            }
            tr._hasRequiredParams = _hasRequiredParams;
            tr.requiredType = requiredType;
            tr.declaredStreamable = declaredStreamable;
            tr.requiredContextItemType = requiredContextItemType;
            tr.absentFocus = absentFocus;
            tr.stackFrameMap = stackFrameMap;
            tr.packageData = packageData;
            tr.systemId = systemId;
            tr.lineNumber = lineNumber;
        }
        public void setChildExpression(Saxon.Hej.expr.Expression expr) {
            setBody(expr);
        }
        public Saxon.Hej.om.StructuredQName getObjectName() {
            return null;
        }
        public string getTracingTag() {
            return "xsl:template";
        }
    
    }
}