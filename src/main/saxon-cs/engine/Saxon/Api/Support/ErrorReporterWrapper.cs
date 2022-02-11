using Saxon.Api;
using ErrorReporter = net.sf.saxon.lib.ErrorReporter;
using XmlProcessingError = net.sf.saxon.s9api.XmlProcessingError;

namespace SaxonCS3.engine.Saxon.Api.Support {
    /// <summary>
    /// <c>ErrorReporter</c> wrapper class for internal use in Saxon.
    /// </summary>
    public class ErrorReporterWrapper : ErrorReporter

    {

        private IErrorReporter reporter;

        /// <summary>
        /// Create a standard <c>ErrorReporter</c>
        /// </summary>
        /// <param name="reporter">The .NET <c>IErrorReporter</c></param>
        public ErrorReporterWrapper(IErrorReporter reporter)
        {
            this.reporter = reporter;
        }

        public IErrorReporter ErrorReporter {
            get { return reporter; }
        }


        public void report(XmlProcessingError xpe)
        {
            reporter.report(new global::Saxon.Api.XmlProcessingError(xpe));
        }
    }
}