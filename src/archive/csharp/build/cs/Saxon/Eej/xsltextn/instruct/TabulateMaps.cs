using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.xsltextn.instruct {
    public class TabulateMaps : Saxon.Hej.expr.instruct.Instruction {
        private readonly Saxon.Hej.expr.Operand rootOp;
        private readonly Saxon.Hej.expr.Operand selectOp;
        public TabulateMaps(Saxon.Hej.expr.Expression root, Saxon.Hej.expr.Expression select)  {
            rootOp = new Saxon.Hej.expr.Operand(this, root, Saxon.Hej.expr.OperandRole.INSPECT);
            selectOp = new Saxon.Hej.expr.Operand(this, select, Saxon.Hej.expr.OperandRole.FOCUS_CONTROLLED_ACTION);
        }
        public Saxon.Hej.expr.Expression getRootExpression() {
            return rootOp.getChildExpression();
        }
        public Saxon.Hej.expr.Expression getSelectExpression() {
            return selectOp.getChildExpression();
        }
        public void setRootExpression(Saxon.Hej.expr.Expression expr) {
            rootOp.setChildExpression(expr);
        }
        public void setSelectExpression(Saxon.Hej.expr.Expression expr) {
            selectOp.setChildExpression(expr);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandList(rootOp, selectOp);
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.SAXON_TABULATE_MAPS;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            rootOp.typeCheck(visitor, contextInfo);
            Saxon.Hej.expr.parser.ContextItemStaticInfo ciso2 = new Saxon.Hej.expr.parser.ContextItemStaticInfo(getRootExpression().getItemType(), false);
            selectOp.typeCheck(visitor, ciso2);
            Saxon.Hej.expr.parser.TypeChecker tc = visitor.getConfiguration().getTypeChecker(false);
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "saxon:tabulate-maps/root", 0);
            Saxon.Hej.value.SequenceType required = Saxon.Hej.value.SequenceType.FUNCTION_ITEM_SEQUENCE;
            setRootExpression(tc.staticTypeCheck(getRootExpression(), required, role, visitor));
            required = Saxon.Hej.ma.map.MapType.SEQUENCE_OF_MAPS;
            role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "saxon:tabulate-maps/select", 0);
            setSelectExpression(tc.staticTypeCheck(getSelectExpression(), required, role, visitor));
            return this;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Eej.xsltextn.instruct.TabulateMaps exp = new Saxon.Eej.xsltextn.instruct.TabulateMaps(getRootExpression().copy(rebindings), getSelectExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE;
        }
        private void processRootItem(Saxon.Hej.@event.Outputter output, Saxon.Hej.om.Item root, Saxon.Hej.expr.XPathContext c2) {
            Saxon.Hej.om.FocusIterator selectIterator = Saxon.Hej.om.SequenceTool.focusTracker(getSelectExpression().iterate(c2));
            Saxon.Hej.om.Item selected;
            while (((selected = selectIterator.next()) != null)) {
                if (!(((selected is Saxon.Eej.xsltextn.pedigree.PedigreeMapItem)))) {
                    if ((selected is Saxon.Hej.ma.map.MapItem)) {
                        throw new Saxon.Hej.trans.XPathException("saxon:tabulate: selected item is a map with no pedigree", Saxon.Hej.trans.SaxonErrorCode.SXTM0001);
                    } else {
                        throw new Saxon.Hej.trans.XPathException("saxon:tabulate: selected item is not a map", Saxon.Hej.trans.SaxonErrorCode.SXTM0001);
                    }
                }
                Saxon.Hej.ma.map.MapItem selectedMap = (Saxon.Hej.ma.map.MapItem)(selected);
                Saxon.Eej.xsltextn.pedigree.Pedigree pedigree = ((Saxon.Eej.xsltextn.pedigree.PedigreeValue)(selected)).getPedigree();
                Saxon.Eej.xsltextn.pedigree.PedigreeValue parent = pedigree.getContainer();
                int level = 0;
                System.Collections.Generic.LinkedList<Saxon.Hej.value.AtomicValue> keys = new System.Collections.Generic.LinkedList<Saxon.Hej.value.AtomicValue>();
                while ((parent != null)) {
                    level++;
                    Saxon.Hej.om.Item latestParent = (Saxon.Hej.om.Item)(parent);
                    if ((latestParent is Saxon.Hej.ma.arrays.ArrayItem)) {
                        keys.AddFirst(new Saxon.Hej.value.Int64Value((pedigree.getIndex()+1)));
                    } else {
                        if ((latestParent is Saxon.Hej.ma.map.MapItem)) {
                            keys.AddFirst(pedigree.getKey());
                            foreach (Saxon.Hej.ma.map.KeyValuePair kvp in ((Saxon.Hej.ma.map.MapItem)(latestParent)).keyValuePairs()) {
                                Saxon.Hej.value.AtomicValue k = kvp.key;
                                if (!(k.isIdentical(pedigree.getKey()))) {
                                    Saxon.Hej.om.GroundedValue value = selectedMap.get(k);
                                    if ((value == null)) {
                                        selectedMap = selectedMap.addEntry(k, kvp.value);
                                    } else {
                                        if ((k is Saxon.Hej.value.StringValue)) {
                                            string ks = k.getStringValue();
                                            for (int suffix = 1; (suffix<System.Int32.MaxValue); suffix++) {
                                                ks = (ks+((suffix+"")));
                                                Saxon.Hej.value.StringValue sv = new Saxon.Hej.value.StringValue(ks);
                                                if ((selectedMap.get(sv) == null)) {
                                                    selectedMap = selectedMap.addEntry(sv, kvp.value);
                                                    break ;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        } else {
                            System.Diagnostics.Debug.Assert(false);
                        }
                    }
                    pedigree = parent.getPedigree();
                    parent = pedigree.getContainer();
                }
                string keysKey = "_keys";
                while ((selectedMap.get(new Saxon.Hej.value.StringValue(keysKey)) != null)) {
                    keysKey = (keysKey+"_");
                }
                System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> copy = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>(keys);
                selectedMap = selectedMap.addEntry(new Saxon.Hej.value.StringValue(keysKey), new Saxon.Hej.ma.arrays.SimpleArrayItem(copy));
                output.append(selectedMap);
            }
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("tabulate", this);
            getRootExpression().export(@out);
            getSelectExpression().export(@out);
            @out.endElement();
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Eej.xsltextn.instruct.TabulateMaps.TabulateMapsElaborator();
        }
        private class TabulateMapsElaborator : Saxon.Hej.expr.elab.PushElaborator {
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Eej.xsltextn.instruct.TabulateMaps expr = (Saxon.Eej.xsltextn.instruct.TabulateMaps)(getExpression());
                Saxon.Hej.expr.elab.PullEvaluator rootPull = expr.getRootExpression().makeElaborator().elaborateForPull();
                return (output, context) => {
                    Saxon.Hej.om.Item root;
                    Saxon.Hej.om.SequenceIterator rootIterator = /*Delegate.iterate()*/rootPull(context);
                    Saxon.Hej.om.SequenceIterator mappedRootIterator = Saxon.Hej.expr.MappingIterator.map(rootIterator, item => Saxon.Hej.tree.iter.SingletonIterator.makeIterator(/*134*/Saxon.Eej.functions.extfn.WithPedigree.makeRootWithPedigree(item)));
                    Saxon.Hej.expr.XPathContext c2 = context.newMinorContext();
                    Saxon.Hej.om.FocusIterator trackingIterator = c2.trackFocus(mappedRootIterator);
                    while (((root = trackingIterator.next()) != null)) {
                        expr.processRootItem(output, root, c2);
                    }
                    return null;
                } ;
            }
        
        }
    
    }
}