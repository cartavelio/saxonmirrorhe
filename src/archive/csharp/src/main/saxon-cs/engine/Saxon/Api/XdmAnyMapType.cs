using Saxon.Hej.ma.map;
using Saxon.Hej.om;

namespace Saxon.Api {
    /// <summary>
    /// Singleton class representing the item type <c>map(*)</c>, which matches any map.
    /// </summary>
    public class XdmAnyMapType : XdmItemType {
        /// <summary>
        /// The singleton instance of this class: an <c>XdmMapType</c> corresponding to the
        /// item type <c>map(*)</c>, which matches any map.
        /// </summary>
        public static readonly XdmAnyMapType Instance = new();

        private XdmAnyMapType() : base(MapType.ANY_MAP_TYPE) {
        }
        /// <summary>
        /// Determine whether this item type matches a given item.
        /// </summary>
        /// <param name="item">the item to be tested against this map type</param>
        /// <returns>true if the item matches this map type, false if it does not match.</returns>

        public override bool Matches(XdmItem item) {
            return ((Item) item.value).getGenre() == Genre.MAP;
        }
        /// <summary>
        /// Determine whether this ItemType subsumes another ItemType. Specifically,
        /// <code>A.subsumes(B)</code> is true if every value that matches the ItemType B also matches
        /// the ItemType A.
        /// </summary>
        /// <param name="other">the other ItemType</param>
        /// <returns>true if this map type subsumes the other ItemType. This includes the case where A and B
        ///         represent the same ItemType.</returns>

        public override bool Subsumes(XdmItemType other) {
            return other.Unwrap() is MapType;
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited