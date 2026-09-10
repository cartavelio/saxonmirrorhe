using System;

namespace Saxon.Api {
    /// <summary>
    /// An <c>IXmlLocation</c> represents the location of a node within an XML document.
    /// It is in two parts: the base URI (or system ID) of the external entity (which will usually
    /// be the XML document entity itself), and the line number of a node relative
    /// to the base URI of the containing external entity.
    /// </summary>

    public interface IXmlLocation
    {

        /// <summary>
        /// The base URI (system ID) of an external entity within an XML document.
        /// Set to null if the base URI is not known (for example, for an XML document
        /// created programmatically where no base URI has been set up).
        /// </summary>

        /**public**/
        Uri BaseUri { get; set; }

        /// <summary>
        /// The line number of a node relative to the start of the external entity.
        /// The value -1 indicates that the line number is not known or not applicable.
        /// </summary>

        /**public**/
        int LineNumber { get; set; }
    }
}