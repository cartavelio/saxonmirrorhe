using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public interface ExportAgent {
        void export(Saxon.Hej.trace.ExpressionPresenter @out);
    
    }
}