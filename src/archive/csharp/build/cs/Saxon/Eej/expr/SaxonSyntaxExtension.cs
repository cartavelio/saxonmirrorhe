using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.expr {
    public class SaxonSyntaxExtension : Saxon.Hej.expr.parser.ParserExtension {
        public SaxonSyntaxExtension()  {
        }
        public override Saxon.Hej.expr.Expression parseTypePattern(Saxon.Hej.expr.parser.XPathParser p) {
            Saxon.Hej.type.ItemType type = p.parseItemType();
            return new Saxon.Hej.pattern.ItemTypePattern(type);
        }
        public override void handleExternalFunctionDeclaration(Saxon.Hej.query.XQueryParser p, Saxon.Hej.query.XQueryFunction func) {
            Saxon.Hej.query.QueryModule qenv = (Saxon.Hej.query.QueryModule)(p.getStaticContext());
            Saxon.Hej.trans.SymbolicName.F fName = new Saxon.Hej.trans.SymbolicName.F(func.getFunctionName(), func.getNumberOfParameters());
            if (!(qenv.getFunctionLibrary().isAvailable(fName, qenv.getXPathVersion()))) {
                p.grumble((("External function "+fName)+" is not available"), "XPST0017");
            }
            Saxon.Hej.om.FunctionItem ext = qenv.getFunctionLibrary().getFunctionItem(fName, qenv);
            if ((ext == null)) {
                p.grumble((("External function "+fName)+" is not available"), "XPST0017");
            } else {
                Saxon.Hej.type.TypeHierarchy th = qenv.getConfiguration().getTypeHierarchy();
                if (!(ext.getFunctionItemType().getResultType().isSameType(func.getResultType(), th))) {
                    p.grumble(((("The declared return type of the external function ("+ext.getFunctionItemType().getResultType())+") does not match the return type of the actual function (")+func.getResultType()));
                }
                for (int i = 0; (i<ext.getArity()); i++) {
                    if (!(ext.getFunctionItemType().getArgumentTypes()[i].isSameType(func.getArgumentTypes()[i], th))) {
                        p.grumble(((((("The declared type of argument "+i)+" of the external function (")+ext.getFunctionItemType().getArgumentTypes()[i])+") does not match the argument type of the actual function (")+func.getArgumentTypes()[i]));
                    }
                }
            }
        }
    
    }
}