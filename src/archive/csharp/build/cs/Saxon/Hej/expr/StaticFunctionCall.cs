using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class StaticFunctionCall : Saxon.Hej.expr.FunctionCall, Saxon.Hej.expr.Callable {
        private readonly Saxon.Hej.om.Function target;
        public StaticFunctionCall(Saxon.Hej.om.Function target, Saxon.Hej.expr.Expression[] arguments)  {
            if ((target.getArity() != arguments.Length)) {
                throw new Saxon.Ejava.lang.IllegalArgumentException((((("Function call to "+target.getFunctionName())+" with wrong number of arguments (")+arguments.Length)+")"));
            }
            this.target = target;
            setOperanda(arguments, target.getOperandRoles());
        }
        public virtual Saxon.Hej.om.Function getTargetFunction() {
            return target;
        }
        public override Saxon.Hej.om.Function getTargetFunction(Saxon.Hej.expr.XPathContext context) {
            return getTargetFunction();
        }
        public override Saxon.Hej.om.StructuredQName getFunctionName() {
            return target.getFunctionName();
        }
        public override bool isCallOn(System.Type function) {
            return function.IsAssignableFrom(target.GetType());
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            checkFunctionCall(target, visitor);
            return base.typeCheck(visitor, contextInfo);
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.Expression[] args = new Saxon.Hej.expr.Expression[getArity()];
            for (int i = 0; (i<args.Length); i++) {
                args[i] = getArg(i).copy(rebindings);
            }
            return new Saxon.Hej.expr.StaticFunctionCall(target, args);
        }
        protected internal override int computeCardinality() {
            return target.getFunctionItemType().getResultType().getCardinality();
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return target.getFunctionItemType().getResultType().getPrimaryType();
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            Saxon.Hej.type.UType result = getItemType().getUType();
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                if ((o.getUsage() == Saxon.Hej.expr.OperandUsage.TRANSMISSION)) {
                    result = result.intersection(o.getChildExpression().getStaticUType(contextItemType));
                }
            }
            return result;
        }
        public virtual Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return target.call(context, arguments);
        }
        public override string getExpressionName() {
            return "staticFunctionCall";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            if ((target is Saxon.Hej.expr.instruct.OriginalFunction)) {
                Saxon.Hej.trace.ExpressionPresenter.ExportOptions options = (Saxon.Hej.trace.ExpressionPresenter.ExportOptions)(@out.getOptions());
                Saxon.Hej.expr.instruct.OriginalFunction pf = (Saxon.Hej.expr.instruct.OriginalFunction)(target);
                @out.startElement("origFC", this);
                @out.emitAttribute("name", pf.getFunctionName());
                @out.emitAttribute("pack", (Saxon.Impl.Helpers.MapUtils.Get(options.packageMap, pf.getComponent().getContainingPackage())+""));
                foreach (Saxon.Hej.expr.Operand o in operands()) {
                    o.getChildExpression().export(@out);
                }
                @out.endElement();
            } else {
                if ((target is Saxon.Hej.expr.UnionCastableFunction)) {
                    Saxon.Hej.type.UnionType targetType = ((Saxon.Hej.expr.UnionConstructorFunction)(target)).getTargetType();
                    @out.startElement("castable", this);
                    if ((targetType is Saxon.Hej.type.LocalUnionType)) {
                        @out.emitAttribute("to", Saxon.Hej.type.AlphaCode.fromItemType(targetType));
                    } else {
                        @out.emitAttribute("as", targetType.toExportString());
                    }
                    @out.emitAttribute("flags", ("u"+((((Saxon.Hej.expr.UnionConstructorFunction)(target)).isAllowEmpty() ? "e" : ""))));
                    foreach (Saxon.Hej.expr.Operand o in operands()) {
                        o.getChildExpression().export(@out);
                    }
                    @out.endElement();
                } else {
                    if ((target is Saxon.Hej.expr.ListCastableFunction)) {
                        @out.startElement("castable", this);
                        @out.emitAttribute("as", ((Saxon.Hej.expr.ListConstructorFunction)(target)).getTargetType().getStructuredQName());
                        @out.emitAttribute("flags", ("l"+((((Saxon.Hej.expr.ListConstructorFunction)(target)).isAllowEmpty() ? "e" : ""))));
                        foreach (Saxon.Hej.expr.Operand o in operands()) {
                            o.getChildExpression().export(@out);
                        }
                        @out.endElement();
                    } else {
                        if ((target is Saxon.Hej.expr.UnionConstructorFunction)) {
                            Saxon.Hej.type.UnionType targetType = ((Saxon.Hej.expr.UnionConstructorFunction)(target)).getTargetType();
                            @out.startElement("cast", this);
                            if ((targetType is Saxon.Hej.type.LocalUnionType)) {
                                @out.emitAttribute("to", Saxon.Hej.type.AlphaCode.fromItemType(targetType));
                            } else {
                                @out.emitAttribute("as", targetType.toExportString());
                            }
                            @out.emitAttribute("flags", ("u"+((((Saxon.Hej.expr.UnionConstructorFunction)(target)).isAllowEmpty() ? "e" : ""))));
                            foreach (Saxon.Hej.expr.Operand o in operands()) {
                                o.getChildExpression().export(@out);
                            }
                            @out.endElement();
                        } else {
                            if ((target is Saxon.Hej.expr.ListConstructorFunction)) {
                                @out.startElement("cast", this);
                                @out.emitAttribute("as", ((Saxon.Hej.expr.ListConstructorFunction)(target)).getTargetType().getStructuredQName());
                                @out.emitAttribute("flags", ("l"+((((Saxon.Hej.expr.ListConstructorFunction)(target)).isAllowEmpty() ? "e" : ""))));
                                foreach (Saxon.Hej.expr.Operand o in operands()) {
                                    o.getChildExpression().export(@out);
                                }
                                @out.endElement();
                            } else {
                                base.export(@out);
                            }
                        }
                    }
                }
            }
        }
    
    }
}