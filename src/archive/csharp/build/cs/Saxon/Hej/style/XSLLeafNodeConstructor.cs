using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public abstract class XSLLeafNodeConstructor : Saxon.Hej.style.StyleElement {
        protected internal Saxon.Hej.expr.Expression select = null;
        protected internal Saxon.Hej.expr.Expression prepareAttributesNameAndSelect() {
            Saxon.Hej.expr.Expression name = null;
            string nameAtt = null;
            string selectAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                if (f.Equals("name")) {
                    nameAtt = Saxon.Hej.value.Whitespace.trim(value);
                    name = makeAttributeValueTemplate(nameAtt, att);
                } else {
                    if (f.Equals("select")) {
                        selectAtt = value;
                        select = makeExpression(selectAtt, att);
                    } else {
                        checkUnknownAttribute(attName);
                    }
                }
            }
            if ((nameAtt == null)) {
                reportAbsence("name");
            }
            return name;
        }
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            if (((select != null) && hasChildNodes())) {
                string errorCode = getErrorCodeForSelectPlusContent();
                compileError((("An "+getDisplayName())+" element with a select attribute must be empty"), errorCode);
            }
            Saxon.Hej.tree.iter.AxisIterator kids = iterateAxis(Saxon.Hej.om.AxisInfo.CHILD);
            Saxon.Hej.om.NodeInfo first = ((/*1430*/Saxon.Hej.om.NodeInfo)kids.next());
            if ((select == null)) {
                if ((first == null)) {
                    select = new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.EMPTY_STRING);
                    select.setRetainedStaticContext(makeRetainedStaticContext());
                } else {
                    if (((((/*1430*/Saxon.Hej.om.NodeInfo)kids.next()) == null) && !(isExpandingText()))) {
                        if ((first.getNodeKind() == Saxon.Hej.type.Type.TEXT)) {
                            select = new Saxon.Hej.expr.StringLiteral(first.getUnicodeStringValue());
                            select.setRetainedStaticContext(makeRetainedStaticContext());
                        }
                    }
                }
            }
        }
        protected internal abstract string getErrorCodeForSelectPlusContent();
        protected internal void compileContent(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.expr.instruct.SimpleNodeConstructor inst, Saxon.Hej.expr.Expression separator) {
            if ((separator == null)) {
                separator = new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.SINGLE_SPACE);
            }
            try {
                if ((select == null)) {
                    select = compileSequenceConstructor(exec, decl, true);
                }
                select = makeSimpleContentConstructor(select, separator, getStaticContext());
                inst.setSelect(select);
            } catch (Saxon.Hej.trans.XPathException err) {
                compileError(err);
            }
        }
        public static Saxon.Hej.expr.Expression makeSimpleContentConstructor(Saxon.Hej.expr.Expression select, Saxon.Hej.expr.Expression separator, Saxon.Hej.expr.StaticContext env) {
            Saxon.Hej.expr.parser.RetainedStaticContext rsc = select.getLocalRetainedStaticContext();
            if ((rsc == null)) {
                rsc = env.makeRetainedStaticContext();
            }
            select = Saxon.Hej.expr.AdjacentTextNodeMerger.makeAdjacentTextNodeMerger(select);
            select = Saxon.Hej.expr.Atomizer.makeAtomizer(select, null);
            select = new Saxon.Hej.expr.AtomicSequenceConverter(select, Saxon.Hej.type.BuiltInAtomicType.STRING);
            select.setRetainedStaticContext(rsc);
            ((Saxon.Hej.expr.AtomicSequenceConverter)(select)).allocateConverterStatically(env.getConfiguration(), false);
            if ((select.getCardinality() != Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) {
                select = Saxon.Hej.functions.SystemFunction.makeCall("string-join", rsc, select, separator);
            }
            return select;
        }
    
    }
}