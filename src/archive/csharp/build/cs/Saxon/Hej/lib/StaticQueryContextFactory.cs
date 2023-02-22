using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class StaticQueryContextFactory {
        public virtual  /* REPLACING public */ Saxon.Hej.query.StaticQueryContext newStaticQueryContext(Saxon.Hej.Configuration config, bool copyFromDefault) {
            return new Saxon.Hej.query.StaticQueryContext(config, copyFromDefault);
        }
    
    }
}