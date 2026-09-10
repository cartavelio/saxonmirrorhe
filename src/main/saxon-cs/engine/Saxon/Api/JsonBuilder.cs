using System;
using Saxon.Hej;


namespace Saxon.Api {
    
    /// <summary>
    /// A JsonBuilder allows XDM values (typically maps and arrays) to be constructed by parsing JSON input
    /// </summary>
    public class JsonBuilder {

        private readonly Configuration config;
        internal JsonBuilder(Configuration config)  {
            this.config = config;
        }

        /// <summary>
        /// Specify whether parsing is to be strict or liberal. The default is strict, in which case input
        /// must conform strictly to the JSON specification. If the <code>Liberal</code> option is set,
        /// certain departures from JSON syntax are permitted (and ignored), for example an extra comma
        /// is allowed after the entries in an array or object.
        /// </summary>
        public bool Liberal { get; set; }

        /// <summary>
        /// Parse a JSON document supplied using a <c>TextReader</c>
        /// </summary>
        /// <param name="jsonReader">Delivers the JSON input text</param>
        /// <returns>Typically an <c>XdmMap</c> or <c>XdmArray</c> containing the parsed representation of the JSON
        /// input text. If the JSON comprises a single token, the returned value may be an <c>XdmAtomicValue</c>
        /// containing a string, number, or boolean; or an empty sequence representing JSON null.</returns>
        /// <exception cref="SaxonApiException">If the JSON input is invalid</exception>
        public XdmValue ParseJson(System.IO.TextReader jsonReader) {
            try {
                Saxon.Hej.expr.XPathContext context = new Saxon.Hej.Controller(config).newXPathContext();
                Saxon.Hej.z.IntPredicateProxy checker = Saxon.Hej.z.IntSetPredicate.ALWAYS_TRUE;
                Saxon.Hej.str.UnicodeString content = Saxon.Hej.functions.UnparsedTextFunction.readFile(checker, jsonReader);
                System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> options = new System.Collections.Generic.Dictionary<string, Saxon.Hej.om.Sequence>();
                Saxon.Impl.Helpers.MapUtils.Put(options, "liberal", Saxon.Hej.value.BooleanValue.get(Liberal));
                Saxon.Impl.Helpers.MapUtils.Put(options, "escape", Saxon.Hej.value.BooleanValue.TRUE);
                Saxon.Hej.om.Item result = Saxon.Hej.ma.json.ParseJsonFn.parse(content.ToString(), options, context);
                return XdmValue.Wrap(result);
            } catch (Exception e) when (e is Saxon.Hej.trans.XPathException || e is System.IO.IOException) {
                throw new SaxonApiException(e);
            }
        }
        
        /// <summary>
        /// Obsolete synonym of <c>ParseJson</c>.
        /// </summary>
        /// <param name="jsonReader">Delivers the JSON input text</param>
        /// <returns>Typically an <c>XdmMap</c> or <c>XdmArray</c> containing the parsed representation of the JSON
        /// input text. If the JSON comprises a single token, the returned value may be an <c>XdmAtomicValue</c>
        /// containing a string, number, or boolean; or an empty sequence representing JSON null.</returns>
        /// <exception cref="SaxonApiException">If the JSON input is invalid</exception>
        [Obsolete]
        public XdmValue parseJson(System.IO.TextReader jsonReader) {
            return ParseJson(jsonReader);
        }
        
        /// <summary>
        /// Parse a JSON document supplied as a <c>string</c>
        /// </summary>
        /// <param name="json">Contains the JSON input text</param>
        /// <returns>Typically an <c>XdmMap</c> or <c>XdmArray</c> containing the parsed representation of the JSON
        /// input text. If the JSON comprises a single token, the returned value may be an <c>XdmAtomicValue</c>
        /// containing a string, number, or boolean; or an empty sequence representing JSON null.</returns>
        /// <exception cref="SaxonApiException">If the JSON input is invalid</exception>
        public Saxon.Api.XdmValue ParseJson(string json) {
            return ParseJson(new System.IO.StringReader(json));
        }
        
        /// <summary>
        /// Obsolete synonym of <c>ParseJson</c>
        /// </summary>
        /// <param name="json">Contains the JSON input text</param>
        /// <returns>Typically an <c>XdmMap</c> or <c>XdmArray</c> containing the parsed representation of the JSON
        /// input text. If the JSON comprises a single token, the returned value may be an <c>XdmAtomicValue</c>
        /// containing a string, number, or boolean; or an empty sequence representing JSON null.</returns>
        /// <exception cref="SaxonApiException">If the JSON input is invalid</exception>
        public Saxon.Api.XdmValue parseJson(string json) {
            return ParseJson(new System.IO.StringReader(json));
        }

    }
}

// Copyright (c) 2021 Saxonica Limited

