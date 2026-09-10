using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.query {
    public class Annotation {
        public static readonly Saxon.Hej.om.StructuredQName UPDATING = new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.XQUERY, "updating");
        public static readonly Saxon.Hej.om.StructuredQName SIMPLE = new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.XQUERY, "simple");
        public static readonly Saxon.Hej.om.StructuredQName PRIVATE = new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.XQUERY, "private");
        public static readonly Saxon.Hej.om.StructuredQName PUBLIC = new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.XQUERY, "public");
        private Saxon.Hej.om.StructuredQName qName = null;
        private System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> annotationParameters = null;
        public Annotation(Saxon.Hej.om.StructuredQName name)  {
            this.qName = name;
        }
        public Saxon.Hej.om.StructuredQName getAnnotationQName() {
            return qName;
        }
        public void addAnnotationParameter(Saxon.Hej.value.AtomicValue value) {
            if ((annotationParameters == null)) {
                annotationParameters = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>();
            }
            annotationParameters.Add(value);
        }
        public System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> getAnnotationParameters() {
            if ((annotationParameters == null)) {
                annotationParameters = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>();
            }
            return annotationParameters;
        }
        public override bool Equals(System.Object other) {
            if (!(((((other is Saxon.Hej.query.Annotation) && qName.Equals(((Saxon.Hej.query.Annotation)(other)).qName)) && (getAnnotationParameters().Count == ((Saxon.Hej.query.Annotation)(other)).getAnnotationParameters().Count))))) {
                return false;
            }
            for (int i = 0; (i<annotationParameters.Count); i++) {
                if (!(annotationParamEqual(annotationParameters[i], ((Saxon.Hej.query.Annotation)(other)).annotationParameters[i]))) {
                    return false;
                }
            }
            return true;
        }
        private static bool annotationParamEqual(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            if (((a is Saxon.Hej.value.StringValue) && (b is Saxon.Hej.value.StringValue))) {
                return a.getUnicodeStringValue().Equals(b.getUnicodeStringValue());
            } else {
                if (((a is Saxon.Hej.value.NumericValue) && (b is Saxon.Hej.value.NumericValue))) {
                    return (((Saxon.Hej.value.NumericValue)(a)).getDoubleValue() == ((Saxon.Hej.value.NumericValue)(b)).getDoubleValue());
                } else {
                    return false;
                }
            }
        }
        public override int GetHashCode() {
            return (qName.GetHashCode()^annotationParameters.GetHashCode());
        }
    
    }
}