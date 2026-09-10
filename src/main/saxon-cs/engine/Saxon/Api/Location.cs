using System;
using Saxon.Hej.expr.parser;

namespace Saxon.Api {
    /// <summary>
    /// A <c>Location</c> represents the location of a construct within a containing
    /// resource. For example, it might identify the location of an error in a schema document,
    /// stylesheet module, or query module, or the location of an element that fails schema
    /// validity checking.
    /// </summary>

    public class Location
    {
        internal Location(Saxon.Hej.s9api.Location loc) {
            loc ??= Loc.NONE;
            SystemId = loc.getSystemId() == null || loc.getSystemId() == ""  ? null : new Uri(loc.getSystemId());
            LineNumber = loc.getLineNumber();
            ColumnNumber = loc.getColumnNumber();
        }

        /// <summary>
        /// The URI of the containing resource
        /// </summary>
        
        public Uri SystemId { get;  }

        /// <summary>
        /// The line number of a node relative to the start of the external entity.
        /// The value -1 indicates that the line number is not known or not applicable.
        /// </summary>
        
        public int LineNumber { get;  }
        
        /// <summary>
        /// The column number of a node relative to the start of the external entity.
        /// The value -1 indicates that the column number is not known or not applicable.
        /// </summary>
        
        public int ColumnNumber { get;  }
        
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited