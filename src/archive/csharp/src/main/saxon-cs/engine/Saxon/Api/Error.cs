

using Saxon.Hej.expr.parser;

namespace Saxon.Api {
    /// <summary>
    /// The <c>Saxon.Api.Error</c> class contains information about an error detected during
    /// compilation or execution of a stylesheet, query, XPath expression, or schema. Note
    /// that this class is not an exception (though in some cases it may wrap an exception);
    /// errors in schemas or stylesheets are considered as normal events and do not cause
    /// termination of processing, so they are not delivered as exceptions.
    /// </summary>
    /// <remarks>
    /// The constructor for this class is internal; instances of this class can be created
    /// internally by SaxonCS, but not by user application code.
    /// </remarks>
    public class Error
    {
        private readonly Saxon.Hej.s9api.XmlProcessingError incident;
        internal Error(Saxon.Hej.s9api.XmlProcessingError err)
        {
            this.incident = err;
        }

        /// <summary>
        /// The error message
        /// </summary>
        public string Message => incident.getMessage();
        
        /// <summary>
        /// The error code (if any)
        /// </summary>
        public QName ErrorCode => incident.getErrorCode();
        
        /// <summary>
        /// The location of the error (typically identifying a module of a schema, query,
        /// or stylesheet, and a line and column number within that module).
        /// </summary>
        public Location Location => new (incident.getLocation() == null ? Loc.NONE : incident.getLocation());

        /// <summary>
        /// This property indicates whether the error is to be treated as a warning condition.
        /// </summary>
        public bool IsWarning => incident.isWarning();
        
        /// <summary>
        /// The underlying cause. This many be null.
        /// </summary>
        public System.Exception Cause => null;

        /// <summary>
        /// Return an equivalent error condition, to be treated as a warning. The original error is not changed.
        /// </summary>
        public Error AsWarning() {
            return new(incident.asWarning());
        }

        /// <summary>
        /// Property to get the host language where this error originates from.
        /// </summary>
        public HostLanguage HostLanguage => incident.getHostLanguage();

        /// <summary>
        /// Property to indicate that this error is to be treated as fatal, and if so,
        /// to supply a message that will be used on termination. 
        /// Execution will be abandoned after reporting this error. This method may be called
        /// by an <c>ErrorReporter</c>, for example if the error is considered so severe that further processing
        /// is not worthwhile, or if too many errors have been signalled. There is no absolute guarantee that
        /// setting this property will cause execution to be abandoned.
        /// The property can also be used to ask whether this error is to be treated as fatal.
        /// </summary>
        public string TerminationMessage {
            get => incident.getTerminationMessage();
            set => incident.setTerminationMessage(value);
        }

        /// <summary>
        /// Property to check if this error is a <c>StaticError</c>.
        /// </summary>
        public bool IsStaticError => incident.isStaticError();

        /// <summary>
        /// Property which indicates whether this condition is a type error.
        /// </summary>
        public bool IsTypeError => incident.isTypeError();

        /// <summary>
        /// Property indicating whether this static error has already been reported.
        /// </summary>
        public bool IsAlreadyReported {
            get => incident.isAlreadyReported();
            set => incident.setAlreadyReported(value);
        }
        

    }
}

// Copyright (c) 2021 Saxonica Limited