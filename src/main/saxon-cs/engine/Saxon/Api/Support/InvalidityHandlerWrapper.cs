using Saxon.Hej.lib;
using Saxon.Hej.om;

namespace Saxon.Api.Support {
    /// <summary>
    /// This class <c>InvalidityHandlerWrapper</c> extends the standard error handler for errors found during
    /// validation of an instance document against a schema, used if a user specifies the -report option on validate.
    /// Its effect is to output the validation errors found into the filename specified, in an XML format.
    /// This is a wrapper class to wrap a .NET <c>InvalidityHandler</c> class for interfacing within Saxon.Ejava.
    /// </summary>
    public class InvalidityHandlerWrapper : InvalidityHandler
    {

        private IInvalidityHandler inHandler;

        /// <summary>
        /// Create a standard Invalidity Handler.
        /// </summary>
        /// <param name="inHandler">The .NET <c>IInvalidityHandler</c>.</param>
        public InvalidityHandlerWrapper(IInvalidityHandler inHandler) {
            this.inHandler = inHandler;
        }


        /// <summary>
        /// Get the value to be associated with a validation exception. May return null.
        /// In the case of the <c>InvalidityGenerator</c>, this returns the XML document
        /// containing the validation report.
        /// </summary>
        /// <returns>A value (or null). This will be the value returned as the value of the variable
        /// <c>$err:value</c> during try/catch processor.</returns>
        public Sequence endReporting()
        {
            XdmValue value = inHandler.EndReporting();

            if (value != null)
            {
                return inHandler.EndReporting().Unwrap();
            }
            return null;

        }

        /// <summary>
        /// Receive notification of a validity error.
        /// </summary>
        /// <param name="i">Information about the nature of the invalidity.</param>
        public void reportInvalidity(Invalidity i)
        {
            if (i is Saxon.Hej.type.ValidationFailure) { 
                ValidationFailure error = new ValidationFailure((Saxon.Hej.type.ValidationFailure)i);
                inHandler.ReportInvalidity(error);
            }

            
        }


        /// <summary>
        /// At the start of a validation episode, initialize the handler.
        /// </summary>
        /// <param name="systemId">Optional; may be used to represent the destination of any report produced.</param>
        public void startReporting(string systemId)
        {
            inHandler.StartReporting(systemId);
        }

        
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited