using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class ApplyTemplates : Saxon.Hej.expr.instruct.Instruction, Saxon.Hej.expr.instruct.ITemplateCall, Saxon.Hej.expr.ComponentInvocation {
        private Saxon.Hej.expr.Operand selectOp;
        private Saxon.Hej.expr.Operand separatorOp;
        private Saxon.Hej.expr.instruct.WithParam[] actualParams;
        private Saxon.Hej.expr.instruct.WithParam[] tunnelParams;
        protected internal bool useCurrentMode = false;
        protected internal bool _useTailRecursion = false;
        protected internal Saxon.Hej.trans.Mode mode;
        protected internal bool implicitSelect;
        protected internal bool inStreamableConstruct = false;
        protected internal Saxon.Hej.trans.rules.RuleManager ruleManager;
        private int bindingSlot = -1;
        protected internal ApplyTemplates()  {
        }
        public ApplyTemplates(Saxon.Hej.expr.Expression select, bool useCurrentMode, bool useTailRecursion, bool implicitSelect, bool inStreamableConstruct, Saxon.Hej.trans.Mode mode, Saxon.Hej.trans.rules.RuleManager ruleManager)  {
            selectOp = new Saxon.Hej.expr.Operand(this, select, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            init(select, useCurrentMode, useTailRecursion, mode);
            this.implicitSelect = implicitSelect;
            this.inStreamableConstruct = inStreamableConstruct;
            this.ruleManager = ruleManager;
        }
        protected internal void init(Saxon.Hej.expr.Expression select, bool useCurrentMode, bool useTailRecursion, Saxon.Hej.trans.Mode mode) {
            this.setSelect(select);
            this.useCurrentMode = useCurrentMode;
            this._useTailRecursion = useTailRecursion;
            this.mode = mode;
            adoptChildExpression(select);
        }
        public void setMode(Saxon.Hej.trans.SimpleMode target) {
            this.mode = target;
        }
        public void setSeparatorExpression(Saxon.Hej.expr.Expression separator) {
            separatorOp = new Saxon.Hej.expr.Operand(this, separator, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
        }
        public Saxon.Hej.expr.Expression getSeparatorExpression() {
            return ((separatorOp == null) ? null : separatorOp.getChildExpression());
        }
        public Saxon.Hej.expr.instruct.WithParam[] getActualParams() {
            return actualParams;
        }
        public Saxon.Hej.expr.instruct.WithParam[] getTunnelParams() {
            return tunnelParams;
        }
        public void setActualParams(Saxon.Hej.expr.instruct.WithParam[] @params) {
            actualParams = @params;
        }
        public void setTunnelParams(Saxon.Hej.expr.instruct.WithParam[] @params) {
            tunnelParams = @params;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            System.Collections.Generic.IList<Saxon.Hej.expr.Operand> operanda = new System.Collections.Generic.List<Saxon.Hej.expr.Operand>();
            operanda.Add(selectOp);
            if ((separatorOp != null)) {
                operanda.Add(separatorOp);
            }
            Saxon.Hej.expr.instruct.WithParam.gatherOperands(this, getActualParams(), operanda);
            Saxon.Hej.expr.instruct.WithParam.gatherOperands(this, getTunnelParams(), operanda);
            return operanda;
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.XSL_APPLY_TEMPLATES;
        }
        public override int getImplementationMethod() {
            return (base.getImplementationMethod()|Saxon.Hej.expr.Expression.WATCH_METHOD);
        }
        public override Saxon.Hej.expr.Expression simplify() {
            Saxon.Hej.expr.instruct.WithParam.simplify(getActualParams());
            Saxon.Hej.expr.instruct.WithParam.simplify(getTunnelParams());
            setSelect(getSelect().simplify());
            return this;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.instruct.WithParam.typeCheck(actualParams, visitor, contextInfo);
            Saxon.Hej.expr.instruct.WithParam.typeCheck(tunnelParams, visitor, contextInfo);
            try {
                selectOp.typeCheck(visitor, contextInfo);
            } catch (Saxon.Hej.trans.XPathException e) {
                if (implicitSelect) {
                    string code = e.getErrorCodeLocalPart();
                    if (("XPTY0020".Equals(code) || "XPTY0019".Equals(code))) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot apply-templates to child nodes when the context item is an atomic value");
                        err.setErrorCode("XTTE0510");
                        err.setIsTypeError(true);
                        throw err;
                    } else {
                        if ("XPDY0002".Equals(code)) {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot apply-templates to child nodes when the context item is absent");
                            err.setErrorCode("XTTE0510");
                            err.setIsTypeError(true);
                            throw err;
                        }
                    }
                }
                throw;
            }
            adoptChildExpression(getSelect());
            if (Saxon.Hej.expr.Literal.isEmptySequence(getSelect())) {
                return getSelect();
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.instruct.WithParam.optimize(visitor, actualParams, contextInfo);
            Saxon.Hej.expr.instruct.WithParam.optimize(visitor, tunnelParams, contextInfo);
            selectOp.typeCheck(visitor, contextInfo);
            selectOp.optimize(visitor, contextInfo);
            if (Saxon.Hej.expr.Literal.isEmptySequence(getSelect())) {
                return getSelect();
            }
            return this;
        }
        public override int getIntrinsicDependencies() {
            return (base.getIntrinsicDependencies()|((useCurrentMode ? Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CURRENT_ITEM : 0)));
        }
        public Saxon.Hej.trans.rules.RuleManager getRuleManager() {
            return ruleManager;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.ApplyTemplates a2 = new Saxon.Hej.expr.instruct.ApplyTemplates(getSelect().copy(rebindings), useCurrentMode, _useTailRecursion, implicitSelect, inStreamableConstruct, mode, ruleManager);
            a2.setActualParams(Saxon.Hej.expr.instruct.WithParam.copy(a2, getActualParams(), rebindings));
            a2.setTunnelParams(Saxon.Hej.expr.instruct.WithParam.copy(a2, getTunnelParams(), rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, a2);
            a2.ruleManager = ruleManager;
            if ((separatorOp != null)) {
                a2.setSeparatorExpression(getSeparatorExpression().copy(rebindings));
            }
            return a2;
        }
        public override bool mayCreateNewNodes() {
            return true;
        }
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            apply(output, context, false);
        }
        public override Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            return apply(output, context, _useTailRecursion);
        }
        protected internal Saxon.Hej.om.NodeInfo makeSeparator(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.NodeInfo separator;
            Saxon.Hej.str.UnicodeString sepValue = separatorOp.getChildExpression().evaluateAsString(context);
            Saxon.Hej.tree.util.Orphan orphan = new Saxon.Hej.tree.util.Orphan(context.getConfiguration());
            orphan.setNodeKind(Saxon.Hej.type.Type.TEXT);
            orphan.setStringValue(sepValue);
            separator = orphan;
            return separator;
        }
        protected internal Saxon.Hej.expr.instruct.TailCall apply(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context, bool returnTailCall) {
            Saxon.Hej.expr.Component.M targetMode = getTargetMode(context);
            Saxon.Hej.trans.Mode thisMode = targetMode.getActor();
            Saxon.Hej.om.NodeInfo separator = null;
            if ((separatorOp != null)) {
                separator = makeSeparator(context);
            }
            Saxon.Hej.expr.instruct.ParameterSet @params = assembleParams(context, getActualParams());
            Saxon.Hej.expr.instruct.ParameterSet tunnels = assembleTunnelParams(context, getTunnelParams());
            if (returnTailCall) {
                Saxon.Hej.expr.XPathContextMajor context2 = context.newContext();
                context2.setOrigin(this);
                return new Saxon.Hej.expr.instruct.ApplyTemplates.ApplyTemplatesPackage(Saxon.Hej.expr.parser.ExpressionTool.lazyEvaluate(getSelect(), context, false), targetMode, @params, tunnels, separator, output, context2, getLocation());
            }
            Saxon.Hej.om.SequenceIterator iter = getSelect().iterate(context);
            if ((iter is Saxon.Hej.tree.iter.EmptyIterator)) {
                return null;
            }
            Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
            c2.trackFocus(iter);
            c2.setCurrentMode(targetMode);
            c2.setOrigin(this);
            c2.setCurrentComponent(targetMode);
            if (inStreamableConstruct) {
                c2.setCurrentGroupIterator(null);
            }
            Saxon.Hej.@event.PipelineConfiguration pipe = output.getPipelineConfiguration();
            pipe.setXPathContext(c2);
            try {
                Saxon.Hej.expr.instruct.TailCall tc = thisMode.applyTemplates(@params, tunnels, separator, output, c2, getLocation());
                while ((tc != null)) {
                    tc = tc.processLeavingTail();
                }
            } catch (System.StackOverflowException e) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException.StackOverflow("Too many nested apply-templates calls. The stylesheet may be looping.", Saxon.Hej.trans.SaxonErrorCode.SXLM0001, getLocation());
                err.setXPathContext(context);
                throw err;
            }
            pipe.setXPathContext(context);
            return null;
        }
        public Saxon.Hej.expr.Component.M getTargetMode(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.Component.M targetMode;
            if (useCurrentMode) {
                targetMode = context.getCurrentMode();
            } else {
                if ((bindingSlot>=0)) {
                    targetMode = (Saxon.Hej.expr.Component.M)(context.getTargetComponent(bindingSlot));
                    if ((targetMode.getVisibility() == Saxon.Hej.trans.Visibility.ABSTRACT)) {
                        throw new Saxon.Ejava.lang.AssertionError("Modes cannot be abstract");
                    }
                } else {
                    targetMode = mode.getDeclaringComponent();
                }
            }
            return targetMode;
        }
        public Saxon.Hej.expr.Expression getSelectExpression() {
            return getSelect();
        }
        public bool isImplicitSelect() {
            return implicitSelect;
        }
        public bool useTailRecursion() {
            return _useTailRecursion;
        }
        public bool usesCurrentMode() {
            return useCurrentMode;
        }
        public Saxon.Hej.trans.Mode getMode() {
            return mode;
        }
        public Saxon.Hej.expr.Component getFixedTarget() {
            return mode.getDeclaringComponent();
        }
        public Saxon.Hej.trans.SymbolicName getSymbolicName() {
            return ((mode == null) ? null : mode.getSymbolicName());
        }
        public override Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            Saxon.Hej.expr.parser.PathMap.PathMapNodeSet result = base.addToPathMap(pathMap, pathMapNodeSet);
            result.setReturnable(false);
            return new Saxon.Hej.expr.parser.PathMap.PathMapNodeSet(pathMap.makeNewRoot(this));
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("applyT", this);
            if (((mode != null) && !(mode.isUnnamedMode()))) {
                @out.emitAttribute("mode", mode.getModeName());
            }
            string flags = "";
            if (useCurrentMode) {
                flags = "c";
            }
            if (_useTailRecursion) {
                flags += "t";
            }
            if (implicitSelect) {
                flags += "i";
            }
            if (!(flags.Length==0)) {
                @out.emitAttribute("flags", flags);
            }
            @out.emitAttribute("bSlot", (""+getBindingSlot()));
            @out.setChildRole("select");
            getSelect().export(@out);
            if ((separatorOp != null)) {
                @out.setChildRole("separator");
                getSeparatorExpression().export(@out);
            }
            if ((getActualParams().Length != 0)) {
                Saxon.Hej.expr.instruct.WithParam.exportParameters(getActualParams(), @out, false);
            }
            if ((getTunnelParams().Length != 0)) {
                Saxon.Hej.expr.instruct.WithParam.exportParameters(getTunnelParams(), @out, true);
            }
            @out.endElement();
        }
        public Saxon.Hej.expr.Expression getSelect() {
            return selectOp.getChildExpression();
        }
        public void setSelect(Saxon.Hej.expr.Expression select) {
            selectOp.setChildExpression(select);
        }
        public void setBindingSlot(int slot) {
            bindingSlot = slot;
        }
        public int getBindingSlot() {
            return bindingSlot;
        }
        protected internal class ApplyTemplatesPackage : Saxon.Hej.expr.instruct.TailCall {
            private Saxon.Hej.om.Sequence selectedItems;
            private Saxon.Hej.expr.Component.M targetMode;
            private Saxon.Hej.expr.instruct.ParameterSet @params;
            private Saxon.Hej.expr.instruct.ParameterSet tunnelParams;
            private Saxon.Hej.om.NodeInfo separator;
            private Saxon.Hej.expr.XPathContextMajor evaluationContext;
            private Saxon.Hej.@event.Outputter output;
            private Saxon.Hej.s9api.Location locationId;
            internal ApplyTemplatesPackage(Saxon.Hej.om.Sequence selectedItems, Saxon.Hej.expr.Component.M targetMode, Saxon.Hej.expr.instruct.ParameterSet @params, Saxon.Hej.expr.instruct.ParameterSet tunnelParams, Saxon.Hej.om.NodeInfo separator, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContextMajor context, Saxon.Hej.s9api.Location locationId)  {
                this.selectedItems = selectedItems;
                this.targetMode = targetMode;
                this.@params = @params;
                this.tunnelParams = tunnelParams;
                this.separator = separator;
                this.output = output;
                evaluationContext = context;
                this.locationId = locationId;
            }
            public Saxon.Hej.expr.instruct.TailCall processLeavingTail() {
                evaluationContext.trackFocus(selectedItems.iterate());
                evaluationContext.setCurrentMode(targetMode);
                evaluationContext.setCurrentComponent(targetMode);
                return targetMode.getActor().applyTemplates(@params, tunnelParams, separator, output, evaluationContext, locationId);
            }
        
        }
        public override string getStreamerName() {
            return "ApplyTemplates";
        }
    
    }
}