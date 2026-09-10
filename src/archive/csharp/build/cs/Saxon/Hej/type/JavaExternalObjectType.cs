using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public class JavaExternalObjectType : Saxon.Hej.type.ExternalObjectType {
        protected internal System.Type javaClass;
        public JavaExternalObjectType(System.Type javaClass)  {
            this.javaClass = javaClass;
        }
        public override string getName() {
            return javaClass.Name;
        }
        public override string getTargetNamespace() {
            return Saxon.Hej.lib.NamespaceConstant.JAVA_TYPE;
        }
        public override Saxon.Hej.om.StructuredQName getTypeName() {
            return classNameToQName(javaClass.Name);
        }
        public override Saxon.Hej.type.ItemType getPrimitiveItemType() {
            return new Saxon.Hej.type.JavaExternalObjectType(typeof(System.Object));
        }
        public Saxon.Hej.type.Affinity getRelationship(Saxon.Hej.type.JavaExternalObjectType other) {
            System.Type j2 = other.javaClass;
            if (javaClass.Equals(j2)) {
                return Saxon.Hej.type.Affinity.SAME_TYPE;
            } else {
                if (javaClass.IsAssignableFrom(j2)) {
                    return Saxon.Hej.type.Affinity.SUBSUMES;
                } else {
                    if (j2.IsAssignableFrom(javaClass)) {
                        return Saxon.Hej.type.Affinity.SUBSUMED_BY;
                    } else {
                        if ((javaClass.IsInterface || j2.IsInterface)) {
                            return Saxon.Hej.type.Affinity.OVERLAPS;
                        } else {
                            return Saxon.Hej.type.Affinity.DISJOINT;
                        }
                    }
                }
            }
        }
        public System.Type getJavaClass() {
            return javaClass;
        }
        public override bool matches(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            if ((item.getGenre() == Saxon.Hej.om.Genre.EXTERNAL)) {
                System.Object obj = ((Saxon.Hej.value.ObjectValue<object>)(item)).getObject();
                return javaClass.IsAssignableFrom(obj.GetType());
            }
            return false;
        }
        public override string ToString() {
            return classNameToQName(javaClass.Name).getEQName();
        }
        public string getDisplayName() {
            return ("java-type:"+javaClass.Name);
        }
        public override int GetHashCode() {
            return javaClass.GetHashCode();
        }
        public override bool Equals(System.Object obj) {
            return ((obj is Saxon.Hej.type.JavaExternalObjectType) && (javaClass == ((Saxon.Hej.type.JavaExternalObjectType)(obj)).javaClass));
        }
        public static string classNameToLocalName(string className) {
            return className.Replace('$', '-').Replace("[", "_-");
        }
        public static string localNameToClassName(string className) {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(className.Length);
            bool atStart = true;
            for (int i = 0; (i<className.Length); i++) {
                char c = className[i];
                if (atStart) {
                    if ((((c == '_') && ((i+1)<className.Length)) && (className[(i+1)] == '-'))) {
                        fsb.append('[');
                        i++;
                    } else {
                        atStart = false;
                        fsb.append(((c == '-') ? '$' : c));
                    }
                } else {
                    fsb.append(((c == '-') ? '$' : c));
                }
            }
            return fsb.ToString();
        }
        public static Saxon.Hej.om.StructuredQName classNameToQName(string className) {
            return new Saxon.Hej.om.StructuredQName("jt", Saxon.Hej.lib.NamespaceConstant.JAVA_TYPE, classNameToLocalName(className));
        }
    
    }
}