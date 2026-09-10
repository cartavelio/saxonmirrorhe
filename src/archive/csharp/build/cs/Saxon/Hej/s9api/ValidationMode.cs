using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    public class  ValidationMode  {
        public static readonly ValidationMode STRICT = new ValidationMode("STRICT", InnerEnum.STRICT, Saxon.Hej.lib.Validation.STRICT);
        public static readonly ValidationMode LAX = new ValidationMode("LAX", InnerEnum.LAX, Saxon.Hej.lib.Validation.LAX);
        public static readonly ValidationMode PRESERVE = new ValidationMode("PRESERVE", InnerEnum.PRESERVE, Saxon.Hej.lib.Validation.PRESERVE);
        public static readonly ValidationMode STRIP = new ValidationMode("STRIP", InnerEnum.STRIP, Saxon.Hej.lib.Validation.STRIP);
        public static readonly ValidationMode DEFAULT = new ValidationMode("DEFAULT", InnerEnum.DEFAULT, Saxon.Hej.lib.Validation.DEFAULT);
        private static readonly List<ValidationMode> valueList = new List<ValidationMode>();
        static ValidationMode() {
            valueList.Add(STRICT);
            valueList.Add(LAX);
            valueList.Add(PRESERVE);
            valueList.Add(STRIP);
            valueList.Add(DEFAULT);
        }
        public static IList<ValidationMode> values() {
            return valueList;
        }
        public enum InnerEnum {
            STRICT,
            LAX,
            PRESERVE,
            STRIP,
            DEFAULT,
        }
        public readonly InnerEnum innerEnumValue;
        public readonly string nameValue;
        public readonly int ordinalValue;
        public static int nextOrdinal = 0;
        internal ValidationMode(string name, InnerEnum innerEnum, int number) { 
            this.number = number;
            nameValue = name;
            ordinalValue = nextOrdinal++;
            innerEnumValue = innerEnum;
    this.number = number;
        } 
        private readonly int number;
        protected internal int getNumber() {
            return number;
        }
        protected internal static Saxon.Hej.s9api.ValidationMode get(int number) {
            switch (number) {
              case Saxon.Hej.lib.Validation.STRICT:
                return /*implicit enum*/ Saxon.Hej.s9api.ValidationMode.STRICT;
              case Saxon.Hej.lib.Validation.LAX:
                return /*implicit enum*/ Saxon.Hej.s9api.ValidationMode.LAX;
              case Saxon.Hej.lib.Validation.STRIP:
                return /*implicit enum*/ Saxon.Hej.s9api.ValidationMode.STRIP;
              case Saxon.Hej.lib.Validation.PRESERVE:
                return /*implicit enum*/ Saxon.Hej.s9api.ValidationMode.PRESERVE;
              case Saxon.Hej.lib.Validation.DEFAULT:
              default:
                return /*implicit enum*/ Saxon.Hej.s9api.ValidationMode.DEFAULT;
            }
        }
    }
}