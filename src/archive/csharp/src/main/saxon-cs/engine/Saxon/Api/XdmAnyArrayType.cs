using Saxon.Hej.ma.arrays;
using Saxon.Hej.om;

namespace Saxon.Api {
    /// <summary>
    /// Singleton class representing the item type <c>array(*)</c>, which matches any array.
    /// </summary>
    public class XdmAnyArrayType : XdmItemType {
        /// <summary>
        /// The singleton instance of this class: an <c>XdmArrayType</c> corresponding to the
        /// item type <c>array(*)</c>, which matches any array.
        /// </summary>
        public static readonly XdmAnyArrayType Instance = new XdmAnyArrayType();

        private XdmAnyArrayType() : base(ArrayItemType.ANY_ARRAY_TYPE) {
        }

        /// <summary>
        /// Determine whether this item type matches a given item.
        /// </summary>
        /// <param name="item">the item to be tested against this array type</param>
        /// <returns>true if the item matches this item type, false if it does not match.</returns>
        public override bool Matches(XdmItem item) {
            return ((Item) item.value).getGenre() == Genre.ARRAY;
        }
        
        /// <summary>
        /// Determine whether this ItemType subsumes another ItemType. Specifically,
        /// <code>A.subsumes(B)</code> is true if every value that matches the ItemType B also matches
        /// the ItemType A.
        /// </summary>
        /// <param name="other">the other ItemType</param>
        /// <returns>true if this ItemType subsumes the other ItemType. This includes the case where A and B
        ///         represent the same ItemType.</returns>

        public override bool Subsumes(XdmItemType other) {
            return other.Unwrap() is ArrayItemType;
        }
    }
}

// Copyright (c) 2021 Saxonica Limited