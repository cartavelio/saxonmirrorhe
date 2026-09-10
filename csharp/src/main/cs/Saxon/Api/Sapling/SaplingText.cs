namespace Saxon.Api.Sapling
{
    /// <summary>
    /// Constructs a new sapling text node with the specified text.
    /// </summary>
    public class SaplingText: SaplingNode
    {
        /// <summary>
        /// The string value of the node.
        /// </summary>
        public string StringValue => ((Saxon.Hej.sapling.SaplingText)javaDelegate).getStringValue();

        /// <summary>
        /// Constructs a new sapling text node.
        /// </summary>
        /// <param name="text">The text content.</param>
        public SaplingText(string text)
        {
            NodeKind = XdmNodeKind.Text;
            javaDelegate = new Saxon.Hej.sapling.SaplingText(text);
        }
    }
}