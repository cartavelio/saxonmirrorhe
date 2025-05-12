namespace Saxon.Api {
    
    /// <summary>
    /// Information about an external parameter to an XSLT stylesheet (<c>xsl:param</c>) or an XQuery query
    /// (<c>declare variable ... external;</c>)
    /// </summary>

    public class ParameterDetails
    {
        /// <summary>
        /// Create parameter details.
        /// </summary>
        /// <param name="type">The declared type of the parameter.</param>
        /// <param name="isRequired">Indicates whether the parameter is required or optional.</param>
        
        internal ParameterDetails(XdmSequenceType type, bool isRequired)
        {
            this.DeclaredType = type;
            this.Required = isRequired;
        }

        /// <summary>
        /// Gets the declared type of the parameter.
        /// </summary>
        /// <returns>The type defined in the <code>as</code> attribute of the <code>xsl:param</code> element, or
        /// the equivalent in XQuery</returns>
        
        public XdmSequenceType DeclaredType { get; }

        /// <summary>
        /// Ask whether the parameter is required (mandatory) or optional
        /// </summary>
        /// <returns><c>true</c> if the parameter is mandatory (<code>required="yes"</code>), false
        /// if it is optional.</returns>

        public bool Required { get; }
        
        /// <summary>
        /// Get details of the location of the parameter declaration
        /// </summary>
        /// <returns>
        /// The location of the declaration of this parameter
        /// </returns>

        public Location Location;
    }

}

// Copyright (c) 2021 - 2023 Saxonica Limited