using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public enum SchemaValidationStatus {
        UNVALIDATED,
        FIXED_UP,
        VALIDATING,
        VALIDATED,
        INVALID,
        INCOMPLETE,
    }
}