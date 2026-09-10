using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class Wildcard : Saxon.Eej.ee.schema.SchemaStructure, Saxon.Eej.ee.schema.UserSchemaComponent, Saxon.Eej.ee.schema.SerializableSchemaComponent, Saxon.Eej.ee.schema.Term {
        private System.Collections.Generic.ISet<string> allowedNamespaces = null;
        private System.Collections.Generic.ISet<string> disallowedNamespaces = null;
        private string processContents = "strict";
        private bool disallowsDefinedNames = false;
        private bool disallowsDefinedSiblings = false;
        private Saxon.Hej.z.IntSet disallowedQNames;
        public Wildcard()  {
        }
        public bool isInexpressible() {
            return false;
        }
        public bool allowsAny() {
            return ((allowedNamespaces == null) && (disallowedNamespaces == null));
        }
        public void setNoNamespacesAllowed() {
            allowedNamespaces = new System.Collections.Generic.HashSet<string>(3);
        }
        public void addAllowedNamespace(string @namespace) {
            if ((allowedNamespaces == null)) {
                allowedNamespaces = new System.Collections.Generic.HashSet<string>(3);
            }
            allowedNamespaces.Add(@namespace);
        }
        public void addDisallowedNamespace(string @namespace) {
            if ((disallowedNamespaces == null)) {
                disallowedNamespaces = new System.Collections.Generic.HashSet<string>(3);
            }
            disallowedNamespaces.Add(@namespace);
        }
        public System.Collections.Generic.ISet<string> getDisallowedNamespaces() {
            return disallowedNamespaces;
        }
        public bool matches(string uri) {
            if ((allowedNamespaces != null)) {
                return allowedNamespaces.Contains(uri);
            } else {
                return ((disallowedNamespaces == null) || !(disallowedNamespaces.Contains(uri)));
            }
        }
        public bool matches(Saxon.Hej.om.StructuredQName name, bool isElement, Saxon.Hej.Configuration config, Saxon.Eej.ee.schema.UserComplexType parentType) {
            Saxon.Hej.om.NodeName nn = new Saxon.Hej.om.FingerprintedQName(name, config.getNamePool());
            return matches(nn, isElement, config, parentType);
        }
        public bool matches(int fp, bool isElement, Saxon.Hej.Configuration config, Saxon.Eej.ee.schema.UserComplexType parentType) {
            Saxon.Hej.om.NodeName nn = new Saxon.Hej.om.CodedName(fp, "", config.getNamePool());
            return matches(nn, isElement, config, parentType);
        }
        public bool matches(Saxon.Hej.om.NodeName name, bool isElement, Saxon.Hej.Configuration config, Saxon.Eej.ee.schema.UserComplexType parentType) {
            int fingerprint = -1;
            if (((disallowedQNames != null) && disallowedQNames.contains(fingerprint))) {
                return false;
            }
            if ((disallowedQNames != null)) {
                fingerprint = name.obtainFingerprint(config.getNamePool());
                if (disallowedQNames.contains(fingerprint)) {
                    return false;
                }
            }
            if (disallowsDefinedNames) {
                if ((fingerprint == -1)) {
                    fingerprint = name.obtainFingerprint(config.getNamePool());
                }
                if ((((isElement ? config.getElementDeclaration(fingerprint) : config.getAttributeDeclaration(fingerprint))) != null)) {
                    return false;
                }
            }
            if (((parentType != null) && disallowsDefinedSiblings)) {
                Saxon.Hej.z.IntHashSet siblings = new Saxon.Hej.z.IntHashSet();
                try {
                    parentType.gatherAllPermittedChildren(siblings, true);
                } catch (Saxon.Hej.type.SchemaException e) {
                    throw new Saxon.Ejava.lang.IllegalStateException(e);
                }
                if (siblings.contains(name.getFingerprint())) {
                    return false;
                }
            }
            if ((allowedNamespaces != null)) {
                return allowedNamespaces.Contains(name.getURI());
            } else {
                return ((disallowedNamespaces == null) || !(disallowedNamespaces.Contains(name.getURI())));
            }
        }
        public string reasonForNonMatch(Saxon.Hej.om.NodeName nn, bool isElement, Saxon.Hej.Configuration config, Saxon.Eej.ee.schema.UserComplexType parentType) {
            int fingerprint = nn.obtainFingerprint(config.getNamePool());
            if (((disallowedQNames != null) && disallowedQNames.contains(fingerprint))) {
                return "The name is in the list of disallowed QNames for the wildcard. ";
            }
            if ((disallowsDefinedNames && (((isElement ? config.getElementDeclaration(fingerprint) : config.getAttributeDeclaration(fingerprint))) != null))) {
                return (("There is a global "+((isElement ? "element" : "attribute")))+" declaration for the name, and the wildcard specifies notQName='##defined'. ");
            }
            if (((parentType != null) && disallowsDefinedSiblings)) {
                Saxon.Hej.z.IntHashSet siblings = new Saxon.Hej.z.IntHashSet();
                try {
                    parentType.gatherAllPermittedChildren(siblings, true);
                } catch (Saxon.Hej.type.SchemaException e) {
                    throw new Saxon.Ejava.lang.IllegalStateException(e);
                }
                if (siblings.contains(nn.getFingerprint())) {
                    return "The wildcard disallows defined siblings, and the name is a permitted sibling. ";
                }
            }
            string uri = nn.getURI();
            if ((allowedNamespaces != null)) {
                if (allowedNamespaces.Contains(uri)) {
                    return null;
                } else {
                    return "The name is not in one of the permitted namespaces for the wildcard. ";
                }
            } else {
                if (((disallowedNamespaces == null) || !(disallowedNamespaces.Contains(uri)))) {
                    return null;
                } else {
                    return (("The name is in "+(((disallowedNamespaces.Count == 1) ? "the disallowed namespace" : "one of the disallowed namespaces")))+" for the wildcard. ");
                }
            }
        }
        public bool disallowsQName(int fingerprint) {
            return ((disallowedQNames != null) && disallowedQNames.contains(fingerprint));
        }
        public System.Collections.Generic.ISet<string> getAllowedNamespaces() {
            return allowedNamespaces;
        }
        public string getProcessContents() {
            return processContents;
        }
        public void setProcessContents(string process) {
            processContents = process;
        }
        public void setDisallowDefinedNames(bool disallow) {
            disallowsDefinedNames = disallow;
        }
        public bool isDisallowDefinedNames() {
            return disallowsDefinedNames;
        }
        public void setDisallowDefinedSiblings(bool disallow) {
            disallowsDefinedSiblings = disallow;
        }
        public bool isDisallowDefinedSiblings() {
            return disallowsDefinedSiblings;
        }
        public void addDisallowedQName(int fingerprint) {
            if ((disallowedQNames == null)) {
                disallowedQNames = new Saxon.Hej.z.IntHashSet(10);
            }
            disallowedQNames.add(fingerprint);
        }
        public bool isDisallowedQName(int fingerprint) {
            return ((disallowedQNames != null) && disallowedQNames.contains(fingerprint));
        }
        public override bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            return true;
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(64);
            if ((disallowedNamespaces != null)) {
                fsb.append("not{");
                appendNamespaces(disallowedNamespaces, fsb);
                fsb.append("}");
            } else {
                if ((allowedNamespaces != null)) {
                    fsb.append("anyOf{");
                    appendNamespaces(allowedNamespaces, fsb);
                    fsb.append("}");
                } else {
                    fsb.append("any");
                }
            }
            if ((disallowsDefinedSiblings && (disallowedQNames != null))) {
                fsb.append(" except defined siblings and other specific names");
            }
            if (disallowsDefinedSiblings) {
                fsb.append(" except defined siblings");
            }
            if ((disallowedQNames != null)) {
                int nr = disallowedQNames.size();
                if ((nr == 1)) {
                    fsb.append(" except one specific name");
                } else {
                    fsb.append(((" except "+nr)+" specific names"));
                }
            }
            return fsb.ToString();
        }
        private void appendNamespaces(System.Collections.Generic.ISet<string> namespaces, Saxon.Ejava.lang.StringBuilder sb) {
            if ((namespaces == null)) {
                return ;
            }
            System.Collections.Generic.IEnumerator<string> iter = namespaces.GetEnumerator();
            bool first = true;
            while (iter.MoveNext() /*UNSAFE CONVERSION*/) {
                if (first) {
                    first = false;
                } else {
                    sb.append(' ');
                }
                string s = iter.Current;
                sb.append((s.Length==0 ? "##local" : s));
            }
        }
        private string listOfNamespaces(System.Collections.Generic.ISet<string> namespaces) {
            if ((namespaces == null)) {
                return "";
            }
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(64);
            appendNamespaces(namespaces, sb);
            return sb.ToString();
        }
        public bool isSubset(Saxon.Eej.ee.schema.Wildcard sup, Saxon.Hej.om.NamePool pool) {
            if (!(isNamespaceSubset(sup))) {
                return false;
            }
            if ((sup.disallowedQNames != null)) {
                for (Saxon.Hej.z.IntIterator it = sup.disallowedQNames.iterator(); it.hasNext(); ) {
                    int fp = it.next();
                    string uri = pool.getURI(fp);
                    if ((this.matches(uri) && !(this.disallowsQName(fp)))) {
                        return false;
                    }
                }
            }
            if ((sup.disallowsDefinedNames && !(this.disallowsDefinedNames))) {
                return false;
            }
            return true;
        }
        private bool isNamespaceSubset(Saxon.Eej.ee.schema.Wildcard sup) {
            if (((sup.allowedNamespaces == null) && (sup.disallowedNamespaces == null))) {
                return true;
            }
            if (((sup.allowedNamespaces != null) && (this.allowedNamespaces != null))) {
                return (this.allowedNamespaces).All(sup.allowedNamespaces.Contains);
            }
            if (((this.allowedNamespaces != null) && (sup.disallowedNamespaces != null))) {
                return Saxon.Ejava.util.Collections.disjoint(this.allowedNamespaces, sup.disallowedNamespaces);
            }
            if (((sup.disallowedNamespaces != null) && (this.disallowedNamespaces != null))) {
                return (sup.disallowedNamespaces).All(this.disallowedNamespaces.Contains);
            }
            return false;
        }
        public bool overlaps(Saxon.Eej.ee.schema.Wildcard other) {
            if (((this.allowedNamespaces == null) && (this.disallowedNamespaces == null))) {
                return true;
            }
            if (((other.allowedNamespaces == null) && (other.disallowedNamespaces == null))) {
                return true;
            }
            if (((this.disallowedNamespaces != null) && (other.disallowedNamespaces != null))) {
                return true;
            }
            if (((this.disallowedNamespaces != null) && (other.allowedNamespaces != null))) {
                return !((other.allowedNamespaces).All(this.disallowedNamespaces.Contains));
            }
            if (((other.disallowedNamespaces != null) && (this.allowedNamespaces != null))) {
                return !((this.allowedNamespaces).All(other.disallowedNamespaces.Contains));
            }
            if (((this.allowedNamespaces != null) && (other.allowedNamespaces != null))) {
                return !(Saxon.Ejava.util.Collections.disjoint(this.allowedNamespaces, other.allowedNamespaces));
            }
            return true;
        }
        public static bool isNotStronger(string str1, string str2) {
            string ranking = "skip lax strict";
            int r1 = ranking.IndexOf(str1);
            int r2 = ranking.IndexOf(str2);
            return (r1<=r2);
        }
        public int compareStrength(Saxon.Eej.ee.schema.Wildcard other) {
            string ranking = "skip lax strict";
            int r1 = ranking.IndexOf(processContents);
            int r2 = ranking.IndexOf(other.processContents);
            return r1.CompareTo(r2);
        }
        public static Saxon.Eej.ee.schema.Wildcard makeIntersection(Saxon.Eej.ee.schema.Wildcard wat1, Saxon.Eej.ee.schema.Wildcard wat2, Saxon.Hej.om.NamePool pool) {
            System.Collections.Generic.ISet<string> allowed1 = wat1.getAllowedNamespaces();
            System.Collections.Generic.ISet<string> allowed2 = wat2.getAllowedNamespaces();
            System.Collections.Generic.ISet<string> disallowed1 = wat1.getDisallowedNamespaces();
            System.Collections.Generic.ISet<string> disallowed2 = wat2.getDisallowedNamespaces();
            Saxon.Eej.ee.schema.Wildcard result = new Saxon.Eej.ee.schema.Wildcard();
            result.setConfiguration(wat1.getConfiguration());
            result.setProcessContents(wat1.getProcessContents());
            result.setLocator(wat1);
            if (((allowed1 != null) && (allowed2 != null))) {
                result.allowedNamespaces = intersection(allowed1, allowed2);
            } else {
                if (((disallowed1 != null) && (disallowed2 != null))) {
                    result.disallowedNamespaces = union(disallowed1, disallowed2);
                } else {
                    if (((allowed1 == null) && (disallowed1 == null))) {
                        result.allowedNamespaces = allowed2;
                        result.disallowedNamespaces = disallowed2;
                    } else {
                        if (((allowed2 == null) && (disallowed2 == null))) {
                            result.allowedNamespaces = allowed1;
                            result.disallowedNamespaces = disallowed1;
                        } else {
                            if (((allowed1 != null) && (disallowed2 != null))) {
                                result.allowedNamespaces = difference(allowed1, disallowed2);
                            } else {
                                if (((allowed2 != null) && (disallowed1 != null))) {
                                    result.allowedNamespaces = difference(allowed2, disallowed1);
                                } else {
                                    throw new Saxon.Ejava.lang.IllegalStateException("Invalid wildcard");
                                }
                            }
                        }
                    }
                }
            }
            if (((result.disallowedNamespaces != null) && result.disallowedNamespaces.Count==0)) {
                result.disallowedNamespaces = null;
            }
            if ((wat1.disallowedQNames != null)) {
                for (Saxon.Hej.z.IntIterator it = wat1.disallowedQNames.iterator(); it.hasNext(); ) {
                    int fp = it.next();
                    string uri = pool.getURI(fp);
                    if (wat2.matches(uri)) {
                        result.addDisallowedQName(fp);
                    }
                }
            }
            if ((wat2.disallowedQNames != null)) {
                for (Saxon.Hej.z.IntIterator it = wat2.disallowedQNames.iterator(); it.hasNext(); ) {
                    int fp = it.next();
                    string uri = pool.getURI(fp);
                    if (wat1.matches(uri)) {
                        result.addDisallowedQName(fp);
                    }
                }
            }
            if ((wat1.disallowsDefinedNames || wat2.disallowsDefinedNames)) {
                result.disallowsDefinedNames = true;
            }
            return result;
        }
        public static Saxon.Eej.ee.schema.Wildcard makeUnion(Saxon.Eej.ee.schema.Wildcard wat1, Saxon.Eej.ee.schema.Wildcard wat2, Saxon.Hej.om.NamePool pool) {
            System.Collections.Generic.ISet<string> allowed1 = wat1.getAllowedNamespaces();
            System.Collections.Generic.ISet<string> allowed2 = wat2.getAllowedNamespaces();
            System.Collections.Generic.ISet<string> disallowed1 = wat1.getDisallowedNamespaces();
            System.Collections.Generic.ISet<string> disallowed2 = wat2.getDisallowedNamespaces();
            Saxon.Eej.ee.schema.Wildcard result = new Saxon.Eej.ee.schema.Wildcard();
            result.setConfiguration(wat1.getConfiguration());
            result.setProcessContents(wat1.getProcessContents());
            result.setLocator(wat1);
            if (((allowed1 != null) && (allowed2 != null))) {
                result.allowedNamespaces = union(allowed1, allowed2);
            } else {
                if (((disallowed1 != null) && (disallowed2 != null))) {
                    result.disallowedNamespaces = intersection(disallowed1, disallowed2);
                } else {
                    if (((allowed1 == null) && (disallowed1 == null))) {
                        result.allowedNamespaces = null;
                        result.disallowedNamespaces = null;
                    } else {
                        if (((allowed2 == null) && (disallowed2 == null))) {
                            result.allowedNamespaces = null;
                            result.disallowedNamespaces = null;
                        } else {
                            if (((allowed1 != null) && (disallowed2 != null))) {
                                result.disallowedNamespaces = difference(disallowed2, allowed1);
                            } else {
                                if (((allowed2 != null) && (disallowed1 != null))) {
                                    result.disallowedNamespaces = difference(disallowed1, allowed2);
                                } else {
                                    throw new Saxon.Ejava.lang.IllegalStateException("Invalid wildcard");
                                }
                            }
                        }
                    }
                }
            }
            if (((result.allowedNamespaces != null) && result.allowedNamespaces.Count==0)) {
                result.allowedNamespaces = null;
            }
            if (((result.disallowedNamespaces != null) && result.disallowedNamespaces.Count==0)) {
                result.disallowedNamespaces = null;
            }
            if ((wat1.disallowedQNames != null)) {
                for (Saxon.Hej.z.IntIterator it = wat1.disallowedQNames.iterator(); it.hasNext(); ) {
                    int fp = it.next();
                    string uri = pool.getURI(fp);
                    if ((!(wat2.matches(uri)) || wat2.disallowsQName(fp))) {
                        result.addDisallowedQName(fp);
                    }
                }
            }
            if ((wat2.disallowedQNames != null)) {
                for (Saxon.Hej.z.IntIterator it = wat2.disallowedQNames.iterator(); it.hasNext(); ) {
                    int fp = it.next();
                    string uri = pool.getURI(fp);
                    if ((!(wat1.matches(uri)) || wat1.disallowsQName(fp))) {
                        result.addDisallowedQName(fp);
                    }
                }
            }
            if ((wat1.disallowsDefinedNames && wat2.disallowsDefinedNames)) {
                result.disallowsDefinedNames = true;
            }
            return result;
        }
        private static System.Collections.Generic.ISet<string> union(System.Collections.Generic.ISet<string> a, System.Collections.Generic.ISet<string> b) {
            System.Collections.Generic.ISet<string> result = new System.Collections.Generic.HashSet<string>((a.Count+b.Count));
            foreach (string s in a) {
                result.Add(s);
            }
            foreach (string s in b) {
                result.Add(s);
            }
            return result;
        }
        private static System.Collections.Generic.ISet<string> intersection(System.Collections.Generic.ISet<string> a, System.Collections.Generic.ISet<string> b) {
            System.Collections.Generic.ISet<string> result = new System.Collections.Generic.HashSet<string>();
            foreach (string s in a) {
                if (b.Contains(s)) {
                    result.Add(s);
                }
            }
            return result;
        }
        private static System.Collections.Generic.ISet<string> difference(System.Collections.Generic.ISet<string> a, System.Collections.Generic.ISet<string> b) {
            System.Collections.Generic.ISet<string> result = new System.Collections.Generic.HashSet<string>();
            foreach (string s in a) {
                if (!(b.Contains(s))) {
                    result.Add(s);
                }
            }
            return result;
        }
        public void serialize(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            serializer.startElement("wildcard");
            serializer.emitAttribute("id", serializer.getId(this, true));
            serializer.emitAttribute("processContents", getProcessContents());
            if ((allowedNamespaces == null)) {
                if ((disallowedNamespaces == null)) {
                    serializer.emitAttribute("constraint", "any");
                } else {
                    serializer.emitAttribute("constraint", "not");
                    serializer.emitAttribute("namespaces", listOfNamespaces(disallowedNamespaces));
                }
            } else {
                serializer.emitAttribute("constraint", "enumeration");
                serializer.emitAttribute("namespaces", listOfNamespaces(allowedNamespaces));
            }
            Saxon.Ejava.lang.StringBuilder disallowedNames = new Saxon.Ejava.lang.StringBuilder(100);
            if (disallowsDefinedNames) {
                disallowedNames.append("##defined ");
            }
            if (disallowsDefinedSiblings) {
                disallowedNames.append("##definedSibling ");
            }
            if (((disallowedQNames != null) && !(disallowedQNames.isEmpty()))) {
                Saxon.Hej.om.NamePool pool = getConfiguration().getNamePool();
                for (Saxon.Hej.z.IntIterator intit = disallowedQNames.iterator(); intit.hasNext(); ) {
                    int nc = intit.next();
                    string name = pool.getClarkName(nc);
                    disallowedNames.append(name);
                    disallowedNames.append(' ');
                }
            }
            if ((disallowedNames.length()>0)) {
                disallowedNames.setLength((disallowedNames.length()-1));
                serializer.emitAttribute("notQName", disallowedNames.ToString());
            }
            serializer.endElement();
        }
        public override Saxon.Hej.om.Function getComponentAsFunction() {
            Saxon.Eej.ee.schema.Wildcard thisWildcard = this;
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return Saxon.Hej.value.StringValue.bmp("Wildcard");
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Eej.ee.schema.Wildcard>(thisWildcard);
                  case "namespace constraint":
                    Saxon.Hej.expr.CallableDelegate constraintFunction = new Saxon.Hej.expr.CallableDelegate((context1, arguments1) => {
                        string key1 = arguments[0].head().getStringValue();
                        switch (key1) {
                          case "class":
                            return Saxon.Hej.value.StringValue.bmp("Namespace Constraint");
                          case "variety":
                            if (allowsAny()) {
                                return Saxon.Hej.value.StringValue.bmp("any");
                            } else {
                                if ((allowedNamespaces != null)) {
                                    return Saxon.Hej.value.StringValue.bmp("enumeration");
                                } else {
                                    return Saxon.Hej.value.StringValue.bmp("not");
                                }
                            }
                          case "namespaces":
                            if (allowsAny()) {
                                return Saxon.Hej.value.EmptySequence.getInstance();
                            } else {
                                System.Collections.Generic.ISet<string> namespaces = ((allowedNamespaces != null) ? allowedNamespaces : disallowedNamespaces);
                                System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> uriList = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>(namespaces.Count);
                                foreach (string ns in namespaces) {
                                    uriList.Add(new Saxon.Hej.value.AnyURIValue(ns));
                                }
                                return new Saxon.Hej.om.AtomicArray(uriList);
                            }
                          case "disallowed names":
                            System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> disallowedList = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>();
                            if (disallowsDefinedNames) {
                                disallowedList.Add(Saxon.Hej.value.StringValue.bmp("defined"));
                            }
                            if (disallowsDefinedSiblings) {
                                disallowedList.Add(Saxon.Hej.value.StringValue.bmp("definedSiblings"));
                            }
                            Saxon.Hej.om.NamePool pool = getConfiguration().getNamePool();
                            Saxon.Hej.z.IntIterator iter = disallowedQNames.iterator();
                            while (iter.hasNext()) {
                                int fp = iter.next();
                                string uri = pool.getURI(fp);
                                string local = pool.getLocalName(fp);
                                Saxon.Hej.value.QNameValue value = new Saxon.Hej.value.QNameValue("", uri, local);
                                disallowedList.Add(value);
                            }
                            return new Saxon.Hej.om.AtomicArray(disallowedList);
                          default:
                            return Saxon.Hej.value.EmptySequence.getInstance();
                        }
                    } );
                    return new Saxon.Hej.functions.CallableFunction(1, constraintFunction, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
                  case "process contents":
                    return new Saxon.Hej.value.StringValue(getProcessContents());
                  default:
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            } );
            return new Saxon.Hej.functions.CallableFunction(1, callable, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
        }
    
    }
}