using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans.rules {
    public enum BuiltInRules {
        DEEP_COPY,
        DEEP_SKIP,
        FAIL,
        SHALLOW_COPY,
        APPLY_TEMPLATES_TO_ATTRIBUTES,
        APPLY_TEMPLATES_TO_CHILDREN,
    }
}