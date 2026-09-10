using System;
using System.Collections.Generic;

namespace Saxon.Api {
    /// <summary>
    /// The <c>ErrorReporter</c> is a generic interface for reporting errors and warnings.
    /// </summary>
    [Serializable]
    public delegate void ErrorReporter(XmlProcessingError error);

    // {
    //
    // private IList<XmlProcessingError> errorList;
    //
    // /// <summary>
    // /// Initializes a new instance of the <see cref="Saxon.Api.ErrorReporter"/> class.
    // /// </summary>
    // /// <param name="errorList">Error list.</param>
    // public ErrorReporter(IList<XmlProcessingError> errorList) {
    //     this.errorList = errorList;
    // }
    //
    // /// <summary>
    // /// Get property on the wrapped error list.
    // /// </summary>
    // public IList<XmlProcessingError> ErrorList {
    //     get { return errorList; }
    // }
    //
    // /// <summary>
    // /// This method is called internally in Saxon to report an error by adding it to the error list.
    // /// </summary>
    // /// <param name="xpe"></param>
    // public void report(net.sf.saxon.s9api.XmlProcessingError xpe) {
    //     errorList.Add(new XmlProcessingError(xpe));
    // }
    // }
}