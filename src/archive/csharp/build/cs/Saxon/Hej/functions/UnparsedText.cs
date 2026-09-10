using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class UnparsedText : Saxon.Hej.functions.UnparsedTextFunction, Saxon.Hej.functions.PushableFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.StringValue hrefVal = (Saxon.Hej.value.StringValue)(arguments[0].head());
            string encoding = ((getArity() == 2) ? arguments[1].head().getStringValue() : null);
            try {
                return Saxon.Hej.om.SequenceTool.itemOrEmpty(evalUnparsedText(hrefVal, getStaticBaseUriString(), encoding, context));
            } catch (Saxon.Hej.trans.XPathException e) {
                if ((e.getErrorCodeQName() == null)) {
                    e.setErrorCode("FOUT1170");
                }
                if (((getArity() == 2) && e.getErrorCodeLocalPart().Equals("FOUT1200"))) {
                    e.setErrorCode("FOUT1190");
                }
                throw;
            }
        }
        public void process(Saxon.Hej.@event.Outputter destination, Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            bool stable = context.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.STABLE_UNPARSED_TEXT);
            if (stable) {
                Saxon.Hej.om.Sequence result = call(context, arguments);
                Saxon.Hej.value.StringValue value = (Saxon.Hej.value.StringValue)(result.head());
                if ((value != null)) {
                    destination.append(value, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                }
            } else {
                Saxon.Hej.value.StringValue href = (Saxon.Hej.value.StringValue)(arguments[0].head());
                System.Uri absoluteURI = getAbsoluteURI(href.getStringValue(), getStaticBaseUriString(), context);
                string encoding = ((getArity() == 2) ? arguments[1].head().getStringValue() : null);
                Saxon.Hej.str.UniStringConsumer consumer = destination.getStringReceiver(false, Saxon.Hej.expr.parser.Loc.NONE);
                consumer.open();
                try {
                    readFile(absoluteURI, encoding, consumer, context);
                    consumer.close();
                } catch (Saxon.Hej.trans.XPathException e) {
                    if (((getArity() == 2) && e.getErrorCodeLocalPart().Equals("FOUT1200"))) {
                        e.setErrorCode("FOUT1190");
                    }
                    throw;
                }
            }
        }
        private const int errorValue = 0;
        public static Saxon.Hej.value.StringValue evalUnparsedText(Saxon.Hej.value.StringValue hrefVal, string @base, string encoding, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.str.UnicodeString content;
            Saxon.Hej.value.StringValue result;
            bool stable = context.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.STABLE_UNPARSED_TEXT);
            try {
                if ((hrefVal == null)) {
                    return null;
                }
                string href = hrefVal.getStringValue();
                System.Uri absoluteURI = getAbsoluteURI(href, @base, context);
                if (stable) {
                    Saxon.Hej.Controller controller = context.getController();
                    lock (controller) {
                        System.Collections.Generic.IDictionary<System.Uri,Saxon.Hej.str.UnicodeString> cache = (System.Collections.Generic.IDictionary<System.Uri,Saxon.Hej.str.UnicodeString>)(controller.getUserData("unparsed-text-cache", ""));
                        if ((cache != null)) {
                            Saxon.Hej.str.UnicodeString existing = Saxon.Impl.Helpers.MapUtils.Get(cache, absoluteURI);
                            if ((existing != null)) {
                                if (((existing.length()>0) && (existing.codePointAt(0) == errorValue))) {
                                    throw new Saxon.Hej.trans.XPathException(existing.substring(1).ToString(), "FOUT1170");
                                }
                                return new Saxon.Hej.value.StringValue(existing);
                            }
                        }
                        Saxon.Hej.trans.XPathException error = null;
                        try {
                            Saxon.Hej.str.UnicodeBuilder consumer = new Saxon.Hej.str.UnicodeBuilder();
                            readFile(absoluteURI, encoding, consumer, context);
                            content = consumer.toUnicodeString();
                        } catch (Saxon.Hej.trans.XPathException e) {
                            error = e;
                            content = Saxon.Hej.str.StringView.tidy((errorValue+e.Message));
                        }
                        if ((cache == null)) {
                            cache = new System.Collections.Generic.Dictionary<System.Uri, Saxon.Hej.str.UnicodeString>();
                            controller.setUserData("unparsed-text-cache", "", cache);
                            Saxon.Impl.Helpers.MapUtils.Put(cache, absoluteURI, content);
                        }
                        if ((error != null)) {
                            throw error;
                        }
                    }
                } else {
                    Saxon.Hej.str.UnicodeBuilder consumer = new Saxon.Hej.str.UnicodeBuilder();
                    readFile(absoluteURI, encoding, consumer, context);
                    return new Saxon.Hej.value.StringValue(consumer.toUnicodeString());
                }
                result = new Saxon.Hej.value.StringValue(content);
            } catch (Saxon.Hej.trans.XPathException err) {
                err.maybeSetErrorCode("FOUT1170");
                throw;
            }
            return result;
        }
    
    }
}