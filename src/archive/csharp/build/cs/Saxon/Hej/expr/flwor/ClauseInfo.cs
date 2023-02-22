using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class ClauseInfo : Saxon.Hej.trace.Traceable {
        private readonly Saxon.Hej.expr.flwor.Clause clause;
        private Saxon.Hej.om.NamespaceResolver nsResolver;
        public ClauseInfo(Saxon.Hej.expr.flwor.Clause clause)  {
            this.clause = clause;
        }
        public Saxon.Hej.expr.flwor.Clause getClause() {
            return clause;
        }
        public Saxon.Hej.s9api.Location getLocation() {
            return clause.getLocation();
        }
        public Saxon.Hej.om.StructuredQName getObjectName() {
            Saxon.Hej.expr.flwor.LocalVariableBinding[] vars = clause.getRangeVariables();
            if (((vars != null) && (vars.Length>0))) {
                return vars[0].getVariableQName();
            } else {
                return null;
            }
        }
        public Saxon.Hej.om.NamespaceResolver getNamespaceResolver() {
            return nsResolver;
        }
        public void setNamespaceResolver(Saxon.Hej.om.NamespaceResolver nsResolver) {
            this.nsResolver = nsResolver;
        }
        public string getSystemId() {
            return clause.getLocation().getSystemId();
        }
        public int getLineNumber() {
            return clause.getLocation().getLineNumber();
        }
        public string getPublicId() {
            return null;
        }
        public int getColumnNumber() {
            return -1;
        }
    
        /* Expanding default method gatherProperties from interface Traceable */
        public virtual void gatherProperties(Saxon.Ejava.util.function.BiConsumer<string,System.Object> consumer) {
        }

    }
}