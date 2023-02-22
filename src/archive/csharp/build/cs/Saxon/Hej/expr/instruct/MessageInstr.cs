using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class MessageInstr : Saxon.Hej.expr.instruct.Instruction {
        private readonly Saxon.Hej.expr.Operand selectOp;
        private readonly Saxon.Hej.expr.Operand terminateOp;
        private readonly Saxon.Hej.expr.Operand errorCodeOp;
        private bool isAssert;
        public MessageInstr(Saxon.Hej.expr.Expression select, Saxon.Hej.expr.Expression terminate, Saxon.Hej.expr.Expression errorCode)  {
            if ((errorCode == null)) {
                errorCode = new Saxon.Hej.expr.StringLiteral(Saxon.Hej.str.BMPString.of((("Q{"+Saxon.Hej.lib.NamespaceConstant.ERR)+"}XTMM9000")));
            }
            selectOp = new Saxon.Hej.expr.Operand(this, select, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            terminateOp = new Saxon.Hej.expr.Operand(this, terminate, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            errorCodeOp = new Saxon.Hej.expr.Operand(this, errorCode, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
        }
        public Saxon.Hej.expr.Expression getSelect() {
            return selectOp.getChildExpression();
        }
        public void setSelect(Saxon.Hej.expr.Expression select) {
            selectOp.setChildExpression(select);
        }
        public Saxon.Hej.expr.Expression getTerminate() {
            return terminateOp.getChildExpression();
        }
        public void setTerminate(Saxon.Hej.expr.Expression terminate) {
            terminateOp.setChildExpression(terminate);
        }
        public Saxon.Hej.expr.Expression getErrorCode() {
            return errorCodeOp.getChildExpression();
        }
        public void setErrorCode(Saxon.Hej.expr.Expression errorCode) {
            errorCodeOp.setChildExpression(errorCode);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandList(selectOp, terminateOp, errorCodeOp);
        }
        public void setIsAssert(bool isAssert) {
            this.isAssert = isAssert;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.MessageInstr exp = new Saxon.Hej.expr.instruct.MessageInstr(getSelect().copy(rebindings), getTerminate().copy(rebindings), getErrorCode().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override int getInstructionNameCode() {
            return (isAssert ? Saxon.Hej.om.StandardNames.XSL_ASSERT : Saxon.Hej.om.StandardNames.XSL_MESSAGE);
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.AnyItemType.getInstance();
        }
        public override int getCardinality() {
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
        }
        public override bool mayCreateNewNodes() {
            return true;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            return base.optimize(visitor, contextInfo);
        }
        private Saxon.Api.Message makeMessage(bool abort, Saxon.Hej.om.StructuredQName errorCode, Saxon.Hej.om.NodeInfo content) {
            return new Saxon.Api.Message((Saxon.Api.XdmNode) Saxon.Api.XdmValue.Wrap(content), new Saxon.Api.QName(errorCode), abort, new Saxon.Api.Location(getLocation()));
        }
        private static Saxon.Ejavax.xml.transform.Result standardErrorResult() {
            return new Saxon.Ejavax.xml.transform.stream.StreamResult(System.Console.Error);
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("message", this);
            @out.setChildRole("select");
            getSelect().export(@out);
            @out.setChildRole("terminate");
            getTerminate().export(@out);
            @out.setChildRole("error");
            getErrorCode().export(@out);
            @out.endElement();
        }
        private class MessageAdapter : Saxon.Hej.@event.ProxyReceiver {
            public MessageAdapter(Saxon.Hej.@event.SequenceReceiver next) : base (next) {
            }
            public override void startDocument(int properties) {
                base.startDocument(properties);
            }
            public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
                base.startElement(elemName, type, attributes, namespaces, location, properties);
            }
            public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
                if ((item is Saxon.Hej.om.NodeInfo)) {
                    int kind = ((Saxon.Hej.om.NodeInfo)(item)).getNodeKind();
                    if ((kind == Saxon.Hej.type.Type.ATTRIBUTE)) {
                        string attName = ((Saxon.Hej.om.NodeInfo)(item)).getDisplayName();
                        processingInstruction("attribute", Saxon.Hej.str.StringView.of((((("name=\""+attName)+"\" value=\"")+item.getUnicodeStringValue())+"\"")), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                        return ;
                    } else {
                        if ((kind == Saxon.Hej.type.Type.NAMESPACE)) {
                            string prefix = ((Saxon.Hej.om.NodeInfo)(item)).getLocalPart();
                            processingInstruction("namespace", Saxon.Hej.str.StringView.of((((("prefix=\""+prefix)+"\" uri=\"")+item.getUnicodeStringValue())+"\"")), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                            return ;
                        }
                    }
                } else {
                    if (((item is Saxon.Hej.om.FunctionItem) && !(((Saxon.Hej.om.FunctionItem)(item)).isArray()))) {
                        string representation = (((Saxon.Hej.om.FunctionItem)(item)).isMap() ? Saxon.Hej.trans.Err.depict(item) : ("Function "+Saxon.Hej.trans.Err.depict(item)));
                        nextReceiver.characters(Saxon.Hej.str.StringView.of(representation), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                        return ;
                    }
                }
                nextReceiver.append(item, locationId, copyNamespaces);
            }
        
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.instruct.MessageInstr.MessageInstrElaborator();
        }
        private class MessageInstrElaborator : Saxon.Hej.expr.elab.PushElaborator {
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.instruct.MessageInstr expr = (Saxon.Hej.expr.instruct.MessageInstr)(getExpression());
                Saxon.Hej.expr.elab.PushEvaluator select = expr.getSelect().makeElaborator().elaborateForPush();
                Saxon.Hej.expr.elab.StringEvaluator terminate = expr.getTerminate().makeElaborator().elaborateForString(true);
                Saxon.Hej.expr.elab.StringEvaluator errorCodeEval = expr.getErrorCode().makeElaborator().elaborateForString(true);
                return (@out, context) => {
                    if (!(((context.getController() is Saxon.Hej.trans.XsltController)))) {
                        Saxon.Hej.om.SequenceIterator iter = expr.getSelect().iterate(context);
                        Saxon.Hej.query.QueryResult.serializeSequence(iter, context.getConfiguration(), standardErrorResult(), new Saxon.Ejava.util.Properties());
                        return null;
                    }
                    Saxon.Hej.trans.XsltController controller = (Saxon.Hej.trans.XsltController)(context.getController());
                    if ((expr.isAssert && !(controller.isAssertionsEnabled()))) {
                        return null;
                    }
                    bool abort = false;
                    string term = Saxon.Hej.value.Whitespace.trim(/*Delegate.eval()*/terminate(context));
                    switch (term) {
                      case "no":
                      case "false":
                      case "0":
                        break ;
                      case "yes":
                      case "true":
                      case "1":
                        abort = true;
                        break ;
                      default:
                        Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("The terminate attribute of xsl:message must be yes|true|1 or no|false|0");
                        e.setXPathContext(context);
                        e.setErrorCode("XTDE0030");
                        throw e;
                    }
                    string code;
                    try {
                        code = /*Delegate.eval()*/errorCodeEval(context);
                    } catch (Saxon.Hej.trans.XPathException err) {
                        code = err.getErrorCodeQName().getEQName();
                    }
                    Saxon.Hej.om.StructuredQName errorCode;
                    try {
                        errorCode = Saxon.Hej.om.StructuredQName.fromLexicalQName(code, false, true, expr.getRetainedStaticContext());
                    } catch (Saxon.Hej.trans.XPathException err) {
                        errorCode = new Saxon.Hej.om.StructuredQName("err", Saxon.Hej.om.NamespaceUri.ERR, "XTMM9000");
                    }
                    controller.incrementMessageCounter(errorCode);
                    Saxon.Hej.@event.Builder builder = controller.makeBuilder();
                    builder.setDurability(Saxon.Hej.om.Durability.TEMPORARY);
                    builder.setTiming(false);
                    Saxon.Hej.@event.Receiver rec = new Saxon.Hej.expr.instruct.MessageInstr.MessageAdapter(new Saxon.Hej.@event.TreeReceiver(builder));
                    rec.open();
                    Saxon.Hej.@event.ComplexContentOutputter cco = new Saxon.Hej.@event.ComplexContentOutputter(rec);
                    cco.startDocument((abort ? Saxon.Hej.@event.ReceiverOption.TERMINATE : Saxon.Hej.@event.ReceiverOption.NONE));
                    try {
                        Saxon.Hej.expr.instruct.TailCall tc = /*Delegate.processLeavingTail()*/select(cco, context);
                        dispatchTailCall(tc);
                    } catch (Saxon.Hej.trans.XPathException e) {
                        cco.append(Saxon.Hej.value.StringValue.bmp(((((((("Error "+e.getErrorCodeLocalPart())+" while evaluating xsl:message at line ")+expr.getLocation().getLineNumber())+" of ")+expr.getLocation().getSystemId())+": ")+e.Message)));
                    }
                    cco.endDocument();
                    cco.close();
                    builder.close();
                    Saxon.Hej.om.NodeInfo content = builder.getCurrentRoot();
                    Saxon.Api.Message message = expr.makeMessage(abort, errorCode, content);
                    controller.getMessageHandler()(message);
                    if (abort) {
                        Saxon.Hej.expr.instruct.TerminationException te = new Saxon.Hej.expr.instruct.TerminationException(((((("Processing terminated by "+Saxon.Hej.lib.StandardDiagnostics.getInstructionNameDefault(expr))+" at line ")+expr.getLocation().getLineNumber())+" in ")+Saxon.Hej.lib.StandardDiagnostics.abbreviateLocationURIDefault(expr.getLocation().getSystemId())));
                        te.setLocation(expr.getLocation());
                        te.setErrorCodeQName(errorCode);
                        te.setErrorObject(content);
                        throw te;
                    }
                    return null;
                } ;
            }
        
        }
    
    }
}