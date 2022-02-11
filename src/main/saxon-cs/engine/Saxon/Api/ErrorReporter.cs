using System;
using System.Collections.Generic;

namespace Saxon.Api {
    /// <summary>
    /// The <c>ErrorReporter</c> is a generic functional class for reporting errors and warnings.
    /// This class wraps an <c>IList</c> of <c>XmlProcessingError</c> objects.
    /// It is designed to be used internally by Saxon.
    /// </summary>
    [Serializable]
    public class ErrorReporter : net.sf.saxon.lib.ErrorReporter
    {

        private IList<XmlProcessingError> errorList;

        /// <summary>
        /// Initializes a new instance of the <see cref="Saxon.Api.ErrorReporter"/> class.
        /// </summary>
        /// <param name="errorList">Error list.</param>
        public ErrorReporter(IList<XmlProcessingError> errorList)
        {
            this.errorList = errorList;
        }

        /// <summary>
        /// Get property on the wrapped error list.
        /// </summary>
        public IList<XmlProcessingError> ErrorList {
            get { return errorList; }
        }
        
        /// <summary>
        /// This method is called internally in Saxon to report an error by adding it to the error list.
        /// </summary>
        /// <param name="xpe"></param>
        public void report(net.sf.saxon.s9api.XmlProcessingError xpe)
        {
            errorList.Add(new XmlProcessingError(xpe));
        }
    }
}