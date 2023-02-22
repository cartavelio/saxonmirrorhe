using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public class  PrimitiveUType  {
        public static readonly PrimitiveUType DOCUMENT = new PrimitiveUType("DOCUMENT", InnerEnum.DOCUMENT, 0);
        public static readonly PrimitiveUType ELEMENT = new PrimitiveUType("ELEMENT", InnerEnum.ELEMENT, 1);
        public static readonly PrimitiveUType ATTRIBUTE = new PrimitiveUType("ATTRIBUTE", InnerEnum.ATTRIBUTE, 2);
        public static readonly PrimitiveUType TEXT = new PrimitiveUType("TEXT", InnerEnum.TEXT, 3);
        public static readonly PrimitiveUType COMMENT = new PrimitiveUType("COMMENT", InnerEnum.COMMENT, 4);
        public static readonly PrimitiveUType PI = new PrimitiveUType("PI", InnerEnum.PI, 5);
        public static readonly PrimitiveUType NAMESPACE = new PrimitiveUType("NAMESPACE", InnerEnum.NAMESPACE, 6);
        public static readonly PrimitiveUType FUNCTION = new PrimitiveUType("FUNCTION", InnerEnum.FUNCTION, 7);
        public static readonly PrimitiveUType STRING = new PrimitiveUType("STRING", InnerEnum.STRING, 8);
        public static readonly PrimitiveUType BOOLEAN = new PrimitiveUType("BOOLEAN", InnerEnum.BOOLEAN, 9);
        public static readonly PrimitiveUType DECIMAL = new PrimitiveUType("DECIMAL", InnerEnum.DECIMAL, 10);
        public static readonly PrimitiveUType FLOAT = new PrimitiveUType("FLOAT", InnerEnum.FLOAT, 11);
        public static readonly PrimitiveUType DOUBLE = new PrimitiveUType("DOUBLE", InnerEnum.DOUBLE, 12);
        public static readonly PrimitiveUType DURATION = new PrimitiveUType("DURATION", InnerEnum.DURATION, 13);
        public static readonly PrimitiveUType DATE_TIME = new PrimitiveUType("DATE_TIME", InnerEnum.DATE_TIME, 14);
        public static readonly PrimitiveUType TIME = new PrimitiveUType("TIME", InnerEnum.TIME, 15);
        public static readonly PrimitiveUType DATE = new PrimitiveUType("DATE", InnerEnum.DATE, 16);
        public static readonly PrimitiveUType G_YEAR_MONTH = new PrimitiveUType("G_YEAR_MONTH", InnerEnum.G_YEAR_MONTH, 17);
        public static readonly PrimitiveUType G_YEAR = new PrimitiveUType("G_YEAR", InnerEnum.G_YEAR, 18);
        public static readonly PrimitiveUType G_MONTH_DAY = new PrimitiveUType("G_MONTH_DAY", InnerEnum.G_MONTH_DAY, 19);
        public static readonly PrimitiveUType G_DAY = new PrimitiveUType("G_DAY", InnerEnum.G_DAY, 20);
        public static readonly PrimitiveUType G_MONTH = new PrimitiveUType("G_MONTH", InnerEnum.G_MONTH, 21);
        public static readonly PrimitiveUType HEX_BINARY = new PrimitiveUType("HEX_BINARY", InnerEnum.HEX_BINARY, 22);
        public static readonly PrimitiveUType BASE64_BINARY = new PrimitiveUType("BASE64_BINARY", InnerEnum.BASE64_BINARY, 23);
        public static readonly PrimitiveUType ANY_URI = new PrimitiveUType("ANY_URI", InnerEnum.ANY_URI, 24);
        public static readonly PrimitiveUType QNAME = new PrimitiveUType("QNAME", InnerEnum.QNAME, 25);
        public static readonly PrimitiveUType NOTATION = new PrimitiveUType("NOTATION", InnerEnum.NOTATION, 26);
        public static readonly PrimitiveUType UNTYPED_ATOMIC = new PrimitiveUType("UNTYPED_ATOMIC", InnerEnum.UNTYPED_ATOMIC, 27);
        public static readonly PrimitiveUType EXTENSION = new PrimitiveUType("EXTENSION", InnerEnum.EXTENSION, 30);
        private static readonly List<PrimitiveUType> valueList = new List<PrimitiveUType>();
        static PrimitiveUType() {
            valueList.Add(DOCUMENT);
            valueList.Add(ELEMENT);
            valueList.Add(ATTRIBUTE);
            valueList.Add(TEXT);
            valueList.Add(COMMENT);
            valueList.Add(PI);
            valueList.Add(NAMESPACE);
            valueList.Add(FUNCTION);
            valueList.Add(STRING);
            valueList.Add(BOOLEAN);
            valueList.Add(DECIMAL);
            valueList.Add(FLOAT);
            valueList.Add(DOUBLE);
            valueList.Add(DURATION);
            valueList.Add(DATE_TIME);
            valueList.Add(TIME);
            valueList.Add(DATE);
            valueList.Add(G_YEAR_MONTH);
            valueList.Add(G_YEAR);
            valueList.Add(G_MONTH_DAY);
            valueList.Add(G_DAY);
            valueList.Add(G_MONTH);
            valueList.Add(HEX_BINARY);
            valueList.Add(BASE64_BINARY);
            valueList.Add(ANY_URI);
            valueList.Add(QNAME);
            valueList.Add(NOTATION);
            valueList.Add(UNTYPED_ATOMIC);
            valueList.Add(EXTENSION);
        }
        public static IList<PrimitiveUType> values() {
            return valueList;
        }
        public enum InnerEnum {
            DOCUMENT,
            ELEMENT,
            ATTRIBUTE,
            TEXT,
            COMMENT,
            PI,
            NAMESPACE,
            FUNCTION,
            STRING,
            BOOLEAN,
            DECIMAL,
            FLOAT,
            DOUBLE,
            DURATION,
            DATE_TIME,
            TIME,
            DATE,
            G_YEAR_MONTH,
            G_YEAR,
            G_MONTH_DAY,
            G_DAY,
            G_MONTH,
            HEX_BINARY,
            BASE64_BINARY,
            ANY_URI,
            QNAME,
            NOTATION,
            UNTYPED_ATOMIC,
            EXTENSION,
        }
        public readonly InnerEnum innerEnumValue;
        public readonly string nameValue;
        public readonly int ordinalValue;
        public static int nextOrdinal = 0;
        internal PrimitiveUType(string name, InnerEnum innerEnum, int bit) { 
            this.bit = bit;
            nameValue = name;
            ordinalValue = nextOrdinal++;
            innerEnumValue = innerEnum;
    this.bit = bit;
        } 
        private readonly int bit;
        public int getBit() {
            return bit;
        }
        public Saxon.Hej.type.UType toUType() {
            return new Saxon.Hej.type.UType((1<<bit));
        }
        public static Saxon.Hej.type.PrimitiveUType forBit(int bit) {
            return values()[bit];
        }
        public override  /* REPLACING public */ string ToString() {
            switch (this.innerEnumValue) {
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.DOCUMENT:
                return "document";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.ELEMENT:
                return "element";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.ATTRIBUTE:
                return "attribute";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.TEXT:
                return "text";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.COMMENT:
                return "comment";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.PI:
                return "processing-instruction";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.NAMESPACE:
                return "namespace";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.FUNCTION:
                return "function";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.STRING:
                return "string";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.BOOLEAN:
                return "boolean";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.DECIMAL:
                return "decimal";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.FLOAT:
                return "float";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.DOUBLE:
                return "double";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.DURATION:
                return "duration";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.DATE_TIME:
                return "dateTime";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.TIME:
                return "time";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.DATE:
                return "date";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.G_YEAR_MONTH:
                return "gYearMonth";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.G_YEAR:
                return "gYear";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.G_MONTH_DAY:
                return "gMonthDay";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.G_DAY:
                return "gDay";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.G_MONTH:
                return "gMoonth";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.HEX_BINARY:
                return "hexBinary";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.BASE64_BINARY:
                return "base64Binary";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.ANY_URI:
                return "anyURI";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.QNAME:
                return "QName";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.NOTATION:
                return "NOTATION";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.UNTYPED_ATOMIC:
                return "untypedAtomic";
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.EXTENSION:
                return "external object";
              default:
                return "???";
            }
        }
        public Saxon.Hej.type.ItemType toItemType() {
            switch (this.innerEnumValue) {
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.DOCUMENT:
                return Saxon.Hej.pattern.NodeKindTest.DOCUMENT;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.ELEMENT:
                return Saxon.Hej.pattern.NodeKindTest.ELEMENT;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.ATTRIBUTE:
                return Saxon.Hej.pattern.NodeKindTest.ATTRIBUTE;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.TEXT:
                return Saxon.Hej.pattern.NodeKindTest.TEXT;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.COMMENT:
                return Saxon.Hej.pattern.NodeKindTest.COMMENT;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.PI:
                return Saxon.Hej.pattern.NodeKindTest.PROCESSING_INSTRUCTION;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.NAMESPACE:
                return Saxon.Hej.pattern.NodeKindTest.NAMESPACE;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.FUNCTION:
                return Saxon.Hej.type.AnyFunctionType.getInstance();
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.STRING:
                return Saxon.Hej.type.BuiltInAtomicType.STRING;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.BOOLEAN:
                return Saxon.Hej.type.BuiltInAtomicType.BOOLEAN;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.DECIMAL:
                return Saxon.Hej.type.BuiltInAtomicType.DECIMAL;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.FLOAT:
                return Saxon.Hej.type.BuiltInAtomicType.FLOAT;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.DOUBLE:
                return Saxon.Hej.type.BuiltInAtomicType.DOUBLE;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.DURATION:
                return Saxon.Hej.type.BuiltInAtomicType.DURATION;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.DATE_TIME:
                return Saxon.Hej.type.BuiltInAtomicType.DATE_TIME;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.TIME:
                return Saxon.Hej.type.BuiltInAtomicType.TIME;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.DATE:
                return Saxon.Hej.type.BuiltInAtomicType.DATE;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.G_YEAR_MONTH:
                return Saxon.Hej.type.BuiltInAtomicType.G_YEAR_MONTH;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.G_YEAR:
                return Saxon.Hej.type.BuiltInAtomicType.G_YEAR;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.G_MONTH_DAY:
                return Saxon.Hej.type.BuiltInAtomicType.G_MONTH_DAY;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.G_DAY:
                return Saxon.Hej.type.BuiltInAtomicType.G_DAY;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.G_MONTH:
                return Saxon.Hej.type.BuiltInAtomicType.G_MONTH;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.HEX_BINARY:
                return Saxon.Hej.type.BuiltInAtomicType.HEX_BINARY;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.BASE64_BINARY:
                return Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.ANY_URI:
                return Saxon.Hej.type.BuiltInAtomicType.ANY_URI;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.QNAME:
                return Saxon.Hej.type.BuiltInAtomicType.QNAME;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.NOTATION:
                return Saxon.Hej.type.BuiltInAtomicType.NOTATION;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.UNTYPED_ATOMIC:
                return Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
              case Saxon.Hej.type.PrimitiveUType.InnerEnum.EXTENSION:
                return Saxon.Hej.type.AnyItemType.getInstance();
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException();
            }
        }
    }
}