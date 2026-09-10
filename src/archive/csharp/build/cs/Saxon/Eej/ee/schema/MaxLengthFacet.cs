using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class MaxLengthFacet : Saxon.Eej.ee.schema.LengthRangeFacet {
        public override string getName() {
            return "maxLength";
        }
        public override void checkFacetRestriction(Saxon.Eej.ee.schema.UserSimpleType type, Saxon.Hej.type.SimpleType @base, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((@base is Saxon.Eej.ee.schema.UserSimpleType)) {
                Saxon.Eej.ee.schema.UserSimpleType ubase = (Saxon.Eej.ee.schema.UserSimpleType)(@base);
                Saxon.Eej.ee.schema.Facet baseFacet = ubase.getFacet(typeof(Saxon.Eej.ee.schema.LengthFacet));
                if ((baseFacet != null)) {
                    if ((requiredLength != baseFacet.toLong())) {
                        throw new Saxon.Hej.type.SchemaException(("In a type derived by restriction, "+"the maxLength facet conflicts with the length of the base type"));
                    }
                    Saxon.Hej.type.SchemaType parent = @base;
                    bool ok = false;
                    while ((parent is Saxon.Eej.ee.schema.UserSimpleType)) {
                        Saxon.Eej.ee.schema.Facet parentMaxLength = ((Saxon.Eej.ee.schema.UserSimpleType)(parent)).getFacet(typeof(Saxon.Eej.ee.schema.MaxLengthFacet));
                        if ((((parentMaxLength != null) && (parentMaxLength.toLong() == requiredLength)) && (((Saxon.Eej.ee.schema.UserSimpleType)(parent)).getFacet(typeof(Saxon.Eej.ee.schema.LengthFacet)) == null))) {
                            ok = true;
                            break ;
                        }
                        parent = parent.getBaseType();
                    }
                    if (!(ok)) {
                        throw new Saxon.Hej.type.SchemaException(("A type cannot have both a length and a maxLength facet unless"+" it is derived from a type having the same maxLength and no length facet"));
                    }
                }
                baseFacet = ubase.getFacet(typeof(Saxon.Eej.ee.schema.MinLengthFacet));
                if ((baseFacet != null)) {
                    if ((requiredLength<baseFacet.toLong())) {
                        throw new Saxon.Hej.type.SchemaException(("In a type derived by restriction, "+"the maxLength facet is shorter than the minLength of the base type"));
                    }
                }
                baseFacet = ubase.getFacet(typeof(Saxon.Eej.ee.schema.MaxLengthFacet));
                if ((baseFacet != null)) {
                    if ((requiredLength>baseFacet.toLong())) {
                        throw new Saxon.Hej.type.SchemaException(("In a type derived by restriction, "+"the maxLength facet is longer than the maxLength of the base type"));
                    }
                }
            } else {
                if ((@base is Saxon.Hej.type.BuiltInListType)) {
                    if ((requiredLength == 0)) {
                        throw new Saxon.Hej.type.SchemaException(("In a type derived by restriction from a built-in list type, "+"the maxLength facet cannot be zero because the base type has minLength=1"));
                    }
                }
            }
        }
        public override bool testLength(int actualLength) {
            return (actualLength<=requiredLength);
        }
    
    }
}