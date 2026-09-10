using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public delegate Saxon.Hej.om.Sequence LambdaCallable (Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments);
}