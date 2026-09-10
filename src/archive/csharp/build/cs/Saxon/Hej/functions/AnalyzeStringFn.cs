using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class AnalyzeStringFn : Saxon.Hej.functions.RegexFunction {
        private class ResultNamesAndTypes {
            public Saxon.Hej.om.NodeName resultName;
            public Saxon.Hej.om.NodeName nonMatchName;
            public Saxon.Hej.om.NodeName matchName;
            public Saxon.Hej.om.NodeName groupName;
            public Saxon.Hej.om.NodeName groupNrName;
            public Saxon.Hej.type.SchemaType resultType = Saxon.Hej.type.Untyped.getInstance();
            public Saxon.Hej.type.SchemaType nonMatchType = Saxon.Hej.type.Untyped.getInstance();
            public Saxon.Hej.type.SchemaType matchType = Saxon.Hej.type.Untyped.getInstance();
            public Saxon.Hej.type.SchemaType groupType = Saxon.Hej.type.Untyped.getInstance();
            public Saxon.Hej.type.SimpleType groupNrType = Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
        
        }
        private Saxon.Hej.functions.AnalyzeStringFn.ResultNamesAndTypes vocab = new Saxon.Hej.functions.AnalyzeStringFn.ResultNamesAndTypes();
        protected internal override bool allowRegexMatchingEmptyString() {
            return false;
        }
        private void init(Saxon.Hej.Configuration config, bool schemaAware) { lock(this) {
            vocab.resultName = new Saxon.Hej.om.FingerprintedQName("", Saxon.Hej.lib.NamespaceConstant.FN, "analyze-string-result");
            vocab.nonMatchName = new Saxon.Hej.om.FingerprintedQName("", Saxon.Hej.lib.NamespaceConstant.FN, "non-match");
            vocab.matchName = new Saxon.Hej.om.FingerprintedQName("", Saxon.Hej.lib.NamespaceConstant.FN, "match");
            vocab.groupName = new Saxon.Hej.om.FingerprintedQName("", Saxon.Hej.lib.NamespaceConstant.FN, "group");
            vocab.groupNrName = new Saxon.Hej.om.NoNamespaceName("nr");
            if (schemaAware) {
                vocab.resultType = config.getSchemaType(new Saxon.Hej.om.StructuredQName("", Saxon.Hej.lib.NamespaceConstant.FN, "analyze-string-result-type"));
                vocab.nonMatchType = Saxon.Hej.type.BuiltInAtomicType.STRING;
                vocab.matchType = config.getSchemaType(new Saxon.Hej.om.StructuredQName("", Saxon.Hej.lib.NamespaceConstant.FN, "match-type"));
                vocab.groupType = config.getSchemaType(new Saxon.Hej.om.StructuredQName("", Saxon.Hej.lib.NamespaceConstant.FN, "group-type"));
                vocab.groupNrType = Saxon.Hej.type.BuiltInAtomicType.POSITIVE_INTEGER;
                if ((((vocab.resultType == null) || (vocab.matchType == null)) || (vocab.groupType == null))) {
                    throw new Saxon.Hej.trans.XPathException("Schema for analyze-string has not been successfully loaded");
                }
            }
        }}
        public override Saxon.Hej.om.NodeInfo call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.Item inputItem = arguments[0].head();
            Saxon.Hej.str.UnicodeString input;
            if ((inputItem == null)) {
                input = Saxon.Hej.str.EmptyUnicodeString.getInstance();
            } else {
                input = inputItem.getUnicodeStringValue();
            }
            Saxon.Hej.regex.RegularExpression re = getRegularExpression(arguments);
            Saxon.Hej.regex.RegexIterator iter = re.analyze(input);
            if ((vocab.resultName == null)) {
                bool schemaAware = context.getController().getExecutable().isSchemaAware();
                Saxon.Hej.Configuration config = context.getConfiguration();
                lock (config) {
                    if ((schemaAware && !(config.isSchemaAvailable(Saxon.Hej.lib.NamespaceConstant.FN)))) {
                        config.addSchemaSource(/*134*/Saxon.Eej.ee.config.StandardSchemaResolver.fetchResource("xpath-functions.scm", config));
                    }
                }
                init(context.getConfiguration(), schemaAware);
            }
            Saxon.Hej.@event.Builder builder = context.getController().makeBuilder();
            Saxon.Hej.@event.ComplexContentOutputter @out = new Saxon.Hej.@event.ComplexContentOutputter(builder);
            Saxon.Hej.functions.AnalyzeStringFn.LocalRegexMatchHandler handler = new Saxon.Hej.functions.AnalyzeStringFn.LocalRegexMatchHandler(@out, vocab);
            builder.setBaseURI(getStaticBaseUriString());
            @out.open();
            @out.startElement(vocab.resultName, vocab.resultType, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
            @out.startContent();
            Saxon.Hej.om.Item item;
            while (((item = ((/*1430*/Saxon.Hej.value.StringValue)iter.next())) != null)) {
                if (iter.isMatching()) {
                    @out.startElement(vocab.matchName, vocab.matchType, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                    @out.startContent();
                    iter.processMatchingSubstring(handler);
                    @out.endElement();
                } else {
                    @out.startElement(vocab.nonMatchName, vocab.nonMatchType, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                    @out.startContent();
                    @out.characters(item.getUnicodeStringValue(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                    @out.endElement();
                }
            }
            @out.endElement();
            @out.close();
            return builder.getCurrentRoot();
        }
        private class LocalRegexMatchHandler : Saxon.Hej.regex.RegexMatchHandler {
            private readonly Saxon.Hej.@event.ComplexContentOutputter @out;
            private readonly Saxon.Hej.functions.AnalyzeStringFn.ResultNamesAndTypes vocab;
            public LocalRegexMatchHandler(Saxon.Hej.@event.ComplexContentOutputter @out, Saxon.Hej.functions.AnalyzeStringFn.ResultNamesAndTypes vocab)  {
                this.@out = @out;
                this.vocab = vocab;
            }
            public void characters(Saxon.Hej.str.UnicodeString s) {
                @out.characters(s, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
            }
            public void onGroupStart(int groupNumber) {
                @out.startElement(vocab.groupName, vocab.groupType, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                @out.attribute(vocab.groupNrName, vocab.groupNrType, (""+groupNumber), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                @out.startContent();
            }
            public void onGroupEnd(int groupNumber) {
                @out.endElement();
            }
        
        }
    
    }
}