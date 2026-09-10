using Saxon.Hej.om;
using Saxon.Hej.type;

namespace Saxon.Api {
    /// <summary>
    /// Singleton class representing the item type <c>function(*)</c>, which matches any function item.
    /// </summary>
    public class XdmAnyFunctionType : XdmItemType {
        /// <summary>
        /// The singleton instance of this class: an <c>XdmItemType</c> corresponding to the
        /// item type <c>function(*)</c>, which matches any function item.
        /// </summary>
        public static readonly XdmAnyFunctionType Instance = new();

        private XdmAnyFunctionType() : base(AnyFunctionType.ANY_FUNCTION) {
        }
        
        /// <summary>
        /// Determine whether this item type matches a given item.
        /// </summary>
        /// <param name="item">the item to be tested against this item type</param>
        /// <returns>true if the item is a function, otherwise false.</returns>
        public override bool Matches(XdmItem item) {
            return (Item) item.value is Function;
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
            return other.Unwrap() is FunctionItemType;
        }
    }
}

// Copyright (c) 2021 Saxonica Limited