using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public abstract class BuiltInType {

        static BuiltInType() {
            register(Saxon.Hej.om.StandardNames.XS_ANY_SIMPLE_TYPE, Saxon.Hej.type.AnySimpleType.getInstance());
            register(Saxon.Hej.om.StandardNames.XS_ANY_TYPE, Saxon.Hej.type.AnyType.getInstance());
            register(Saxon.Hej.om.StandardNames.XS_UNTYPED, Saxon.Hej.type.Untyped.getInstance());
            register(Saxon.Hej.om.StandardNames.XS_ERROR, Saxon.Hej.type.ErrorType.getInstance());
        }
        private static readonly Saxon.Hej.z.IntHashMap<Saxon.Hej.type.SchemaType> lookup = new Saxon.Hej.z.IntHashMap<Saxon.Hej.type.SchemaType>(100);
        private static readonly System.Collections.Generic.IDictionary<string,Saxon.Hej.type.SchemaType> lookupByLocalName = new System.Collections.Generic.Dictionary<string,Saxon.Hej.type.SchemaType>(100);
        private BuiltInType()  {
        }
        public static Saxon.Hej.type.SchemaType getSchemaType(int fingerprint) {
            Saxon.Hej.type.SchemaType st = lookup.get(fingerprint);
            if ((st == null)) {
                if (((Saxon.Hej.type.BuiltInAtomicType.DOUBLE == null) || (Saxon.Hej.type.BuiltInListType.NMTOKENS == null))) {
                }
                st = lookup.get(fingerprint);
            }
            return st;
        }
        public static Saxon.Hej.type.SchemaType getSchemaTypeByLocalName(string name) {
            Saxon.Hej.type.SchemaType st = Saxon.Impl.Helpers.MapUtils.Get(lookupByLocalName, name);
            if ((st == null)) {
                if (((Saxon.Hej.type.BuiltInAtomicType.DOUBLE == null) || (Saxon.Hej.type.BuiltInListType.NMTOKENS == null))) {
                }
                st = Saxon.Impl.Helpers.MapUtils.Get(lookupByLocalName, name);
            }
            return st;
        }
        public static void register(int fingerprint, Saxon.Hej.type.SchemaType type) {
            lookup.put(fingerprint, type);
            Saxon.Impl.Helpers.MapUtils.Put(lookupByLocalName, type.getName(), type);
        }
    
    }
}