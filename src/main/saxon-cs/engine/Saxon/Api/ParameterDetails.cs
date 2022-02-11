namespace Saxon.Api {
    
    /// <summary>
    /// Information about an external parameter to an XSLT stylesheet (<c>xsl:param</c>) or an XQuery query
    /// (<c>declare variable ... external;</c>)
    /// </summary>

    public class ParameterDetails
    {
        private readonly XdmSequenceType type;

        /// <summary>
        /// Create parameter details.
        /// </summary>
        /// <param name="type">The declared type of the parameter.</param>
        /// <param name="isRequired">Indicates whether the parameter is required or optional.</param>
        
        internal ParameterDetails(XdmSequenceType type, bool isRequired)
        {
            this.type = type;
            this.Required = isRequired;
        }

        /// <summary>
        /// Gets the declared item type of the parameter.
        /// </summary>
        /// <returns>The type defined in the <code>as</code> attribute of the <code>xsl:param</code> element,
        /// without its occurrence indicator</returns>
        public XdmItemType DeclaredItemType => type.itemType;

        /// <summary>
        /// Gets the declared cardinality of the parameter.
        /// </summary>
        /// <returns>The occurrence indicator from the type appearing in the <code>as</code> attribute
        /// of the <code>xsl:param</code> element</returns>

        public char GetDeclaredCardinality()
        {
            return type.occurrenceIn;
        }

        /// <summary>
        /// Gets the underlying declared type of the parameter.
        /// </summary>
        /// <returns>The underlying declared type.</returns>

        public XdmSequenceType GetUnderlyingDeclaredType()
        {
            return type;
        }


        /// <summary>
        /// Ask whether the parameter is required (mandatory) or optional
        /// </summary>
        /// <returns><c>true</c> if the parameter is mandatory (<code>required="yes"</code>), false
        /// if it is optional.</returns>

        public bool Required { set; get; }
        
        /// <summary>
        /// Get details of the location of the variable declaration
        /// </summary>

        public Location Location;
    }

}

// Copyright (c) 2021 Saxonica Limited