using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.expr {
    public class QueryLibraryImpl : Saxon.Hej.query.QueryLibrary {
        public QueryLibraryImpl(Saxon.Hej.query.StaticQueryContext sqc) : base (sqc) {
        }
        public override void link(Saxon.Hej.query.QueryModule importer) {
            Saxon.Hej.query.XQueryFunctionLibrary lib = getGlobalFunctionLibrary();
            foreach (Saxon.Hej.query.XQueryFunction fn in lib.getFunctionDefinitions()) {
                importer.getTopLevelModule().getGlobalFunctionLibrary().declareFunction(fn);
            }
            foreach (Saxon.Hej.expr.instruct.GlobalVariable libvar in getImportedGlobalVariables()) {
                Saxon.Hej.expr.instruct.GlobalVariable newvar = ((libvar is Saxon.Hej.expr.instruct.GlobalParam) ? new Saxon.Hej.expr.instruct.GlobalParam() : new Saxon.Hej.expr.instruct.GlobalVariable());
                newvar.setPackageData(importer.getPackageData());
                newvar.setVariableQName(libvar.getVariableQName());
                newvar.setRequiredType(libvar.getRequiredType());
                newvar.setBody(libvar.getBody());
                newvar.setLineNumber(libvar.getLineNumber());
                newvar.setColumnNumber(libvar.getColumnNumber());
                newvar.setSystemId(libvar.getSystemId());
                newvar.setOriginalVariable(libvar);
                importer.declareVariable(newvar);
            }
        }
    
    }
}