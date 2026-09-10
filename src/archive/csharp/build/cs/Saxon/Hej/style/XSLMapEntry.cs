using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLMapEntry : Saxon.Hej.style.StyleElement {
        internal Saxon.Hej.expr.Expression key = null;
        internal Saxon.Hej.expr.Expression select = null;
        public override bool isInstruction() {
            return true;
        }
        protected internal Saxon.Hej.type.ItemType getReturnedItemType() {
            return Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override void prepareAttributes() {
            string keyAtt = null;
            string selectAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                if (f.Equals("key")) {
                    keyAtt = value;
                    key = makeExpression(keyAtt, att);
                } else {
                    if (f.Equals("select")) {
                        selectAtt = value;
                        select = makeExpression(selectAtt, att);
                    } else {
                        checkUnknownAttribute(attName);
                    }
                }
            }
            if ((keyAtt == null)) {
                reportAbsence("key");
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            key = typeCheck("key", key);
            select = typeCheck("select", select);
            if ((select != null)) {
                foreach (Saxon.Hej.om.NodeInfo kid in children()) {
                    if (!(((kid is Saxon.Hej.style.XSLFallback)))) {
                        compileError("An xsl:map-entry element with a select attribute must be empty", "XTSE3280");
                        return ;
                    }
                }
            }
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            if ((select == null)) {
                select = compileSequenceConstructor(exec, decl, false);
                select = select.simplify();
            }
            Saxon.Hej.expr.Expression exp = Saxon.Hej.ma.map.MapFunctionSet.getInstance().makeFunction("entry", 2).makeFunctionCall(key, select);
            if (getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.STRICT_STREAMABILITY)) {
                exp = new Saxon.Hej.expr.instruct.SequenceInstr(exp);
            }
            return exp;
        }
    
    }
}