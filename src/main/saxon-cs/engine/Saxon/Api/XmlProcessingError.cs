using System;
using net.sf.saxon.trans;

namespace Saxon.Api {
    /// <summary>
    /// The <c>XmlProcessorError</c> class contains information about an error detected during
    /// compilation or execution of a stylesheet, query, XPath expression, or schema.
    /// </summary>
    public class XmlProcessingError : StaticError
    {
        net.sf.saxon.s9api.XmlProcessingError error;
        internal XmlProcessingError(net.sf.saxon.s9api.XmlProcessingError err) : base(XPathException.fromXmlProcessingError(err))
        {
            this.error = err;
        }

        /// <summary>
        /// This get property returns a <c>XmlProcessingError</c> containing the same information, but to be treated as a warning condition.
        /// </summary>
        public XmlProcessingError AsWarning {
            get {
                XmlProcessingError e2 = new XmlProcessingError(error);
                e2.IsWarning = true;
                return e2;
            }
        }

        /// <summary>
        /// Property to get the host language where this error originates from.
        /// </summary>
        public HostLanguage HostLanguage {

            get {
                if (error.getHostLanguage() == net.sf.saxon.s9api.HostLanguage.XSLT)
                {
                    return HostLanguage.XSLT;
                }
                else {
                    return HostLanguage.XPATH;
                }
            }
        }

        /// <summary>
        /// Property to indicate that this error is to be treated as fatal, that is,
        /// execution will be abandoned after reporting this error. This method may be called
        /// by <c>ErrorReporter</c>, for example if the error is considered so server that further processing
        /// is not worthwhile, or if too many errors have been signalled. there is no absolute guarantee that
        /// setting this property will cause execution to be abandoned.
        /// It can also be used to ask whether this error is to be treated as fatal, 
        /// and if so, return the relevant message.
        /// </summary>
        public String FatalMessage {

            set {
                error.setFatal(value);
            }
            get {
                return error.getFatalErrorMessage();
            }
        }

        /// <summary>
        /// Property to check if this error is a <c>StaticError</c>.
        /// </summary>
        public bool StaticError {
            get { return error.isStaticError(); }
        }

        /// <summary>
        /// Property which indicates whether this condition is a type error.
        /// </summary>
        public bool TypeError {
            get { return error.isTypeError(); }
        }


        /// <summary>
        /// Ask whether this static error has already been reported.
        /// </summary>
        /// <returns></returns>
        public bool IsAlreadyReported() {
            return error.isAlreadyReported();
        }


        /// <summary>
        /// Say whether this error has already been reported.
        /// </summary>
        /// <param name="reported">true if the error has been reported</param>
        public void SetAlreadyReported(bool reported)
        {
            error.setAlreadyReported(reported);
        }

    }
}