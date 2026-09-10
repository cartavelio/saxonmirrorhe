using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    public class JsonBuilder {
        private Saxon.Hej.Configuration config;
        private bool liberal;
        protected internal JsonBuilder(Saxon.Hej.Configuration config)  {
            this.config = config;
        }
        public void setLiberal(bool liberal) {
            this.liberal = liberal;
        }
        public bool isLiberal() {
            return liberal;
        }
        public Saxon.Api.XdmValue parseJson(System.IO.TextReader jsonReader) {
            try {
                Saxon.Hej.expr.XPathContext context = new Saxon.Hej.Controller(config).newXPathContext();
                Saxon.Hej.z.IntPredicateProxy checker = Saxon.Hej.z.IntSetPredicate.ALWAYS_TRUE;
                Saxon.Hej.str.UnicodeString content = Saxon.Hej.functions.UnparsedTextFunction.readFile(checker, jsonReader);
                System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> options = new System.Collections.Generic.Dictionary<string, Saxon.Hej.om.Sequence>();
                Saxon.Impl.Helpers.MapUtils.Put(options, "liberal", Saxon.Hej.value.BooleanValue.get(liberal));
                Saxon.Impl.Helpers.MapUtils.Put(options, "escape", Saxon.Hej.value.BooleanValue.TRUE);
                Saxon.Hej.om.Item result = Saxon.Hej.ma.json.ParseJsonFn.parse(content.ToString(), options, context);
                return Saxon.Api.XdmValue.Wrap(result);
            } catch (Exception e) when (e is Saxon.Hej.trans.XPathException || e is System.IO.IOException) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Api.XdmValue parseJson(string json) {
            return parseJson(new System.IO.StringReader(json));
        }
    
    }
}