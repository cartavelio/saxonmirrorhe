
namespace Saxon.Api {
    /// <summary>
    /// This object represents a failure detected when validating an instance against a type
    /// defined in a schema.
    /// </summary>
    /// <remarks>
    /// The object holds information about the schema constraints that were violated, and
    /// the location in the instance document where the invalidity was detected. It is
    /// not an exception, because detection of invalid instances is not considered an
    /// abnormal condition and does not cause program termination.
    /// </remarks>
    public class ValidationFailure
    {
        private readonly Saxon.Hej.type.ValidationFailure failure;

        internal ValidationFailure(Saxon.Hej.type.ValidationFailure failure) {
            this.failure = failure; 
        }

        /// <summary>
        /// Return the character position where the current document event ends.
        /// </summary>
        /// <returns>the column number, or -1 if none is available</returns>
        public int ColumnNumber => failure.getColumnNumber();

        /// <summary>
        /// Get the constraint clause number.
        /// </summary>
        /// <returns>The section number of the clause containing the constraint that has been violated.
        /// Generally a decimal number in the form n.n.n; possibly a sequence of such numbers separated
        ///  by semicolons.Or null if the information is not available.</returns>
        public string ConstraintClauseNumber => failure.getConstraintClauseNumber();

        /// <summary>
        /// Get the constraint name.
        /// </summary>
        /// <returns>The name of the violated constraint, in the form of a fragment identifier within
        ///  the published XML Schema specification; or null if the information is not available.</returns>
        public string ConstraintName => failure.getConstraintName();

        /// <summary>
        /// Get the constraint name and clause in the format defined in XML Schema Part C (Outcome Tabulations).
        /// This mandates the format validation-rule-name.clause-number
        /// </summary>
        /// <returns>The constraint reference, for example "cos-ct-extends.1.2"; or null if the reference
        ///  is not known.</returns>
        public string ConstraintReference => failure.getConstraintReference();

        /// <summary>
        /// Get additional location text, if any.
        /// </summary>
        /// <returns>Additional information about the location of the error, designed to be output
        /// as a prefix to the error message if desired.</returns>
        public string ContextPath => failure.getContextPath().ToString();

        /// <summary>
        /// Get the error code associated with the validity error. This is relevant only when validation
        /// is run from within XSLT or XQuery, which define different error codes for validation errors.
        /// </summary>
        /// <returns>The error code associated with the error, if any. The error is returned as a simple
        /// string if it is in the standard error namespace, or as an EQName (that is Q{uri}local) 
        /// otherwise.</returns>
        public string ErrorCode => failure.getErrorCode();
        
        /// <summary>
        /// Return the line number where the current document event ends.
        /// </summary>
        /// <returns>The line number, or -1 if none is available.</returns>
        public int LineNumber => failure.getLineNumber();

        /// <summary>
        /// Get the text of a message explaining what is wrong.
        /// </summary>
        /// <returns>A human-readable message explaining the validity error.</returns>
        public string Message => failure.getMessage();

        /// <summary>
        /// Get a hierarchic path giving the logical position in the instance document where the
        /// validation error was found.
        /// </summary>
        /// <returns>A path to the location in the document.</returns>
        /// 
        public string Path => failure.getPath().ToString();

        /// <summary>
        /// Return the public identifier for the current document event.
        /// </summary>
        /// <returns>A string containing the public identifier, or null if none is available.</returns>
        public string PublicId => failure.getPublicId();

        /// <summary>
        /// Get the "schema part" component of the constraint reference.
        /// </summary>
        /// <returns>1 or 2 depending on whether the violated constraint is in XML Schema Part 1 or Part 2;
        /// or -1 if there is no constraint reference.</returns>
        public int SchemaPart => failure.getSchemaPart();

        /// <summary>
        /// Return the system identifier for the current document event.
        /// </summary>
        /// <returns>A string containing the system identifier, or null if none is available.</returns>
        public string SystemId => failure.getSystemId();
    }
}

// Copyright (c) 2021 Saxonica Limited