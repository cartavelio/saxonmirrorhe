namespace Saxon.Api {
    /// <summary>
    /// The <c>IErrorReporter</c> is a generic functional interface for reporting errors and warnings.
    /// </summary>
    /// <remarks>
    /// <p>The error or warning is reported in the form of an <c>XmlProcessingError</c> which
    /// adds the possibility to report errors without expensive construction of an exception object.</p>
    /// </remarks>
    public interface IErrorReporter {
        
        /// <summary>
        /// Report an error. This method is called by Saxon when an error needs to be reported to the calling application.
        /// </summary>
        /// <param name="error">Details of the error to be reported</param>
        void report(XmlProcessingError error);

    }
}