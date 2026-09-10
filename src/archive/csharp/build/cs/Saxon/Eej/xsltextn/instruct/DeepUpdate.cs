using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.xsltextn.instruct {
    public class DeepUpdate : Saxon.Hej.expr.instruct.Instruction {
        private readonly Saxon.Hej.expr.Operand rootOp;
        private readonly Saxon.Hej.expr.Operand selectOp;
        private readonly Saxon.Hej.expr.Operand actionOp;
        public DeepUpdate(Saxon.Hej.expr.Expression root, Saxon.Hej.expr.Expression select, Saxon.Hej.expr.Expression action)  {
            rootOp = new Saxon.Hej.expr.Operand(this, root, Saxon.Hej.expr.OperandRole.INSPECT);
            selectOp = new Saxon.Hej.expr.Operand(this, select, Saxon.Hej.expr.OperandRole.FOCUS_CONTROLLED_ACTION);
            actionOp = new Saxon.Hej.expr.Operand(this, action, Saxon.Hej.expr.OperandRole.FOCUS_CONTROLLED_ACTION);
        }
        public Saxon.Hej.expr.Expression getRootExpression() {
            return rootOp.getChildExpression();
        }
        public Saxon.Hej.expr.Expression getSelectExpression() {
            return selectOp.getChildExpression();
        }
        public Saxon.Hej.expr.Expression getActionExpression() {
            return actionOp.getChildExpression();
        }
        public void setRootExpression(Saxon.Hej.expr.Expression expr) {
            rootOp.setChildExpression(expr);
        }
        public void setSelectExpression(Saxon.Hej.expr.Expression expr) {
            selectOp.setChildExpression(expr);
        }
        public void setActionExpression(Saxon.Hej.expr.Expression expr) {
            actionOp.setChildExpression(expr);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandList(rootOp, selectOp, actionOp);
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.SAXON_DEEP_UPDATE;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            rootOp.typeCheck(visitor, contextInfo);
            Saxon.Hej.expr.parser.ContextItemStaticInfo ciso2 = new Saxon.Hej.expr.parser.ContextItemStaticInfo(getRootExpression().getItemType(), false);
            selectOp.typeCheck(visitor, ciso2);
            Saxon.Hej.expr.parser.ContextItemStaticInfo ciso3 = new Saxon.Hej.expr.parser.ContextItemStaticInfo(getSelectExpression().getItemType(), false);
            actionOp.typeCheck(visitor, ciso3);
            Saxon.Hej.expr.parser.TypeChecker tc = visitor.getConfiguration().getTypeChecker(false);
            Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "deep-update/root", 0);
            Saxon.Hej.value.SequenceType required = Saxon.Hej.value.SequenceType.FUNCTION_ITEM_SEQUENCE;
            setRootExpression(tc.staticTypeCheck(getRootExpression(), required, role, visitor));
            role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "deep-update/select", 0);
            setSelectExpression(tc.staticTypeCheck(getSelectExpression(), required, role, visitor));
            role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "deep-update/action", 0);
            setActionExpression(tc.staticTypeCheck(getActionExpression(), Saxon.Hej.value.SequenceType.ANY_SEQUENCE, role, visitor));
            return this;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Eej.xsltextn.instruct.DeepUpdate exp = new Saxon.Eej.xsltextn.instruct.DeepUpdate(getRootExpression().copy(rebindings), getSelectExpression().copy(rebindings), getActionExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getRootExpression().getItemType();
        }
        public override Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Item root;
            Saxon.Hej.om.SequenceIterator rootIterator = getRootExpression().iterate(context);
            Saxon.Hej.om.SequenceIterator mappedRootIterator = Saxon.Hej.expr.MappingIterator.map(rootIterator, item => Saxon.Hej.tree.iter.SingletonIterator.makeIterator(makeRootWithPedigree(item)));
            Saxon.Hej.expr.XPathContext c2 = context.newMinorContext();
            Saxon.Hej.om.FocusIterator trackingIterator = c2.trackFocus(mappedRootIterator);
            while (((root = trackingIterator.next()) != null)) {
                Saxon.Hej.om.Item replacedRoot = processRootItem(root, c2);
                output.append(replacedRoot);
            }
            return null;
        }
        private static Saxon.Hej.om.Item makeRootWithPedigree(Saxon.Hej.om.Item root) {
            if ((root is Saxon.Hej.ma.map.MapItem)) {
                return new Saxon.Eej.xsltextn.pedigree.PedigreeMapItem((Saxon.Hej.ma.map.MapItem)(root));
            } else {
                if ((root is Saxon.Hej.ma.arrays.ArrayItem)) {
                    return new Saxon.Eej.xsltextn.pedigree.PedigreeArrayItem((Saxon.Hej.ma.arrays.ArrayItem)(root));
                } else {
                    System.Diagnostics.Debug.Assert(false);
                    return null;
                }
            }
        }
        private Saxon.Hej.om.Item processRootItem(Saxon.Hej.om.Item root, Saxon.Hej.expr.XPathContext c2) {
            Saxon.Ejava.util.IdentityHashMap<Saxon.Hej.om.Item,Saxon.Hej.om.Item> substitutions = new Saxon.Ejava.util.IdentityHashMap<Saxon.Hej.om.Item, Saxon.Hej.om.Item>();
            Saxon.Hej.expr.XPathContext c3 = c2.newMinorContext();
            Saxon.Hej.om.FocusIterator selectIterator = c3.trackFocus(getSelectExpression().iterate(c2));
            Saxon.Hej.om.Item selected;
            while (((selected = selectIterator.next()) != null)) {
                if (!(((selected is Saxon.Eej.xsltextn.pedigree.PedigreeValue)))) {
                    if ((selected is Saxon.Hej.ma.map.MapItem)) {
                        throw new Saxon.Hej.trans.XPathException("saxon:deep-update: selected item is a map with no pedigree");
                    } else {
                        if ((selected is Saxon.Hej.ma.arrays.ArrayItem)) {
                            throw new Saxon.Hej.trans.XPathException("saxon:deep-update: selected item is an array with no pedigree");
                        } else {
                            throw new Saxon.Hej.trans.XPathException((((("saxon:deep-update: selected value "+Saxon.Hej.trans.Err.depict(selected))+" is an instance of ")+Saxon.Hej.om.SequenceTool.getItemType(selected, c2.getConfiguration().getTypeHierarchy()))+" (expected a map/array with pedigree)"));
                        }
                    }
                }
                Saxon.Hej.om.SequenceIterator actionIter = getActionExpression().iterate(c3);
                Saxon.Hej.om.GroundedValue actionVal = Saxon.Hej.om.SequenceTool.toGroundedValue(actionIter);
                Saxon.Eej.xsltextn.pedigree.Pedigree pedigree = ((Saxon.Eej.xsltextn.pedigree.PedigreeValue)(selected)).getPedigree();
                Saxon.Eej.xsltextn.pedigree.PedigreeValue parent = pedigree.getContainer();
                Saxon.Hej.om.GroundedValue val = actionVal;
                while ((parent != null)) {
                    Saxon.Hej.om.Item latestParent = (Saxon.Hej.om.Item)(parent);
                    if (substitutions.ContainsKey((Saxon.Hej.om.Item)(parent))) {
                        latestParent = Saxon.Impl.Helpers.MapUtils.Get(substitutions, (Saxon.Hej.om.Item)(parent));
                    }
                    if ((latestParent is Saxon.Hej.ma.arrays.ArrayItem)) {
                        val = ((Saxon.Hej.ma.arrays.ArrayItem)(latestParent)).put(pedigree.getIndex(), val);
                    } else {
                        if ((latestParent is Saxon.Hej.ma.map.MapItem)) {
                            val = ((Saxon.Hej.ma.map.MapItem)(latestParent)).addEntry(pedigree.getKey(), val);
                        } else {
                            System.Diagnostics.Debug.Assert(false);
                        }
                    }
                    Saxon.Impl.Helpers.MapUtils.Put(substitutions, (Saxon.Hej.om.Item)(parent), (Saxon.Hej.om.Item)(val));
                    Saxon.Impl.Helpers.MapUtils.Put(substitutions, latestParent, (Saxon.Hej.om.Item)(val));
                    pedigree = parent.getPedigree();
                    parent = pedigree.getContainer();
                }
                root = (Saxon.Hej.om.Item)(val);
            }
            return root;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("deepUpdate", this);
            @out.setChildRole("root");
            getRootExpression().export(@out);
            @out.setChildRole("select");
            getSelectExpression().export(@out);
            @out.setChildRole("action");
            getActionExpression().export(@out);
            @out.endElement();
        }
    
    }
}