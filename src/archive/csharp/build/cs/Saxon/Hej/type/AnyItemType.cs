using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public class AnyItemType : Saxon.Hej.type.ItemTypeWithSequenceTypeCache {
        private Saxon.Hej.value.SequenceType _one;
        private Saxon.Hej.value.SequenceType _oneOrMore;
        private Saxon.Hej.value.SequenceType _zeroOrOne;
        private Saxon.Hej.value.SequenceType _zeroOrMore;
        private AnyItemType()  {
        }
        private static readonly Saxon.Hej.type.AnyItemType theInstance = new Saxon.Hej.type.AnyItemType();
        public static Saxon.Hej.type.AnyItemType getInstance() {
            return theInstance;
        }
        public Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.ANY;
        }
        public Saxon.Hej.type.UType getUType() {
            return Saxon.Hej.type.UType.ANY;
        }
        public bool isAtomicType() {
            return false;
        }
        public string getBasicAlphaCode() {
            return "";
        }
        public bool isPlainType() {
            return false;
        }
        public bool matches(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            return true;
        }
        public Saxon.Hej.type.ItemType getPrimitiveItemType() {
            return this;
        }
        public int getPrimitiveType() {
            return Saxon.Hej.type.Type.ITEM;
        }
        public Saxon.Hej.type.PlainType getAtomizedItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
        }
        public bool isAtomizable(Saxon.Hej.type.TypeHierarchy th) {
            return true;
        }
        public double getDefaultPriority() {
            return -2;
        }
        public override string ToString() {
            return "item()";
        }
        public override int GetHashCode() {
            return "AnyItemType".GetHashCode();
        }
        public Saxon.Hej.value.SequenceType one() {
            if ((_one == null)) {
                _one = new Saxon.Hej.value.SequenceType(this, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE);
            }
            return _one;
        }
        public Saxon.Hej.value.SequenceType zeroOrOne() {
            if ((_zeroOrOne == null)) {
                _zeroOrOne = new Saxon.Hej.value.SequenceType(this, Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE);
            }
            return _zeroOrOne;
        }
        public Saxon.Hej.value.SequenceType oneOrMore() {
            if ((_oneOrMore == null)) {
                _oneOrMore = new Saxon.Hej.value.SequenceType(this, Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE);
            }
            return _oneOrMore;
        }
        public Saxon.Hej.value.SequenceType zeroOrMore() {
            if ((_zeroOrMore == null)) {
                _zeroOrMore = new Saxon.Hej.value.SequenceType(this, Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE);
            }
            return _zeroOrMore;
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