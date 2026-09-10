using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class LengthFacet : Saxon.Eej.ee.schema.LengthRangeFacet {
        public override string getName() {
            return "length";
        }
        public override void checkFacetRestriction(Saxon.Eej.ee.schema.UserSimpleType type, Saxon.Hej.type.SimpleType @base, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((@base is Saxon.Eej.ee.schema.UserSimpleType)) {
                Saxon.Eej.ee.schema.UserSimpleType ubase = (Saxon.Eej.ee.schema.UserSimpleType)(@base);
                Saxon.Eej.ee.schema.Facet baseFacet = ubase.getFacet(typeof(Saxon.Eej.ee.schema.LengthFacet));
                if ((baseFacet != null)) {
                    if ((requiredLength != ((Saxon.Eej.ee.schema.LengthFacet)(baseFacet)).requiredLength)) {
                        throw new Saxon.Hej.type.SchemaException(("In a type derived by restriction, "+"the length facet must not be changed"));
                    }
                }
                baseFacet = ubase.getFacet(typeof(Saxon.Eej.ee.schema.MinLengthFacet));
                if ((baseFacet != null)) {
                    if ((requiredLength<baseFacet.toLong())) {
                        throw new Saxon.Hej.type.SchemaException(("In a type derived by restriction, "+"the length facet is shorter than the minLength of the base type"));
                    }
                    if ((baseFacet.isFixed() && (requiredLength != baseFacet.toLong()))) {
                        throw new Saxon.Hej.type.SchemaException("The minLength facet is fixed in the base type and cannot be varied");
                    }
                }
                baseFacet = ubase.getFacet(typeof(Saxon.Eej.ee.schema.MaxLengthFacet));
                if ((baseFacet != null)) {
                    if ((requiredLength>baseFacet.toLong())) {
                        throw new Saxon.Hej.type.SchemaException(("In a type derived by restriction, "+"the length facet is longer than the maxLength of the base type"));
                    }
                    if ((baseFacet.isFixed() && (requiredLength != baseFacet.toLong()))) {
                        throw new Saxon.Hej.type.SchemaException("The maxLength facet is fixed in the base type and cannot be varied");
                    }
                }
            }
            Saxon.Eej.ee.schema.Facet minLength = type.getFacet(typeof(Saxon.Eej.ee.schema.MinLengthFacet));
            if ((minLength != null)) {
                if ((minLength.toLong()>requiredLength)) {
                    throw new Saxon.Hej.type.SchemaException("The minLength facet is longer than the length facet");
                }
                long thisMin = minLength.toLong();
                Saxon.Hej.type.SchemaType parent = @base;
                bool ok = false;
                while ((parent is Saxon.Eej.ee.schema.UserSimpleType)) {
                    Saxon.Eej.ee.schema.Facet parentMinLength = ((Saxon.Eej.ee.schema.UserSimpleType)(parent)).getFacet(typeof(Saxon.Eej.ee.schema.MinLengthFacet));
                    if ((((parentMinLength != null) && (parentMinLength.toLong() == thisMin)) && (((Saxon.Eej.ee.schema.UserSimpleType)(parent)).getFacet(typeof(Saxon.Eej.ee.schema.LengthFacet)) == null))) {
                        ok = true;
                        break ;
                    }
                    parent = parent.getBaseType();
                }
                if (((thisMin == 1) && (parent is Saxon.Hej.type.BuiltInListType))) {
                    ok = true;
                }
                if (!(ok)) {
                    throw new Saxon.Hej.type.SchemaException(("A type cannot have both a length and a minLength facet unless "+"it is derived from a type having the same minLength and no length facet"));
                }
            }
            Saxon.Eej.ee.schema.Facet maxLength = type.getFacet(typeof(Saxon.Eej.ee.schema.MaxLengthFacet));
            if ((maxLength != null)) {
                if ((maxLength.toLong()<requiredLength)) {
                    throw new Saxon.Hej.type.SchemaException("The maxLength facet is shorter than the length facet");
                }
                long thisMax = maxLength.toLong();
                Saxon.Hej.type.SchemaType parent = @base;
                bool ok = false;
                while ((parent is Saxon.Eej.ee.schema.UserSimpleType)) {
                    Saxon.Eej.ee.schema.Facet parentMaxLength = ((Saxon.Eej.ee.schema.UserSimpleType)(parent)).getFacet(typeof(Saxon.Eej.ee.schema.MaxLengthFacet));
                    if ((((parentMaxLength != null) && (parentMaxLength.toLong() == thisMax)) && (((Saxon.Eej.ee.schema.UserSimpleType)(parent)).getFacet(typeof(Saxon.Eej.ee.schema.LengthFacet)) == null))) {
                        ok = true;
                        break ;
                    }
                    parent = parent.getBaseType();
                }
                if (!(ok)) {
                    throw new Saxon.Hej.type.SchemaException(("A type cannot have both a length and a maxLength facet unless "+"it is derived from a type having the same maxLength and no length facet"));
                }
            }
        }
        public override bool testLength(int actualLength) {
            return (actualLength == requiredLength);
        }
    
    }
}