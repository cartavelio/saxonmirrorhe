using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLMerge : Saxon.Hej.style.StyleElement {
        private int numberOfMergeSources = 0;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return false;
        }
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                checkUnknownAttribute(attName);
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            int childMask = 0;
            System.Collections.Generic.ISet<string> mergeSourceNames = new System.Collections.Generic.HashSet<string>();
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                if ((child is Saxon.Hej.style.XSLMergeSource)) {
                    string name = ((Saxon.Hej.style.XSLMergeSource)(child)).getSourceName();
                    if (mergeSourceNames.Contains(name)) {
                        compileError("Duplicate xsl:merge-source/@name", "XTSE3190");
                    }
                    mergeSourceNames.Add(name);
                    childMask = (childMask|1);
                    numberOfMergeSources++;
                } else {
                    if ((child is Saxon.Hej.style.XSLMergeAction)) {
                        if ((((childMask&2)) == 2)) {
                            compileError("xsl:merge must have only one xsl:merge-action child element", "XTSE0010");
                        }
                        childMask = (childMask|2);
                    } else {
                        if ((child.getNodeKind() == Saxon.Hej.type.Type.TEXT)) {
                            if (!(Saxon.Hej.value.Whitespace.isAllWhite(child.getUnicodeStringValue()))) {
                                compileError("No character data is allowed within xsl:merge", "XXXX");
                            }
                        } else {
                            if ((child is Saxon.Hej.style.XSLFallback)) {
                                if ((((childMask&2)) == 0)) {
                                    compileError("xsl:fallback child of xsl:merge can appear only after xsl:merge-action", "XTSE0010");
                                }
                            } else {
                                compileError((("Child element "+Saxon.Hej.trans.Err.wrap(child.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" is not allowed as a child of xsl:merge"), "XTSE0010");
                            }
                        }
                    }
                }
            }
            if ((childMask == 1)) {
                compileError("xsl:merge element requires an xsl:merge-action", "XTSE0010");
            } else {
                if ((childMask == 2)) {
                    compileError("xsl:merge element requires at least one xsl:merge-source child element", "XTSE0010");
                }
            }
        }
        private void checkCompatibleMergeKeys(Saxon.Hej.expr.sort.MergeInstr.MergeSource[] sources) {
            for (int i = 0; (i<sources[0].mergeKeyDefinitions.size()); i++) {
                if (!(sources[0].mergeKeyDefinitions.getSortKeyDefinition(i).isFixed())) {
                    break ;
                }
                for (int z = 1; (z<sources.Length); z++) {
                    if (!(sources[z].mergeKeyDefinitions.getSortKeyDefinition(i).isFixed())) {
                        break ;
                    }
                    if (!(compareSortKeyDefinitions(sources[z].mergeKeyDefinitions.getSortKeyDefinition(i), sources[0].mergeKeyDefinitions.getSortKeyDefinition(i)))) {
                        compileError((((((("The "+Saxon.Hej.expr.parser.RoleDiagnostic.ordinal((i+1)))+" merge key definition of the ")+Saxon.Hej.expr.parser.RoleDiagnostic.ordinal((z+1)))+" merge source is incompatible with the ")+Saxon.Hej.expr.parser.RoleDiagnostic.ordinal((i+1)))+" merge key definition of the first merge source"), "XTDE2210");
                    }
                }
            }
        }
        private bool compareSortKeyDefinitions(Saxon.Hej.expr.sort.SortKeyDefinition sd1, Saxon.Hej.expr.sort.SortKeyDefinition sd2) {
            return ((((sameFixedExpression(sd1.getLanguage(), sd2.getLanguage()) && sameFixedExpression(sd1.getOrder(), sd2.getOrder())) && sameFixedExpression(sd1.getCollationNameExpression(), sd2.getCollationNameExpression())) && sameFixedExpression(sd1.getCaseOrder(), sd2.getCaseOrder())) && sameFixedExpression(sd1.getDataTypeExpression(), sd2.getDataTypeExpression()));
        }
        private bool sameFixedExpression(Saxon.Hej.expr.Expression e1, Saxon.Hej.expr.Expression e2) {
            return ((((e1 == null) && (e2 == null))) || (((e1 != null) && e1.Equals(e2))));
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.sort.MergeInstr merge = new Saxon.Hej.expr.sort.MergeInstr();
            int entries = numberOfMergeSources;
            Saxon.Hej.expr.sort.MergeInstr.MergeSource[] sources = new Saxon.Hej.expr.sort.MergeInstr.MergeSource[entries];
            Saxon.Hej.expr.Expression action = Saxon.Hej.expr.Literal.makeEmptySequence();
            int w = 0;
            int sortKeyDefLen = 0;
            foreach (Saxon.Hej.om.NodeInfo node in children()) {
                if ((node is Saxon.Hej.style.XSLMergeSource)) {
                    Saxon.Hej.style.XSLMergeSource source = (Saxon.Hej.style.XSLMergeSource)(node);
                    Saxon.Hej.expr.sort.SortKeyDefinitionList sortKeyDefs = source.makeSortKeys(compilation, decl);
                    if ((sortKeyDefLen == 0)) {
                        sortKeyDefLen = sortKeyDefs.size();
                    } else {
                        if ((sortKeyDefLen != sortKeyDefs.size())) {
                            compileError("Each xsl:merge-source must have the same number of xsl:merge-key children", "XTSE2200");
                        }
                    }
                    Saxon.Hej.expr.Expression select = source.getSelect();
                    if (source.isSortBeforeMerge()) {
                        select = new Saxon.Hej.expr.sort.SortExpression(select, sortKeyDefs.copy(new Saxon.Hej.expr.parser.RebindingMap()));
                    }
                    Saxon.Hej.expr.sort.MergeInstr.MergeSource ms = source.makeMergeSource(merge, select);
                    ms.mergeKeyDefinitions = sortKeyDefs;
                    sources[w++] = ms;
                } else {
                    if ((node is Saxon.Hej.style.XSLMergeAction)) {
                        action = ((Saxon.Hej.style.XSLMergeAction)(node)).compileSequenceConstructor(compilation, decl, true);
                        if ((action == null)) {
                            action = Saxon.Hej.expr.Literal.makeEmptySequence();
                        }
                        try {
                            action = action.simplify();
                        } catch (Saxon.Hej.trans.XPathException e) {
                            compileError(e);
                        }
                    } else {
                    }
                }
            }
            checkCompatibleMergeKeys(sources);
            merge.init(sources, action);
            return merge;
        }
    
    }
}