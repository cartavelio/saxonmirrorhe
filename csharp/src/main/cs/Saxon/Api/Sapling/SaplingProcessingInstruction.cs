using Saxon.Ejava.lang;

namespace Saxon.Api.Sapling
{
    /// <summary>
    /// A processing instruction node in a sapling tree.
    /// </summary>
    public class SaplingProcessingInstruction: SaplingNode
    {
        /// <summary>
        /// The processing instruction target.
        /// </summary>
        public string Target
        {
            get
            {
                return ((Saxon.Hej.sapling.SaplingProcessingInstruction)javaDelegate).getName();
            }
        }

        /// <summary>
        /// The processing instruction content.
        /// </summary>
        public string Content
        {
            get
            {
                return ((Saxon.Hej.sapling.SaplingProcessingInstruction)javaDelegate).getStringValue();
            }
        }
        
        /// <summary>
        /// Constructs a new processing instruction sapling node with the specified values.
        /// </summary>
        /// <param name="target">The PI target, must not be null</param>
        /// <param name="data">The PI content, must not be null</param>
        /// <exception cref="NullPointerException">If either target or data is null.</exception>
        public SaplingProcessingInstruction(string target, string data)
        {
            NodeKind = XdmNodeKind.ProcessingInstruction;
            javaDelegate = new Saxon.Hej.sapling.SaplingProcessingInstruction(target, data);
        }
    }
}