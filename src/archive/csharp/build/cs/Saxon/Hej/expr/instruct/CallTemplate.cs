using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class CallTemplate : Saxon.Hej.expr.instruct.Instruction, Saxon.Hej.expr.instruct.ITemplateCall, Saxon.Hej.expr.ComponentInvocation {
        private Saxon.Hej.expr.instruct.NamedTemplate template;
        private readonly Saxon.Hej.om.StructuredQName calledTemplateName;
        private Saxon.Hej.expr.instruct.WithParam[] actualParams = Saxon.Hej.expr.instruct.WithParam.EMPTY_ARRAY;
        private Saxon.Hej.expr.instruct.WithParam[] tunnelParams = Saxon.Hej.expr.instruct.WithParam.EMPTY_ARRAY;
        private bool useTailRecursion;
        private int bindingSlot = -1;
        private readonly bool isWithinDeclaredStreamableConstruct;
        public CallTemplate(Saxon.Hej.expr.instruct.NamedTemplate template, Saxon.Hej.om.StructuredQName calledTemplateName, bool useTailRecursion, bool inStreamable)  {
            this.template = template;
            this.calledTemplateName = calledTemplateName;
            this.useTailRecursion = useTailRecursion;
            this.isWithinDeclaredStreamableConstruct = inStreamable;
        }
        public void setActualParameters(Saxon.Hej.expr.instruct.WithParam[] actualParams, Saxon.Hej.expr.instruct.WithParam[] tunnelParams) {
            this.actualParams = actualParams;
            this.tunnelParams = tunnelParams;
            foreach (Saxon.Hej.expr.instruct.WithParam actualParam in actualParams) {
                adoptChildExpression(actualParam.getSelectExpression());
            }
            foreach (Saxon.Hej.expr.instruct.WithParam tunnelParam in tunnelParams) {
                adoptChildExpression(tunnelParam.getSelectExpression());
            }
        }
        public void setTailRecursive(bool tailRecursive) {
            this.useTailRecursion = tailRecursive;
        }
        public Saxon.Hej.om.StructuredQName getCalledTemplateName() {
            return calledTemplateName;
        }
        public Saxon.Hej.trans.SymbolicName getSymbolicName() {
            return ((calledTemplateName == null) ? null : new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_TEMPLATE, calledTemplateName));
        }
        public Saxon.Hej.expr.Component getTarget() {
            return template.getDeclaringComponent();
        }
        public Saxon.Hej.expr.Component getFixedTarget() {
            Saxon.Hej.expr.Component c = getTarget();
            Saxon.Hej.trans.Visibility v = c.getVisibility();
            if (((v == Saxon.Hej.trans.Visibility.PRIVATE) || (v == Saxon.Hej.trans.Visibility.FINAL))) {
                return c;
            } else {
                return null;
            }
        }
        public Saxon.Hej.expr.instruct.WithParam[] getActualParams() {
            return actualParams;
        }
        public Saxon.Hej.expr.instruct.WithParam[] getTunnelParams() {
            return tunnelParams;
        }
        public void setTargetTemplate(Saxon.Hej.expr.instruct.NamedTemplate target) {
            this.template = target;
        }
        public Saxon.Hej.expr.instruct.NamedTemplate getTargetTemplate() {
            return template;
        }
        public bool usesTailRecursion() {
            return useTailRecursion;
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.XSL_CALL_TEMPLATE;
        }
        public void setBindingSlot(int slot) {
            bindingSlot = slot;
        }
        public int getBindingSlot() {
            return bindingSlot;
        }
        public override Saxon.Hej.expr.Expression simplify() {
            Saxon.Hej.expr.instruct.WithParam.simplify(actualParams);
            Saxon.Hej.expr.instruct.WithParam.simplify(tunnelParams);
            return this;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.instruct.WithParam.typeCheck(actualParams, visitor, contextInfo);
            Saxon.Hej.expr.instruct.WithParam.typeCheck(tunnelParams, visitor, contextInfo);
            bool backwards = visitor.getStaticContext().isInBackwardsCompatibleMode();
            Saxon.Hej.expr.parser.TypeChecker tc = visitor.getConfiguration().getTypeChecker(backwards);
            for (int p = 0; (p<actualParams.Length); p++) {
                Saxon.Hej.expr.instruct.WithParam wp = actualParams[p];
                Saxon.Hej.expr.instruct.NamedTemplate.LocalParamInfo lp = template.getLocalParamInfo(wp.getVariableQName());
                if ((lp != null)) {
                    int pos = p;
                    Saxon.Hej.value.SequenceType req = lp.requiredType;
                    Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => {
                        Saxon.Hej.expr.parser.RoleDiagnostic role0 = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.PARAM, wp.getVariableQName().getDisplayName(), pos);
                        role0.setErrorCode("XTTE0590");
                        return role0;
                    } ;
                    Saxon.Hej.expr.Expression select = tc.staticTypeCheck(wp.getSelectExpression(), req, role, visitor);
                    wp.setSelectExpression(this, select);
                    wp.setTypeChecked(true);
                }
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.expr.instruct.WithParam.optimize(visitor, actualParams, contextItemType);
            Saxon.Hej.expr.instruct.WithParam.optimize(visitor, tunnelParams, contextItemType);
            return this;
        }
        protected internal override int computeCardinality() {
            if ((template == null)) {
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
            } else {
                return template.getRequiredType().getCardinality();
            }
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            if ((template == null)) {
                return Saxon.Hej.type.AnyItemType.getInstance();
            } else {
                return template.getRequiredType().getPrimaryType();
            }
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.CallTemplate ct = new Saxon.Hej.expr.instruct.CallTemplate(template, calledTemplateName, useTailRecursion, isWithinDeclaredStreamableConstruct);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, ct);
            ct.actualParams = Saxon.Hej.expr.instruct.WithParam.copy(ct, actualParams, rebindings);
            ct.tunnelParams = Saxon.Hej.expr.instruct.WithParam.copy(ct, tunnelParams, rebindings);
            return ct;
        }
        public override int getIntrinsicDependencies() {
            return (Saxon.Hej.expr.StaticProperty.DEPENDS_ON_XSLT_CONTEXT|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_FOCUS);
        }
        public override bool mayCreateNewNodes() {
            return true;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            System.Collections.Generic.List<Saxon.Hej.expr.Operand> list = new System.Collections.Generic.List<Saxon.Hej.expr.Operand>(10);
            Saxon.Hej.expr.instruct.WithParam.gatherOperands(this, actualParams, list);
            Saxon.Hej.expr.instruct.WithParam.gatherOperands(this, tunnelParams, list);
            return list;
        }
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.instruct.NamedTemplate t;
            Saxon.Hej.expr.Component target = getFixedTarget();
            if ((bindingSlot>=0)) {
                target = context.getTargetComponent(bindingSlot);
                if (target.isHiddenAbstractComponent()) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Cannot call an abstract template ("+calledTemplateName.getDisplayName())+") with no implementation"), "XTDE3052");
                    err.setLocation(getLocation());
                    throw err;
                }
            }
            t = (Saxon.Hej.expr.instruct.NamedTemplate)(target.getActor());
            Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
            c2.setCurrentComponent(target);
            c2.setOrigin(this);
            c2.openStackFrame(t.getStackFrameMap());
            c2.setLocalParameters(assembleParams(context, actualParams));
            c2.setTunnelParameters(assembleTunnelParams(context, tunnelParams));
            if (isWithinDeclaredStreamableConstruct) {
                c2.setCurrentGroupIterator(null);
            }
            c2.setCurrentMergeGroupIterator(null);
            try {
                Saxon.Hej.expr.instruct.TailCall tc = t.expand(output, c2);
                dispatchTailCall(tc);
            } catch (System.StackOverflowException e) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException.StackOverflow("Too many nested template or function calls. The stylesheet may be looping.", Saxon.Hej.trans.SaxonErrorCode.SXLM0001, getLocation());
                err.setXPathContext(context);
                throw err;
            }
        }
        public override Saxon.Hej.om.StructuredQName getObjectName() {
            return ((template == null) ? null : template.getTemplateName());
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("callT", this);
            string flags = "";
            if (((template != null) && (template.getTemplateName() != null))) {
                @out.emitAttribute("name", template.getTemplateName());
            }
            @out.emitAttribute("bSlot", (""+getBindingSlot()));
            if (isWithinDeclaredStreamableConstruct) {
                flags += "d";
            }
            if (useTailRecursion) {
                flags += "t";
            }
            if (!(flags.Length==0)) {
                @out.emitAttribute("flags", flags);
            }
            if ((actualParams.Length>0)) {
                Saxon.Hej.expr.instruct.WithParam.exportParameters(actualParams, @out, false);
            }
            if ((tunnelParams.Length>0)) {
                Saxon.Hej.expr.instruct.WithParam.exportParameters(tunnelParams, @out, true);
            }
            @out.endElement();
        }
        public class CallTemplatePackage : Saxon.Hej.expr.instruct.TailCall {
            private readonly Saxon.Hej.expr.Component targetComponent;
            private readonly Saxon.Hej.expr.instruct.ParameterSet @params;
            private readonly Saxon.Hej.expr.instruct.ParameterSet tunnelParams;
            private readonly Saxon.Hej.expr.instruct.CallTemplate instruction;
            private readonly Saxon.Hej.@event.Outputter output;
            private readonly Saxon.Hej.expr.XPathContext evaluationContext;
            public CallTemplatePackage(Saxon.Hej.expr.Component targetComponent, Saxon.Hej.expr.instruct.ParameterSet @params, Saxon.Hej.expr.instruct.ParameterSet tunnelParams, Saxon.Hej.expr.instruct.CallTemplate instruction, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext evaluationContext)  {
                this.targetComponent = targetComponent;
                if (!(((targetComponent.getActor() is Saxon.Hej.expr.instruct.NamedTemplate)))) {
                    throw new System.InvalidCastException("Target of call-template must be a named template");
                }
                this.@params = @params;
                this.tunnelParams = tunnelParams;
                this.instruction = instruction;
                this.output = output;
                this.evaluationContext = evaluationContext;
            }
            public Saxon.Hej.expr.instruct.TailCall processLeavingTail() {
                Saxon.Hej.expr.instruct.NamedTemplate template = (Saxon.Hej.expr.instruct.NamedTemplate)(targetComponent.getActor());
                Saxon.Hej.expr.XPathContextMajor c2 = evaluationContext.newContext();
                c2.setCurrentComponent(targetComponent);
                c2.setOrigin(instruction);
                c2.setLocalParameters(@params);
                c2.setTunnelParameters(tunnelParams);
                c2.openStackFrame(template.getStackFrameMap());
                c2.setCurrentMergeGroupIterator(null);
                c2.setCaller(evaluationContext.getMajorContext().getCaller());
                return template.expand(output, c2);
            }
        
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder buff = new Saxon.Ejava.lang.StringBuilder(64);
            buff.append("CallTemplate#");
            if ((template.getObjectName() != null)) {
                buff.append(template.getObjectName().getDisplayName());
            }
            bool first = true;
            foreach (Saxon.Hej.expr.instruct.WithParam p in getActualParams()) {
                buff.append((first ? "(" : ", "));
                buff.append(p.getVariableQName().getDisplayName());
                buff.append("=");
                buff.append(p.getSelectExpression().ToString());
                first = false;
            }
            if (!(first)) {
                buff.append(")");
            }
            return buff.ToString();
        }
        public override string toShortString() {
            return ("CallTemplate#"+template.getObjectName().getDisplayName());
        }
        public override string getStreamerName() {
            return "CallTemplate";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.instruct.CallTemplate.CallTemplateElaborator();
        }
        public class CallTemplateElaborator : Saxon.Hej.expr.elab.PushElaborator {
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.instruct.CallTemplate expr = (Saxon.Hej.expr.instruct.CallTemplate)(getExpression());
                int bindingSlot = expr.getBindingSlot();
                if (expr.useTailRecursion) {
                    return (output, context) => {
                        Saxon.Hej.expr.Component targetComponent;
                        if ((bindingSlot>=0)) {
                            targetComponent = context.getTargetComponent(bindingSlot);
                        } else {
                            targetComponent = expr.getFixedTarget();
                        }
                        if ((targetComponent == null)) {
                            throw new Saxon.Hej.trans.XPathException("Internal Saxon error: No binding available for call-template instruction", Saxon.Hej.trans.SaxonErrorCode.SXPK0001, expr.getLocation());
                        }
                        if (targetComponent.isHiddenAbstractComponent()) {
                            throw new Saxon.Hej.trans.XPathException((("Cannot call an abstract template ("+expr.calledTemplateName.getDisplayName())+") with no implementation"), "XTDE3052", expr.getLocation());
                        }
                        Saxon.Hej.expr.instruct.ParameterSet @params = assembleParams(context, expr.actualParams);
                        Saxon.Hej.expr.instruct.ParameterSet tunnels = assembleTunnelParams(context, expr.tunnelParams);
                        if ((@params == null)) {
                            @params = Saxon.Hej.expr.instruct.ParameterSet.EMPTY_PARAMETER_SET;
                        }
                        Saxon.Ejava.util.Arrays.fill(context.getStackFrame().getStackFrameValues(), null);
                        return new Saxon.Hej.expr.instruct.CallTemplate.CallTemplatePackage(targetComponent, @params, tunnels, expr, output, context);
                    } ;
                } else {
                    return (output, context) => {
                        Saxon.Hej.expr.instruct.NamedTemplate t;
                        Saxon.Hej.expr.Component target = expr.getFixedTarget();
                        if ((bindingSlot>=0)) {
                            target = context.getTargetComponent(bindingSlot);
                            if (target.isHiddenAbstractComponent()) {
                                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Cannot call an abstract template ("+expr.calledTemplateName.getDisplayName())+") with no implementation"), "XTDE3052");
                                err.setLocation(expr.getLocation());
                                throw err;
                            }
                        }
                        t = (Saxon.Hej.expr.instruct.NamedTemplate)(target.getActor());
                        Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
                        c2.setCurrentComponent(target);
                        c2.setOrigin(expr);
                        c2.openStackFrame(t.getStackFrameMap());
                        c2.setLocalParameters(assembleParams(context, expr.actualParams));
                        c2.setTunnelParameters(assembleTunnelParams(context, expr.tunnelParams));
                        if (expr.isWithinDeclaredStreamableConstruct) {
                            c2.setCurrentGroupIterator(null);
                        }
                        c2.setCurrentMergeGroupIterator(null);
                        try {
                            Saxon.Hej.expr.instruct.TailCall tc = t.expand(output, c2);
                            dispatchTailCall(tc);
                        } catch (System.StackOverflowException e) {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException.StackOverflow("Too many nested template or function calls. The stylesheet may be looping.", Saxon.Hej.trans.SaxonErrorCode.SXLM0001, expr.getLocation());
                            err.setXPathContext(context);
                            throw err;
                        }
                        return null;
                    } ;
                }
            }
        
        }
    
    }
}