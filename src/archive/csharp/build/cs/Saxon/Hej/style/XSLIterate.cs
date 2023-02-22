using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLIterate : Saxon.Hej.style.StyleElement {
        internal Saxon.Hej.expr.Expression select = null;
        internal bool compilable;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool isPermittedChild(Saxon.Hej.style.StyleElement child) {
            return ((child is Saxon.Hej.style.XSLLocalParam) || (child is Saxon.Hej.style.XSLOnCompletion));
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override bool mayContainParam() {
            return true;
        }
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string value = att.getValue();
                string f = attName.getDisplayName();
                switch (f) {
                  case "select":
                    select = makeExpression(value, att);
                    break ;
                  case "array":
                    requireXslt40("array");
                    select = arrayToSequence(makeExpression(value, att));
                    break ;
                  case "map":
                    requireXslt40("map");
                    select = mapToSequence(makeExpression(value, att));
                    break ;
                  default:
                    checkUnknownAttribute(attName);
                    break ;
                }
            }
            if ((select == null)) {
                select = Saxon.Hej.expr.Literal.makeEmptySequence();
                if ((getEffectiveVersion()>=40)) {
                    compileError("One of the attributes select|array|map must be present");
                } else {
                    reportAbsence("select");
                }
            }
        }
        public void setCompilable(bool compilable) {
            this.compilable = compilable;
        }
        public bool isCompilable() {
            return compilable;
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            select = typeCheck("select", select);
            if (!(hasChildNodes())) {
                compileWarning("An empty xsl:iterate instruction has no effect", Saxon.Hej.trans.SaxonErrorCode.SXWN9009);
            }
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> nonFinallyChildren = new System.Collections.Generic.List<Saxon.Hej.om.NodeInfo>();
            Saxon.Hej.expr.Expression finallyExp = null;
            System.Collections.Generic.IList<Saxon.Hej.style.XSLLocalParam> @params = new System.Collections.Generic.List<Saxon.Hej.style.XSLLocalParam>();
            foreach (Saxon.Hej.om.NodeInfo node in children()) {
                if ((node is Saxon.Hej.style.XSLLocalParam)) {
                    @params.Add((Saxon.Hej.style.XSLLocalParam)(node));
                } else {
                    if ((node is Saxon.Hej.style.XSLOnCompletion)) {
                        finallyExp = ((Saxon.Hej.style.XSLOnCompletion)(node)).compile(exec, decl);
                    } else {
                        nonFinallyChildren.Add(node);
                    }
                }
            }
            Saxon.Hej.expr.instruct.LocalParam[] compiledParams = new Saxon.Hej.expr.instruct.LocalParam[@params.Count];
            for (int i = 0; (i<@params.Count); i++) {
                compiledParams[i] = (Saxon.Hej.expr.instruct.LocalParam)(@params[i].compile(exec, decl));
                if (compiledParams[i].isImplicitlyRequiredParam()) {
                    compileError("The parameter must be given an initial value because () is not valid, given the declared type", "XTSE3520");
                }
            }
            Saxon.Hej.expr.instruct.LocalParamBlock paramBlock = new Saxon.Hej.expr.instruct.LocalParamBlock(compiledParams);
            Saxon.Hej.expr.Expression action = compileSequenceConstructor(exec, decl, new Saxon.Hej.tree.iter.NodeListIterator(nonFinallyChildren), false);
            if ((action == null)) {
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            try {
                action = action.simplify();
                return new Saxon.Hej.expr.instruct.IterateInstr(select, paramBlock, action, finallyExp);
            } catch (Saxon.Hej.trans.XPathException err) {
                compileError(err);
                return null;
            }
        }
    
    }
}