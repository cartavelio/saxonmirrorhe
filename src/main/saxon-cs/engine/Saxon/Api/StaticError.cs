using System;
using javax.xml.transform;
using net.sf.saxon.trans;

namespace Saxon.Api {
    /// <summary>
    /// The <c>StaticError</c> class contains information about a static error detected during
    /// compilation of a stylesheet, query, or XPath expression.
    /// </summary>

    [Serializable]
    public class StaticError : Exception
    {

        private XPathException exception;
        internal bool isWarning;

        // internal constructor: Create a new StaticError, wrapping a Saxon XPathException

        internal StaticError(java.lang.Exception err)
        {
            if (err is XPathException)
            {
                this.exception = (XPathException)err;
            }
            else
            {
                this.exception = XPathException.makeXPathException(err);
            }
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
                        exception.getErrorCodeNamespace(),
                        exception.getErrorCodeLocalPart());
                } else {
                    return null;
                }
            }
        }

        /// <summary>
        /// Return the message associated with this error.
        /// </summary>

        public override String Message
        {
            get
            {
                return exception.getMessage();
            }
        }


        /// <summary>
        /// Return the message associated with this error concatenated with the message from the causing exception.
        /// </summary> 
        public String InnerMessage
        {
            get {

                return exception.getMessage() + ": " + exception.getCause().Message;
            }

        }




        /// <summary>
        /// The URI of the query or stylesheet module in which the error was detected
        /// (as a string).
        /// </summary>
        /// <remarks>
        /// May be null if the location of the error is unknown, or if the error is not
        /// localized to a specific module, or if the module in question has no known URI
        /// (for example, if it was supplied as an anonymous <c>Stream</c>).
        /// </remarks>

        public String ModuleUri
        {
            get
            {
                if (exception.getLocator() == null)
                {
                    return null;
                }
                return exception.getLocator().getSystemId();
            }
        }

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
        /// The line number locating the error within a query or stylesheet module.
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
            get
            {
                return isWarning;
            }
            set
            {
                isWarning = value;
            }
        }

        /// <summary>
        /// Indicate whether this condition is a type error.
        /// </summary>

        public bool IsTypeError
        {
            get
            {
                return exception.isTypeError();
            }
        }

        /// <summary>
        /// Return the underlying exception. This is unstable as this is an internal object.
        /// </summary>
        /// <returns>The underlying <c>XPathException</c></returns>
        
        public XPathException UnderlyingException
        {
            get
            {
                return exception;
            }
        }

        /// <summary>
        /// Return the error message.
        /// </summary>

        public override String ToString()
        {
            return exception.getMessage();
        }
    }
}