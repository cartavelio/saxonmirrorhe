using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public abstract class UserSimpleType : Saxon.Eej.ee.schema.UserDefinedType, Saxon.Hej.type.SimpleType, Saxon.Eej.ee.schema.UserSchemaComponent, Saxon.Eej.ee.schema.SerializableSchemaComponent {
        private System.Collections.Generic.IList<Saxon.Eej.ee.schema.Facet> facetList = new System.Collections.Generic.List<Saxon.Eej.ee.schema.Facet>(10);
        private System.Collections.Generic.IList<Saxon.Eej.ee.schema.Facet> extendedFacetList;
        private Saxon.Hej.type.SchemaComponent contextComponent;
        public UserSimpleType()  {
        }
        public override bool isSimpleType() {
            return true;
        }
        public override bool isAtomicType() {
            return false;
        }
        public override bool isIdType() {
            return false;
        }
        public override bool isIdRefType() {
            return false;
        }
        public virtual bool isListType() {
            return false;
        }
        public virtual bool isUnionType() {
            return false;
        }
        public Saxon.Hej.type.SchemaComponent getContextComponent() {
            return contextComponent;
        }
        public void setContextComponent(Saxon.Hej.type.SchemaComponent contextComponent) {
            this.contextComponent = contextComponent;
        }
        public System.Collections.Generic.IList<Saxon.Eej.ee.schema.Facet> getExtendedFacetList() {
            if ((extendedFacetList != null)) {
                return extendedFacetList;
            }
            extendedFacetList = addInheritedFacets(facetList);
            return extendedFacetList;
        }
        protected internal virtual System.Collections.Generic.IList<Saxon.Eej.ee.schema.Facet> addInheritedFacets(System.Collections.Generic.IList<Saxon.Eej.ee.schema.Facet> localFacets) {
            Saxon.Hej.type.SchemaType baseType = getBaseType();
            if ((baseType is Saxon.Eej.ee.schema.UserSimpleType)) {
                System.Collections.Generic.IList<Saxon.Eej.ee.schema.Facet> extendedFacetList = new System.Collections.Generic.List<Saxon.Eej.ee.schema.Facet>(10);
                System.Collections.Generic.ISet<string> facetsFound = new System.Collections.Generic.HashSet<string>();
                foreach (Saxon.Eej.ee.schema.Facet facet in localFacets) {
                    extendedFacetList.Add(facet);
                    facetsFound.Add(facet.getName());
                }
                System.Collections.Generic.IList<Saxon.Eej.ee.schema.Facet> baseFacetList = ((Saxon.Eej.ee.schema.UserSimpleType)(baseType)).getExtendedFacetList();
                foreach (Saxon.Eej.ee.schema.Facet facet in baseFacetList) {
                    if ((((facet is Saxon.Eej.ee.schema.PatternFacet) || (facet is Saxon.Eej.ee.schema.AssertionFacet)) || !(facetsFound.Contains(facet.getName())))) {
                        extendedFacetList.Add(facet);
                        facetsFound.Add(facet.getName());
                    }
                }
                return extendedFacetList;
            } else {
                return localFacets;
            }
        }
        public override string getEQName() {
            return ((("Q{"+getTargetNamespace())+"}")+getName());
        }
        public void addFacet(Saxon.Eej.ee.schema.Facet facet) {
            facetList.Add(facet);
        }
        public void mergeEnumerationFacets(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            bool enumFound = false;
            Saxon.Eej.ee.schema.EnumerationFacetSet efs = new Saxon.Eej.ee.schema.EnumerationFacetSet();
            Saxon.Eej.ee.schema.PatternFacet pf = null;
            Saxon.Hej.lib.ConversionRules rules = getConfiguration().getConversionRules();
            rules = rules.copy();
            rules.setNotationSet(compiler.getPreparedSchema());
            for (int i = (facetList.Count-1); (i>=0); i--) {
                if ((facetList[i] is Saxon.Eej.ee.schema.EnumerationFacet)) {
                    Saxon.Eej.ee.schema.EnumerationFacet e = (Saxon.Eej.ee.schema.EnumerationFacet)(facetList[i]);
                    Saxon.Hej.type.SchemaType @base = getBaseType();
                    Saxon.Hej.om.AtomicSequence val;
                    if ((@base is Saxon.Hej.type.SimpleType)) {
                        try {
                            val = e.getTypedValue((Saxon.Hej.type.SimpleType)(@base), rules);
                        } catch (Saxon.Hej.type.SchemaException se) {
                            compiler.error(("Enumeration value does not match base type. "+se.Message), this);
                            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                            return ;
                        }
                    } else {
                        val = new Saxon.Hej.value.StringValue(e.getValue());
                    }
                    if ((val.getLength() == 1)) {
                        Saxon.Hej.value.AtomicValue atom = ((/*1430*/Saxon.Hej.value.AtomicValue)val.head());
                        foreach (Saxon.Eej.ee.schema.Facet facet in facetList) {
                            if (!((((((facet is Saxon.Eej.ee.schema.EnumerationFacet) || (facet is Saxon.Eej.ee.schema.EnumerationFacetSet)) || (facet is Saxon.Eej.ee.schema.AssertionFacet)) || (facet is Saxon.Eej.ee.schema.WhitespaceFacet))))) {
                                if (!(facet.testAtomicValue(atom))) {
                                    Saxon.Hej.str.UnicodeString value = atom.getUnicodeStringValue();
                                    string message = ((((("Enumeration value "+Saxon.Hej.trans.Err.wrap(value, Saxon.Hej.trans.Err.VALUE))+" is not a valid instance of the type: ")+"it contravenes the ")+facet.getName())+" facet");
                                    string userMessage = facet.getMessage();
                                    if ((userMessage != null)) {
                                        message += (". "+userMessage);
                                    }
                                    compiler.warning(message, Saxon.Hej.trans.SaxonErrorCode.SXSD1010, this);
                                }
                            }
                        }
                    }
                    efs.addEnumerationValue(val, e.getValue().ToString());
                    Saxon.Impl.Helpers.ListUtils.Remove(facetList, i);
                    enumFound = true;
                } else {
                    if (((facetList[i] is Saxon.Eej.ee.schema.PatternFacet) && !(((Saxon.Eej.ee.schema.PatternFacet)(facetList[i])).isDoNotMerge()))) {
                        if ((pf == null)) {
                            pf = (Saxon.Eej.ee.schema.PatternFacet)(facetList[i]);
                        } else {
                            Saxon.Hej.str.UnicodeString pat = facetList[i].getValue();
                            pf.addBranch(pat);
                            Saxon.Impl.Helpers.ListUtils.Remove(facetList, i);
                        }
                    }
                }
            }
            if (enumFound) {
                facetList.Add(efs);
            }
            Saxon.Hej.type.SchemaType baseType = getBaseType();
            if ((baseType is Saxon.Eej.ee.schema.UserSimpleType)) {
                ((Saxon.Eej.ee.schema.UserSimpleType)(baseType)).mergeEnumerationFacets(compiler);
            }
        }
        public Saxon.Eej.ee.schema.Facet getFacet(System.Type name) {
            foreach (Saxon.Eej.ee.schema.Facet f in getExtendedFacetList()) {
                if ((f.GetType() == name)) {
                    return f;
                }
            }
            return null;
        }
        public void setFacetList(System.Collections.Generic.IList<Saxon.Eej.ee.schema.Facet> facetList) {
            this.facetList = facetList;
        }
        public void convertFacetValues() {
            foreach (Saxon.Eej.ee.schema.Facet facet in facetList) {
                if ((facet is Saxon.Eej.ee.schema.ValueRangeFacet)) {
                    ((Saxon.Eej.ee.schema.ValueRangeFacet)(facet)).convertToTargetType((Saxon.Hej.type.SimpleType)(getBaseType()), getConfiguration());
                }
            }
        }
        public int getWhitespaceAction() {
            int action = -1;
            Saxon.Eej.ee.schema.Facet ws = getFacet(typeof(Saxon.Eej.ee.schema.WhitespaceFacet));
            if ((ws != null)) {
                action = ws.getWhitespaceAction();
            }
            if ((action == -1)) {
                if (((this is Saxon.Eej.ee.schema.UserAtomicType) && (((Saxon.Eej.ee.schema.UserAtomicType)(this)).getPrimitiveType() == Saxon.Hej.om.StandardNames.XS_STRING))) {
                    Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
                    if (th.isSubType((Saxon.Eej.ee.schema.UserAtomicType)(this), Saxon.Hej.type.BuiltInAtomicType.TOKEN)) {
                        return Saxon.Hej.value.Whitespace.COLLAPSE;
                    } else {
                        if (th.isSubType((Saxon.Eej.ee.schema.UserAtomicType)(this), Saxon.Hej.type.BuiltInAtomicType.NORMALIZED_STRING)) {
                            return Saxon.Hej.value.Whitespace.REPLACE;
                        } else {
                            return Saxon.Hej.value.Whitespace.PRESERVE;
                        }
                    }
                } else {
                    if (isUnionType()) {
                        return Saxon.Hej.value.Whitespace.PRESERVE;
                    } else {
                        return Saxon.Hej.value.Whitespace.COLLAPSE;
                    }
                }
            }
            return action;
        }
        public Saxon.Hej.str.UnicodeString applyWhitespaceNormalization(Saxon.Hej.str.UnicodeString value) {
            int action = getWhitespaceAction();
            return Saxon.Hej.value.Whitespace.applyWhitespaceNormalization(action, value);
        }
        public Saxon.Hej.type.SchemaType getBuiltInBaseType() {
            Saxon.Hej.type.SchemaType @base = this;
            do {
                @base = @base.getBaseType();
                System.Diagnostics.Debug.Assert((@base != null));
            } while ((@base.getFingerprint()>1023));
            return @base;
        }
        public override void checkTypeDerivationIsOK(Saxon.Hej.type.SchemaType type, int block) {
            string s = isTypeDerivationOK(this, type, block);
            if ((s != null)) {
                throw new Saxon.Hej.type.SchemaException(s);
            }
        }
        public override bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if (!(isValidationNeeded())) {
                return true;
            }
            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATING);
            if ((getFingerprint()<1023)) {
                setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED);
                return true;
            }
            Saxon.Hej.type.SchemaType @base = getBaseType();
            if ((@base == null)) {
                Saxon.Eej.ee.schema.TypeReference @ref = new Saxon.Eej.ee.schema.TypeReference(Saxon.Hej.om.StandardNames.XS_ANY_SIMPLE_TYPE, getConfiguration(), this);
                @ref.setTarget(Saxon.Hej.type.AnySimpleType.getInstance());
                setBaseTypeReference(@ref);
            } else {
                if ((@base is Saxon.Eej.ee.schema.UserSimpleType)) {
                    ((Saxon.Eej.ee.schema.UserSimpleType)(@base)).validate(compiler);
                } else {
                    if (!(@base.isSimpleType())) {
                        compiler.error((("The base type of the simple type "+getDescription())+" is not a simple type"), this);
                        setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                        return false;
                    }
                }
            }
            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED);
            return true;
        }
        public override void analyzeContentExpression(Saxon.Hej.expr.Expression expression, int kind) {
            Saxon.Hej.type.BuiltInAtomicType.analyzeContentExpression(this, expression, kind);
        }
        public override Saxon.Hej.om.AtomicSequence atomize(Saxon.Hej.om.NodeInfo node) {
            if (node.isNilled()) {
                return Saxon.Hej.om.EmptyAtomicSequence.getInstance();
            }
            try {
                Saxon.Hej.om.NamespaceResolver resolver = null;
                if (isNamespaceSensitive()) {
                    resolver = ((node.getNodeKind() == Saxon.Hej.type.Type.ELEMENT) ? node.getAllNamespaces() : node.getParent().getAllNamespaces());
                }
                return getTypedValue(node.getUnicodeStringValue(), resolver, getConfiguration().getConversionRules());
            } catch (Saxon.Hej.type.ValidationException err) {
                throw new Saxon.Hej.trans.XPathException(("Internal error: value doesn't match its type annotation. "+err.Message));
            }
        }
        public static string isTypeDerivationOK(Saxon.Hej.type.SimpleType d, Saxon.Hej.type.SchemaType b, int derivation) {
            if ((d == b)) {
                return null;
            }
            if ((((derivation&Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION)) != 0)) {
                return ("Simple type derivation is not OK: "+"'restriction' is in the set of disallowed derivations");
            }
            Saxon.Hej.type.SchemaType actualBase = d.getBaseType();
            if (((actualBase != null) && !(actualBase.allowsDerivation(Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION)))) {
                return ((("Simple type derivation is not OK: "+"the base type of type ")+d.getDescription())+" does not allow derivation by restriction");
            }
            if (((actualBase == b) || (actualBase.getFingerprint() == b.getFingerprint()))) {
                return null;
            }
            string reason = null;
            if ((actualBase != Saxon.Hej.type.AnyType.getInstance())) {
                if ((actualBase is Saxon.Hej.type.SimpleType)) {
                    reason = isTypeDerivationOK((Saxon.Hej.type.SimpleType)(actualBase), b, derivation);
                    if ((reason == null)) {
                        return null;
                    }
                } else {
                    try {
                        /*134*/Saxon.Eej.ee.schema.UserComplexType.checkTypeDerivation((Saxon.Hej.type.ComplexType)(actualBase), b, derivation);
                        return null;
                    } catch (Saxon.Hej.type.SchemaException err) {
                        reason = err.Message;
                    }
                }
            }
            if ((d.isListType() || d.isUnionType())) {
                if ((b == Saxon.Hej.type.AnySimpleType.getInstance())) {
                    return null;
                }
            }
            if ((((b.isSimpleType() && ((Saxon.Hej.type.SimpleType)(b)).isUnionType()) && (b != Saxon.Hej.type.ErrorType.getInstance())) && ((Saxon.Eej.ee.schema.UserUnionType)(b)).getExtendedFacetList().Count==0)) {
                foreach (Saxon.Eej.ee.schema.TypeReference @ref in ((Saxon.Eej.ee.schema.UserUnionType)(b)).getMemberTypeReferences()) {
                    Saxon.Hej.type.SimpleType member = (Saxon.Hej.type.SimpleType)(@ref.getTarget());
                    reason = isTypeDerivationOK(d, member, derivation);
                    if ((reason == null)) {
                        return null;
                    }
                }
            }
            return (((("The type "+d.getDescription())+" is not validly derived from the type ")+b.getDescription())+((((reason == null) || reason.Contains(" is not validly derived from the type ")) ? "" : (". "+reason))));
        }
        public System.Collections.Generic.IList<Saxon.Eej.ee.schema.Facet> getLocalFacetList() {
            return facetList;
        }
        public Saxon.Hej.str.UnicodeString preprocess(Saxon.Hej.str.UnicodeString input) {
            foreach (Saxon.Eej.ee.schema.Facet f in getExtendedFacetList()) {
                if ((f is Saxon.Eej.ee.schema.PreprocessFacet)) {
                    input = ((Saxon.Eej.ee.schema.PreprocessFacet)(f)).preprocess(input);
                }
            }
            return input;
        }
        public Saxon.Hej.str.UnicodeString postprocess(Saxon.Hej.str.UnicodeString input) {
            foreach (Saxon.Eej.ee.schema.Facet f in getExtendedFacetList()) {
                if ((f is Saxon.Eej.ee.schema.PreprocessFacet)) {
                    input = ((Saxon.Eej.ee.schema.PreprocessFacet)(f)).postprocess(input);
                }
            }
            return input;
        }
        public Saxon.Hej.type.ValidationFailure checkAgainstFacets(Saxon.Hej.om.AtomicSequence primitiveValue, Saxon.Hej.str.UnicodeString lexicalValue, System.Collections.Generic.IList<Saxon.Eej.ee.schema.Facet> facets, Saxon.Hej.lib.ConversionRules rules) {
            Saxon.Hej.str.UnicodeString normalizedValue = null;
            foreach (Saxon.Eej.ee.schema.Facet f in facets) {
                if ((f is Saxon.Eej.ee.schema.PatternFacet)) {
                    if ((normalizedValue == null)) {
                        if ((lexicalValue == null)) {
                            normalizedValue = primitiveValue.getUnicodeStringValue();
                        } else {
                            normalizedValue = applyWhitespaceNormalization(lexicalValue);
                        }
                    }
                    if (!(f.testAtomicValue(new Saxon.Hej.value.StringValue(normalizedValue)))) {
                        return makeValidationFailure(primitiveValue, this, f);
                    }
                } else {
                    if ((f is Saxon.Eej.ee.schema.AssertionFacet)) {
                        if ((primitiveValue is Saxon.Hej.value.AtomicValue)) {
                            Saxon.Hej.value.AtomicValue av = (Saxon.Hej.value.AtomicValue)(primitiveValue);
                            if (!(((Saxon.Eej.ee.schema.AssertionFacet)(f)).testAtomicValue(av, rules))) {
                                return makeValidationFailure(av, this, f);
                            }
                        } else {
                            if (!(((Saxon.Eej.ee.schema.AssertionFacet)(f)).testListValue(primitiveValue, rules))) {
                                return makeValidationFailure(primitiveValue, this, f);
                            }
                        }
                    } else {
                        if ((primitiveValue is Saxon.Hej.value.AtomicValue)) {
                            Saxon.Hej.value.AtomicValue av = (Saxon.Hej.value.AtomicValue)(primitiveValue);
                            if (!(f.testAtomicValue(av))) {
                                return makeValidationFailure(av, this, f);
                            }
                        } else {
                            if (!(f.testListValue(primitiveValue))) {
                                return makeValidationFailure(primitiveValue, this, f);
                            }
                        }
                    }
                }
            }
            return null;
        }
        public static Saxon.Hej.type.ValidationFailure makeValidationFailure(Saxon.Hej.om.AtomicSequence primitiveValue, Saxon.Hej.type.SimpleType type, Saxon.Eej.ee.schema.Facet f) {
            string message = ((((((("Value "+Saxon.Hej.trans.Err.wrap(displayValue(primitiveValue), Saxon.Hej.trans.Err.VALUE))+" contravenes the ")+f.getName())+" facet ")+(((f.getValue() == null) ? "" : (Saxon.Hej.trans.Err.wrap(f.getValue(), Saxon.Hej.trans.Err.VALUE)+' '))))+"of the type ")+type.getDescription());
            if ((f.getMessage() != null)) {
                message += (". "+f.getMessage());
            }
            Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
            ve.setConstraintReference(2, f.getConstraintName(), "1");
            ve.setErrorCode("FORG0001");
            return ve;
        }
        private static string displayValue(Saxon.Hej.om.AtomicSequence primitiveValue) {
            string v = primitiveValue.getUnicodeStringValue().ToString();
            if ((v.Length>30)) {
                v = (v.Substring(0, 30 - 0)+"...");
            }
            return v;
        }
        public override void elaborate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((getValidationStatus() == Saxon.Hej.type.SchemaValidationStatus.UNVALIDATED)) {
                if ((getBaseType() is Saxon.Eej.ee.schema.UserSchemaComponent)) {
                    ((Saxon.Eej.ee.schema.UserSchemaComponent)(getBaseType())).elaborate(compiler);
                }
                foreach (Saxon.Eej.ee.schema.Facet facet in facetList) {
                    if ((facet is Saxon.Eej.ee.schema.ValueRangeFacet)) {
                        ((Saxon.Eej.ee.schema.ValueRangeFacet)(facet)).convertToTargetType((Saxon.Hej.type.SimpleType)(getBuiltInBaseType()), getConfiguration());
                    }
                }
                mergeEnumerationFacets(compiler);
                setValidationStatus(Saxon.Hej.type.SchemaValidationStatus.VALIDATED);
            }
        }
        public virtual void serialize(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            string id = serializer.getId(this, true);
            serializer.startElement("simpleType");
            serializer.emitAttribute("id", id);
            if (!(isAnonymousType())) {
                serializer.emitAttribute("name", getName());
                if (!(getTargetNamespace().isEmpty())) {
                    serializer.emitAttribute("targetNamespace", getTargetNamespace().ToString());
                }
            }
            serializer.emitAttribute("base", serializer.getTypeLink(getBaseType()));
            if ((finalProhibitions != 0)) {
                string ex = "";
                if ((((finalProhibitions&Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION)) != 0)) {
                    ex = "restriction";
                }
                if ((((finalProhibitions&Saxon.Hej.type.Derivation.DERIVATION_LIST)) != 0)) {
                    ex += (((ex.Length==0 ? "" : " "))+"list");
                }
                if ((((finalProhibitions&Saxon.Hej.type.Derivation.DERIVATION_UNION)) != 0)) {
                    ex += (((ex.Length==0 ? "" : " "))+"union");
                }
                serializer.emitAttribute("final", ex);
            }
            serializeVariety(serializer);
            bool seenEnumeration = false;
            foreach (Saxon.Eej.ee.schema.Facet f in getExtendedFacetList()) {
                if ((f is Saxon.Eej.ee.schema.EnumerationFacetSet)) {
                    if (seenEnumeration) {
                        continue ;
                    }
                    seenEnumeration = true;
                }
                f.serializeFacet(serializer);
            }
            serializer.endElement();
        }
        protected internal abstract void serializeVariety(Saxon.Eej.ee.schema.SchemaModelSerializer serializer);
        public override Saxon.Hej.om.FunctionItem getComponentAsFunction() {
            return getComponentAsFunction(this);
        }
        public static Saxon.Hej.om.FunctionItem getComponentAsFunction(Saxon.Hej.type.SimpleType type) {
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return Saxon.Hej.value.StringValue.bmp("Simple Type Definition");
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Hej.type.SimpleType>(type);
                  case "name":
                    if (type.isAnonymousType()) {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    } else {
                        return new Saxon.Hej.value.StringValue(type.getName(), Saxon.Hej.type.BuiltInAtomicType.NCNAME);
                    }
                  case "target namespace":
                    if (type.isAnonymousType()) {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    } else {
                        return new Saxon.Hej.value.AnyURIValue((type.getTargetNamespace().toUnicodeString()));
                    }
                  case "final":
                    {
                        System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> list = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>();
                        int f = type.getFinalProhibitions();
                        if ((((f&Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION)) != 0)) {
                            list.Add(Saxon.Hej.value.StringValue.bmp("restriction"));
                        }
                        if ((((f&Saxon.Hej.type.Derivation.DERIVATION_EXTENSION)) != 0)) {
                            list.Add(Saxon.Hej.value.StringValue.bmp("extension"));
                        }
                        if ((((f&Saxon.Hej.type.Derivation.DERIVATION_LIST)) != 0)) {
                            list.Add(Saxon.Hej.value.StringValue.bmp("list"));
                        }
                        if ((((f&Saxon.Hej.type.Derivation.DERIVATION_UNION)) != 0)) {
                            list.Add(Saxon.Hej.value.StringValue.bmp("union"));
                        }
                        return new Saxon.Hej.om.AtomicArray(list);
                    }
                  case "context":
                    if (type.isAnonymousType()) {
                        Saxon.Eej.ee.schema.UserSimpleType stype = (Saxon.Eej.ee.schema.UserSimpleType)(type);
                        Saxon.Hej.type.SchemaComponent contextComponent = stype.getContextComponent();
                        if ((contextComponent == null)) {
                            return Saxon.Hej.value.EmptySequence.getInstance();
                        } else {
                            return contextComponent.getComponentAsFunction();
                        }
                    } else {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    }
                  case "base type definition":
                    return type.getBaseType().getComponentAsFunction();
                  case "facets":
                    {
                        if ((type is Saxon.Eej.ee.schema.UserSimpleType)) {
                            System.Collections.Generic.IList<Saxon.Hej.om.Item> list = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                            foreach (Saxon.Eej.ee.schema.Facet f in ((Saxon.Eej.ee.schema.UserSimpleType)(type)).getExtendedFacetList()) {
                                list.Add(f.getFacetAsFunctionItem());
                            }
                            return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(list);
                        } else {
                            if ((type is Saxon.Hej.type.BuiltInAtomicType)) {
                                System.Collections.Generic.IList<Saxon.Eej.ee.schema.Facet> facetList = getExtendedFacetList((Saxon.Hej.type.BuiltInAtomicType)(type), context.getConfiguration());
                                System.Collections.Generic.IList<Saxon.Hej.om.Item> list = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                                foreach (Saxon.Eej.ee.schema.Facet f in facetList) {
                                    list.Add(f.getFacetAsFunctionItem());
                                }
                                return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(list);
                            } else {
                                return Saxon.Hej.value.EmptySequence.getInstance();
                            }
                        }
                    }
                  case "fundamental facets":
                    return Saxon.Hej.value.EmptySequence.getInstance();
                  case "variety":
                    string variety = "atomic";
                    if (type.isListType()) {
                        variety = "list";
                    } else {
                        if (type.isUnionType()) {
                            variety = "union";
                        }
                    }
                    return new Saxon.Hej.value.StringValue(variety);
                  case "primitive type definition":
                    if (((type == Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC) || !(type.isAtomicType()))) {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    } else {
                        return getComponentAsFunction((Saxon.Hej.type.AtomicType)((((Saxon.Hej.type.AtomicType)(type)).getPrimitiveItemType())));
                    }
                  case "item type definition":
                    if (type.isListType()) {
                        return ((Saxon.Hej.type.ListType)(type)).getItemType().getComponentAsFunction();
                    } else {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    }
                  case "member type definitions":
                    if (type.isUnionType()) {
                        System.Collections.Generic.IList<Saxon.Hej.om.Item> list = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                        foreach (Saxon.Eej.ee.schema.TypeReference member in ((Saxon.Eej.ee.schema.UserUnionType)(type)).getMemberTypeReferences()) {
                            list.Add(member.getTarget().getComponentAsFunction());
                        }
                        return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(list);
                    } else {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    }
                  default:
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            } );
            return new Saxon.Hej.functions.CallableFunction(1, callable, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
        }
        private static System.Collections.Generic.IList<Saxon.Eej.ee.schema.Facet> getExtendedFacetList(Saxon.Hej.type.BuiltInAtomicType type, Saxon.Hej.Configuration config) {
            System.Collections.Generic.IList<Saxon.Eej.ee.schema.Facet> list = new System.Collections.Generic.List<Saxon.Eej.ee.schema.Facet>(4);
            int prim = type.getPrimitiveType();
            int fp = type.getFingerprint();
            if ((prim == Saxon.Hej.om.StandardNames.XS_INTEGER)) {
                Saxon.Hej.value.IntegerValue min = Saxon.Hej.value.Int64Value.getMinInclusive(type);
                if ((min != null)) {
                    Saxon.Eej.ee.schema.MinInclusiveFacet facet = new Saxon.Eej.ee.schema.MinInclusiveFacet();
                    facet.setRequiredValue(min);
                    list.Add(facet);
                }
                Saxon.Hej.value.IntegerValue max = Saxon.Hej.value.Int64Value.getMaxInclusive(type);
                if ((max != null)) {
                    Saxon.Eej.ee.schema.MaxInclusiveFacet facet = new Saxon.Eej.ee.schema.MaxInclusiveFacet();
                    facet.setRequiredValue(max);
                    list.Add(facet);
                }
            } else {
                if ((prim == Saxon.Hej.om.StandardNames.XS_STRING)) {
                    switch (fp) {
                      case Saxon.Hej.om.StandardNames.XS_NORMALIZED_STRING:
                      case Saxon.Hej.om.StandardNames.XS_TOKEN:
                        break ;
                      case Saxon.Hej.om.StandardNames.XS_LANGUAGE:
                        {
                            Saxon.Eej.ee.schema.PatternFacet pat = new Saxon.Eej.ee.schema.PatternFacet(config);
                            pat.setValue("[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*");
                            list.Add(pat);
                            break ;
                        }
                      case Saxon.Hej.om.StandardNames.XS_NMTOKEN:
                        {
                            Saxon.Eej.ee.schema.PatternFacet pat = new Saxon.Eej.ee.schema.PatternFacet(config);
                            pat.setValue("\\c+");
                            list.Add(pat);
                            break ;
                        }
                      case Saxon.Hej.om.StandardNames.XS_NAME:
                        {
                            Saxon.Eej.ee.schema.PatternFacet pat = new Saxon.Eej.ee.schema.PatternFacet(config);
                            pat.setValue("\\i\\c+");
                            list.Add(pat);
                            break ;
                        }
                      case Saxon.Hej.om.StandardNames.XS_NCNAME:
                      case Saxon.Hej.om.StandardNames.XS_ID:
                      case Saxon.Hej.om.StandardNames.XS_IDREF:
                      case Saxon.Hej.om.StandardNames.XS_ENTITY:
                        {
                            Saxon.Eej.ee.schema.PatternFacet pat = new Saxon.Eej.ee.schema.PatternFacet(config);
                            pat.setValue("\\i\\c+");
                            list.Add(pat);
                            pat = new Saxon.Eej.ee.schema.PatternFacet(config);
                            pat.setValue("[\\i-[:]][\\c-[:]]*");
                            list.Add(pat);
                            break ;
                        }
                      default:
                        break ;
                    }
                } else {
                    if ((fp == Saxon.Hej.om.StandardNames.XS_DAY_TIME_DURATION)) {
                        Saxon.Eej.ee.schema.PatternFacet pat = new Saxon.Eej.ee.schema.PatternFacet(config);
                        pat.setValue("[^YM]*(T.*)?");
                        list.Add(pat);
                    } else {
                        if ((fp == Saxon.Hej.om.StandardNames.XS_YEAR_MONTH_DURATION)) {
                            Saxon.Eej.ee.schema.PatternFacet pat = new Saxon.Eej.ee.schema.PatternFacet(config);
                            pat.setValue("[^DT]*");
                            list.Add(pat);
                        } else {
                            if ((fp == Saxon.Hej.om.StandardNames.XS_DATE_TIME_STAMP)) {
                                Saxon.Eej.ee.schema.ExplicitTimezoneFacet etf = new Saxon.Eej.ee.schema.ExplicitTimezoneFacet();
                                try {
                                    etf.setValue("required");
                                } catch (Saxon.Hej.type.SchemaException e) {
                                    throw new Saxon.Ejava.lang.AssertionError(e);
                                }
                                list.Add(etf);
                            }
                        }
                    }
                }
            }
            return list;
        }
    
        /* Creating stub method isBuiltInType to satisfy interface SimpleType */
        public virtual bool isBuiltInType() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getTypedValue to satisfy interface SimpleType */
        public virtual Saxon.Hej.om.AtomicSequence getTypedValue(Saxon.Hej.str.UnicodeString arg1, Saxon.Hej.om.NamespaceResolver arg2, Saxon.Hej.lib.ConversionRules arg3) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method validateContent to satisfy interface SimpleType */
        public virtual Saxon.Hej.type.ValidationFailure validateContent(Saxon.Hej.str.UnicodeString arg1, Saxon.Hej.om.NamespaceResolver arg2, Saxon.Hej.lib.ConversionRules arg3) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method isNamespaceSensitive to satisfy interface HyperType */
        public virtual bool isNamespaceSensitive() {
            throw new System.NotImplementedException();
        }

    }
}