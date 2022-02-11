using System;
using System.Collections.Generic;

namespace Saxon.Api {
    /// <summary>
    /// The <c>ErrorReporter</c> is a generic interface for reporting errors and warnings.
    /// </summary>
    [Serializable]
    public delegate void ErrorReporter(XmlProcessingError error);
}