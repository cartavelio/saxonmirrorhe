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

        public override bool Matches(XdmItem item) {
            return ((Item) item.value).getGenre() == Genre.ARRAY;
        }

        public override bool Subsumes(XdmItemType other) {
            return other.Unwrap() is ArrayItemType;
        }
    }
}

// Copyright (c) 2021 Saxonica Limited