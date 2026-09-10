using net.sf.saxon.s9api;

namespace Saxon.Api {
    /// <summary>
    /// Singleton class representing the item type <c>node()</c>, which matches any node.
    /// </summary>

    public class XdmAnyNodeType : XdmItemType
    {

        /// <summary>
        /// The singleton instance of this class: an <c>XdmItemType</c> corresponding to the
        /// item type <c>node()</c>, which matches any node.
        /// </summary>

        public static XdmAnyNodeType Instance = new XdmAnyNodeType();

        internal XdmAnyNodeType()
        {
            this.type = ItemType.ANY_NODE;
        }

        /// <summary>
        /// Determine whether this item type matches a given item.
        /// </summary>
        /// <param name="item">the item to be tested against this item type</param>
        /// <returns>true if the item matches this item type, false if it does not match.</returns>
        public override bool Matches(XdmItem item)
        {
            return ((net.sf.saxon.om.Item)item.value) is net.sf.saxon.om.NodeInfo;
        }

        /// <summary>
        ///  Determine whether this ItemType subsumes another ItemType. Specifically,
        /// <code>A.subsumes(B)</code> is true if every value that matches the ItemType B also matches
        /// the ItemType A.
        /// </summary>
        /// <param name="other">the other ItemType</param>
        /// <returns>true if this ItemType subsumes the other ItemType. This includes the case where A and B
        ///         represent the same ItemType.</returns>
        public override bool Subsumes(XdmItemType other)
        {
            return true;
        }
    }
}