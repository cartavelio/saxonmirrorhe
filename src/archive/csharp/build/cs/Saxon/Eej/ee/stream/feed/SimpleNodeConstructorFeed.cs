using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class SimpleNodeConstructorFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
        private Saxon.Hej.str.UnicodeBuilder buffer = new Saxon.Hej.str.UnicodeBuilder();
        private readonly bool disableOutputEscaping;
        private bool isSimpleText;
        public SimpleNodeConstructorFeed(Saxon.Hej.expr.Expression expr, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (expr, result, context) {
            disableOutputEscaping = ((expr is Saxon.Hej.expr.instruct.ValueOf) && ((Saxon.Hej.expr.instruct.ValueOf)(expr)).isDisableOutputEscaping());
            Saxon.Hej.expr.instruct.SimpleNodeConstructor constructor = (Saxon.Hej.expr.instruct.SimpleNodeConstructor)(getExpression());
            int kind = constructor.getItemType().getPrimitiveType();
            if (((kind == Saxon.Hej.type.Type.TEXT) && !(disableOutputEscaping))) {
                isSimpleText = true;
            }
        }
        public override void append(Saxon.Hej.om.Item item) {
            if (!(hasFailed())) {
                if (isSimpleText) {
                    try {
                        getNextOutputter().characters(item.getUnicodeStringValue(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                    } catch (Saxon.Hej.trans.XPathException e) {
                        dynamicError(e);
                    }
                } else {
                    buffer.accept(item.getUnicodeStringValue());
                }
            }
        }
        public override Saxon.Hej.str.UniStringConsumer getStringReceiver(bool asTextNode) {
            if (isSimpleText) {
                return new Inner_AbstractUniStringConsumer_d359e299(this);
            } else {
                return base.getStringReceiver(asTextNode);
            }
        }
        public override void close() {
            if (!(hasFailed())) {
                Saxon.Hej.expr.instruct.SimpleNodeConstructor constructor = (Saxon.Hej.expr.instruct.SimpleNodeConstructor)(getExpression());
                int kind = constructor.getItemType().getPrimitiveType();
                switch (kind) {
                  case Saxon.Hej.type.Type.ATTRIBUTE:
                    try {
                        getNextOutputter().attribute(constructor.evaluateNodeName(getContext()), Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, buffer.ToString(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                    } catch (Saxon.Hej.trans.XPathException e) {
                        dynamicError(e);
                    }
                    break ;
                  case Saxon.Hej.type.Type.TEXT:
                    if (!(isSimpleText)) {
                        try {
                            int options = (disableOutputEscaping ? Saxon.Hej.@event.ReceiverOption.DISABLE_ESCAPING : Saxon.Hej.@event.ReceiverOption.NONE);
                            getNextOutputter().characters(buffer.toUnicodeString(), Saxon.Hej.expr.parser.Loc.NONE, options);
                        } catch (Saxon.Hej.trans.XPathException e) {
                            dynamicError(e);
                        }
                    }
                    break ;
                  case Saxon.Hej.type.Type.COMMENT:
                    try {
                        getNextOutputter().comment(buffer.toUnicodeString(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                    } catch (Saxon.Hej.trans.XPathException e) {
                        dynamicError(e);
                    }
                    break ;
                  case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                    try {
                        getNextOutputter().processingInstruction(constructor.evaluateNodeName(getContext()).getLocalPart(), buffer.toUnicodeString(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                    } catch (Saxon.Hej.trans.XPathException e) {
                        dynamicError(e);
                    }
                    break ;
                  default:
                    throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown node kind "+kind));
                }
                buffer.clear();
                base.close();
            }
        }
    
        private class Inner_AbstractUniStringConsumer_d359e299 : Saxon.Hej.str.AbstractUniStringConsumer {
            private readonly SimpleNodeConstructorFeed outerInstance;
            public Inner_AbstractUniStringConsumer_d359e299(SimpleNodeConstructorFeed outerInstance) {
                this.outerInstance = outerInstance;
            }
            public override  /* REPLACING public */ Saxon.Hej.str.UniStringConsumer accept(Saxon.Hej.str.UnicodeString chars) {
                try {
                    /*guess*/outerInstance.getNextOutputter().characters(/*localparam*/chars, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                } catch (Saxon.Hej.trans.XPathException e) {
                    /*guess*/outerInstance.dynamicError(/*exception*/e);
                }
                return this;
            }
        }

    }
}