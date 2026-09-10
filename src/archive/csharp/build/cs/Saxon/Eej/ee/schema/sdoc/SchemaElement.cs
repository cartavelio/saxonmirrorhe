using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public abstract class SchemaElement : Saxon.Hej.tree.linked.ElementImpl {
        private Saxon.Eej.ee.schema.sdoc.SchemaNodeFactory factory;
        public const int USE_DEFAULT_NS = 0;
        public const int REQUIRE_TARGET_NS = 1;
        public const int REQUIRE_NULL_NS = 2;
        public const int USE_DEFAULT_NS_WITHOUT_IMPORT = 3;
        protected internal const string NAMESPACE_ANY = "##any";
        protected internal const string NAMESPACE_LOCAL = "##local";
        protected internal const string NAMESPACE_OTHER = "##other";
        protected internal const string NAMESPACE_TARGET = "##targetNamespace";
        public SchemaElement()  {
        }
        protected internal bool isTopLevel() {
            Saxon.Hej.om.NodeInfo parent = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent());
            return ((((parent is Saxon.Eej.ee.schema.sdoc.XSDSchema) || (parent is Saxon.Eej.ee.schema.sdoc.XSDRedefine)) || (parent is Saxon.Eej.ee.schema.sdoc.XSDOverride)));
        }
        public void setSchemaNodeFactory(Saxon.Eej.ee.schema.sdoc.SchemaNodeFactory factory) {
            this.factory = factory;
        }
        public Saxon.Eej.ee.schema.sdoc.SchemaNodeFactory getSchemaNodeFactory() {
            return factory;
        }
        public virtual void processAllAttributes() {
            prepareAttributes();
            foreach (Saxon.Hej.om.NodeInfo child in children(Saxon.Hej.pattern.NodeSelector.of(x => (x is Saxon.Eej.ee.schema.sdoc.SchemaElement)))) {
                ((Saxon.Eej.ee.schema.sdoc.SchemaElement)(child)).processAllAttributes();
            }
        }
        protected internal abstract void prepareAttributes();
        public void allowAttributes(Saxon.Hej.om.AttributeMap atts, string[] allowed) {
            foreach (Saxon.Hej.om.AttributeInfo att in atts) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                if (attName.hasURI("")) {
                    if ((Saxon.Ejava.util.Arrays.binarySearch(allowed, attName.getLocalPart())<0)) {
                        error(((("Attribute "+Saxon.Hej.trans.Err.wrap(attName.getLocalPart(), Saxon.Hej.trans.Err.ATTRIBUTE))+" is not allowed on element ")+Saxon.Hej.trans.Err.wrap(getDisplayName(), Saxon.Hej.trans.Err.ELEMENT)));
                    }
                } else {
                    if (attName.hasURI(Saxon.Hej.lib.NamespaceConstant.SCHEMA)) {
                        error("Attributes in the XML Schema namespace are not allowed");
                    }
                }
            }
        }
        public void requireAttribute(Saxon.Hej.om.AttributeMap atts, string required) {
            if ((atts.getValue("", required) == null)) {
                missingAttribute(required);
            }
        }
        public virtual void validateSubtree(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            checkNoTextNodes();
            validate(compiler);
            validateChildren(compiler);
            postValidate(compiler);
        }
        private void checkNoTextNodes() {
            Saxon.Eej.ee.schema.sdoc.SchemaElement prev = this;
            foreach (Saxon.Hej.om.NodeInfo n in children()) {
                if ((n is Saxon.Eej.ee.schema.sdoc.SchemaElement)) {
                    prev = (Saxon.Eej.ee.schema.sdoc.SchemaElement)(n);
                }
                if ((n.getNodeKind() == Saxon.Hej.type.Type.TEXT)) {
                    prev.error(("Character data is not allowed as a child of "+Saxon.Hej.trans.Err.wrap(getDisplayName())));
                }
            }
        }
        protected internal virtual void validateChildren(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            foreach (Saxon.Hej.om.NodeInfo child in children(Saxon.Hej.pattern.NodeSelector.of(x => (x is Saxon.Eej.ee.schema.sdoc.SchemaElement)))) {
                ((Saxon.Eej.ee.schema.sdoc.SchemaElement)(child)).validateSubtree(compiler);
            }
        }
        public virtual void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
        }
        public virtual void postValidate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
        }
        public Saxon.Eej.ee.schema.sdoc.XSDSchema getXSDSchema() {
            Saxon.Hej.om.NodeInfo n = this;
            while (true) {
                if ((n is Saxon.Eej.ee.schema.sdoc.XSDSchema)) {
                    return (Saxon.Eej.ee.schema.sdoc.XSDSchema)(n);
                }
                n = n.getParent();
                if ((n == null)) {
                    throw new Saxon.Hej.type.SchemaException("Node does not have an xs:schema ancestor");
                }
            }
        }
        public virtual Saxon.Eej.ee.schema.SingleNamespaceSchema getSchema() {
            return getXSDSchema().getSchema();
        }
        protected internal Saxon.Eej.ee.schema.UserComplexType getContainingComplexType() {
            Saxon.Hej.om.NodeInfo parent = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent());
            while (true) {
                if ((parent is Saxon.Eej.ee.schema.sdoc.XSDComplexType)) {
                    return ((Saxon.Eej.ee.schema.sdoc.XSDComplexType)(parent)).getComplexType();
                }
                if ((parent == null)) {
                    throw new Saxon.Hej.type.SchemaException((("Found "+Saxon.Hej.trans.Err.wrap(getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" with no xs:complexType ancestor"));
                }
                parent = parent.getParent();
            }
        }
        protected internal Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition getContainingSimpleType() {
            Saxon.Hej.om.NodeInfo parent = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent());
            while (true) {
                if ((parent is Saxon.Eej.ee.schema.sdoc.XSDSimpleType)) {
                    return ((Saxon.Eej.ee.schema.sdoc.XSDSimpleType)(parent)).getSimpleTypeDefinition();
                }
                if ((parent == null)) {
                    throw new Saxon.Hej.type.SchemaException((("Found "+Saxon.Hej.trans.Err.wrap(getDisplayName()))+" with no xs:simpleType ancestor"));
                }
                parent = parent.getParent();
            }
        }
        protected internal Saxon.Hej.om.NamespaceResolver makeNamespaceContext() {
            return getAllNamespaces();
        }
        public void checkPermittedType(int fp, string name, string role) {
            if (((fp == Saxon.Hej.om.StandardNames.XS_DAY_TIME_DURATION) || (fp == Saxon.Hej.om.StandardNames.XS_YEAR_MONTH_DURATION))) {
                if ((getConfiguration().getXsdVersion() == Saxon.Hej.Configuration.XSD10)) {
                    warning((("Type "+name)+" is recognized by Saxon but is not interoperable"));
                }
            } else {
                if ((fp == Saxon.Hej.om.StandardNames.XS_DATE_TIME_STAMP)) {
                    if ((getConfiguration().getXsdVersion() == Saxon.Hej.Configuration.XSD10)) {
                        error("Type xs:dateTimeStamp requires XSD 1.1 support to be enabled");
                    }
                } else {
                    if (((fp == Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC) || (fp == Saxon.Hej.om.StandardNames.XS_UNTYPED))) {
                        error(((("Type "+name)+" cannot be used as ")+role));
                    } else {
                        if ((fp == Saxon.Hej.om.StandardNames.XS_ANY_ATOMIC_TYPE)) {
                            if ((getConfiguration().getXsdVersion() == Saxon.Hej.Configuration.XSD10)) {
                                error("Type xs:anyAtomicType requires XSD 1.1 support to be enabled");
                            }
                        }
                    }
                }
            }
        }
        public bool parseBooleanAttribute(string name, string value) {
            value = Saxon.Hej.value.Whitespace.trim(value);
            if ((value.Equals("true") || value.Equals("1"))) {
                return true;
            } else {
                if ((value.Equals("false") || value.Equals("0"))) {
                    return false;
                } else {
                    invalidAttributeValue(name, value, "Must be true(1) or false(0)");
                    return false;
                }
            }
        }
        protected internal void error(string err) {
            Saxon.Hej.lib.ErrorReporter reporter = getSchemaNodeFactory().getPipelineConfiguration().getErrorReporter();
            try {
                getXSDSchema().incrementErrorCount();
            } catch (Saxon.Hej.type.SchemaException e2) {
                throw new Saxon.Ejava.lang.IllegalStateException((("Schema document contains no xs:schema element. "+"Problem occurred while reporting error: ")+e2.Message));
            }
            if ((reporter != null)) {
                reporter.report(new Saxon.Hej.trans.XmlProcessingIncident(err, Saxon.Hej.trans.SaxonErrorCode.SXSQ0001, this));
            }
        }
        protected internal void error(Saxon.Hej.type.SchemaException exception) {
            Saxon.Hej.lib.ErrorReporter reporter = getSchemaNodeFactory().getPipelineConfiguration().getErrorReporter();
            try {
                getXSDSchema().incrementErrorCount();
            } catch (Saxon.Hej.type.SchemaException e2) {
                throw new Saxon.Ejava.lang.IllegalStateException((("Schema document contains no xs:schema element. "+"Problem occurred while reporting error: ")+exception.Message));
            }
            if ((reporter != null)) {
                reporter.report(new Saxon.Hej.trans.XmlProcessingException(exception));
            } else {
                throw exception;
            }
        }
        protected internal void warning(string err) {
            Saxon.Hej.trans.XmlProcessingIncident exception = ((/*1430*/Saxon.Hej.trans.XmlProcessingIncident)new Saxon.Hej.trans.XmlProcessingIncident(err, Saxon.Hej.trans.SaxonErrorCode.SXWN9018, this).asWarning());
            Saxon.Hej.lib.ErrorReporter reporter = getSchemaNodeFactory().getPipelineConfiguration().getErrorReporter();
            reporter.report(exception);
        }
        protected internal int getFingerprint(string name, int option) {
            Saxon.Hej.om.StructuredQName qname = getComponentName(name, option);
            return getNamePool().allocateFingerprint(qname.getURI(), qname.getLocalPart());
        }
        protected internal Saxon.Hej.om.StructuredQName getComponentName(string name, int option) {
            try {
                Saxon.Eej.ee.schema.sdoc.XSDSchema root = getXSDSchema();
                string[] parts = Saxon.Hej.om.NameChecker.getQNameParts(Saxon.Hej.value.Whitespace.trim(name));
                string uri = null;
                if (parts[0].Length==0) {
                    if (((option == USE_DEFAULT_NS) || (option == USE_DEFAULT_NS_WITHOUT_IMPORT))) {
                        uri = getURIForPrefix("", true);
                        if (("".Equals(uri) && root.isChameleon())) {
                            uri = root.getTargetNamespace();
                        }
                    } else {
                        if ((option == REQUIRE_TARGET_NS)) {
                            uri = getAttributeValue("", "targetNamespace");
                            if ((uri == null)) {
                                uri = root.getTargetNamespace();
                            }
                        } else {
                            if ((option == REQUIRE_NULL_NS)) {
                                uri = "";
                            }
                        }
                    }
                } else {
                    if (((option == REQUIRE_TARGET_NS) || (option == REQUIRE_NULL_NS))) {
                        throw new Saxon.Hej.type.SchemaException(("No namespace prefix is allowed in this name "+Saxon.Hej.trans.Err.wrap(name)));
                    }
                    uri = getURIForPrefix(parts[0], true);
                }
                if ((uri == null)) {
                    throw new Saxon.Hej.type.SchemaException(("Undeclared namespace prefix in "+Saxon.Hej.trans.Err.wrap(name)));
                }
                if ((((((option == USE_DEFAULT_NS) && !(root.getTargetNamespace().Equals(uri))) && !(root.isImportedNamespace(uri))) && !(uri.Equals(Saxon.Hej.lib.NamespaceConstant.SCHEMA))) && !(uri.Equals(Saxon.Hej.lib.NamespaceConstant.SCHEMA_INSTANCE)))) {
                    throw new Saxon.Hej.type.SchemaException((("Cannot reference schema components in "+(("".Equals(uri) ? "the null namespace" : ("namespace "+uri))))+" as it has not been imported"));
                }
                return new Saxon.Hej.om.StructuredQName(parts[0], uri, parts[1]);
            } catch (Saxon.Hej.om.QNameException err) {
                throw new Saxon.Hej.type.SchemaException(err.Message);
            }
        }
        protected internal void checkOnlyChildIsAnnotation() {
            bool foundAnnotation = false;
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                int fp = child.getFingerprint();
                if ((fp == Saxon.Hej.om.StandardNames.XS_ANNOTATION)) {
                    if (foundAnnotation) {
                        duplicateElement("annotation");
                    }
                    foundAnnotation = true;
                } else {
                    illegalElement(child);
                }
            }
        }
        protected internal void illegalElement(Saxon.Hej.om.NodeInfo child) {
            if ((child.getNodeKind() == Saxon.Hej.type.Type.TEXT)) {
            } else {
                if ((child is Saxon.Eej.ee.schema.sdoc.XSDSchema)) {
                    error(((("Element "+Saxon.Hej.trans.Err.wrap(child.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" is not allowed as a child of ")+Saxon.Hej.trans.Err.wrap(getDisplayName(), Saxon.Hej.trans.Err.ELEMENT)));
                } else {
                    if ((child is Saxon.Eej.ee.schema.sdoc.SchemaElement)) {
                        ((Saxon.Eej.ee.schema.sdoc.SchemaElement)(child)).error(((("Element "+Saxon.Hej.trans.Err.wrap(child.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" is not allowed as a child of ")+Saxon.Hej.trans.Err.wrap(getDisplayName(), Saxon.Hej.trans.Err.ELEMENT)));
                    } else {
                        error(((("Element "+Saxon.Hej.trans.Err.wrap(child.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" is not allowed as a child of ")+Saxon.Hej.trans.Err.wrap(getDisplayName(), Saxon.Hej.trans.Err.ELEMENT)));
                    }
                }
            }
        }
        protected internal void duplicateElement(string name) {
            error(((("Only one "+Saxon.Hej.trans.Err.wrap(name, Saxon.Hej.trans.Err.ELEMENT))+" is allowed as a child of ")+Saxon.Hej.trans.Err.wrap(getDisplayName(), Saxon.Hej.trans.Err.ELEMENT)));
        }
        protected internal void duplicate(string category, string name) {
            error(((((("The schema already contains "+indefiniteArticle(category, "a"))+' ')+category)+" named ")+Saxon.Hej.trans.Err.wrap(name)));
        }
        protected internal void mutuallyExclusive(string name1, string name2) {
            if (name1.Equals(name2)) {
                duplicateElement(name1);
            } else {
                error((((((("The "+Saxon.Hej.trans.Err.wrap(name1, Saxon.Hej.trans.Err.ELEMENT))+" and ")+Saxon.Hej.trans.Err.wrap(name2, Saxon.Hej.trans.Err.ELEMENT))+" elements ")+"must not both appear as children of the same ")+Saxon.Hej.trans.Err.wrap(getDisplayName(), Saxon.Hej.trans.Err.ELEMENT)));
            }
        }
        protected internal void mutuallyExclusiveElementAndAttribute(string elem, string att) {
            error((((((((((((indefiniteArticle(elem, "A")+' ')+Saxon.Hej.trans.Err.wrap(elem, Saxon.Hej.trans.Err.ELEMENT))+" element must not appear as a child of ")+indefiniteArticle(getDisplayName(), "a"))+' ')+Saxon.Hej.trans.Err.wrap(getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" with ")+indefiniteArticle(att, "a"))+' ')+Saxon.Hej.trans.Err.wrap(att, Saxon.Hej.trans.Err.ATTRIBUTE))+" attribute"));
        }
        protected internal void mutuallyExclusiveAttributes(string name1, string name2) {
            error(((((((("The "+Saxon.Hej.trans.Err.wrap(name1, Saxon.Hej.trans.Err.ATTRIBUTE))+" and ")+Saxon.Hej.trans.Err.wrap(name2, Saxon.Hej.trans.Err.ATTRIBUTE))+" attributes ")+"must not both appear on the same ")+Saxon.Hej.trans.Err.wrap(getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" element"));
        }
        protected internal void checkMutuallyExclusiveAttributes(string name1, string name2) {
            Saxon.Hej.om.AttributeMap atts = attributes();
            if (((atts.getValue("", name1) != null) && (atts.getValue("", name2) != null))) {
                mutuallyExclusiveAttributes(name1, name2);
            }
        }
        protected internal void mustPrecede(string name1, string name2) {
            error((((("The "+Saxon.Hej.trans.Err.wrap(name1, Saxon.Hej.trans.Err.ELEMENT))+" must come before the ")+Saxon.Hej.trans.Err.wrap(name2, Saxon.Hej.trans.Err.ELEMENT))+" element"));
        }
        protected internal void mustBeFirstElement(string name) {
            error(((((indefiniteArticle(name, "A")+' ')+Saxon.Hej.trans.Err.wrap(name, Saxon.Hej.trans.Err.ELEMENT))+" element is allowed only as the first child of ")+Saxon.Hej.trans.Err.wrap(getDisplayName(), Saxon.Hej.trans.Err.ELEMENT)));
        }
        protected internal string indefiniteArticle(string word, string start) {
            string article = start;
            if (("aeiouAEIOU".IndexOf(word[0])>=0)) {
                article += "n";
            } else {
                if ((word.StartsWith("xs") || word.StartsWith("xml"))) {
                    article += "n";
                }
            }
            return article;
        }
        public void invalidAttributeValue(string name, string value, string message) {
            string s = ((((("The value "+Saxon.Hej.trans.Err.wrap(value, Saxon.Hej.trans.Err.VALUE))+" is not valid for the ")+Saxon.Hej.trans.Err.wrap(name, Saxon.Hej.trans.Err.ATTRIBUTE))+" attribute of ")+Saxon.Hej.trans.Err.wrap(getDisplayName(), Saxon.Hej.trans.Err.ELEMENT));
            if ((message != null)) {
                s += (". "+message);
            }
            error(s);
        }
        public void missingAttribute(string name) {
            string s = (((("The required attribute "+Saxon.Hej.trans.Err.wrap(name, Saxon.Hej.trans.Err.ATTRIBUTE))+" is missing from the ")+Saxon.Hej.trans.Err.wrap(getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" element");
            error(s);
        }
        public void missingChildElement(string name) {
            string s = (((("The "+Saxon.Hej.trans.Err.wrap(getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" element must have a child ")+Saxon.Hej.trans.Err.wrap(name, Saxon.Hej.trans.Err.ELEMENT))+" element");
            error(s);
        }
        public int parseFinalOrBlock(string attrName, string finalValue, int allowed) {
            if (Saxon.Hej.value.Whitespace.trim(finalValue).Equals("#all")) {
                return allowed;
            } else {
                int result = 0;
                Saxon.Ejava.util.StringTokenizer tokenizer = new Saxon.Ejava.util.StringTokenizer(finalValue, " \t\n\r", false);
                while (tokenizer.hasMoreTokens()) {
                    string token = tokenizer.nextToken();
                    int v;
                    switch (token) {
                      case "union":
                        v = Saxon.Hej.type.Derivation.DERIVATION_UNION;
                        break ;
                      case "list":
                        v = Saxon.Hej.type.Derivation.DERIVATION_LIST;
                        break ;
                      case "restriction":
                        v = Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION;
                        break ;
                      case "extension":
                        v = Saxon.Hej.type.Derivation.DERIVATION_EXTENSION;
                        break ;
                      case "substitution":
                        v = Saxon.Hej.type.Derivation.DERIVE_BY_SUBSTITUTION;
                        break ;
                      default:
                        string err = ((("Invalid "+Saxon.Hej.trans.Err.wrap(attrName, Saxon.Hej.trans.Err.ATTRIBUTE))+" value ")+Saxon.Hej.trans.Err.wrap(token, Saxon.Hej.trans.Err.VALUE));
                        error(err);
                        return 0;
                    }
                    if ((((v&~(allowed))) != 0)) {
                        string err = (((("The "+Saxon.Hej.trans.Err.wrap(attrName, Saxon.Hej.trans.Err.ATTRIBUTE))+" value ")+Saxon.Hej.trans.Err.wrap(token, Saxon.Hej.trans.Err.VALUE))+" is not allowed here");
                        error(err);
                        return 0;
                    }
                    result |= v;
                }
                return result;
            }
        }
        public void processId() {
            string id = attributes().getValue("", "id");
            if ((id != null)) {
                Saxon.Hej.type.SchemaException err = getSchemaNodeFactory().registerId(id, this);
                if ((err != null)) {
                    err.setLocation(this);
                    error(err);
                }
            }
        }
        protected internal Saxon.Hej.om.NodeName allocateAnonymousTypeCode() {
            Saxon.Hej.tree.iter.AxisIterator iter = iterateAxis(Saxon.Hej.om.AxisInfo.PRECEDING_OR_ANCESTOR, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
            string moduleName = getSystemId();
            while (true) {
                int slash = moduleName.IndexOf('/');
                if ((slash<0)) {
                    break ;
                } else {
                    if ((slash == (moduleName.Length-1))) {
                        moduleName = moduleName.Substring(0, slash - 0);
                    } else {
                        moduleName = moduleName.Substring((slash+1));
                    }
                }
            }
            if (!(Saxon.Hej.om.NameChecker.isValidNmtoken(Saxon.Hej.str.StringView.of(moduleName)))) {
                moduleName = ("module"+moduleName.GetHashCode());
            }
            int count = 0;
            string topname = "";
            while (true) {
                Saxon.Eej.ee.schema.sdoc.SchemaElement node = (Saxon.Eej.ee.schema.sdoc.SchemaElement)(((/*1430*/Saxon.Hej.om.NodeInfo)iter.next()));
                count++;
                if ((node == null)) {
                    break ;
                }
                if (node.isTopLevel()) {
                    Saxon.Hej.om.AttributeMap atts = node.attributes();
                    topname = atts.getValue("", "name");
                    if ((topname == null)) {
                        topname = "";
                    }
                    break ;
                }
            }
            int tries = 0;
            Saxon.Hej.om.NamePool pool = getNamePool();
            string baseName = ((((((topname+"_anonymous_type_")+count)+"_at_line_")+getLineNumber())+"_of_")+moduleName);
            string name = baseName;
            while ((pool.getFingerprint(Saxon.Hej.lib.NamespaceConstant.ANONYMOUS, name) != -1)) {
                tries++;
                name = ((baseName+"_")+tries);
            }
            int fp = getNamePool().allocateFingerprint(Saxon.Hej.lib.NamespaceConstant.ANONYMOUS, name);
            return new Saxon.Hej.om.FingerprintedQName("anon", Saxon.Hej.lib.NamespaceConstant.ANONYMOUS, name, fp);
        }
        public int processMaxOccurs(string attValue) {
            attValue = Saxon.Hej.value.Whitespace.trim(attValue);
            if (attValue.Equals("unbounded")) {
                return -1;
            }
            try {
                Saxon.Hej.type.ConversionResult maxOccursValue = Saxon.Hej.value.IntegerValue.stringToInteger(attValue);
                if ((maxOccursValue is Saxon.Hej.type.ValidationFailure)) {
                    invalidAttributeValue("maxOccurs", attValue, "Must be numeric or 'unbounded'");
                } else {
                    if ((((Saxon.Hej.value.IntegerValue)(maxOccursValue)).compareTo(System.Int32.MaxValue)>0)) {
                        warning("maxOccurs value exceeds Saxon limits: taken as unbounded");
                    } else {
                        if ((((Saxon.Hej.value.IntegerValue)(maxOccursValue)).compareTo(0)<0)) {
                            invalidAttributeValue("maxOccurs", attValue, "The value is negative");
                        } else {
                            return (int)(((Saxon.Hej.value.IntegerValue)(maxOccursValue)).longValue());
                        }
                    }
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                invalidAttributeValue("maxOccurs", attValue, "Must be numeric or 'unbounded'");
            }
            return -1;
        }
        public int processMinOccurs(string attValue) {
            attValue = Saxon.Hej.value.Whitespace.trim(attValue);
            try {
                Saxon.Hej.type.ConversionResult minOccursValue = Saxon.Hej.value.IntegerValue.stringToInteger(attValue);
                if ((minOccursValue is Saxon.Hej.type.ValidationFailure)) {
                    invalidAttributeValue("minOccurs", attValue, "Must be numeric");
                } else {
                    if ((((Saxon.Hej.value.IntegerValue)(minOccursValue)).compareTo(System.Int32.MaxValue)>0)) {
                        warning(("minOccurs value exceeds Saxon limits: taken as "+System.Int32.MaxValue));
                        return System.Int32.MaxValue;
                    } else {
                        if ((((Saxon.Hej.value.IntegerValue)(minOccursValue)).compareTo(0)<0)) {
                            invalidAttributeValue("minOccurs", attValue, "The value is negative");
                        } else {
                            return (int)(((Saxon.Hej.value.IntegerValue)(minOccursValue)).longValue());
                        }
                    }
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                invalidAttributeValue("minOccurs", attValue, "Must be numeric");
            }
            return -1;
        }
        protected internal void checkContentModel(int[][] stateTerms, int[][] stateTargets, bool allowWildcards) {
            Saxon.Hej.om.NamePool pool = getNamePool();
            int state = 0;
            Saxon.Hej.tree.iter.AxisIterator iter = iterateAxis(Saxon.Hej.om.AxisInfo.CHILD);
            Saxon.Hej.tree.linked.NodeImpl child;
            while (((child = (Saxon.Hej.tree.linked.NodeImpl)(((/*1430*/Saxon.Hej.om.NodeInfo)iter.next()))) != null)) {
                int fp = child.getFingerprint();
                int[] terms = stateTerms[state];
                int[] targets = stateTargets[state];
                bool continueOuter = false;
                for (int i = 0; (i<terms.Length); i++) {
                    if ((fp == terms[i])) {
                        state = targets[i];
                        continueOuter = true;
                        break ;
                    }
                }
                if (continueOuter) {
                    continue ;
                }
                if ((allowWildcards && !(Saxon.Hej.lib.NamespaceConstant.SCHEMA.Equals(child.getURI())))) {
                    for (int i = 0; (i<terms.Length); i++) {
                        if ((terms[i] == -1)) {
                            state = targets[i];
                            continueOuter = true;
                            break ;
                        }
                    }
                    if (continueOuter) {
                        continue ;
                    }
                }
                Saxon.Ejava.lang.StringBuilder msg = new Saxon.Ejava.lang.StringBuilder(64);
                if ((child.getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) {
                    msg.append((("Element "+Saxon.Hej.trans.Err.wrap(child.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" cannot appear here: expected "));
                    if ((terms.Length == 0)) {
                        msg.append("no more child elements");
                    } else {
                        if ((terms.Length == 1)) {
                            msg.append(((terms[0] == -1) ? "*" : pool.getEQName(terms[0])));
                        } else {
                            msg.append("one of {");
                            for (int i = 0; (i<terms.Length); i++) {
                                msg.append(((terms[i] == -1) ? "*" : pool.getEQName(terms[i])));
                                msg.append(((i == (terms.Length-1)) ? "}" : ", "));
                            }
                        }
                    }
                    if ((isFinalState(state) && (terms.Length>0))) {
                        msg.append(" or nothing");
                    }
                } else {
                    msg.append((("Text node '"+Saxon.Hej.trans.Err.wrap(child.getUnicodeStringValue()))+"' cannot appear here"));
                }
                if ((child is Saxon.Eej.ee.schema.sdoc.SchemaElement)) {
                    ((Saxon.Eej.ee.schema.sdoc.SchemaElement)(child)).error(msg.ToString());
                } else {
                    error(msg.ToString());
                }
            }
            if (!(isFinalState(state))) {
                throw new Saxon.Hej.type.SchemaException("Content of element is incomplete");
            }
        }
        protected internal virtual bool isFinalState(int state) {
            return false;
        }
        protected internal void processAllowedNamespaces(string attValue, Saxon.Eej.ee.schema.Wildcard wildcard) {
            Saxon.Ejava.util.StringTokenizer tokenizer = new Saxon.Ejava.util.StringTokenizer(attValue, " \t\n\r", false);
            int count = tokenizer.countTokens();
            if ((count == 0)) {
                wildcard.setNoNamespacesAllowed();
            } else {
                while (tokenizer.hasMoreTokens()) {
                    string ns = tokenizer.nextToken();
                    if (((count>1) && ((NAMESPACE_ANY.Equals(ns) || NAMESPACE_OTHER.Equals(ns))))) {
                        error((ns+" is not valid when multiple namespaces are listed."));
                        return ;
                    }
                    if (ns.Equals(NAMESPACE_TARGET)) {
                        wildcard.addAllowedNamespace(getXSDSchema().getTargetNamespace());
                    } else {
                        if (ns.Equals(NAMESPACE_LOCAL)) {
                            wildcard.addAllowedNamespace("");
                        } else {
                            if (ns.Equals(NAMESPACE_OTHER)) {
                                wildcard.addDisallowedNamespace("");
                                wildcard.addDisallowedNamespace(getXSDSchema().getTargetNamespace());
                            } else {
                                if (ns.Equals(NAMESPACE_ANY)) {
                                } else {
                                    if (ns.Contains("##")) {
                                        error(("Invalid namespace URI "+Saxon.Hej.trans.Err.wrap(ns)));
                                    } else {
                                        wildcard.addAllowedNamespace(ns);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        protected internal void processDisallowedNamespaces(string attValue, Saxon.Eej.ee.schema.Wildcard wildcard) {
            Saxon.Ejava.util.StringTokenizer tokenizer = new Saxon.Ejava.util.StringTokenizer(attValue, " \t\n\r", false);
            while (tokenizer.hasMoreTokens()) {
                string ns = tokenizer.nextToken();
                if (ns.Equals(Saxon.Eej.ee.schema.sdoc.XSDAny.NAMESPACE_TARGET)) {
                    wildcard.addDisallowedNamespace(getXSDSchema().getTargetNamespace());
                } else {
                    if (ns.Equals(Saxon.Eej.ee.schema.sdoc.XSDAny.NAMESPACE_LOCAL)) {
                        wildcard.addDisallowedNamespace("");
                    } else {
                        if (ns.Contains("##")) {
                            error(("Invalid namespace URI "+Saxon.Hej.trans.Err.wrap(ns)));
                        } else {
                            wildcard.addDisallowedNamespace(ns);
                        }
                    }
                }
            }
        }
        protected internal void processDisallowedQNames(string attValue, Saxon.Eej.ee.schema.Wildcard wildcard) {
            Saxon.Ejava.util.StringTokenizer tokenizer = new Saxon.Ejava.util.StringTokenizer(attValue, " \t\n\r", false);
            while (tokenizer.hasMoreTokens()) {
                string qname = tokenizer.nextToken();
                if (qname.Equals("##defined")) {
                    wildcard.setDisallowDefinedNames(true);
                } else {
                    if (qname.Equals("##definedSibling")) {
                        wildcard.setDisallowDefinedSiblings(true);
                    } else {
                        int fp = -1;
                        try {
                            fp = getFingerprint(qname, USE_DEFAULT_NS_WITHOUT_IMPORT);
                        } catch (Saxon.Hej.type.SchemaException e) {
                            error(e.Message);
                            continue ;
                        }
                        string uri = getNamePool().getURI(fp);
                        if (!(wildcard.matches(uri))) {
                            error((("QName "+Saxon.Hej.trans.Err.wrap(qname, Saxon.Hej.trans.Err.GENERAL))+" is in a namespace that is not allowed by the wildcard"));
                        }
                        wildcard.addDisallowedQName(fp);
                    }
                }
            }
        }
        public void checkLocalTargetNamespace(string elementOrAttribute) {
            Saxon.Hej.tree.iter.AxisIterator ancestors = iterateAxis(Saxon.Hej.om.AxisInfo.ANCESTOR, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.om.StandardNames.XS_RESTRICTION, getNamePool()));
            Saxon.Hej.om.NodeInfo restriction = ((/*1430*/Saxon.Hej.om.NodeInfo)ancestors.next());
            if ((restriction == null)) {
                error((("A local targetNamespace is allowed only for an "+elementOrAttribute)+" declared within an xs:restriction"));
            } else {
                ancestors = iterateAxis(Saxon.Hej.om.AxisInfo.ANCESTOR, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.om.StandardNames.XS_COMPLEX_TYPE, getNamePool()));
                Saxon.Hej.om.NodeInfo complexType = ((/*1430*/Saxon.Hej.om.NodeInfo)ancestors.next());
                if ((complexType == null)) {
                    error("A local targetNamespace is allowed only within a complex type restriction");
                } else {
                    Saxon.Eej.ee.schema.UserComplexType ct = ((Saxon.Eej.ee.schema.sdoc.XSDComplexType)(complexType)).getComplexType();
                    if (((ct != null) && (ct.getBaseTypeFingerprint() == Saxon.Hej.om.StandardNames.XS_ANY_TYPE))) {
                        error((("A local targetNamespace is not allowed on an "+elementOrAttribute)+" within a restriction of xs:anyType"));
                    }
                }
            }
        }
        protected internal class XPathStaticContext : Saxon.Hej.sxpath.IndependentContext {
            private readonly Saxon.Eej.ee.schema.sdoc.SchemaElement schemaElement;
            public XPathStaticContext(Saxon.Eej.ee.schema.sdoc.SchemaElement schemaElement) : base (schemaElement.getConfiguration()) {
                this.schemaElement = schemaElement;
                Saxon.Hej.Configuration config = schemaElement.getConfiguration();
                getPackageData().setSchemaAware(true);
                Saxon.Hej.functions.FunctionLibraryList lib = new Saxon.Hej.functions.FunctionLibraryList();
                lib.addFunctionLibrary(Saxon.Hej.functions.registry.XPath20FunctionSet.getInstance());
                lib.addFunctionLibrary(getConfiguration().getBuiltInExtensionLibraryList());
                lib.addFunctionLibrary(new Saxon.Hej.functions.registry.ConstructorFunctionLibrary(getConfiguration()));
                lib.addFunctionLibrary(config.getIntegratedFunctionLibrary());
                config.addExtensionBinders(lib);
                setFunctionLibrary(lib);
                setXPathLanguageLevel(20);
            }
            public override Saxon.Hej.om.NamespaceResolver getNamespaceResolver() {
                return schemaElement.getAllNamespaces();
            }
            public override Saxon.Hej.expr.instruct.Executable getExecutable() {
                try {
                    return schemaElement.getXSDSchema().getExecutable();
                } catch (Saxon.Hej.type.SchemaException e) {
                    return null;
                }
            }
        
        }
    
    }
}