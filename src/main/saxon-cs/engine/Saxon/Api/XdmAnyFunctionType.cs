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
        
        public override bool Matches(XdmItem item) {
            return (Item) item.value is Function;
        }

        public override bool Subsumes(XdmItemType other) {
            return other.Unwrap() is FunctionItemType;
        }
    }
}

// Copyright (c) 2021 Saxonica Limited