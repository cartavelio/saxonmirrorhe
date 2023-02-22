using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    public class  OccurrenceIndicator  {
        public static readonly OccurrenceIndicator ZERO = new OccurrenceIndicator("ZERO", InnerEnum.ZERO);
        public static readonly OccurrenceIndicator ZERO_OR_ONE = new OccurrenceIndicator("ZERO_OR_ONE", InnerEnum.ZERO_OR_ONE);
        public static readonly OccurrenceIndicator ZERO_OR_MORE = new OccurrenceIndicator("ZERO_OR_MORE", InnerEnum.ZERO_OR_MORE);
        public static readonly OccurrenceIndicator ONE = new OccurrenceIndicator("ONE", InnerEnum.ONE);
        public static readonly OccurrenceIndicator ONE_OR_MORE = new OccurrenceIndicator("ONE_OR_MORE", InnerEnum.ONE_OR_MORE);
        private static readonly List<OccurrenceIndicator> valueList = new List<OccurrenceIndicator>();
        static OccurrenceIndicator() {
            valueList.Add(ZERO);
            valueList.Add(ZERO_OR_ONE);
            valueList.Add(ZERO_OR_MORE);
            valueList.Add(ONE);
            valueList.Add(ONE_OR_MORE);
        }
        public static IList<OccurrenceIndicator> values() {
            return valueList;
        }
        public enum InnerEnum {
            ZERO,
            ZERO_OR_ONE,
            ZERO_OR_MORE,
            ONE,
            ONE_OR_MORE,
        }
        public readonly InnerEnum innerEnumValue;
        public readonly string nameValue;
        public readonly int ordinalValue;
        public static int nextOrdinal = 0;
        internal OccurrenceIndicator(string name, InnerEnum innerEnum) { 
            nameValue = name;
            ordinalValue = nextOrdinal++;
            innerEnumValue = innerEnum;
        } 
        protected internal int getCardinality() {
            Saxon.Hej.s9api.OccurrenceIndicator indicator = this;
            switch (indicator.innerEnumValue) {
              case Saxon.Hej.s9api.OccurrenceIndicator.InnerEnum.ZERO:
                return Saxon.Hej.expr.StaticProperty.EMPTY;
              case Saxon.Hej.s9api.OccurrenceIndicator.InnerEnum.ZERO_OR_ONE:
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
              case Saxon.Hej.s9api.OccurrenceIndicator.InnerEnum.ZERO_OR_MORE:
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
              case Saxon.Hej.s9api.OccurrenceIndicator.InnerEnum.ONE:
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ONE;
              case Saxon.Hej.s9api.OccurrenceIndicator.InnerEnum.ONE_OR_MORE:
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE;
              default:
                return Saxon.Hej.expr.StaticProperty.EMPTY;
            }
        }
        protected internal static Saxon.Hej.s9api.OccurrenceIndicator getOccurrenceIndicator(int cardinality) {
            switch (cardinality) {
              case Saxon.Hej.expr.StaticProperty.EMPTY:
                return /*implicit enum*/ Saxon.Hej.s9api.OccurrenceIndicator.ZERO;
              case Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE:
                return /*implicit enum*/ Saxon.Hej.s9api.OccurrenceIndicator.ZERO_OR_ONE;
              case Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE:
                return /*implicit enum*/ Saxon.Hej.s9api.OccurrenceIndicator.ZERO_OR_MORE;
              case Saxon.Hej.expr.StaticProperty.ALLOWS_ONE:
                return /*implicit enum*/ Saxon.Hej.s9api.OccurrenceIndicator.ONE;
              case Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE:
                return /*implicit enum*/ Saxon.Hej.s9api.OccurrenceIndicator.ONE_OR_MORE;
              default:
                return /*implicit enum*/ Saxon.Hej.s9api.OccurrenceIndicator.ZERO_OR_MORE;
            }
        }
        public bool allowsZero() {
            return Saxon.Hej.value.Cardinality.allowsZero(getCardinality());
        }
        public bool allowsMany() {
            return Saxon.Hej.value.Cardinality.allowsMany(getCardinality());
        }
        public bool allows(int size) {
            Saxon.Hej.s9api.OccurrenceIndicator indicator = this;
            switch (indicator.innerEnumValue) {
              case Saxon.Hej.s9api.OccurrenceIndicator.InnerEnum.ZERO:
                return (size == 0);
              case Saxon.Hej.s9api.OccurrenceIndicator.InnerEnum.ZERO_OR_ONE:
                return (size<=1);
              case Saxon.Hej.s9api.OccurrenceIndicator.InnerEnum.ZERO_OR_MORE:
                return true;
              case Saxon.Hej.s9api.OccurrenceIndicator.InnerEnum.ONE:
                return (size == 1);
              case Saxon.Hej.s9api.OccurrenceIndicator.InnerEnum.ONE_OR_MORE:
                return (size>0);
              default:
                return false;
            }
        }
        public bool subsumes(Saxon.Hej.s9api.OccurrenceIndicator other) {
            return Saxon.Hej.value.Cardinality.subsumes(getCardinality(), other.getCardinality());
        }
        public override  /* REPLACING public */ string ToString() {
            Saxon.Hej.s9api.OccurrenceIndicator indicator = this;
            switch (indicator.innerEnumValue) {
              case Saxon.Hej.s9api.OccurrenceIndicator.InnerEnum.ZERO:
                return "0";
              case Saxon.Hej.s9api.OccurrenceIndicator.InnerEnum.ZERO_OR_ONE:
                return "?";
              case Saxon.Hej.s9api.OccurrenceIndicator.InnerEnum.ZERO_OR_MORE:
                return "*";
              case Saxon.Hej.s9api.OccurrenceIndicator.InnerEnum.ONE:
                return "";
              case Saxon.Hej.s9api.OccurrenceIndicator.InnerEnum.ONE_OR_MORE:
                return "+";
              default:
                return "!!!";
            }
        }
    }
}