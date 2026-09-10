using System;
using Saxon.Ejavax.xml.transform;
using Saxon.Hej.trans;

namespace Saxon.Api {
    /// <summary>
    /// The <c>SaxonApiException</c> class contains information about a static or dynamic error detected during
    /// compilation or evaluation of a stylesheet, query, or XPath expression, or other errors that occur
    /// when using the Saxon API.
    /// </summary>

    [Serializable]
    public class SaxonApiException : Exception
    {

        private XPathException exception;
        internal bool isWarning;

        /// <summary>
        /// Construct a <c>SaxonApiException</c> wrapping another exception (often an XPathException)
        /// </summary>
        /// <param name="err">the exception to be wrapped</param>

        public SaxonApiException(Exception err) {
            this.exception = (err is XPathException xPathException
                                ? xPathException
                                : XPathException.makeXPathException(err));
        }

        /// <summary>
        /// Construct a <c>SaxonApiException</c> with a given message (and no error code)
        /// </summary>
        /// <param name="message">The message to be associated with the exception</param>
        public SaxonApiException(string message) : this(new Saxon.Ejava.lang.Exception(message)) {
            
        }
        
        /// <summary>
        /// Construct a <c>SaxonApiException</c> with a given message, supplying an underlying exception as the cause
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="err">The underlying cause of the exception</param>
        public SaxonApiException(string message, Exception err) : this(new Saxon.Ejava.lang.Exception(message, err)) {
            
        }

        /// <summary>
        /// The error code, as a <c>QName</c>. May be null if no error code has been assigned.
        /// </summary>

        public QName ErrorCode
        {
            get
            {
                if (exception.getErrorCodeLocalPart() != null) {
                    return new QName("err",
                        exception.getErrorCodeNamespace().ToString(),
                        exception.getErrorCodeLocalPart());
                } else {
                    return null;
                }
            }
        }

        /// <summary>
        /// Return the message associated with this error.
        /// </summary>
        /// <returns>
        /// The exception message
        /// </returns>

        public override string Message => exception.getMessage();


        /// <summary>
        /// Return the message associated with this error concatenated with the message from the causing exception.
        /// </summary>
        ///  
        public string InnerMessage => exception.getMessage() + ": " + exception.getCause().Message;

        /// <summary>
        /// The URI of the query or stylesheet module in which the error was detected
        /// (as a string).
        /// </summary>
        /// <remarks>
        /// May be null if the location of the error is unknown, or if the error is not
        /// localized to a specific module, or if the module in question has no known URI
        /// (for example, if it was supplied as an anonymous <c>Stream</c>).
        /// </remarks>

        public string ModuleUri => exception.getLocator() == null ? null : exception.getLocator().getSystemId();

        /// <summary>
        /// The line number locating the error within a query or stylesheet module.
        /// </summary>
        /// <remarks>
        /// May be set to -1 if the location of the error is unknown.
        /// </remarks>        

        public int LineNumber
        {
            get
            {
                SourceLocator loc = exception.getLocator();
                if (loc == null)
                {
                    if (exception.getException() is TransformerException)
                    {
                        loc = ((TransformerException)exception.getException()).getLocator();
                        if (loc != null)
                        {
                            return loc.getLineNumber();
                        }
                    }
                    return -1;
                }
                return loc.getLineNumber();
            }
        }


        /// <summary>
        /// The column number locating the error (typically within a query or stylesheet module).
        /// </summary>
        /// <remarks>
        /// May be set to -1 if the location of the error is unknown.
        /// </remarks>        

        public int ColumnNumber
        {
            get
            {
                SourceLocator loc = exception.getLocator();
                if (loc == null)
                {
                    if (exception.getException() is TransformerException)
                    {
                        loc = ((TransformerException)exception.getException()).getLocator();
                        if (loc != null)
                        {
                            return loc.getColumnNumber();
                        }
                    }
                    return -1;
                }
                return loc.getColumnNumber();
            }
        }

        /// <summary>
        /// Indicate whether this error is being reported as a warning condition. If so, applications
        /// may ignore the condition, though the results may not be as intended.
        /// </summary>

        public bool IsWarning
        {
            get => isWarning;
            set => isWarning = value;
        }

        /// <summary>
        /// Indicate whether this condition is a type error.
        /// </summary>

        public bool IsTypeError => exception.isTypeError();

        /// <summary>
        /// Return the underlying exception. This is unstable as this is an internal object.
        /// </summary>
        /// <returns>The underlying <c>XPathException</c></returns>
        
		public Saxon.Hej.trans.XPathException UnderlyingException => exception;

        /// <summary>
        /// Return the error message.
        /// </summary>

        public override string ToString()
        {
            return exception.getMessage();
        }
    }
}

// Copyright (c) 2021 Saxonica Limited