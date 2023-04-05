namespace Saxon.Api {
    /// <summary>
    /// Enumeration identifying the various Schema validation modes
    /// </summary>

    public enum SchemaValidationMode
    {
        /// <summary>No validation (or strip validation, which removes existing type annotations)</summary> 
        None,
        /// <summary>Strict validation</summary>
        Strict,
        /// <summary>Lax validation</summary>
        Lax,
        /// <summary>Validation mode preserve, which preserves any existing type annotations</summary>
        Preserve,
        /// <summary>Unspecified validation: this means that validation is defined elsewhere, for example in the
        /// Saxon <c>Configuration</c></summary>
        Unspecified
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited