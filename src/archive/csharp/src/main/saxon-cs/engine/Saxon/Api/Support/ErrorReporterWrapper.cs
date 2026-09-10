
using ErrorReporter = Saxon.Hej.lib.ErrorReporter;
using XmlProcessingError = Saxon.Hej.s9api.XmlProcessingError;

namespace Saxon.Api.Support {
    /// <summary>
    /// <c>ErrorReporter</c> wrapper class for internal use in Saxon. Creates
    /// an instance of <c>Saxon.Hej.lib.ErrorReporter</c> that delegates
    /// to an instance of <c>Saxon.Api.ErrorReporter</c>
    /// </summary>
    public class ErrorReporterWrapper : Saxon.Hej.lib.ErrorReporter

    {

        private readonly Saxon.Api.ErrorReporter reporter;

        /// <summary>
        /// Create a standard <c>ErrorReporter</c>
        /// </summary>
        /// <param name="reporter">The .NET <c>ErrorReporter</c></param>
        public ErrorReporterWrapper(Saxon.Api.ErrorReporter reporter)
        {
            this.reporter = reporter;
        }

        public Saxon.Api.ErrorReporter ErrorReporter => reporter;
        

        public void report(Saxon.Hej.s9api.XmlProcessingError error) {
            this.reporter.Invoke(new Saxon.Api.Error(error));
        }
    }
}
// Copyright (c) 2021 Saxonica Limited