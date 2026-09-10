
namespace Saxon.Api {
    /// <summary>
    /// Interface for reporting validation errors found during validation of an instance document
    /// against a schema.
    /// </summary>
    public interface IInvalidityHandler {

        /// <summary>
        /// At the start of a validation episode, initialize the handler.
        /// </summary>
        /// <param name="systemId">Optional; may be used to represent the destination of any
        /// report produced.</param>
		/**public**/ void StartReporting(string systemId);


        /// <summary>
        /// Report a validation error found during validation of an instance document
        /// against a schema.
        /// </summary>
        /// <param name="i">Details of the validation error.</param>
		/**public**/ void ReportInvalidity (ValidationFailure i);
		

        /// <summary>
        /// At the end of a validation episode, do any closedown actions, and optionally return
        /// information collected in the course of validation (for example a list of error messages).
        /// </summary>
        /// <returns>A value to be associated with a validation exception. May be the empty sequence.
        /// In the case of the <c>InvalidityReportGenerator</c>, this returns the XML document
        /// containing the validation report. This will be the value returned as the value of
        /// the variable <c>$err:value</c> during try/catch processing.</returns>
		/**public**/ XdmValue EndReporting(); 

    }
}

// Copyright (c) 2021 Saxonica Limited