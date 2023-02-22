
using ErrorReporter = net.sf.saxon.lib.ErrorReporter;
using XmlProcessingError = net.sf.saxon.s9api.XmlProcessingError;

namespace Saxon.Api.Support {
    /// <summary>
    /// <c>ErrorReporter</c> wrapper class for internal use in Saxon.
    /// </summary>
    public class ErrorReporterWrapper : net.sf.saxon.lib.ErrorReporter

    {

        private readonly Saxon.Api.ErrorReporter reporter;

        /// <summary>
        /// Create a standard <c>ErrorReporter</c>
        /// </summary>
        /// <param name="reporter">The .NET <c>IErrorReporter</c></param>
        public ErrorReporterWrapper(Saxon.Api.ErrorReporter reporter)
        {
            this.reporter = reporter;
        }

        public Saxon.Api.ErrorReporter ErrorReporter => reporter;
        

        public void report(net.sf.saxon.s9api.XmlProcessingError error) {
            this.reporter.Invoke(new Saxon.Api.XmlProcessingError(error));
        }
    }
}