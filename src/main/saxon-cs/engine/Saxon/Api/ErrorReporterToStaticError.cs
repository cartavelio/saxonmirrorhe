using System;
using System.Collections.Generic;

namespace Saxon.Api {
    /// <summary>
    /// <c>ErrorReporter</c> for gathering errors as <c>XmlProcessorError</c> to then
    /// convert them to a list of <c>StaticError</c>s.
    /// </summary>
    [Serializable]
    public class ErrorReporterToStaticError : net.sf.saxon.lib.ErrorReporter
    {

        private IList<StaticError> errorList;

        /// <summary>
        /// Initializes a new instance of the <see cref="Saxon.Api.ErrorReporter"/> class.
        /// </summary>
        /// <param name="errorList">Error list.</param>
        public ErrorReporterToStaticError(IList<StaticError> errorList)
        {
            this.errorList = errorList;
        }

        public IList<StaticError> ErrorList
        {
            get { return errorList; }
        }


        public void report(net.sf.saxon.s9api.XmlProcessingError xpe)
        {
            errorList.Add(new StaticError(net.sf.saxon.trans.XPathException.fromXmlProcessingError(xpe)));
        }
    }
}