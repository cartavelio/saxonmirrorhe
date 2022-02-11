using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public class AnyExternalObjectType : Saxon.Hej.type.ItemType {
        public static Saxon.Hej.type.AnyExternalObjectType THE_INSTANCE = new Saxon.Hej.type.AnyExternalObjectType();
        protected internal AnyExternalObjectType()  {
        }
        public bool isAtomicType() {
            return false;
        }
        public virtual bool matches(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            return (item.getGenre() == Saxon.Hej.om.Genre.EXTERNAL);
        }
        public virtual bool isPlainType() {
            return false;
        }
        public int getPrimitiveType() {
            return -1;
        }
        public string getBasicAlphaCode() {
            return "X";
        }
        public virtual Saxon.Hej.type.ItemType getPrimitiveItemType() {
            return this;
        }
        public Saxon.Hej.type.UType getUType() {
            return Saxon.Hej.type.UType.EXTENSION;
        }
        public Saxon.Hej.type.PlainType getAtomizedItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.STRING;
        }
        public bool isAtomizable(Saxon.Hej.type.TypeHierarchy th) {
            return true;
        }
        public Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.EXTERNAL;
        }
        public double getDefaultPriority() {
            return -1;
        }
    
        /* Expanding default method getNormalizedDefaultPriority from interface ItemType */
        public virtual double getNormalizedDefaultPriority() {
            return (((getDefaultPriority()+1))/2);
        }

        /* Expanding default method getFullAlphaCode from interface ItemType */
        public virtual string getFullAlphaCode() {
            return getBasicAlphaCode();
        }

        /* Expanding default method toExportString from interface ItemType */
        public virtual string toExportString() {
            return ToString();
        }

        /* Expanding default method explainMismatch from interface ItemType */
        public virtual Saxon.Ejava.util.Optional<string> explainMismatch(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            return Saxon.Ejava.util.Optional<string>.empty();
        }

    }
}