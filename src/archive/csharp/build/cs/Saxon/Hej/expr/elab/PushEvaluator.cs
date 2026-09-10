using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.elab {
    public delegate Saxon.Hej.expr.instruct.TailCall PushEvaluator (Saxon.Hej.@event.Outputter @out, Saxon.Hej.expr.XPathContext context);
}