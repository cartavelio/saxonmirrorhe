using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.xtupdate {
    public class SaxonInsert : Saxon.Hej.style.ExtensionInstruction {
        private Saxon.Hej.expr.Expression select;
        private string positionAtt;
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override void prepareAttributes() {
            string selectAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string value = att.getValue();
                string f = attName.getDisplayName();
                switch (f) {
                  case "select":
                    selectAtt = value;
                    select = makeExpression(selectAtt, att);
                    break ;
                  case "position":
                    string val = Saxon.Hej.value.Whitespace.trim(value);
                    if (!(((((val.Equals("after") || val.Equals("before")) || val.Equals("first")) || val.Equals("last"))))) {
                        invalidAttribute("position", "after|before|first|last");
                    }
                    positionAtt = val;
                    break ;
                  default:
                    checkUnknownAttribute(attName);
                    break ;
                }
            }
            if ((selectAtt == null)) {
                select = new Saxon.Hej.expr.ContextItemExpression();
                select.setLocation(allocateLocation());
            }
            if ((positionAtt == null)) {
                positionAtt = "last";
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            getConfiguration().checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT, "saxon:insert", getPackageData().getLocalLicenseId());
            select = typeCheck("select", select);
            /*134*/Saxon.Eej.ee.xtupdate.SaxonUpdate.checkContainment(this);
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.Expression content = compileSequenceConstructor(exec, decl, false);
            int pos = interpretPosition(positionAtt);
            return new Saxon.Eej.ee.update.InsertExpression(content, select, pos, Saxon.Hej.lib.Validation.PRESERVE, true, true);
        }
        public static int interpretPosition(string p) {
            switch (p) {
              case "first":
                return Saxon.Hej.expr.parser.Token.FIRST_INTO;
              case "last":
                return Saxon.Hej.expr.parser.Token.LAST_INTO;
              case "before":
                return Saxon.Hej.expr.parser.Token.BEFORE;
              case "after":
                return Saxon.Hej.expr.parser.Token.AFTER;
              default:
                return -1;
            }
        }
        public static string encodePosition(int p) {
            switch (p) {
              case Saxon.Hej.expr.parser.Token.FIRST_INTO:
                return "first";
              case Saxon.Hej.expr.parser.Token.LAST_INTO:
                return "last";
              case Saxon.Hej.expr.parser.Token.BEFORE:
                return "before";
              case Saxon.Hej.expr.parser.Token.AFTER:
                return "after";
              default:
                return "??";
            }
        }
    
    }
}