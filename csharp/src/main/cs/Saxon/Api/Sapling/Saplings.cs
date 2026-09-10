using System;

namespace Saxon.Api.Sapling
{
    /// <summary>
    /// This is a non-instantiable class holding a number of convenience methods for creating
    /// sapling nodes of different kinds.
    /// </summary>
    public class Saplings
    {
        private Saplings()
        {
            // This class can't be instantiated.
        }
    
        /// <summary>
        /// Creates an empty sapling document.
        /// </summary>
        /// <returns>A sapling document with no base URI</returns>
        public static SaplingDocument Document()
        {
            return new SaplingDocument();
        }

        /// <summary>
        /// Creates an empty sapling document.
        /// </summary>
        /// <param name="baseUri">The base URI</param>
        /// <returns>A sapling document with the specified base URI.</returns>
        public static SaplingDocument Document(Uri baseUri)
        {
            return new SaplingDocument(baseUri);
        }

        /// <summary>
        /// Creates an empty element node.
        /// </summary>
        /// <param name="name">The element name, in no namespace</param>
        /// <returns>An empty sapling element node with no attributes and no namespaces.</returns>
        public static SaplingElement Element(string name)
        {
            return new SaplingElement(name);
        }

        /// <summary>
        /// Creates an empty element node.
        /// </summary>
        /// <param name="name">The element name.</param>
        /// <returns>An empty sapling element node with no attributes and at most one namespace declaration,
        /// for any namespace required by the name.</returns>
        public static SaplingElement Element(QName name)
        {
            return new SaplingElement(name);
        }

        /// <summary>
        /// Creates a sapling text node.
        /// </summary>
        /// <param name="text">The string value.</param>
        /// <returns>A sapling text node with the specified content.</returns>
        public static SaplingText Text(string text)
        {
            return new SaplingText(text);
        }

        /// <summary>
        /// Creates a sapling comment.
        /// </summary>
        /// <param name="value">The comment text.</param>
        /// <returns>A sapling comment node with the specified content. The content must not contain "--",
        /// but the constructor does not enforce this constraint.</returns>
        public static SaplingComment Comment(string value)
        {
            return new SaplingComment(value);
        }

        /// <summary>
        /// Creates a sapling processing instruction.
        /// </summary>
        /// <param name="target">The PI target. Must be a valid NCName, but the constructor does not enforce this constraint.</param>
        /// <param name="content">The PI content. Must not contain the string "?>", but the constructor does not enforce this constraint.</param>
        /// <returns>A sapling processing instruction with the specified values.</returns>
        public static SaplingProcessingInstruction ProcessingInstruction(string target, string content)
        {
            return new SaplingProcessingInstruction(target, content);
        }
    }
}