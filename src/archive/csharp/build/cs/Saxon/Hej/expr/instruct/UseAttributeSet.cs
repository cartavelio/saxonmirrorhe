using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    #pragma warning disable 659
    public class UseAttributeSet : Saxon.Hej.expr.instruct.Instruction, Saxon.Hej.expr.ComponentInvocation, Saxon.Hej.expr.ContextOriginator {
        private readonly Saxon.Hej.om.StructuredQName targetName;
        private Saxon.Hej.expr.instruct.AttributeSet target;
        private readonly bool declaredStreamable;
        private int bindingSlot = -1;
        public UseAttributeSet(Saxon.Hej.om.StructuredQName name, bool streamable)  {
            this.targetName = name;
            this.declaredStreamable = streamable;
        }
        public override bool isInstruction() {
            return false;
        }
        public static Saxon.Hej.expr.Expression makeUseAttributeSets(Saxon.Hej.om.StructuredQName[] targets, Saxon.Hej.style.StyleElement instruction) {
            System.Collections.Generic.IList<Saxon.Hej.expr.instruct.UseAttributeSet> list = makeUseAttributeSetInstructions(targets, instruction);
            return makeCompositeExpression(list);
        }
        public static System.Collections.Generic.IList<Saxon.Hej.expr.instruct.UseAttributeSet> makeUseAttributeSetInstructions(Saxon.Hej.om.StructuredQName[] targets, Saxon.Hej.style.StyleElement instruction) {
            System.Collections.Generic.IList<Saxon.Hej.expr.instruct.UseAttributeSet> list = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.UseAttributeSet>(targets.Length);
            foreach (Saxon.Hej.om.StructuredQName name in targets) {
                Saxon.Hej.expr.instruct.UseAttributeSet use = makeUseAttributeSet(name, instruction);
                if ((use != null)) {
                    list.Add(use);
                }
            }
            return list;
        }
        public static Saxon.Hej.expr.Expression makeCompositeExpression(System.Collections.Generic.IList<Saxon.Hej.expr.instruct.UseAttributeSet> targets) {
            if ((targets.Count == 0)) {
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            } else {
                if ((targets.Count == 1)) {
                    return targets[0];
                } else {
                    return new Saxon.Hej.expr.instruct.Block(targets.ToArray());
                }
            }
        }
        private static Saxon.Hej.expr.instruct.UseAttributeSet makeUseAttributeSet(Saxon.Hej.om.StructuredQName name, Saxon.Hej.style.StyleElement instruction) {
            Saxon.Hej.expr.instruct.AttributeSet target;
            if ((name.hasURI(Saxon.Hej.om.NamespaceUri.XSLT) && name.getLocalPart().Equals("original"))) {
                target = (Saxon.Hej.expr.instruct.AttributeSet)(instruction.getXslOriginal(Saxon.Hej.om.StandardNames.XSL_ATTRIBUTE_SET));
            } else {
                Saxon.Hej.expr.Component invokee = instruction.getContainingPackage().getComponent(new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_ATTRIBUTE_SET, name));
                instruction.getPrincipalStylesheetModule().getAttributeSetDeclarations(name);
                if ((invokee == null)) {
                    instruction.compileError(("Unknown attribute set "+name.getEQName()), "XTSE0710");
                    return null;
                }
                target = (Saxon.Hej.expr.instruct.AttributeSet)(invokee.getActor());
            }
            Saxon.Hej.expr.instruct.UseAttributeSet invocation = new Saxon.Hej.expr.instruct.UseAttributeSet(name, target.isDeclaredStreamable());
            invocation.setTarget(target);
            invocation.setBindingSlot(-1);
            invocation.setRetainedStaticContext(instruction.makeRetainedStaticContext());
            return invocation;
        }
        public bool isDeclaredStreamable() {
            return declaredStreamable;
        }
        public void setTarget(Saxon.Hej.expr.instruct.AttributeSet target) {
            this.target = target;
        }
        public void setBindingSlot(int slot) {
            bindingSlot = slot;
        }
        public int getBindingSlot() {
            return bindingSlot;
        }
        public Saxon.Hej.trans.SymbolicName getSymbolicName() {
            return new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_ATTRIBUTE_SET, targetName);
        }
        public Saxon.Hej.expr.instruct.AttributeSet getTargetAttributeSet() {
            return target;
        }
        public Saxon.Hej.expr.Component getFixedTarget() {
            if (((target != null) && (bindingSlot<0))) {
                return target.getDeclaringComponent();
            }
            return null;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return new System.Collections.Generic.List<Saxon.Hej.expr.Operand>();
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            return this;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.UseAttributeSet ua = new Saxon.Hej.expr.instruct.UseAttributeSet(targetName, declaredStreamable);
            ua.setTarget(target);
            ua.setBindingSlot(bindingSlot);
            return ua;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            return this;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.pattern.NodeKindTest.ATTRIBUTE;
        }
        public override int getIntrinsicDependencies() {
            return (Saxon.Hej.expr.StaticProperty.DEPENDS_ON_XSLT_CONTEXT|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_FOCUS);
        }
        public Saxon.Hej.om.StructuredQName getTargetAttributeSetName() {
            return targetName;
        }
        public override string getExpressionName() {
            return "useAS";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("useAS", this);
            @out.emitAttribute("name", targetName);
            @out.emitAttribute("bSlot", (""+getBindingSlot()));
            if (isDeclaredStreamable()) {
                @out.emitAttribute("flags", "s");
            }
            @out.endElement();
        }
        public override bool Equals(System.Object obj) {
            if (!(((obj is Saxon.Hej.expr.instruct.UseAttributeSet)))) {
                return false;
            }
            return targetName.Equals(((Saxon.Hej.expr.instruct.UseAttributeSet)(obj)).targetName);
        }
        protected internal override int computeHashCode() {
            return (0x56423719^targetName.GetHashCode());
        }
        public override string getStreamerName() {
            return "UseAttributeSet";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.instruct.UseAttributeSet.UseAttributeSetElaborator();
        }
        public class UseAttributeSetElaborator : Saxon.Hej.expr.elab.PushElaborator {
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.instruct.UseAttributeSet expr = (Saxon.Hej.expr.instruct.UseAttributeSet)(getExpression());
                return (output, context) => {
                    Saxon.Hej.expr.Component target;
                    if ((expr.bindingSlot<0)) {
                        target = expr.getFixedTarget();
                    } else {
                        target = context.getTargetComponent(expr.bindingSlot);
                        if (target.isHiddenAbstractComponent()) {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Cannot expand an abstract attribute set ("+expr.targetName.getDisplayName())+") with no implementation"), "XTDE3052");
                            err.setLocation(expr.getLocation());
                            throw err;
                        }
                    }
                    if ((target == null)) {
                        throw new Saxon.Ejava.lang.AssertionError(("Failed to locate attribute set "+expr.getTargetAttributeSetName().getEQName()));
                    }
                    Saxon.Hej.expr.instruct.AttributeSet @as = (Saxon.Hej.expr.instruct.AttributeSet)(target.getActor());
                    Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
                    c2.setCurrentComponent(target);
                    c2.setOrigin(expr);
                    Saxon.Hej.expr.instruct.SlotManager sm = @as.getStackFrameMap();
                    if ((sm == null)) {
                        sm = Saxon.Hej.expr.instruct.SlotManager.EMPTY;
                    }
                    c2.openStackFrame(sm);
                    @as.expand(output, c2);
                    return null;
                } ;
            }
        
        }
    
    }
}