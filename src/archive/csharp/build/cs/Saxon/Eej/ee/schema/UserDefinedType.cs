using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public abstract class UserDefinedType : Saxon.Eej.ee.schema.SchemaStructure, Saxon.Eej.ee.schema.UserSchemaComponent, Saxon.Hej.type.SchemaType {
        protected internal int finalProhibitions = 0;
        private Saxon.Eej.ee.schema.TypeReference baseTypeReference;
        private Saxon.Hej.om.StructuredQName containingDeclarationName;
        private bool containingDeclarationIsElementNode = true;
        private int derivation = Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION;
        private int fingerprint = -1;
        private Saxon.Hej.om.StructuredQName qName = null;
        protected internal System.Collections.Generic.ISet<Saxon.Eej.ee.schema.UserComplexType> extendedTypes;
        public UserDefinedType()  {
        }
        public string getName() {
            return getTypeName().getLocalPart();
        }
        public string getTargetNamespace() {
            return getTypeName().getURI();
        }
        public int getFingerprint() {
            return fingerprint;
        }
        public Saxon.Hej.om.StructuredQName getTypeName() {
            return qName;
        }
        public virtual Saxon.Hej.om.StructuredQName getStructuredQName() {
            return qName;
        }
        public virtual string getEQName() {
            return null;
        }
        public bool isAnonymousType() {
            return ((fingerprint == -1) || getTargetNamespace().Equals(Saxon.Hej.lib.NamespaceConstant.ANONYMOUS));
        }
        public void setTypeName(Saxon.Hej.om.StructuredQName name, int fingerprint) {
            this.qName = name;
            this.fingerprint = fingerprint;
        }
        public Saxon.Eej.ee.schema.TypeReference getBaseTypeReference() {
            return baseTypeReference;
        }
        public int getBaseTypeFingerprint() {
            if ((baseTypeReference != null)) {
                return baseTypeReference.getTargetFingerprint();
            }
            return -1;
        }
        public string getDisplayName() {
            return getTypeName().getDisplayName();
        }
        public bool isComplexType() {
            return !(isSimpleType());
        }
        public virtual int getBlock() {
            return 0;
        }
        public Saxon.Hej.om.NamePool getNamePool() {
            return getConfiguration().getNamePool();
        }
        public Saxon.Hej.type.SchemaType getBaseType() {
            if ((baseTypeReference == null)) {
                if (isSimpleType()) {
                    return Saxon.Hej.type.AnySimpleType.getInstance();
                } else {
                    return Saxon.Hej.type.AnyType.getInstance();
                }
            }
            try {
                return (Saxon.Hej.type.SchemaType)(baseTypeReference.getTarget());
            } catch (Saxon.Hej.type.MissingComponentException e) {
                throw new Saxon.Ejava.lang.IllegalStateException(e);
            }
        }
        public void setBaseTypeReference(Saxon.Eej.ee.schema.TypeReference baseType) {
            this.baseTypeReference = baseType;
        }
        public int getDerivationMethod() {
            return derivation;
        }
        public bool allowsDerivation(int derivation) {
            return (((finalProhibitions&derivation)) == 0);
        }
        public int getFinalProhibitions() {
            return finalProhibitions;
        }
        public void setDerivationMethodName(string method) {
            switch (method) {
              case "restriction":
                derivation = Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION;
                break ;
              case "union":
                derivation = Saxon.Hej.type.Derivation.DERIVATION_UNION;
                break ;
              case "list":
                derivation = Saxon.Hej.type.Derivation.DERIVATION_LIST;
                break ;
              case "extension":
                derivation = Saxon.Hej.type.Derivation.DERIVATION_EXTENSION;
                break ;
              default:
                throw new Saxon.Hej.type.SchemaException(("Unknown derivation method: "+method));
            }
        }
        public void setDerivationMethod(int method) {
            derivation = method;
        }
        public void setFinalProhibitions(int finalValue) {
            finalProhibitions = finalValue;
        }
        public virtual void checkTypeDerivationIsOK(Saxon.Hej.type.SchemaType type, int block) {
            /*134*/Saxon.Eej.ee.schema.UserComplexType.checkTypeDerivation((Saxon.Hej.type.ComplexType)(this), type, block);
        }
        public void setContainingDeclaration(Saxon.Hej.om.StructuredQName declarationName, bool isElement) {
            containingDeclarationName = declarationName;
            containingDeclarationIsElementNode = isElement;
        }
        public virtual string getDescription() {
            int fp = getFingerprint();
            Saxon.Hej.om.NamePool pool = getNamePool();
            if (((fp != -1) && !(getTypeName().hasURI(Saxon.Hej.lib.NamespaceConstant.ANONYMOUS)))) {
                return getTypeName().getEQName();
            }
            if ((containingDeclarationName != null)) {
                return (("of "+((containingDeclarationIsElementNode ? "element " : "attribute ")))+containingDeclarationName.getDisplayName());
            }
            string sysId = getSystemId();
            if ((sysId == null)) {
                return pool.getEQName(fp);
            }
            int lastSlash = sysId.LastIndexOf('/');
            string module = ((lastSlash<0) ? sysId : sysId.Substring((lastSlash+1)));
            return ((("at "+module)+"#")+getLineNumber());
        }
        public override string ToString() {
            return (((isComplexType() ? "ComplexType " : "SimpleType "))+getDescription());
        }
        public Saxon.Hej.om.StructuredQName getContainingDeclarationName() {
            return containingDeclarationName;
        }
        public bool containingDeclarationIsElement() {
            return containingDeclarationIsElementNode;
        }
        public bool isSameType(Saxon.Hej.type.SchemaType other) {
            if ((this == other)) {
                return true;
            }
            if ((other is Saxon.Eej.ee.schema.UserDefinedType)) {
                Saxon.Eej.ee.schema.UserDefinedType so = (Saxon.Eej.ee.schema.UserDefinedType)(other);
                return ((getFingerprint() == other.getFingerprint()) && hasSameLocation(so));
            }
            return (getFingerprint() == other.getFingerprint());
        }
        public static bool isSameURI(string s1, string s2) {
            if (((s1 == null) || (s2 == null))) {
                return false;
            }
            if (s1.Equals(s2)) {
                return true;
            }
            s1 = Saxon.Hej.om.DocumentKey.normalizeURI(s1);
            s2 = Saxon.Hej.om.DocumentKey.normalizeURI(s2);
            return s1.Equals(s2);
        }
        protected internal void registerExtensionType(Saxon.Eej.ee.schema.UserComplexType derived) {
            if ((extendedTypes == null)) {
                extendedTypes = new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.UserComplexType>(5);
            }
            extendedTypes.Add(derived);
        }
        public System.Collections.Generic.ISet<Saxon.Eej.ee.schema.UserComplexType> getExtensionTypes() {
            if ((extendedTypes == null)) {
                return new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.UserComplexType>();
            } else {
                return extendedTypes;
            }
        }
        public void setExtendedTypes(System.Collections.Generic.ISet<Saxon.Eej.ee.schema.UserComplexType> types) {
            extendedTypes = types;
        }
    
        /* Creating stub method isSimpleType to satisfy interface SchemaType */
        public virtual bool isSimpleType() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method isAtomicType to satisfy interface SchemaType */
        public virtual bool isAtomicType() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method analyzeContentExpression to satisfy interface SchemaType */
        public virtual void analyzeContentExpression(Saxon.Hej.expr.Expression arg1, int arg2) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method atomize to satisfy interface SchemaType */
        public virtual Saxon.Hej.om.AtomicSequence atomize(Saxon.Hej.om.NodeInfo arg1) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method isIdType to satisfy interface SchemaType */
        public virtual bool isIdType() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method isIdRefType to satisfy interface SchemaType */
        public virtual bool isIdRefType() {
            throw new System.NotImplementedException();
        }

    }
}