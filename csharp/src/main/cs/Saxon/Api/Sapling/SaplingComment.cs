using Saxon.Ejava.lang;

namespace Saxon.Api.Sapling
{
    /// <summary>
    /// A comment node in a sapling tree.
    /// </summary>
    public class SaplingComment: SaplingNode
    {
        /// <summary>
        /// The string value of the comment.
        /// </summary>
        public string StringValue
        {
            get
            {
                return ((Saxon.Hej.sapling.SaplingComment)javaDelegate).getStringValue();
            }
        }
        
        /// <summary>
        /// Construct a comment with the given string value.
        /// </summary>
        /// <param name="value">The string value of the comment node. This should not contain "--" as a substring,
        /// but that condition is not checked.</param>
        /// <exception cref="NullPointerException">Throws a <c>NullPointerException</c> if the value is null.</exception>
        public SaplingComment(string value)
        {
            NodeKind = XdmNodeKind.Comment;
            javaDelegate = new Saxon.Hej.sapling.SaplingComment(value);
        }
    }
}