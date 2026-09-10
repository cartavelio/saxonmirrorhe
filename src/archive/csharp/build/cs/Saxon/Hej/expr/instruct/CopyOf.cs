using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class CopyOf : Saxon.Hej.expr.instruct.Instruction, Saxon.Hej.expr.instruct.ValidatingInstruction {
        private readonly Saxon.Hej.expr.Operand selectOp;
        private readonly bool copyNamespaces;
        private bool copyAccumulators;
        private readonly int validation;
        private readonly Saxon.Hej.type.SchemaType schemaType;
        private bool requireDocumentOrElement = false;
        private readonly bool rejectDuplicateAttributes;
        private readonly bool validating;
        private bool copyLineNumbers = false;
        private bool copyForUpdate = false;
        private bool isSchemaAware = true;
        private double invocations = 1;
        private double numberOfItems = 20;
        public CopyOf(Saxon.Hej.expr.Expression select, bool copyNamespaces, int validation, Saxon.Hej.type.SchemaType schemaType, bool rejectDuplicateAttributes)  {
            selectOp = new Saxon.Hej.expr.Operand(this, select, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            this.copyNamespaces = copyNamespaces;
            this.validation = validation;
            this.schemaType = schemaType;
            validating = (((schemaType != null) || (validation == Saxon.Hej.lib.Validation.STRICT)) || (validation == Saxon.Hej.lib.Validation.LAX));
            this.rejectDuplicateAttributes = rejectDuplicateAttributes;
        }
        public Saxon.Hej.expr.Expression getSelect() {
            return selectOp.getChildExpression();
        }
        public void setSelect(Saxon.Hej.expr.Expression select) {
            selectOp.setChildExpression(select);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return selectOp;
        }
        public int getValidationAction() {
            return validation;
        }
        public bool isValidating() {
            return validating;
        }
        public Saxon.Hej.type.SchemaType getSchemaType() {
            return schemaType;
        }
        public void setSchemaAware(bool schemaAware) {
            this.isSchemaAware = schemaAware;
        }
        public void setCopyLineNumbers(bool copy) {
            copyLineNumbers = copy;
        }
        public override bool mayCreateNewNodes() {
            return !(getSelect().getItemType().isPlainType());
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.XSL_COPY_OF;
        }
        public void setRequireDocumentOrElement(bool requireDocumentOrElement) {
            this.requireDocumentOrElement = requireDocumentOrElement;
        }
        public bool isDocumentOrElementRequired() {
            return requireDocumentOrElement;
        }
        public void setCopyForUpdate(bool forUpdate) {
            copyForUpdate = forUpdate;
        }
        public bool isCopyForUpdate() {
            return copyForUpdate;
        }
        public override int getImplementationMethod() {
            return ((ITERATE_METHOD|PROCESS_METHOD)|WATCH_METHOD);
        }
        public bool isCopyNamespaces() {
            return copyNamespaces;
        }
        public void setCopyAccumulators(bool copy) {
            copyAccumulators = copy;
        }
        public bool isCopyAccumulators() {
            return copyAccumulators;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.CopyOf c = new Saxon.Hej.expr.instruct.CopyOf(getSelect().copy(rebindings), copyNamespaces, validation, schemaType, rejectDuplicateAttributes);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, c);
            c.setCopyForUpdate(copyForUpdate);
            c.setCopyLineNumbers(copyLineNumbers);
            c.isSchemaAware = isSchemaAware;
            c.setCopyAccumulators(copyAccumulators);
            return c;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            Saxon.Hej.type.ItemType @in = getSelect().getItemType();
            if (!(isSchemaAware)) {
                return @in;
            }
            Saxon.Hej.Configuration config = getConfiguration();
            if ((schemaType != null)) {
                Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
                Saxon.Hej.type.Affinity e = th.relationship(@in, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
                if (((e == Saxon.Hej.type.Affinity.SAME_TYPE) || (e == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                    return new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ELEMENT, schemaType, config, false);
                }
                Saxon.Hej.type.Affinity a = th.relationship(@in, Saxon.Hej.pattern.NodeKindTest.ATTRIBUTE);
                if (((a == Saxon.Hej.type.Affinity.SAME_TYPE) || (a == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                    return new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ATTRIBUTE, schemaType, config, false);
                }
            } else {
                switch (validation) {
                  case Saxon.Hej.lib.Validation.PRESERVE:
                    return @in;
                  case Saxon.Hej.lib.Validation.STRIP:
                    {
                        Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
                        Saxon.Hej.type.Affinity e = th.relationship(@in, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
                        if (((e == Saxon.Hej.type.Affinity.SAME_TYPE) || (e == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                            return new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.type.Untyped.getInstance(), config, false);
                        }
                        Saxon.Hej.type.Affinity a = th.relationship(@in, Saxon.Hej.pattern.NodeKindTest.ATTRIBUTE);
                        if (((a == Saxon.Hej.type.Affinity.SAME_TYPE) || (a == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                            return new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ATTRIBUTE, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, config, false);
                        }
                        if (((e != Saxon.Hej.type.Affinity.DISJOINT) || (a != Saxon.Hej.type.Affinity.DISJOINT))) {
                            if ((@in is Saxon.Hej.pattern.NodeTest)) {
                                return Saxon.Hej.pattern.AnyNodeTest.getInstance();
                            } else {
                                return Saxon.Hej.type.AnyItemType.getInstance();
                            }
                        } else {
                            return @in;
                        }
                    }
                  case Saxon.Hej.lib.Validation.STRICT:
                  case Saxon.Hej.lib.Validation.LAX:
                    if ((@in is Saxon.Hej.pattern.NodeTest)) {
                        Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
                        int fp = ((Saxon.Hej.pattern.NodeTest)(@in)).getFingerprint();
                        if ((fp != -1)) {
                            Saxon.Hej.type.Affinity e = th.relationship(@in, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
                            if (((e == Saxon.Hej.type.Affinity.SAME_TYPE) || (e == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                                Saxon.Hej.type.SchemaDeclaration elem = config.getElementDeclaration(fp);
                                if ((elem != null)) {
                                    try {
                                        return new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ELEMENT, elem.getType(), config, false);
                                    } catch (Saxon.Hej.type.MissingComponentException e1) {
                                        return new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.type.AnyType.getInstance(), config, false);
                                    }
                                } else {
                                    return new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.type.AnyType.getInstance(), config, false);
                                }
                            }
                            Saxon.Hej.type.Affinity a = th.relationship(@in, Saxon.Hej.pattern.NodeKindTest.ATTRIBUTE);
                            if (((a == Saxon.Hej.type.Affinity.SAME_TYPE) || (a == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                                Saxon.Hej.type.SchemaDeclaration attr = config.getElementDeclaration(fp);
                                if ((attr != null)) {
                                    try {
                                        return new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ATTRIBUTE, attr.getType(), config, false);
                                    } catch (Saxon.Hej.type.MissingComponentException e1) {
                                        return new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ATTRIBUTE, Saxon.Hej.type.AnySimpleType.getInstance(), config, false);
                                    }
                                } else {
                                    return new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ATTRIBUTE, Saxon.Hej.type.AnySimpleType.getInstance(), config, false);
                                }
                            }
                        } else {
                            Saxon.Hej.type.Affinity e = th.relationship(@in, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
                            if (((e == Saxon.Hej.type.Affinity.SAME_TYPE) || (e == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                                return Saxon.Hej.pattern.NodeKindTest.ELEMENT;
                            }
                            Saxon.Hej.type.Affinity a = th.relationship(@in, Saxon.Hej.pattern.NodeKindTest.ATTRIBUTE);
                            if (((a == Saxon.Hej.type.Affinity.SAME_TYPE) || (a == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                                return Saxon.Hej.pattern.NodeKindTest.ATTRIBUTE;
                            }
                        }
                        return Saxon.Hej.pattern.AnyNodeTest.getInstance();
                    } else {
                        if ((@in is Saxon.Hej.type.AtomicType)) {
                            return @in;
                        } else {
                            return Saxon.Hej.type.AnyItemType.getInstance();
                        }
                    }
                }
            }
            return getSelect().getItemType();
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            return getSelect().getStaticUType(contextItemType);
        }
        public override int getCardinality() {
            return getSelect().getCardinality();
        }
        public override int getDependencies() {
            return getSelect().getDependencies();
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            typeCheckChildren(visitor, contextInfo);
            if (isDocumentOrElementRequired()) {
                Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.TYPE_OP, "validate", 0, "XQTY0030");
                Saxon.Hej.Configuration config = visitor.getConfiguration();
                setSelect(config.getTypeChecker(false).staticTypeCheck(getSelect(), Saxon.Hej.value.SequenceType.SINGLE_NODE, role, visitor));
                Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
                Saxon.Hej.type.ItemType t = getSelect().getItemType();
                if (th.isSubType(t, Saxon.Hej.pattern.NodeKindTest.ATTRIBUTE)) {
                    throw new Saxon.Hej.trans.XPathException("validate{} expression cannot be applied to an attribute", "XQTY0030");
                }
                if (th.isSubType(t, Saxon.Hej.pattern.NodeKindTest.TEXT)) {
                    throw new Saxon.Hej.trans.XPathException("validate{} expression cannot be applied to a text node", "XQTY0030");
                }
                if (th.isSubType(t, Saxon.Hej.pattern.NodeKindTest.COMMENT)) {
                    throw new Saxon.Hej.trans.XPathException("validate{} expression cannot be applied to a comment node", "XQTY0030");
                }
                if (th.isSubType(t, Saxon.Hej.pattern.NodeKindTest.PROCESSING_INSTRUCTION)) {
                    throw new Saxon.Hej.trans.XPathException("validate{} expression cannot be applied to a processing instruction node", "XQTY0030");
                }
                if (th.isSubType(t, Saxon.Hej.pattern.NodeKindTest.NAMESPACE)) {
                    throw new Saxon.Hej.trans.XPathException("validate{} expression cannot be applied to a namespace node", "XQTY0030");
                }
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            selectOp.optimize(visitor, contextItemType);
            if (Saxon.Hej.expr.Literal.isEmptySequence(getSelect())) {
                return getSelect();
            }
            adoptChildExpression(getSelect());
            if (getSelect().getItemType().isPlainType()) {
                return getSelect();
            }
            return this;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("copyOf", this);
            if ((validation != Saxon.Hej.lib.Validation.SKIP)) {
                @out.emitAttribute("validation", Saxon.Hej.lib.Validation.describe(validation));
            }
            if ((schemaType != null)) {
                @out.emitAttribute("type", schemaType.getStructuredQName());
            }
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(16);
            if (requireDocumentOrElement) {
                fsb.append('p');
            }
            if (rejectDuplicateAttributes) {
                fsb.append('a');
            }
            if (validating) {
                fsb.append('v');
            }
            if (copyLineNumbers) {
                fsb.append('l');
            }
            if (copyForUpdate) {
                fsb.append('u');
            }
            if (isSchemaAware) {
                fsb.append('s');
            }
            if (copyNamespaces) {
                fsb.append('c');
            }
            if (copyAccumulators) {
                fsb.append('m');
            }
            if ((fsb.length() != 0)) {
                @out.emitAttribute("flags", fsb.ToString());
            }
            getSelect().export(@out);
            @out.endElement();
        }
        public override string getStreamerName() {
            return "CopyOf";
        }
        public override Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            Saxon.Hej.expr.parser.PathMap.PathMapNodeSet result = base.addToPathMap(pathMap, pathMapNodeSet);
            result.setReturnable(false);
            Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
            Saxon.Hej.type.ItemType type = getItemType();
            if (((th.relationship(type, Saxon.Hej.pattern.NodeKindTest.ELEMENT) != Saxon.Hej.type.Affinity.DISJOINT) || (th.relationship(type, Saxon.Hej.pattern.NodeKindTest.DOCUMENT) != Saxon.Hej.type.Affinity.DISJOINT))) {
                result.addDescendants();
            }
            return new Saxon.Hej.expr.parser.PathMap.PathMapNodeSet(pathMap.makeNewRoot(this));
        }
        private void copyOneNode(Saxon.Hej.expr.XPathContext context, Saxon.Hej.@event.Outputter @out, Saxon.Hej.om.NodeInfo item, int copyOptions) {
            Saxon.Hej.Controller controller = context.getController();
            bool copyBaseURI = (@out.getSystemId() == null);
            int kind = item.getNodeKind();
            if ((requireDocumentOrElement && !((((kind == Saxon.Hej.type.Type.ELEMENT) || (kind == Saxon.Hej.type.Type.DOCUMENT)))))) {
                Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("Operand of validate expression must be a document or element node");
                e.setXPathContext(context);
                e.setErrorCode("XQTY0030");
                throw e;
            }
            Saxon.Hej.Configuration config = controller.getConfiguration();
            switch (kind) {
              case Saxon.Hej.type.Type.ELEMENT:
                {
                    Saxon.Hej.@event.Outputter eval = @out;
                    if (validating) {
                        Saxon.Hej.lib.ParseOptions options = new Saxon.Hej.lib.ParseOptions().withSchemaValidationMode(validation);
                        Saxon.Hej.type.SchemaType type = schemaType;
                        if (((type == null) && (((validation == Saxon.Hej.lib.Validation.STRICT) || (validation == Saxon.Hej.lib.Validation.LAX))))) {
                            string xsitype = item.getAttributeValue(Saxon.Hej.om.NamespaceUri.SCHEMA_INSTANCE, "type");
                            if ((xsitype != null)) {
                                Saxon.Hej.om.StructuredQName typeName;
                                try {
                                    typeName = Saxon.Hej.om.StructuredQName.fromLexicalQName(xsitype, true, false, item.getAllNamespaces());
                                } catch (Saxon.Hej.trans.XPathException e) {
                                    throw new Saxon.Hej.trans.XPathException(((("Invalid QName in xsi:type attribute of element being validated: "+xsitype)+". ")+e.Message), "XTTE1510");
                                }
                                type = config.getSchemaType(typeName);
                                if ((type == null)) {
                                    throw new Saxon.Hej.trans.XPathException(("Unknown xsi:type in element being validated: "+xsitype), "XTTE1510");
                                }
                            }
                        }
                        options = options.withTopLevelType(type).withTopLevelElement(Saxon.Hej.om.NameOfNode.makeName(item).getStructuredQName()).withErrorReporter(context.getErrorReporter());
                        config.prepareValidationReporting(context, options);
                        Saxon.Hej.@event.Receiver validator = config.getElementValidator(@out, options, getLocation());
                        eval = new Saxon.Hej.@event.ComplexContentOutputter(validator);
                    }
                    if (copyBaseURI) {
                        eval.setSystemId(computeNewBaseUri(item, getStaticBaseURIString()));
                    }
                    Saxon.Hej.@event.PipelineConfiguration pipe = @out.getPipelineConfiguration();
                    if (copyLineNumbers) {
                        Saxon.Hej.@event.LocationCopier copier = new Saxon.Hej.@event.LocationCopier(false, @out.getSystemId());
                        pipe.setCopyInformee(copier.notifyElementNode /* instance method reference */);
                    }
                    item.copy(eval, copyOptions, getLocation());
                    if (copyLineNumbers) {
                        pipe.setCopyInformee(null);
                    }
                    break ;
                }
              case Saxon.Hej.type.Type.ATTRIBUTE:
                if (((schemaType != null) && schemaType.isComplexType())) {
                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("When copying an attribute with schema validation, the requested type must not be a complex type");
                    e.setLocation(getLocation());
                    e.setXPathContext(context);
                    e.setErrorCode("XTTE1535");
                    throw dynamicError(getLocation(), e, context);
                }
                try {
                    copyAttribute(item, (Saxon.Hej.type.SimpleType)(schemaType), validation, this, @out, context, rejectDuplicateAttributes);
                } catch (Saxon.Hej.@event.NoOpenStartTagException err) {
                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException(err.Message);
                    e.setLocation(getLocation());
                    e.setXPathContext(context);
                    e.setErrorCodeQName(err.getErrorCodeQName());
                    throw dynamicError(getLocation(), e, context);
                }
                break ;
              case Saxon.Hej.type.Type.TEXT:
                @out.characters(item.getUnicodeStringValue(), getLocation(), Saxon.Hej.@event.ReceiverOption.NONE);
                break ;
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                if (copyBaseURI) {
                    @out.setSystemId(item.getBaseURI());
                }
                @out.processingInstruction(item.getDisplayName(), item.getUnicodeStringValue(), getLocation(), Saxon.Hej.@event.ReceiverOption.NONE);
                break ;
              case Saxon.Hej.type.Type.COMMENT:
                @out.comment(item.getUnicodeStringValue(), getLocation(), Saxon.Hej.@event.ReceiverOption.NONE);
                break ;
              case Saxon.Hej.type.Type.NAMESPACE:
                try {
                    @out.@namespace(item.getLocalPart(), Saxon.Hej.om.NamespaceUri.of(item.getStringValue()), Saxon.Hej.@event.ReceiverOption.NONE);
                } catch (Saxon.Hej.@event.NoOpenStartTagException err) {
                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException(err.Message);
                    e.setXPathContext(context);
                    e.setErrorCodeQName(err.getErrorCodeQName());
                    throw dynamicError(getLocation(), e, context);
                }
                break ;
              case Saxon.Hej.type.Type.DOCUMENT:
                {
                    Saxon.Hej.lib.ParseOptions options = new Saxon.Hej.lib.ParseOptions().withSchemaValidationMode(validation).withSpaceStrippingRule(Saxon.Hej.om.NoElementsSpaceStrippingRule.getInstance()).withTopLevelType(schemaType).withErrorReporter(context.getErrorReporter());
                    config.prepareValidationReporting(context, options);
                    Saxon.Hej.@event.Receiver val = config.getDocumentValidator(@out, item.getBaseURI(), options, getLocation());
                    if (copyBaseURI) {
                        val.setSystemId(item.getBaseURI());
                    }
                    Saxon.Hej.@event.PipelineConfiguration savedPipe = null;
                    if (copyLineNumbers) {
                        savedPipe = new Saxon.Hej.@event.PipelineConfiguration(val.getPipelineConfiguration());
                        Saxon.Hej.@event.LocationCopier copier = new Saxon.Hej.@event.LocationCopier(true, item.getBaseURI());
                        val.getPipelineConfiguration().setCopyInformee(copier.notifyElementNode /* instance method reference */);
                    }
                    item.copy(val, copyOptions, getLocation());
                    if (copyLineNumbers) {
                        val.setPipelineConfiguration(savedPipe);
                    }
                    break ;
                }
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown node kind "+item.getNodeKind()));
            }
        }
        public static string computeNewBaseUri(Saxon.Hej.om.NodeInfo source, string staticBaseURI) {
            string newBaseUri;
            string xmlBase = source.getAttributeValue(Saxon.Hej.om.NamespaceUri.XML, "base");
            if ((xmlBase != null)) {
                try {
                    System.Uri xmlBaseUri = new System.Uri(xmlBase, System.UriKind.RelativeOrAbsolute);
                    if (xmlBaseUri.IsAbsoluteUri) {
                        newBaseUri = xmlBase;
                    } else {
                        if ((staticBaseURI != null)) {
                            System.Uri sbu = new System.Uri(staticBaseURI, System.UriKind.RelativeOrAbsolute);
                            System.Uri abs = new System.Uri(sbu, xmlBaseUri);
                            newBaseUri = abs.ToString();
                        } else {
                            newBaseUri = source.getBaseURI();
                        }
                    }
                } catch (System.UriFormatException err) {
                    newBaseUri = source.getBaseURI();
                }
            } else {
                newBaseUri = source.getBaseURI();
            }
            return newBaseUri;
        }
        public static void copyAttribute(Saxon.Hej.om.NodeInfo source, Saxon.Hej.type.SimpleType schemaType, int validation, Saxon.Hej.expr.instruct.Instruction instruction, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context, bool rejectDuplicates) {
            int opt = (rejectDuplicates ? Saxon.Hej.@event.ReceiverOption.REJECT_DUPLICATES : Saxon.Hej.@event.ReceiverOption.NONE);
            Saxon.Hej.str.UnicodeString value = source.getUnicodeStringValue();
            Saxon.Hej.type.SimpleType annotation = validateAttribute(source, schemaType, validation, context);
            try {
                output.attribute(Saxon.Hej.om.NameOfNode.makeName(source), annotation, value.ToString(), instruction.getLocation(), opt);
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetContext(context);
                e.maybeSetLocation(instruction.getLocation());
                if (((instruction.getPackageData().getHostLanguage() == Saxon.Api.HostLanguage.XQUERY) && e.getErrorCodeLocalPart().Equals("XTTE0950"))) {
                    e.setErrorCode("XQTY0086");
                }
                throw;
            }
        }
        public static Saxon.Hej.type.SimpleType validateAttribute(Saxon.Hej.om.NodeInfo source, Saxon.Hej.type.SimpleType schemaType, int validation, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.str.UnicodeString value = source.getUnicodeStringValue();
            Saxon.Hej.type.SimpleType annotation = Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
            if ((schemaType != null)) {
                if (schemaType.isNamespaceSensitive()) {
                    Saxon.Hej.trans.XPathException nsErr = new Saxon.Hej.trans.XPathException(("Cannot create a parentless attribute whose "+"type is namespace-sensitive (such as xs:QName)"));
                    nsErr.setErrorCode("XTTE1545");
                    throw nsErr;
                }
                Saxon.Hej.type.ValidationFailure valErr = schemaType.validateContent(value, Saxon.Hej.expr.instruct.DummyNamespaceResolver.getInstance(), context.getConfiguration().getConversionRules());
                if ((valErr != null)) {
                    valErr.setMessage(("Attribute being copied does not match the required type. "+valErr.getMessage()));
                    valErr.setErrorCode("XTTE1510");
                    throw valErr.makeException();
                }
                annotation = schemaType;
            } else {
                if (((validation == Saxon.Hej.lib.Validation.STRICT) || (validation == Saxon.Hej.lib.Validation.LAX))) {
                    try {
                        annotation = context.getConfiguration().validateAttribute(Saxon.Hej.om.NameOfNode.makeName(source).getStructuredQName(), value, validation);
                    } catch (Saxon.Hej.type.ValidationException e) {
                        Saxon.Hej.trans.XPathException err = Saxon.Hej.trans.XPathException.makeXPathException(e);
                        err.setErrorCodeQName(e.getErrorCodeQName());
                        err.setIsTypeError(true);
                        throw err;
                    }
                } else {
                    if ((validation == Saxon.Hej.lib.Validation.PRESERVE)) {
                        annotation = (Saxon.Hej.type.SimpleType)(source.getSchemaType());
                        if ((!(annotation.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) && annotation.isNamespaceSensitive())) {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot preserve type annotation when copying an attribute with namespace-sensitive content");
                            err.setErrorCode(((context.getController().getExecutable().getHostLanguage() == Saxon.Api.HostLanguage.XSLT) ? "XTTE0950" : "XQTY0086"));
                            err.setIsTypeError(true);
                            throw err;
                        }
                    }
                }
            }
            return annotation;
        }
        private bool mustPush() {
            return ((((schemaType != null) || (validation == Saxon.Hej.lib.Validation.LAX)) || (validation == Saxon.Hej.lib.Validation.STRICT)) || copyForUpdate);
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.elab.PullEvaluator pull = makeElaborator().elaborateForPull();
            return /*Delegate.iterate()*/pull(context);
        }
        private Saxon.Hej.expr.ItemMappingIterator makeVirtualCopy(Saxon.Hej.om.SequenceIterator input, Saxon.Hej.Controller controller, bool isXSLT) {
            if ((validation == Saxon.Hej.lib.Validation.PRESERVE)) {
                Saxon.Hej.expr.ItemMappingFunction copier = Saxon.Hej.expr.ItemMapper.of(item => {
                    if ((item is Saxon.Hej.om.NodeInfo)) {
                        if (((Saxon.Hej.om.NodeInfo)(item)).getTreeInfo().isTyped()) {
                            if ((!(copyNamespaces) && (((Saxon.Hej.om.NodeInfo)(item)).getNodeKind() == Saxon.Hej.type.Type.ELEMENT))) {
                                Saxon.Hej.@event.Sink sink = new Saxon.Hej.@event.Sink(controller.makePipelineConfiguration());
                                ((Saxon.Hej.om.NodeInfo)(item)).copy(sink, Saxon.Hej.om.CopyOptions.TYPE_ANNOTATIONS, getLocation());
                            }
                            if (((((Saxon.Hej.om.NodeInfo)(item)).getNodeKind() == Saxon.Hej.type.Type.ATTRIBUTE) && ((Saxon.Hej.type.SimpleType)(((Saxon.Hej.om.NodeInfo)(item)).getSchemaType())).isNamespaceSensitive())) {
                                throw new Saxon.Hej.trans.XPathException("Cannot copy an attribute with namespace-sensitive content except as part of its containing element", "XTTE0950");
                            }
                        }
                        Saxon.Hej.tree.wrapper.VirtualCopy vc = Saxon.Hej.tree.wrapper.VirtualCopy.makeVirtualCopy((Saxon.Hej.om.NodeInfo)(item));
                        vc.setDropNamespaces(!(copyNamespaces));
                        ((/*1430*/Saxon.Hej.tree.wrapper.VirtualTreeInfo)vc.getTreeInfo()).setCopyAccumulators(copyAccumulators);
                        if ((isXSLT && copyAccumulators)) {
                            ((/*1430*/Saxon.Hej.tree.wrapper.VirtualTreeInfo)vc.getTreeInfo()).setCopyAccumulators(true);
                            Saxon.Hej.expr.accum.AccumulatorManager am = ((Saxon.Hej.trans.XsltController)(controller)).getAccumulatorManager();
                            am.setApplicableAccumulators(((/*1430*/Saxon.Hej.tree.wrapper.VirtualTreeInfo)vc.getTreeInfo()), am.getApplicableAccumulators(((Saxon.Hej.om.NodeInfo)(item)).getTreeInfo()));
                        }
                        if ((((Saxon.Hej.om.NodeInfo)(item)).getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) {
                            vc.setSystemId(computeNewBaseUri((Saxon.Hej.om.NodeInfo)(item), getStaticBaseURIString()));
                        }
                        return vc;
                    } else {
                        return item;
                    }
                } );
                return new Saxon.Hej.expr.ItemMappingIterator(input, copier, true);
            } else {
                if ((validation == Saxon.Hej.lib.Validation.STRIP)) {
                    Saxon.Hej.expr.ItemMappingFunction copier = Saxon.Hej.expr.ItemMapper.of(item => {
                        if (!(((item is Saxon.Hej.om.NodeInfo)))) {
                            return item;
                        }
                        Saxon.Hej.tree.wrapper.VirtualCopy vc = Saxon.Hej.tree.wrapper.VirtualUntypedCopy.makeVirtualUntypedTree((Saxon.Hej.om.NodeInfo)(item), (Saxon.Hej.om.NodeInfo)(item));
                        if (copyAccumulators) {
                            ((/*1430*/Saxon.Hej.tree.wrapper.VirtualTreeInfo)vc.getTreeInfo()).setCopyAccumulators(true);
                            Saxon.Hej.expr.accum.AccumulatorManager am = ((Saxon.Hej.trans.XsltController)(controller)).getAccumulatorManager();
                            am.setApplicableAccumulators(((/*1430*/Saxon.Hej.tree.wrapper.VirtualTreeInfo)vc.getTreeInfo()), am.getApplicableAccumulators(((Saxon.Hej.om.NodeInfo)(item)).getTreeInfo()));
                        }
                        vc.setDropNamespaces(!(copyNamespaces));
                        if ((((Saxon.Hej.om.NodeInfo)(item)).getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) {
                            vc.setSystemId(computeNewBaseUri((Saxon.Hej.om.NodeInfo)(item), getStaticBaseURIString()));
                        }
                        return vc;
                    } );
                    return new Saxon.Hej.expr.ItemMappingIterator(input, copier, true);
                } else {
                    return null;
                }
            }
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.instruct.CopyOf.CopyOfElaborator();
        }
        private class CopyOfElaborator : Saxon.Hej.expr.elab.PushElaborator {
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.instruct.CopyOf expr = (Saxon.Hej.expr.instruct.CopyOf)(getExpression());
                bool isXSLT = expr.getRetainedStaticContext().getPackageData().isXSLT();
                if ((((expr.schemaType == null) && !(expr.copyForUpdate)) && (((expr.validation == Saxon.Hej.lib.Validation.PRESERVE) || (expr.validation == Saxon.Hej.lib.Validation.STRIP))))) {
                    Saxon.Hej.expr.elab.PullEvaluator select = expr.getSelect().makeElaborator().elaborateForPull();
                    return context => {
                        Saxon.Hej.Controller controller = context.getController();
                        return expr.makeVirtualCopy(/*Delegate.iterate()*/select(context), controller, isXSLT);
                    } ;
                } else {
                    Saxon.Api.HostLanguage host = expr.getPackageData().getHostLanguage();
                    Saxon.Hej.expr.elab.PushEvaluator push = elaborateForPush();
                    return context => {
                        Saxon.Hej.Controller controller = context.getController();
                        System.Diagnostics.Debug.Assert((controller != null));
                        Saxon.Hej.@event.PipelineConfiguration pipe = controller.makePipelineConfiguration();
                        pipe.setXPathContext(context);
                        Saxon.Hej.@event.SequenceCollector @out = new Saxon.Hej.@event.SequenceCollector(pipe, (int)(((expr.numberOfItems/expr.invocations))));
                        if (expr.copyForUpdate) {
                            @out.setTreeModel(Saxon.Hej.om.TreeModel.LINKED_TREE);
                        }
                        pipe.setHostLanguage(host);
                        try {
                            Saxon.Hej.expr.instruct.TailCall tc = /*Delegate.processLeavingTail()*/push(new Saxon.Hej.@event.ComplexContentOutputter(@out), context);
                            Saxon.Hej.expr.Expression.dispatchTailCall(tc);
                        } catch (Saxon.Hej.trans.XPathException err) {
                            err.maybeSetLocation(expr.getLocation());
                            err.maybeSetContext(context);
                            throw;
                        }
                        Saxon.Hej.om.GroundedValue result = @out.getSequence();
                        expr.invocations++;
                        expr.numberOfItems += result.getLength();
                        return result.iterate();
                    } ;
                }
            }
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                if (((Saxon.Hej.expr.instruct.CopyOf)(getExpression())).copyForUpdate) {
                    Saxon.Hej.expr.elab.PushEvaluator pushEval = elaborateForPush();
                    return context => {
                        Saxon.Hej.Controller controller = context.getController();
                        System.Diagnostics.Debug.Assert((controller != null));
                        Saxon.Hej.@event.SequenceCollector seq = controller.allocateSequenceOutputter(1);
                        seq.setTreeModel(Saxon.Hej.om.TreeModel.LINKED_TREE);
                        Saxon.Hej.expr.instruct.TailCall tc = /*Delegate.processLeavingTail()*/pushEval(new Saxon.Hej.@event.ComplexContentOutputter(seq), context);
                        Saxon.Hej.expr.Expression.dispatchTailCall(tc);
                        seq.close();
                        return seq.getFirstItem();
                    } ;
                } else {
                    return base.elaborateForItem();
                }
            }
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.instruct.CopyOf expr = (Saxon.Hej.expr.instruct.CopyOf)(getExpression());
                if (expr.copyAccumulators) {
                    if (expr.mustPush()) {
                        Saxon.Hej.expr.elab.PullEvaluator selectPull = expr.getSelect().makeElaborator().elaborateForPull();
                        return (output, context) => {
                            Saxon.Hej.om.SequenceTool.supply(/*Delegate.iterate()*/selectPull(context), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((item => {
                                if ((item is Saxon.Hej.om.NodeInfo)) {
                                    Saxon.Hej.tree.tiny.TinyBuilder builder = new Saxon.Hej.tree.tiny.TinyBuilder(output.getPipelineConfiguration());
                                    Saxon.Hej.@event.ComplexContentOutputter cco = new Saxon.Hej.@event.ComplexContentOutputter(builder);
                                    cco.open();
                                    expr.copyOneNode(context, cco, (Saxon.Hej.om.NodeInfo)(item), Saxon.Hej.om.CopyOptions.ALL_NAMESPACES);
                                    cco.close();
                                    Saxon.Hej.tree.tiny.TinyNodeImpl copy = (Saxon.Hej.tree.tiny.TinyNodeImpl)(builder.getCurrentRoot());
                                    copy.getTree().setCopiedFrom((Saxon.Hej.om.NodeInfo)(item));
                                    output.append(copy);
                                } else {
                                    output.append(item);
                                }
                            } )));
                            return null;
                        } ;
                    } else {
                        Saxon.Hej.expr.elab.PullEvaluator pull = elaborateForPull();
                        return (output, context) => {
                            Saxon.Hej.om.SequenceTool.supply(/*Delegate.iterate()*/pull(context), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)(/* Unresolved method reference */output.append));
                            return null;
                        } ;
                    }
                } else {
                    int copyOptions = (((((expr.validation == Saxon.Hej.lib.Validation.SKIP) ? 0 : Saxon.Hej.om.CopyOptions.TYPE_ANNOTATIONS))|((expr.copyNamespaces ? Saxon.Hej.om.CopyOptions.ALL_NAMESPACES : 0)))|((expr.copyForUpdate ? Saxon.Hej.om.CopyOptions.FOR_UPDATE : 0)));
                    Saxon.Hej.expr.elab.PullEvaluator selectPull = expr.getSelect().makeElaborator().elaborateForPull();
                    return (output, context) => {
                        Saxon.Hej.om.SequenceTool.supply(/*Delegate.iterate()*/selectPull(context), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((item => {
                            if ((item is Saxon.Hej.om.NodeInfo)) {
                                expr.copyOneNode(context, output, (Saxon.Hej.om.NodeInfo)(item), copyOptions);
                            } else {
                                output.append(item, expr.getLocation(), Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES);
                            }
                        } )));
                        return null;
                    } ;
                }
            }
        
        }
    
    }
}