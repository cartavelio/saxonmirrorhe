using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    public class  XdmNodeKind  {
        public static readonly XdmNodeKind DOCUMENT = new XdmNodeKind("DOCUMENT", InnerEnum.DOCUMENT, Saxon.Hej.type.Type.DOCUMENT);
        public static readonly XdmNodeKind ELEMENT = new XdmNodeKind("ELEMENT", InnerEnum.ELEMENT, Saxon.Hej.type.Type.ELEMENT);
        public static readonly XdmNodeKind ATTRIBUTE = new XdmNodeKind("ATTRIBUTE", InnerEnum.ATTRIBUTE, Saxon.Hej.type.Type.ATTRIBUTE);
        public static readonly XdmNodeKind TEXT = new XdmNodeKind("TEXT", InnerEnum.TEXT, Saxon.Hej.type.Type.TEXT);
        public static readonly XdmNodeKind COMMENT = new XdmNodeKind("COMMENT", InnerEnum.COMMENT, Saxon.Hej.type.Type.COMMENT);
        public static readonly XdmNodeKind PROCESSING_INSTRUCTION = new XdmNodeKind("PROCESSING_INSTRUCTION", InnerEnum.PROCESSING_INSTRUCTION, Saxon.Hej.type.Type.PROCESSING_INSTRUCTION);
        public static readonly XdmNodeKind NAMESPACE = new XdmNodeKind("NAMESPACE", InnerEnum.NAMESPACE, Saxon.Hej.type.Type.NAMESPACE);
        private static readonly List<XdmNodeKind> valueList = new List<XdmNodeKind>();
        static XdmNodeKind() {
            valueList.Add(DOCUMENT);
            valueList.Add(ELEMENT);
            valueList.Add(ATTRIBUTE);
            valueList.Add(TEXT);
            valueList.Add(COMMENT);
            valueList.Add(PROCESSING_INSTRUCTION);
            valueList.Add(NAMESPACE);
        }
        public static IList<XdmNodeKind> values() {
            return valueList;
        }
        public enum InnerEnum {
            DOCUMENT,
            ELEMENT,
            ATTRIBUTE,
            TEXT,
            COMMENT,
            PROCESSING_INSTRUCTION,
            NAMESPACE,
        }
        public readonly InnerEnum innerEnumValue;
        public readonly string nameValue;
        public readonly int ordinalValue;
        public static int nextOrdinal = 0;
        internal XdmNodeKind(string name, InnerEnum innerEnum, int number) { 
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
        public static Saxon.Hej.s9api.XdmNodeKind forType(int type) {
            switch (type) {
              case Saxon.Hej.type.Type.DOCUMENT:
                return /*implicit enum*/ Saxon.Hej.s9api.XdmNodeKind.DOCUMENT;
              case Saxon.Hej.type.Type.ELEMENT:
                return /*implicit enum*/ Saxon.Hej.s9api.XdmNodeKind.ELEMENT;
              case Saxon.Hej.type.Type.ATTRIBUTE:
                return /*implicit enum*/ Saxon.Hej.s9api.XdmNodeKind.ATTRIBUTE;
              case Saxon.Hej.type.Type.TEXT:
                return /*implicit enum*/ Saxon.Hej.s9api.XdmNodeKind.TEXT;
              case Saxon.Hej.type.Type.COMMENT:
                return /*implicit enum*/ Saxon.Hej.s9api.XdmNodeKind.COMMENT;
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                return /*implicit enum*/ Saxon.Hej.s9api.XdmNodeKind.PROCESSING_INSTRUCTION;
              case Saxon.Hej.type.Type.NAMESPACE:
                return /*implicit enum*/ Saxon.Hej.s9api.XdmNodeKind.NAMESPACE;
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException();
            }
        }
    }
}