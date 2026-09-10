using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class XQueryFunctionAnnotationHandler : Saxon.Hej.lib.FunctionAnnotationHandler {
        private class DisallowedCombination {
            public DisallowedCombination(Saxon.Hej.om.StructuredQName one, Saxon.Hej.om.StructuredQName two, string errorCode, params string[]  where)  {
                this.one = one;
                this.two = two;
                this.errorCode = errorCode;
                this.where = new System.Collections.Generic.HashSet<string>(where.Length);
                Saxon.Ejava.util.Collections.addAll(this.where, where);
            }
            public Saxon.Hej.om.StructuredQName one;
            public Saxon.Hej.om.StructuredQName two;
            public string errorCode;
            public System.Collections.Generic.ISet<string> where;
        
        }
        private static readonly Saxon.Hej.lib.XQueryFunctionAnnotationHandler.DisallowedCombination[] blackList = {new Saxon.Hej.lib.XQueryFunctionAnnotationHandler.DisallowedCombination(Saxon.Hej.query.Annotation.SIMPLE, null, "XUST0032", "DV"), new Saxon.Hej.lib.XQueryFunctionAnnotationHandler.DisallowedCombination(Saxon.Hej.query.Annotation.UPDATING, null, "XUST0032", "DV"), new Saxon.Hej.lib.XQueryFunctionAnnotationHandler.DisallowedCombination(Saxon.Hej.query.Annotation.PUBLIC, null, "XQST0125", "IF"), new Saxon.Hej.lib.XQueryFunctionAnnotationHandler.DisallowedCombination(Saxon.Hej.query.Annotation.PRIVATE, null, "XQST0125", "IF"), new Saxon.Hej.lib.XQueryFunctionAnnotationHandler.DisallowedCombination(Saxon.Hej.query.Annotation.PRIVATE, Saxon.Hej.query.Annotation.PRIVATE, "XQST0106", "DF"), new Saxon.Hej.lib.XQueryFunctionAnnotationHandler.DisallowedCombination(Saxon.Hej.query.Annotation.PRIVATE, Saxon.Hej.query.Annotation.PUBLIC, "XQST0106", "DF"), new Saxon.Hej.lib.XQueryFunctionAnnotationHandler.DisallowedCombination(Saxon.Hej.query.Annotation.PUBLIC, Saxon.Hej.query.Annotation.PUBLIC, "XQST0106", "DF"), new Saxon.Hej.lib.XQueryFunctionAnnotationHandler.DisallowedCombination(Saxon.Hej.query.Annotation.PUBLIC, Saxon.Hej.query.Annotation.PRIVATE, "XQST0106", "DF"), new Saxon.Hej.lib.XQueryFunctionAnnotationHandler.DisallowedCombination(Saxon.Hej.query.Annotation.PRIVATE, Saxon.Hej.query.Annotation.PRIVATE, "XQST0116", "DV"), new Saxon.Hej.lib.XQueryFunctionAnnotationHandler.DisallowedCombination(Saxon.Hej.query.Annotation.PRIVATE, Saxon.Hej.query.Annotation.PUBLIC, "XQST0116", "DV"), new Saxon.Hej.lib.XQueryFunctionAnnotationHandler.DisallowedCombination(Saxon.Hej.query.Annotation.PUBLIC, Saxon.Hej.query.Annotation.PUBLIC, "XQST0116", "DV"), new Saxon.Hej.lib.XQueryFunctionAnnotationHandler.DisallowedCombination(Saxon.Hej.query.Annotation.PUBLIC, Saxon.Hej.query.Annotation.PRIVATE, "XQST0116", "DV"), new Saxon.Hej.lib.XQueryFunctionAnnotationHandler.DisallowedCombination(Saxon.Hej.query.Annotation.UPDATING, Saxon.Hej.query.Annotation.UPDATING, "XUST0033", "DF", "IF"), new Saxon.Hej.lib.XQueryFunctionAnnotationHandler.DisallowedCombination(Saxon.Hej.query.Annotation.UPDATING, Saxon.Hej.query.Annotation.SIMPLE, "XUST0033", "DF", "IF"), new Saxon.Hej.lib.XQueryFunctionAnnotationHandler.DisallowedCombination(Saxon.Hej.query.Annotation.SIMPLE, Saxon.Hej.query.Annotation.SIMPLE, "XUST0033", "DF", "IF"), new Saxon.Hej.lib.XQueryFunctionAnnotationHandler.DisallowedCombination(Saxon.Hej.query.Annotation.SIMPLE, Saxon.Hej.query.Annotation.UPDATING, "XUST0033", "DF", "IF")};
        public XQueryFunctionAnnotationHandler()  {
        }
        public void check(Saxon.Hej.query.AnnotationList annotations, string construct) {
            for (int i = 0; (i<annotations.size()); i++) {
                Saxon.Hej.query.Annotation ann = annotations.get(i);
                foreach (Saxon.Hej.lib.XQueryFunctionAnnotationHandler.DisallowedCombination dc in blackList) {
                    if ((dc.one.Equals(ann.getAnnotationQName()) && dc.where.Contains(construct))) {
                        if ((dc.two == null)) {
                            throw new Saxon.Hej.trans.XPathException((("Annotation %"+ann.getAnnotationQName().getLocalPart())+" is not allowed here"), dc.errorCode);
                        } else {
                            for (int j = 0; (j<i); j++) {
                                Saxon.Hej.query.Annotation other = annotations.get(j);
                                if (dc.two.Equals(other.getAnnotationQName())) {
                                    if (dc.two.Equals(ann.getAnnotationQName())) {
                                        throw new Saxon.Hej.trans.XPathException((("Annotation %"+ann.getAnnotationQName().getLocalPart())+" cannot appear more than once"), dc.errorCode);
                                    } else {
                                        throw new Saxon.Hej.trans.XPathException((((("Annotations %"+ann.getAnnotationQName().getLocalPart())+" and ")+other.getAnnotationQName().getLocalPart())+" cannot appear together"), dc.errorCode);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public Saxon.Hej.om.NamespaceUri getAssertionNamespace() {
            return Saxon.Hej.om.NamespaceUri.of("http://www.w3.org/2012/xquery");
        }
        public bool satisfiesAssertion(Saxon.Hej.query.Annotation assertion, Saxon.Hej.query.AnnotationList annotationList) {
            return true;
        }
        public Saxon.Hej.type.Affinity relationship(Saxon.Hej.query.AnnotationList firstList, Saxon.Hej.query.AnnotationList secondList) {
            return Saxon.Hej.type.Affinity.OVERLAPS;
        }
    
    }
}