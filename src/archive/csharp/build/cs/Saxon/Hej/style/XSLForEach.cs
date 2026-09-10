using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLForEach : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression select = null;
        private bool containsTailCall = false;
        private Saxon.Hej.expr.Expression threads = null;
        private Saxon.Hej.expr.Expression separator = null;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool isPermittedChild(Saxon.Hej.style.StyleElement child) {
            return ((child is Saxon.Hej.style.XSLSort));
        }
        protected internal override bool markTailCalls() {
            System.Diagnostics.Debug.Assert((select != null));
            if (Saxon.Hej.value.Cardinality.allowsMany(select.getCardinality())) {
                return false;
            } else {
                Saxon.Hej.style.StyleElement last = getLastChildInstruction();
                containsTailCall = ((last != null) && last.markTailCalls());
                return containsTailCall;
            }
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                if (f.Equals("select")) {
                    select = makeExpression(value, att);
                } else {
                    if (f.Equals("array")) {
                        requireXslt40("array");
                        select = arrayToSequence(makeExpression(value, att));
                    } else {
                        if (f.Equals("map")) {
                            requireXslt40("map");
                            select = mapToSequence(makeExpression(value, att));
                        } else {
                            if (f.Equals("separator")) {
                                requireXslt40("separator");
                                separator = makeAttributeValueTemplate(value, att);
                            } else {
                                if ((attName.getLocalPart().Equals("threads") && attName.hasURI(Saxon.Hej.lib.NamespaceConstant.SAXON))) {
                                    threads = makeAttributeValueTemplate(Saxon.Hej.value.Whitespace.trim(value), att);
                                    if (getCompilation().getCompilerInfo().isCompileWithTracing()) {
                                        compileWarning("saxon:threads - no multithreading takes place when compiling with trace enabled", Saxon.Hej.trans.SaxonErrorCode.SXWN9012);
                                        threads = new Saxon.Hej.expr.StringLiteral("0");
                                    } else {
                                        if (!("EE".Equals(getConfiguration().getEditionCode()))) {
                                            compileWarning("saxon:threads - ignored when not running Saxon-EE", Saxon.Hej.trans.SaxonErrorCode.SXWN9013);
                                            threads = new Saxon.Hej.expr.StringLiteral("0");
                                        }
                                    }
                                } else {
                                    checkUnknownAttribute(attName);
                                }
                            }
                        }
                    }
                }
            }
            if ((select == null)) {
                if ((getEffectiveVersion()>=40)) {
                    compileError("One of the attributes select|array|map must be present");
                } else {
                    reportAbsence("select");
                }
                select = Saxon.Hej.expr.Literal.makeEmptySequence();
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            checkSortComesFirst(false);
            select = typeCheck("select", select);
            if ((separator != null)) {
                separator = typeCheck("separator", separator);
            }
            if ((threads != null)) {
                threads = typeCheck("threads", threads);
            }
            if (!(hasChildNodes())) {
                compileWarning("An empty xsl:for-each instruction has no effect", Saxon.Hej.trans.SaxonErrorCode.SXWN9009);
            }
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.sort.SortKeyDefinitionList sortKeys = makeSortKeys(compilation, decl);
            Saxon.Hej.expr.Expression sortedSequence = select;
            if ((sortKeys != null)) {
                sortedSequence = new Saxon.Hej.expr.sort.SortExpression(select, sortKeys);
            }
            Saxon.Hej.expr.Expression block = compileSequenceConstructor(compilation, decl, true);
            if ((block == null)) {
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            try {
                Saxon.Hej.expr.instruct.ForEach result = new Saxon.Hej.expr.instruct.ForEach(sortedSequence, block.simplify(), containsTailCall, threads);
                result.setInstruction(true);
                result.setLocation(allocateLocation());
                if ((separator != null)) {
                    result.setSeparatorExpression(separator);
                }
                return result;
            } catch (Saxon.Hej.trans.XPathException err) {
                compileError(err);
                return null;
            }
        }
    
    }
}