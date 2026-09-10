using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class CompileStylesheet : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.value.ObjectValue<Saxon.Hej.PreparedStylesheet> call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.NodeInfo doc = (Saxon.Hej.om.NodeInfo)(arguments[0].head());
            Saxon.Hej.Configuration config = context.getConfiguration();
            Saxon.Hej.trans.CompilerInfo compilerInfo = new Saxon.Hej.trans.CompilerInfo(config.getDefaultXsltCompilerInfo());
            compilerInfo.setErrorReporter(context.getErrorReporter());
            compilerInfo.setResourceResolver(context.getResourceResolver());
            Saxon.Hej.PreparedStylesheet pss = Saxon.Hej.style.Compilation.compileSingletonPackage(config, compilerInfo, doc.asActiveSource());
            return new Saxon.Hej.value.ObjectValue<Saxon.Hej.PreparedStylesheet>(pss);
        }
    
    }
}