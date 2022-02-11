using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class  FunctionStreamability  {
        public static readonly FunctionStreamability UNCLASSIFIED = new FunctionStreamability("UNCLASSIFIED", InnerEnum.UNCLASSIFIED, "unclassified");
        public static readonly FunctionStreamability ABSORBING = new FunctionStreamability("ABSORBING", InnerEnum.ABSORBING, "absorbing");
        public static readonly FunctionStreamability INSPECTION = new FunctionStreamability("INSPECTION", InnerEnum.INSPECTION, "inspection");
        public static readonly FunctionStreamability FILTER = new FunctionStreamability("FILTER", InnerEnum.FILTER, "filter");
        public static readonly FunctionStreamability SHALLOW_DESCENT = new FunctionStreamability("SHALLOW_DESCENT", InnerEnum.SHALLOW_DESCENT, "shallow-descent");
        public static readonly FunctionStreamability DEEP_DESCENT = new FunctionStreamability("DEEP_DESCENT", InnerEnum.DEEP_DESCENT, "deep-descent");
        public static readonly FunctionStreamability ASCENT = new FunctionStreamability("ASCENT", InnerEnum.ASCENT, "ascent");
        private static readonly List<FunctionStreamability> valueList = new List<FunctionStreamability>();
        static FunctionStreamability() {
            valueList.Add(UNCLASSIFIED);
            valueList.Add(ABSORBING);
            valueList.Add(INSPECTION);
            valueList.Add(FILTER);
            valueList.Add(SHALLOW_DESCENT);
            valueList.Add(DEEP_DESCENT);
            valueList.Add(ASCENT);
        }
        public static IList<FunctionStreamability> values() {
            return valueList;
        }
        public enum InnerEnum {
            UNCLASSIFIED,
            ABSORBING,
            INSPECTION,
            FILTER,
            SHALLOW_DESCENT,
            DEEP_DESCENT,
            ASCENT,
        }
        public readonly InnerEnum innerEnumValue;
        public readonly string nameValue;
        public readonly int ordinalValue;
        public static int nextOrdinal = 0;
        internal FunctionStreamability(string name, InnerEnum innerEnum, string v) { 
            this.v = v;
            nameValue = name;
            ordinalValue = nextOrdinal++;
            innerEnumValue = innerEnum;
    streamabilityStr = v;
        } 
        private string v;
        public string streamabilityStr;
        public bool isConsuming() {
            return (((this == /*implicit enum*/ Saxon.Hej.trans.FunctionStreamability.ABSORBING) || (this == /*implicit enum*/ Saxon.Hej.trans.FunctionStreamability.SHALLOW_DESCENT)) || (this == /*implicit enum*/ Saxon.Hej.trans.FunctionStreamability.DEEP_DESCENT));
        }
        public bool isStreaming() {
            return (this != /*implicit enum*/ Saxon.Hej.trans.FunctionStreamability.UNCLASSIFIED);
        }
        public static Saxon.Hej.trans.FunctionStreamability of(string v) {
            switch (v) {
              case "unclassified":
                return /*implicit enum*/ Saxon.Hej.trans.FunctionStreamability.UNCLASSIFIED;
              case "absorbing":
                return /*implicit enum*/ Saxon.Hej.trans.FunctionStreamability.ABSORBING;
              case "inspection":
                return /*implicit enum*/ Saxon.Hej.trans.FunctionStreamability.INSPECTION;
              case "filter":
                return /*implicit enum*/ Saxon.Hej.trans.FunctionStreamability.FILTER;
              case "shallow-descent":
                return /*implicit enum*/ Saxon.Hej.trans.FunctionStreamability.SHALLOW_DESCENT;
              case "deep-descent":
                return /*implicit enum*/ Saxon.Hej.trans.FunctionStreamability.DEEP_DESCENT;
              case "ascent":
                return /*implicit enum*/ Saxon.Hej.trans.FunctionStreamability.ASCENT;
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException();
            }
        }
    }
}