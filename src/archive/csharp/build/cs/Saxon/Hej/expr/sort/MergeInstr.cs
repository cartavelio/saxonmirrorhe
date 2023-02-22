using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class MergeInstr : Saxon.Hej.expr.instruct.Instruction {
        protected internal Saxon.Hej.expr.sort.MergeInstr.MergeSource[] mergeSources;
        private Saxon.Hej.expr.Operand actionOp;
        protected internal Saxon.Hej.expr.sort.AtomicComparer[] comparators;
        public class MergeSource {
            private readonly Saxon.Hej.expr.sort.MergeInstr instruction;
            public Saxon.Hej.s9api.Location location;
            public Saxon.Hej.expr.Operand forEachItemOp = null;
            public Saxon.Hej.expr.Operand forEachStreamOp = null;
            public Saxon.Hej.expr.Operand rowSelectOp = null;
            public string sourceName = null;
            public Saxon.Hej.expr.sort.SortKeyDefinitionList mergeKeyDefinitions = null;
            public string baseURI = null;
            public int validation;
            public Saxon.Hej.type.SchemaType schemaType;
            public bool streamable;
            public System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> accumulators;
            public System.Object invertedAction;
            public MergeSource(Saxon.Hej.expr.sort.MergeInstr mi)  {
                this.instruction = mi;
            }
            public MergeSource(Saxon.Hej.expr.sort.MergeInstr instruction, Saxon.Hej.expr.Expression forEachItem, Saxon.Hej.expr.Expression forEachStream, Saxon.Hej.expr.Expression rSelect, string name, Saxon.Hej.expr.sort.SortKeyDefinitionList sKeys, string baseURI)  {
                this.instruction = instruction;
                if ((forEachItem != null)) {
                    initForEachItem(instruction, forEachItem);
                }
                if ((forEachStream != null)) {
                    initForEachStream(instruction, forEachStream);
                }
                if ((rSelect != null)) {
                    initRowSelect(instruction, rSelect);
                }
                this.sourceName = name;
                this.mergeKeyDefinitions = sKeys;
                this.baseURI = baseURI;
            }
            public void initForEachItem(Saxon.Hej.expr.sort.MergeInstr instruction, Saxon.Hej.expr.Expression forEachItem) {
                forEachItemOp = new Saxon.Hej.expr.Operand(instruction, forEachItem, Saxon.Hej.expr.OperandRole.INSPECT);
            }
            public void initForEachStream(Saxon.Hej.expr.sort.MergeInstr instruction, Saxon.Hej.expr.Expression forEachStream) {
                forEachStreamOp = new Saxon.Hej.expr.Operand(instruction, forEachStream, Saxon.Hej.expr.OperandRole.INSPECT);
            }
            public void initRowSelect(Saxon.Hej.expr.sort.MergeInstr instruction, Saxon.Hej.expr.Expression rowSelect) {
                rowSelectOp = new Saxon.Hej.expr.Operand(instruction, rowSelect, ROW_SELECT);
            }
            public void setStreamable(bool streamable) {
                this.streamable = streamable;
                if ((streamable && instruction.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.STREAMING_FALLBACK))) {
                    this.streamable = false;
                    Saxon.Hej.expr.Expression select = rowSelectOp.getChildExpression();
                    rowSelectOp.setChildExpression(Saxon.Hej.functions.SystemFunction.makeCall("snapshot", select.getRetainedStaticContext(), select));
                }
            }
            public Saxon.Hej.expr.sort.MergeInstr.MergeSource copyMergeSource(Saxon.Hej.expr.sort.MergeInstr newInstr, Saxon.Hej.expr.parser.RebindingMap rebindings) {
                Saxon.Hej.expr.sort.SortKeyDefinition[] newKeyDef = new Saxon.Hej.expr.sort.SortKeyDefinition[mergeKeyDefinitions.size()];
                for (int i = 0; (i<mergeKeyDefinitions.size()); i++) {
                    newKeyDef[i] = mergeKeyDefinitions.getSortKeyDefinition(i).copy(rebindings);
                }
                Saxon.Hej.expr.sort.MergeInstr.MergeSource ms = new Saxon.Hej.expr.sort.MergeInstr.MergeSource(newInstr, copy(getForEachItem(), rebindings), copy(getForEachSource(), rebindings), copy(getRowSelect(), rebindings), sourceName, new Saxon.Hej.expr.sort.SortKeyDefinitionList(newKeyDef), baseURI);
                ms.validation = validation;
                ms.schemaType = schemaType;
                ms.streamable = streamable;
                ms.location = location;
                return ms;
            }
            private static Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.parser.RebindingMap rebindings) {
                return ((exp == null) ? null : exp.copy(rebindings));
            }
            public Saxon.Hej.expr.Expression getForEachItem() {
                return ((forEachItemOp == null) ? null : forEachItemOp.getChildExpression());
            }
            public void setForEachItem(Saxon.Hej.expr.Expression forEachItem) {
                if ((forEachItem != null)) {
                    forEachItemOp.setChildExpression(forEachItem);
                }
            }
            public Saxon.Hej.expr.Expression getForEachSource() {
                return ((forEachStreamOp == null) ? null : forEachStreamOp.getChildExpression());
            }
            public void setForEachStream(Saxon.Hej.expr.Expression forEachStream) {
                if ((forEachStream != null)) {
                    forEachStreamOp.setChildExpression(forEachStream);
                }
            }
            public Saxon.Hej.expr.Expression getRowSelect() {
                return rowSelectOp.getChildExpression();
            }
            public void setRowSelect(Saxon.Hej.expr.Expression rowSelect) {
                rowSelectOp.setChildExpression(rowSelect);
            }
            public Saxon.Hej.expr.sort.SortKeyDefinitionList getMergeKeyDefinitionSet() {
                return mergeKeyDefinitions;
            }
            public void setMergeKeyDefinitionSet(Saxon.Hej.expr.sort.SortKeyDefinitionList keys) {
                mergeKeyDefinitions = keys;
            }
            public void prepareForStreaming() {
                if (streamable) {
                    /*134*/Saxon.Eej.ee.stream.adjunct.MergeInstrAdjunct.checkStreamability(this, instruction.getConfiguration());
                }
            }
        
        }
        public MergeInstr()  {
        }
        public Saxon.Hej.expr.sort.MergeInstr init(Saxon.Hej.expr.sort.MergeInstr.MergeSource[] mSources, Saxon.Hej.expr.Expression action) {
            actionOp = new Saxon.Hej.expr.Operand(this, action, Saxon.Hej.expr.OperandRole.FOCUS_CONTROLLED_ACTION);
            this.mergeSources = mSources;
            foreach (Saxon.Hej.expr.sort.MergeInstr.MergeSource mSource in mSources) {
                adoptChildExpression(mSource.getForEachItem());
                adoptChildExpression(mSource.getForEachSource());
                adoptChildExpression(mSource.getRowSelect());
            }
            adoptChildExpression(action);
            return this;
        }
        public Saxon.Hej.expr.sort.MergeInstr.MergeSource[] getMergeSources() {
            return mergeSources;
        }
        public void setAction(Saxon.Hej.expr.Expression action) {
            actionOp.setChildExpression(action);
        }
        public Saxon.Hej.expr.Expression getAction() {
            return actionOp.getChildExpression();
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.XSL_MERGE;
        }
        public override void checkPermittedContents(Saxon.Hej.type.SchemaType parentType, bool whole) {
            getAction().checkPermittedContents(parentType, false);
        }
        public override bool allowExtractingCommonSubexpressions() {
            return false;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getAction().getItemType();
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            Saxon.Hej.expr.parser.TypeChecker tc = config.getTypeChecker(false);
            Saxon.Hej.type.ItemType inputType = null;
            foreach (Saxon.Hej.expr.sort.MergeInstr.MergeSource mergeSource in mergeSources) {
                Saxon.Hej.expr.parser.ContextItemStaticInfo rowContextItemType = contextInfo;
                if ((mergeSource.getForEachItem() != null)) {
                    mergeSource.forEachItemOp.typeCheck(visitor, contextInfo);
                    rowContextItemType = config.makeContextItemStaticInfo(mergeSource.getForEachItem().getItemType(), false);
                } else {
                    if ((mergeSource.getForEachSource() != null)) {
                        mergeSource.forEachStreamOp.typeCheck(visitor, contextInfo);
                        Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "xsl:merge/for-each-source", 0);
                        mergeSource.setForEachStream(tc.staticTypeCheck(mergeSource.getForEachSource(), Saxon.Hej.value.SequenceType.STRING_SEQUENCE, role, visitor));
                        rowContextItemType = config.makeContextItemStaticInfo(Saxon.Hej.pattern.NodeKindTest.DOCUMENT, false);
                    }
                }
                mergeSource.rowSelectOp.typeCheck(visitor, rowContextItemType);
                Saxon.Hej.type.ItemType rowItemType = mergeSource.getRowSelect().getItemType();
                if ((inputType == null)) {
                    inputType = rowItemType;
                } else {
                    inputType = Saxon.Hej.type.Type.getCommonSuperType(inputType, rowItemType, th);
                }
                Saxon.Hej.expr.parser.ContextItemStaticInfo cit = config.makeContextItemStaticInfo(inputType, false);
                if ((mergeSource.mergeKeyDefinitions != null)) {
                    foreach (Saxon.Hej.expr.sort.SortKeyDefinition skd in mergeSource.mergeKeyDefinitions) {
                        Saxon.Hej.expr.Expression sortKey = skd.getSortKey();
                        sortKey = sortKey.typeCheck(visitor, cit);
                        if ((sortKey != null)) {
                            Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "xsl:merge-key/select", 0);
                            role.setErrorCode("XTTE1020");
                            sortKey = Saxon.Hej.expr.CardinalityChecker.makeCardinalityChecker(sortKey, Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE, role);
                            skd.setSortKey(sortKey, true);
                        }
                        Saxon.Hej.expr.Expression exp = skd.getLanguage().typeCheck(visitor, config.makeContextItemStaticInfo(inputType, false));
                        skd.setLanguage(exp);
                        exp = skd.getOrder().typeCheck(visitor, cit);
                        skd.setOrder(exp);
                        exp = skd.getCollationNameExpression();
                        if ((exp != null)) {
                            exp = exp.typeCheck(visitor, cit);
                            skd.setCollationNameExpression(exp);
                        }
                        exp = skd.getCaseOrder().typeCheck(visitor, cit);
                        skd.setCaseOrder(exp);
                        exp = skd.getDataTypeExpression();
                        if ((exp != null)) {
                            exp = exp.typeCheck(visitor, cit);
                            skd.setDataTypeExpression(exp);
                        }
                    }
                }
            }
            actionOp.typeCheck(visitor, config.makeContextItemStaticInfo(inputType, false));
            if (Saxon.Hej.expr.Literal.isEmptySequence(getAction())) {
                return getAction();
            }
            if (((mergeSources.Length == 1) && Saxon.Hej.expr.Literal.isEmptySequence(mergeSources[0].getRowSelect()))) {
                return mergeSources[0].getRowSelect();
            }
            fixupGroupReferences();
            return this;
        }
        public void fixupGroupReferences() {
            fixupGroupReferences(this, this, false);
        }
        private static void fixupGroupReferences(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.sort.MergeInstr instr, bool isInLoop) {
            if ((exp == null)) {
            } else {
                if (exp.isCallOn(typeof(Saxon.Hej.functions.CurrentMergeGroup))) {
                    Saxon.Hej.functions.CurrentMergeGroup fn = (Saxon.Hej.functions.CurrentMergeGroup)(((Saxon.Hej.expr.SystemFunctionCall)(exp)).getTargetFunction());
                    fn.setControllingInstruction(instr, isInLoop);
                } else {
                    if (exp.isCallOn(typeof(Saxon.Hej.functions.CurrentMergeKey))) {
                        Saxon.Hej.functions.CurrentMergeKey fn = (Saxon.Hej.functions.CurrentMergeKey)(((Saxon.Hej.expr.SystemFunctionCall)(exp)).getTargetFunction());
                        fn.setControllingInstruction(instr);
                    } else {
                        if ((exp is Saxon.Hej.expr.sort.MergeInstr)) {
                            Saxon.Hej.expr.sort.MergeInstr instr2 = (Saxon.Hej.expr.sort.MergeInstr)(exp);
                            if ((instr2 == instr)) {
                                fixupGroupReferences(instr2.getAction(), instr, false);
                            } else {
                                foreach (Saxon.Hej.expr.sort.MergeInstr.MergeSource m in instr2.getMergeSources()) {
                                    foreach (Saxon.Hej.expr.sort.SortKeyDefinition skd in m.mergeKeyDefinitions) {
                                        fixupGroupReferences(skd.getOrder(), instr, isInLoop);
                                        fixupGroupReferences(skd.getCaseOrder(), instr, isInLoop);
                                        fixupGroupReferences(skd.getDataTypeExpression(), instr, isInLoop);
                                        fixupGroupReferences(skd.getLanguage(), instr, isInLoop);
                                        fixupGroupReferences(skd.getCollationNameExpression(), instr, isInLoop);
                                        fixupGroupReferences(skd.getOrder(), instr, isInLoop);
                                    }
                                    if ((m.forEachItemOp != null)) {
                                        fixupGroupReferences(m.getForEachItem(), instr, isInLoop);
                                    }
                                    if ((m.forEachStreamOp != null)) {
                                        fixupGroupReferences(m.getForEachSource(), instr, isInLoop);
                                    }
                                    if ((m.rowSelectOp != null)) {
                                        fixupGroupReferences(m.getRowSelect(), instr, isInLoop);
                                    }
                                }
                            }
                        } else {
                            foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                                fixupGroupReferences(o.getChildExpression(), instr, (isInLoop || o.isEvaluatedRepeatedly()));
                            }
                        }
                    }
                }
            }
        }
        public override bool mayCreateNewNodes() {
            int props = getAction().getSpecialProperties();
            return (((props&Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED)) == 0);
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            Saxon.Hej.type.ItemType inputType = null;
            foreach (Saxon.Hej.expr.sort.MergeInstr.MergeSource mergeSource in mergeSources) {
                Saxon.Hej.expr.parser.ContextItemStaticInfo rowContextItemType = contextInfo;
                if ((mergeSource.getForEachItem() != null)) {
                    mergeSource.forEachItemOp.optimize(visitor, contextInfo);
                    rowContextItemType = config.makeContextItemStaticInfo(mergeSource.getForEachItem().getItemType(), false);
                } else {
                    if ((mergeSource.getForEachSource() != null)) {
                        mergeSource.forEachStreamOp.optimize(visitor, contextInfo);
                        rowContextItemType = config.makeContextItemStaticInfo(Saxon.Hej.pattern.NodeKindTest.DOCUMENT, false);
                    }
                }
                mergeSource.rowSelectOp.optimize(visitor, rowContextItemType);
                Saxon.Hej.type.ItemType rowItemType = mergeSource.getRowSelect().getItemType();
                if ((inputType == null)) {
                    inputType = rowItemType;
                } else {
                    inputType = Saxon.Hej.type.Type.getCommonSuperType(inputType, rowItemType, th);
                }
            }
            Saxon.Hej.expr.parser.ContextItemStaticInfo cit = config.makeContextItemStaticInfo(inputType, false);
            setAction(getAction().optimize(visitor, cit));
            if (Saxon.Hej.expr.Literal.isEmptySequence(getAction())) {
                return getAction();
            }
            if (((mergeSources.Length == 1) && Saxon.Hej.expr.Literal.isEmptySequence(mergeSources[0].getRowSelect()))) {
                return mergeSources[0].getRowSelect();
            }
            return this;
        }
        public override void prepareForStreaming() {
            foreach (Saxon.Hej.expr.sort.MergeInstr.MergeSource mergeSource in mergeSources) {
                mergeSource.prepareForStreaming();
            }
        }
        private void checkMergeAtt(Saxon.Hej.expr.sort.SortKeyDefinition[] sortKeyDefs) {
            for (int i = 1; (i<sortKeyDefs.Length); i++) {
                if (!(sortKeyDefs[0].isEqual(sortKeyDefs[i]))) {
                    throw new Saxon.Hej.trans.XPathException(("Corresponding xsl:merge-key attributes in different xsl:merge-source elements "+"do not have the same effective values"), "XTDE2210");
                }
            }
        }
        private Saxon.Hej.expr.LastPositionFinder getLastPositionFinder(Saxon.Hej.expr.XPathContext context) {
            return new Inner_LastPositionFinder_d970e2581(this, context);
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            try {
                Saxon.Hej.expr.sort.AtomicComparer[] comps = getComparators(context);
                Saxon.Hej.expr.sort.GroupIterator mgi = context.getCurrentMergeGroupIterator();
                Saxon.Hej.expr.XPathContextMajor c1 = context.newContext();
                c1.setCurrentMergeGroupIterator(mgi);
                Saxon.Hej.om.SequenceIterator inputIterator = getMergedInputIterator(context, comps, c1);
                inputIterator = new Saxon.Hej.expr.sort.MergeGroupingIterator(inputIterator, getComparer(mergeSources[0].mergeKeyDefinitions, comps), getLastPositionFinder(context));
                c1.setCurrentMergeGroupIterator((Saxon.Hej.expr.sort.GroupIterator)(inputIterator));
                Saxon.Hej.expr.XPathContext c3 = c1.newMinorContext();
                c3.trackFocus(inputIterator);
                return new Saxon.Hej.expr.ContextMappingIterator(cxt => getAction().iterate(cxt), c3);
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetLocation(getLocation());
                throw;
            }
        }
        private Saxon.Hej.om.SequenceIterator getMergedInputIterator(Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.sort.AtomicComparer[] comps, Saxon.Hej.expr.XPathContextMajor c1) {
            Saxon.Hej.om.SequenceIterator inputIterator = Saxon.Hej.tree.iter.EmptyIterator.getInstance();
            foreach (Saxon.Hej.expr.sort.MergeInstr.MergeSource ms in mergeSources) {
                Saxon.Hej.om.SequenceIterator anchorsIter = null;
                if ((ms.streamable && (ms.getForEachSource() != null))) {
                    System.Collections.Generic.IList<Saxon.Hej.expr.ContextMappingIterator> msIters = /*134*/Saxon.Eej.ee.stream.adjunct.MergeInstrAdjunct.makeSourceIterators(ms, getPackageData(), c1);
                    foreach (Saxon.Hej.expr.ContextMappingIterator iter in msIters) {
                        inputIterator = makeMergeIterator(inputIterator, comps, ms, iter);
                    }
                } else {
                    if ((ms.getForEachSource() != null)) {
                        Saxon.Hej.lib.ParseOptions options = new Saxon.Hej.lib.ParseOptions(context.getConfiguration().getParseOptions());
                        options.setSchemaValidationMode(ms.validation);
                        options.setTopLevelType(ms.schemaType);
                        options.setApplicableAccumulators(ms.accumulators);
                        Saxon.Hej.om.SequenceIterator uriIter = ms.getForEachSource().iterate(c1);
                        Saxon.Hej.trans.XsltController controller = (Saxon.Hej.trans.XsltController)(context.getController());
                        Saxon.Hej.expr.accum.AccumulatorManager accumulatorManager = controller.getAccumulatorManager();
                        anchorsIter = Saxon.Hej.expr.ItemMappingIterator.map(uriIter, baseItem => {
                            string uri = baseItem.getStringValue();
                            Saxon.Hej.om.NodeInfo node = Saxon.Hej.functions.DocumentFn.makeDoc(uri, getRetainedStaticContext().getStaticBaseUriString(), getPackageData(), options, c1, getLocation(), true);
                            if ((node != null)) {
                                accumulatorManager.setApplicableAccumulators(node.getTreeInfo(), ms.accumulators);
                            }
                            return node;
                        } );
                        Saxon.Hej.expr.XPathContext c2 = c1.newMinorContext();
                        Saxon.Hej.om.FocusIterator anchorsIterFocus = c2.trackFocus(anchorsIter);
                        while ((anchorsIterFocus.next() != null)) {
                            Saxon.Hej.expr.XPathContext c4 = c2.newMinorContext();
                            Saxon.Hej.om.FocusIterator rowIntr = c4.trackFocus(ms.getRowSelect().iterate(c2));
                            Saxon.Hej.expr.sort.MergeInstr.MergeKeyMappingFunction addMergeKeys = new Saxon.Hej.expr.sort.MergeInstr.MergeKeyMappingFunction(c4, ms);
                            Saxon.Hej.expr.ContextMappingIterator contextMapKeysItr = new Saxon.Hej.expr.ContextMappingIterator(it => addMergeKeys.map(it) /* converted from instance method reference */, c4);
                            inputIterator = makeMergeIterator(inputIterator, comps, ms, contextMapKeysItr);
                        }
                    } else {
                        if ((ms.getForEachItem() != null)) {
                            anchorsIter = ms.getForEachItem().iterate(c1);
                            Saxon.Hej.expr.XPathContext c2 = c1.newMinorContext();
                            Saxon.Hej.om.FocusIterator anchorsIterFocus = c2.trackFocus(anchorsIter);
                            while ((anchorsIterFocus.next() != null)) {
                                inputIterator = getInputIterator(comps, inputIterator, ms, c2);
                            }
                        } else {
                            inputIterator = getInputIterator(comps, inputIterator, ms, c1);
                        }
                    }
                }
            }
            return inputIterator;
        }
        private Saxon.Hej.om.SequenceIterator getInputIterator(Saxon.Hej.expr.sort.AtomicComparer[] comps, Saxon.Hej.om.SequenceIterator inputIterator, Saxon.Hej.expr.sort.MergeInstr.MergeSource ms, Saxon.Hej.expr.XPathContext c2) {
            Saxon.Hej.expr.XPathContext c4 = c2.newMinorContext();
            c4.setTemporaryOutputState(Saxon.Hej.om.StandardNames.XSL_MERGE_KEY);
            Saxon.Hej.om.FocusIterator rowIntr = c4.trackFocus(ms.getRowSelect().iterate(c2));
            Saxon.Hej.expr.sort.MergeInstr.MergeKeyMappingFunction addMergeKeys = new Saxon.Hej.expr.sort.MergeInstr.MergeKeyMappingFunction(c4, ms);
            Saxon.Hej.expr.ContextMappingIterator contextMapKeysItr = new Saxon.Hej.expr.ContextMappingIterator(it => addMergeKeys.map(it) /* converted from instance method reference */, c4);
            inputIterator = makeMergeIterator(inputIterator, comps, ms, contextMapKeysItr);
            return inputIterator;
        }
        private Saxon.Hej.expr.sort.AtomicComparer[] getComparators(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.sort.AtomicComparer[] comps = comparators;
            if ((comparators == null)) {
                Saxon.Hej.expr.sort.SortKeyDefinition[] tempSKeys = new Saxon.Hej.expr.sort.SortKeyDefinition[mergeSources.Length];
                for (int i = 0; (i<mergeSources[0].mergeKeyDefinitions.size()); i++) {
                    for (int j = 0; (j<mergeSources.Length); j++) {
                        tempSKeys[j] = mergeSources[j].mergeKeyDefinitions.getSortKeyDefinition(i).fix(context);
                    }
                    checkMergeAtt(tempSKeys);
                }
                comps = new Saxon.Hej.expr.sort.AtomicComparer[mergeSources[0].mergeKeyDefinitions.size()];
                for (int s = 0; (s<mergeSources[0].mergeKeyDefinitions.size()); s++) {
                    Saxon.Hej.expr.sort.AtomicComparer comp = mergeSources[0].mergeKeyDefinitions.getSortKeyDefinition(s).getFinalComparator();
                    if ((comp == null)) {
                        comp = mergeSources[0].mergeKeyDefinitions.getSortKeyDefinition(s).makeComparator(context);
                    }
                    comps[s] = comp;
                }
            }
            return comps;
        }
        private Saxon.Hej.om.SequenceIterator makeMergeIterator(Saxon.Hej.om.SequenceIterator result, Saxon.Hej.expr.sort.AtomicComparer[] comps, Saxon.Hej.expr.sort.MergeInstr.MergeSource ms, Saxon.Hej.expr.ContextMappingIterator contextMapKeysItr) {
            if (((result == null) || (result is Saxon.Hej.tree.iter.EmptyIterator))) {
                result = contextMapKeysItr;
            } else {
                result = new Saxon.Hej.expr.sort.MergeIterator(result, contextMapKeysItr, getComparer(ms.mergeKeyDefinitions, comps));
            }
            return result;
        }
        private readonly static Saxon.Hej.expr.OperandRole ROW_SELECT = new Saxon.Hej.expr.OperandRole((Saxon.Hej.expr.OperandRole.USES_NEW_FOCUS|Saxon.Hej.expr.OperandRole.HIGHER_ORDER), Saxon.Hej.expr.OperandUsage.INSPECTION, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            System.Collections.Generic.IList<Saxon.Hej.expr.Operand> list = new System.Collections.Generic.List<Saxon.Hej.expr.Operand>(6);
            list.Add(actionOp);
            if ((mergeSources != null)) {
                foreach (Saxon.Hej.expr.sort.MergeInstr.MergeSource ms in mergeSources) {
                    if ((ms.forEachItemOp != null)) {
                        list.Add(ms.forEachItemOp);
                    }
                    if ((ms.forEachStreamOp != null)) {
                        list.Add(ms.forEachStreamOp);
                    }
                    if ((ms.rowSelectOp != null)) {
                        list.Add(ms.rowSelectOp);
                    }
                    list.Add(new Saxon.Hej.expr.Operand(this, ms.mergeKeyDefinitions, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC));
                }
            }
            return list;
        }
        public Saxon.Hej.expr.Expression getGroupingKey() {
            return mergeSources[0].mergeKeyDefinitions.getSortKeyDefinition(0).getSortKey();
        }
        public System.Collections.Generic.Comparer<Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys>> getComparer(Saxon.Hej.expr.sort.SortKeyDefinitionList sKeys, Saxon.Hej.expr.sort.AtomicComparer[] comps) {
            return new Inner_Comparator_d970e4062(sKeys, comps);
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.sort.MergeInstr newMerge = new Saxon.Hej.expr.sort.MergeInstr();
            Saxon.Hej.expr.sort.MergeInstr.MergeSource[] c2 = new Saxon.Hej.expr.sort.MergeInstr.MergeSource[mergeSources.Length];
            Saxon.Hej.expr.Expression a2 = getAction().copy(rebindings);
            for (int c = 0; (c<mergeSources.Length); c++) {
                c2[c] = mergeSources[c].copyMergeSource(newMerge, rebindings);
            }
            return newMerge.init(c2, a2);
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("merge", this);
            foreach (Saxon.Hej.expr.sort.MergeInstr.MergeSource mergeSource in mergeSources) {
                @out.startSubsidiaryElement("mergeSrc");
                if (((mergeSource.sourceName != null) && !(mergeSource.sourceName.StartsWith("saxon-merge-source-")))) {
                    @out.emitAttribute("name", mergeSource.sourceName);
                }
                if (((mergeSource.validation != Saxon.Hej.lib.Validation.SKIP) && (mergeSource.validation != Saxon.Hej.lib.Validation.BY_TYPE))) {
                    @out.emitAttribute("validation", Saxon.Hej.lib.Validation.describe(mergeSource.validation));
                }
                if ((mergeSource.validation == Saxon.Hej.lib.Validation.BY_TYPE)) {
                    Saxon.Hej.type.SchemaType type = mergeSource.schemaType;
                    if ((type != null)) {
                        @out.emitAttribute("type", type.getStructuredQName());
                    }
                }
                if (((mergeSource.accumulators != null) && !(mergeSource.accumulators.Count==0))) {
                    Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(256);
                    foreach (Saxon.Hej.expr.accum.Accumulator acc in mergeSource.accumulators) {
                        if ((fsb.length() != 0)) {
                            fsb.append(" ");
                        }
                        fsb.append(acc.getAccumulatorName().getEQName());
                    }
                    @out.emitAttribute("accum", fsb.ToString());
                }
                if (mergeSource.streamable) {
                    @out.emitAttribute("flags", "s");
                }
                if ((mergeSource.getForEachItem() != null)) {
                    @out.setChildRole("forEachItem");
                    mergeSource.getForEachItem().export(@out);
                }
                if ((mergeSource.getForEachSource() != null)) {
                    @out.setChildRole("forEachStream");
                    mergeSource.getForEachSource().export(@out);
                }
                @out.setChildRole("selectRows");
                mergeSource.getRowSelect().export(@out);
                mergeSource.getMergeKeyDefinitionSet().export(@out);
                @out.endSubsidiaryElement();
            }
            @out.setChildRole("action");
            getAction().export(@out);
            @out.endElement();
        }
        public override Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.SequenceIterator iter = iterate(context);
            try {
                Saxon.Hej.om.SequenceTool.supply(iter, it => output.append(it, getLocation(), Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES));
            } catch (Saxon.Hej.trans.UncheckedXPathException err) {
                iter.close();
                Saxon.Hej.trans.XPathException e = err.getXPathException();
                e.maybeSetLocation(getLocation());
                e.maybeSetContext(context);
                throw e;
            } finally {
                iter.close();
            }
            return null;
        }
        public override string getStreamerName() {
            return "MergeInstr";
        }
        public class MergeKeyMappingFunction {
            private readonly Saxon.Hej.expr.sort.MergeInstr.MergeSource ms;
            private readonly Saxon.Hej.expr.XPathContext baseContext;
            private readonly Saxon.Hej.expr.XPathContext keyContext;
            private readonly Saxon.Hej.tree.iter.ManualIterator manualIterator;
            public MergeKeyMappingFunction(Saxon.Hej.expr.XPathContext baseContext, Saxon.Hej.expr.sort.MergeInstr.MergeSource ms)  {
                this.baseContext = baseContext;
                this.ms = ms;
                keyContext = baseContext.newMinorContext();
                keyContext.setTemporaryOutputState(Saxon.Hej.om.StandardNames.XSL_MERGE_KEY);
                manualIterator = new Saxon.Hej.tree.iter.ManualIterator();
                manualIterator.setPosition(1);
                keyContext.setCurrentIterator(manualIterator);
            }
            public Saxon.Hej.om.SequenceIterator map(Saxon.Hej.expr.XPathContext context) {
                Saxon.Hej.om.Item currentItem = context.getContextItem();
                manualIterator.setContextItem(currentItem);
                Saxon.Hej.expr.sort.ItemWithMergeKeys newItem = new Saxon.Hej.expr.sort.ItemWithMergeKeys(currentItem, ms.mergeKeyDefinitions, ms.sourceName, keyContext);
                return Saxon.Hej.tree.iter.SingletonIterator.makeIterator(new Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys>(newItem));
            }
        
        }
    
        private class Inner_LastPositionFinder_d970e2581 : Saxon.Hej.expr.LastPositionFinder {
            private readonly MergeInstr outerInstance;
            public Inner_LastPositionFinder_d970e2581(MergeInstr outerInstance, Saxon.Hej.expr.XPathContext context) {
                this.outerInstance = outerInstance;
                this.context = context;
                last = -1;
            }
            private Saxon.Hej.expr.XPathContext context;
            private int last;
            public bool supportsGetLength() {
                return true;
            }
            public int getLength() {
                try {
                    if ((/*field*/last>=0)) {
                        return /*field*/last;
                    } else {
                        Saxon.Hej.expr.sort.AtomicComparer[] comps = outerInstance.getComparators(/*extraParam*/context);
                        Saxon.Hej.expr.sort.GroupIterator mgi = /*extraParam*/context.getCurrentMergeGroupIterator();
                        Saxon.Hej.expr.XPathContextMajor c1 = /*extraParam*/context.newContext();
                        /*localvar*/c1.setCurrentMergeGroupIterator(/*localvar*/mgi);
                        Saxon.Hej.om.SequenceIterator inputIterator = outerInstance.getMergedInputIterator(/*extraParam*/context, /*localvar*/comps, /*localvar*/c1);
                        /*localvar*/inputIterator = new Saxon.Hej.expr.sort.MergeGroupingIterator(/*localvar*/inputIterator, outerInstance.getComparer(outerInstance.mergeSources[0].mergeKeyDefinitions, /*localvar*/comps), null);
                        return /*field*/last = Saxon.Hej.functions.Count.steppingCount(/*localvar*/inputIterator);
                    }
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Hej.trans.UncheckedXPathException(/*exception*/e);
                }
            }
        }

        private class Inner_Comparator_d970e4062 : System.Collections.Generic.Comparer<Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys>> {
            public Inner_Comparator_d970e4062(Saxon.Hej.expr.sort.SortKeyDefinitionList sKeys, Saxon.Hej.expr.sort.AtomicComparer[] comps) {
                this.sKeys = sKeys;
                this.comps = comps;
            }
            private Saxon.Hej.expr.sort.SortKeyDefinitionList sKeys;
            private Saxon.Hej.expr.sort.AtomicComparer[] comps;
            public int compare(Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys> a, Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys> b) {
                Saxon.Hej.expr.sort.ItemWithMergeKeys aItem = /*localparam*/a.getObject();
                Saxon.Hej.expr.sort.ItemWithMergeKeys bItem = /*localparam*/b.getObject();
                for (int i = 0; (/*localvar*/i</*extraParam*/sKeys.size()); /*localvar*/i++) {
                    int val;
                    try {
                        /*localvar*/val = /*extraParam*/comps[/*localvar*/i].compareAtomicValues(/*localvar*/aItem.sortKeyValues[/*localvar*/i], /*localvar*/bItem.sortKeyValues[/*localvar*/i]);
                    } catch (Saxon.Hej.trans.NoDynamicContextException e) {
                        throw new Saxon.Ejava.lang.IllegalStateException(/*exception*/e);
                    }
                    if ((/*localvar*/val != 0)) {
                        return /*localvar*/val;
                    }
                }
                return 0;
            }
            // Extra method to make the class a C# Comparer
            public override int Compare(Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys> a, Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys> b) {
                return compare(a, b);
            }
        }

    }
}