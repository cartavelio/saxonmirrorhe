using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public interface ITemplateCall : Saxon.Hej.expr.ContextOriginator {
        Saxon.Hej.expr.instruct.WithParam[] getActualParams();
        Saxon.Hej.expr.instruct.WithParam[] getTunnelParams();
    
    }
}