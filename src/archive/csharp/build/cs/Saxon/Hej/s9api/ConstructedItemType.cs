using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    internal class ConstructedItemType : Saxon.Api.XdmItemType {
    // Injecting C# code
        public override bool Matches(Saxon.Api.XdmItem item) {return matches(item);}
        public override bool Subsumes(Saxon.Api.XdmItemType other) {return subsumes(other);}
    // End of injected C# code
        private readonly Saxon.Hej.Configuration config;
        protected internal ConstructedItemType(Saxon.Hej.type.ItemType underlyingType, Saxon.Hej.Configuration config) : base (underlyingType) {
            Saxon.Ejava.util.Objects.requireNonNull(config);
            Saxon.Ejava.util.Objects.requireNonNull(underlyingType);
            this.config = config;
        }
        public Saxon.Hej.lib.ConversionRules getConversionRules() {
            return config.getConversionRules();
        }
        public bool matches(Saxon.Api.XdmItem item) {
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            return underlyingType.matches(((Saxon.Hej.om.Item)item.Unwrap()), th);
        }
        public bool subsumes(Saxon.Api.XdmItemType other) {
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            return th.isSubType(((Saxon.Hej.type.ItemType)other.Unwrap()), underlyingType);
        }
        public Saxon.Hej.type.ItemType getUnderlyingItemType() {
            return underlyingType;
        }
        protected internal Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
    
    }
}