using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.query {
    public abstract class QueryLibrary : Saxon.Hej.query.QueryModule {
        public QueryLibrary(Saxon.Hej.query.StaticQueryContext sqc) : base (sqc) {
        }
        public abstract void link(Saxon.Hej.query.QueryModule importer);
    
    }
}