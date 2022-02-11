using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class  Optionality  {
        public static readonly Optionality REQUIRED = new Optionality("REQUIRED", InnerEnum.REQUIRED);
        public static readonly Optionality OPTIONAL = new Optionality("OPTIONAL", InnerEnum.OPTIONAL);
        public static readonly Optionality PROHIBITED = new Optionality("PROHIBITED", InnerEnum.PROHIBITED);
        private static readonly List<Optionality> valueList = new List<Optionality>();
        static Optionality() {
            valueList.Add(REQUIRED);
            valueList.Add(OPTIONAL);
            valueList.Add(PROHIBITED);
        }
        public static IList<Optionality> values() {
            return valueList;
        }
        public enum InnerEnum {
            REQUIRED,
            OPTIONAL,
            PROHIBITED,
        }
        public readonly InnerEnum innerEnumValue;
        public readonly string nameValue;
        public readonly int ordinalValue;
        public static int nextOrdinal = 0;
        internal Optionality(string name, InnerEnum innerEnum) { 
            nameValue = name;
            ordinalValue = nextOrdinal++;
            innerEnumValue = innerEnum;
        } 
    }
}