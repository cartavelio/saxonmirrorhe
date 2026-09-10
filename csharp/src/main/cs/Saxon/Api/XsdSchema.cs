namespace Saxon.Api {
    
    /// <summary>
    /// An <c>XsdSchema</c> object represents a compiled XSD schema. It is
    /// always constructed using the <c>Compile</c> methods in the
    /// <c>XsdCompiler</c> class.
    /// </summary>
    /// <remarks>
    /// <para>The abstract <c>XsdSchema</c> class is included in all Saxon editions,
    /// but the concrete subclass that represents a real schema is available
    /// only in Saxon-EE.</para>
    /// </remarks>
    public abstract class XsdSchema {
        
        /// <summary>
        /// Obtain a <c>SchemaValidator</c> that can be used to validate
        /// instance documents against this schema.
        /// </summary>

        public abstract Saxon.Api.SchemaValidator NewValidator();
        
        /// <summary>
        /// Combine this schema with another to create a new <c>XsdSchema</c>
        /// Neither of the input schemas is modified. The two schemas must be
        /// compatible, for example they must not contain conflicting definitions
        /// with the same name.
        /// </summary>
        /// <param name="other">The other schema</param>

        public abstract XsdSchema Combine(XsdSchema other);
        
        /// <summary>
        /// Export the contents of this schema to an SCM file.
        /// The destination will typically be a <c>Serializer</c>.
        /// The resulting SCM file can be used to reconstitute the compiled
        /// schema by calling <c>XsdCompiler.ImportComponents</c>.
        /// </summary>
        /// <param name="destination">The destination for the SCM file</param>

        public abstract void ExportComponents(IDestination destination);

        /// <summary>
        /// Trapdoor method to obtain access to the underlying implementation.
        /// Details may not be stable between releases.
        /// </summary>
        /// 
        public abstract Saxon.Hej.type.Schema UnderlyingSchema { get; }
    }

}