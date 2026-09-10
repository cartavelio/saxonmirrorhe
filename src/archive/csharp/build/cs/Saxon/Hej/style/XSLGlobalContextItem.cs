using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLGlobalContextItem : Saxon.Hej.style.XSLContextItem {
        public override bool isDeclaration() {
            return true;
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.tree.iter.AxisIterator prior = iterateAxis(Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING, new Saxon.Hej.pattern.SameNameTest(this));
            if ((((/*1430*/Saxon.Hej.om.NodeInfo)prior.next()) != null)) {
                compileError("xsl:global-context-item must not appear twice within the same stylesheet module", "XTSE3087");
            }
        }
        public override void index(Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.style.PrincipalStylesheetModule top) {
            prepareAttributes();
            Saxon.Hej.expr.instruct.GlobalContextRequirement req = new Saxon.Hej.expr.instruct.GlobalContextRequirement();
            req.setMayBeOmitted(isMayBeOmitted());
            req.setAbsentFocus(isAbsentFocus());
            req.addRequiredItemType(getRequiredContextItemType());
            try {
                top.getStylesheetPackage().setContextItemRequirements(req);
            } catch (Saxon.Hej.trans.XPathException e) {
                e.setLocation(decl.getSourceElement());
                throw;
            }
        }
    
    }
}