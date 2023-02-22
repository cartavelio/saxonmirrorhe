using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class DocumentInstr : Saxon.Hej.expr.instruct.ParentNodeConstructor {
        private readonly bool textOnly;
        private readonly Saxon.Hej.str.UnicodeString constantText;
        public DocumentInstr(bool textOnly, Saxon.Hej.str.UnicodeString constantText)  {
            this.textOnly = textOnly;
            this.constantText = constantText;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return contentOp;
        }
        public override int getImplementationMethod() {
            return Saxon.Hej.expr.Expression.EVALUATE_METHOD;
        }
        public bool isTextOnly() {
            return textOnly;
        }
        public Saxon.Hej.str.UnicodeString getConstantText() {
            return constantText;
        }
        protected internal override void checkContentSequence(Saxon.Hej.expr.StaticContext env) {
            checkContentSequence(env, getContentOperand(), getValidationOptions());
        }
        protected internal static void checkContentSequence(Saxon.Hej.expr.StaticContext env, Saxon.Hej.expr.Operand content, Saxon.Hej.lib.ParseOptions validationOptions) {
            Saxon.Hej.expr.Operand[] components;
            if ((content.getChildExpression() is Saxon.Hej.expr.instruct.Block)) {
                components = ((Saxon.Hej.expr.instruct.Block)(content.getChildExpression())).getOperanda();
            } else {
                components = new Saxon.Hej.expr.Operand[]{content};
            }
            int validation = ((validationOptions == null) ? Saxon.Hej.lib.Validation.PRESERVE : validationOptions.getSchemaValidationMode());
            Saxon.Hej.type.SchemaType type = ((validationOptions == null) ? null : validationOptions.getTopLevelType());
            int elementCount = 0;
            bool isXSLT = content.getChildExpression().getPackageData().isXSLT();
            foreach (Saxon.Hej.expr.Operand o in components) {
                Saxon.Hej.expr.Expression component = o.getChildExpression();
                Saxon.Hej.type.ItemType it = component.getItemType();
                if ((it is Saxon.Hej.pattern.NodeTest)) {
                    Saxon.Hej.type.UType possibleNodeKinds = it.getUType();
                    if (possibleNodeKinds.Equals(Saxon.Hej.type.UType.ATTRIBUTE)) {
                        Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException("Cannot create an attribute node whose parent is a document node");
                        de.setErrorCode((isXSLT ? "XTDE0420" : "XPTY0004"));
                        de.setLocator(component.getLocation());
                        throw de;
                    } else {
                        if (possibleNodeKinds.Equals(Saxon.Hej.type.UType.NAMESPACE)) {
                            Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException("Cannot create a namespace node whose parent is a document node");
                            de.setErrorCode((isXSLT ? "XTDE0420" : "XQTY0024"));
                            de.setLocator(component.getLocation());
                            throw de;
                        } else {
                            if (possibleNodeKinds.Equals(Saxon.Hej.type.UType.ELEMENT)) {
                                elementCount++;
                                if (((elementCount>1) && ((((validation == Saxon.Hej.lib.Validation.STRICT) || (validation == Saxon.Hej.lib.Validation.LAX)) || (type != null))))) {
                                    Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException("A valid document must have only one child element");
                                    if (isXSLT) {
                                        de.setErrorCode("XTTE1550");
                                    } else {
                                        de.setErrorCode("XQDY0061");
                                    }
                                    de.setLocator(component.getLocation());
                                    throw de;
                                }
                                if (((validation == Saxon.Hej.lib.Validation.STRICT) && (component is Saxon.Hej.expr.instruct.FixedElement))) {
                                    Saxon.Hej.type.SchemaDeclaration decl = env.getConfiguration().getElementDeclaration(((Saxon.Hej.expr.instruct.FixedElement)(component)).getFixedElementName().getFingerprint());
                                    if ((decl != null)) {
                                        ((Saxon.Hej.expr.instruct.FixedElement)(component)).getContentExpression().checkPermittedContents(decl.getType(), true);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        protected internal override int computeSpecialProperties() {
            int p = base.computeSpecialProperties();
            p |= Saxon.Hej.expr.StaticProperty.SINGLE_DOCUMENT_NODESET;
            if ((getValidationAction() == Saxon.Hej.lib.Validation.SKIP)) {
                p |= Saxon.Hej.expr.StaticProperty.ALL_NODES_UNTYPED;
            }
            return p;
        }
        public Saxon.Hej.expr.Expression getStringValueExpression() {
            if (textOnly) {
                if ((constantText != null)) {
                    return new Saxon.Hej.expr.StringLiteral(new Saxon.Hej.value.UntypedAtomicValue(constantText));
                } else {
                    if ((getContentExpression() is Saxon.Hej.expr.instruct.ValueOf)) {
                        return ((Saxon.Hej.expr.instruct.ValueOf)(getContentExpression())).convertToCastAsString();
                    } else {
                        Saxon.Hej.expr.Expression fn = Saxon.Hej.functions.SystemFunction.makeCall("string-join", getRetainedStaticContext(), getContentExpression(), new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.EMPTY_STRING));
                        Saxon.Hej.expr.CastExpression cast = new Saxon.Hej.expr.CastExpression(fn, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, false);
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, cast);
                        return cast;
                    }
                }
            } else {
                throw new Saxon.Ejava.lang.AssertionError("getStringValueExpression() called on non-text-only document instruction");
            }
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.DocumentInstr doc = new Saxon.Hej.expr.instruct.DocumentInstr(textOnly, constantText);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, doc);
            doc.setContentExpression(getContentExpression().copy(rebindings));
            doc.setValidationAction(getValidationAction(), getSchemaType());
            return doc;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.pattern.NodeKindTest.DOCUMENT;
        }
        public override Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            if ((preservingTypes && !(textOnly))) {
                output.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
                getContentExpression().process(output, context);
                output.endDocument();
                return null;
            } else {
                Saxon.Hej.om.Item item = evaluateItem(context);
                if ((item != null)) {
                    output.append(item, getLocation(), Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES);
                }
                return null;
            }
        }
        public override Saxon.Hej.om.NodeInfo evaluateItem(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Controller controller = context.getController();
            Saxon.Hej.Configuration config = controller.getConfiguration();
            Saxon.Hej.om.NodeInfo root;
            if (textOnly) {
                Saxon.Hej.str.UnicodeString textValue;
                if ((constantText != null)) {
                    textValue = constantText;
                } else {
                    Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder();
                    Saxon.Hej.om.SequenceIterator iter = getContentExpression().iterate(context);
                    Saxon.Hej.om.Item item;
                    while (((item = iter.next()) != null)) {
                        sb.accept(item.getUnicodeStringValue());
                    }
                    textValue = sb.toUnicodeString();
                }
                root = Saxon.Hej.value.TextFragmentValue.makeTextFragment(config, textValue, getStaticBaseURIString());
            } else {
                try {
                    Saxon.Hej.@event.PipelineConfiguration pipe = controller.makePipelineConfiguration();
                    pipe.setXPathContext(context);
                    Saxon.Hej.@event.Builder builder;
                    builder = controller.makeBuilder();
                    builder.setUseEventLocation(false);
                    if ((builder is Saxon.Hej.tree.tiny.TinyBuilder)) {
                        ((Saxon.Hej.tree.tiny.TinyBuilder)(builder)).setStatistics(config.getTreeStatistics().SOURCE_DOCUMENT_STATISTICS);
                    }
                    builder.setBaseURI(getStaticBaseURIString());
                    builder.setTiming(false);
                    pipe.setHostLanguage(getPackageData().getHostLanguage());
                    builder.setPipelineConfiguration(pipe);
                    Saxon.Hej.@event.ComplexContentOutputter @out = Saxon.Hej.@event.ComplexContentOutputter.makeComplexContentReceiver(builder, getValidationOptions());
                    @out.open();
                    @out.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
                    getContentExpression().process(@out, context);
                    @out.endDocument();
                    @out.close();
                    root = builder.getCurrentRoot();
                } catch (Saxon.Hej.trans.XPathException e) {
                    e.maybeSetLocation(getLocation());
                    e.maybeSetContext(context);
                    throw;
                }
            }
            return root;
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.XSL_DOCUMENT;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("doc", this);
            if (!(@out.isRelocatable())) {
                @out.emitAttribute("base", getStaticBaseURIString());
            }
            string flags = "";
            if (textOnly) {
                flags += "t";
            }
            if (isLocal()) {
                flags += "l";
            }
            if (!(flags.Length==0)) {
                @out.emitAttribute("flags", flags);
            }
            if ((constantText != null)) {
                @out.emitAttribute("text", constantText.ToString());
            }
            if (((getValidationAction() != Saxon.Hej.lib.Validation.SKIP) && (getValidationAction() != Saxon.Hej.lib.Validation.BY_TYPE))) {
                @out.emitAttribute("validation", Saxon.Hej.lib.Validation.describe(getValidationAction()));
            }
            Saxon.Hej.type.SchemaType schemaType = getSchemaType();
            if ((schemaType != null)) {
                @out.emitAttribute("type", schemaType.getStructuredQName());
            }
            getContentExpression().export(@out);
            @out.endElement();
        }
        public override string getStreamerName() {
            return "DocumentInstr";
        }
    
    }
}