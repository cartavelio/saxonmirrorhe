using System;
using System.Collections.Generic;

namespace Saxon.Api {
    /// <summary>
    /// The <c>ErrorReporter</c> is a generic interface for reporting errors and warnings.
    /// </summary>
    /// <param name="error">The error condition being reported. Note that this is not an
    /// exception object (it cannot be thrown).</param>
    [Serializable]
    public delegate void ErrorReporter(Error error);
}

// Copyright (c) 2021 Saxonica Limited