using net.sf.saxon.ma.arrays;
using net.sf.saxon.om;
using net.sf.saxon.s9api;

namespace Saxon.Api {
    /// <summary>
    /// Singleton class representing the item type <c>array(*)</c>, which matches any array.
    /// </summary>
    public class XdmAnyArrayType : XdmItemType {
        /// <summary>
        /// The singleton instance of this class: an <c>XdmArrayType</c> corresponding to the
        /// item type <c>array(*)</c>, which matches any array.
        /// </summary>
        public static XdmAnyArrayType Instance = new XdmAnyArrayType();

        internal XdmAnyArrayType() {
            this.type = ArrayItemType.ANY_ARRAY_TYPE;
        }

        public override bool Matches(XdmItem item) {
            return ((Item) item.value).getGenre() == Genre.ARRAY;
        }
    }
}