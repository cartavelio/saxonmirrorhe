using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class CurrentItemExpression : Saxon.Hej.expr.ContextItemExpression {
        public CurrentItemExpression()  {
            setErrorCodeForUndefinedContext("XTDE1360", false);
        }
    
    }
}