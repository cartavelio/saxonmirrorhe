using net.sf.saxon.ma.map;
using net.sf.saxon.om;

namespace Saxon.Api {
    /// <summary>
    /// Singleton class representing the item type <c>map(*)</c>, which matches any map.
    /// </summary>
    public class XdmAnyMapType : XdmItemType {
        /// <summary>
        /// The singleton instance of this class: an <c>XdmMapType</c> corresponding to the
        /// item type <c>map(*)</c>, which matches any map.
        /// </summary>
        public static XdmAnyMapType Instance = new XdmAnyMapType();

        internal XdmAnyMapType() {
            this.type = MapType.ANY_MAP_TYPE;
        }
        
        public override bool Matches(XdmItem item) {
            return ((Item) item.value).getGenre() == Genre.MAP;
        }
    }
}