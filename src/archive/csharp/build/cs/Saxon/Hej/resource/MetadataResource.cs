using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.resource {
    public class MetadataResource : Saxon.Hej.lib.Resource {
        private readonly System.Collections.Generic.IDictionary<string,Saxon.Hej.om.GroundedValue> properties;
        private readonly string resourceURI;
        private readonly Saxon.Hej.lib.Resource content;
        public MetadataResource(string resourceURI, Saxon.Hej.lib.Resource content, System.Collections.Generic.IDictionary<string,Saxon.Hej.om.GroundedValue> properties)  {
            this.resourceURI = resourceURI;
            this.content = content;
            this.properties = properties;
        }
        public string getContentType() {
            return content.getContentType();
        }
        public string getResourceURI() {
            return resourceURI;
        }
        public Saxon.Hej.om.Item getItem(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.ma.map.DictionaryMap map = new Saxon.Hej.ma.map.DictionaryMap();
            foreach (System.Collections.Generic.KeyValuePair<string,Saxon.Hej.om.GroundedValue> entry in properties) {
                map.initialPut(entry.Key, entry.Value);
            }
            map.initialPut("name", Saxon.Hej.value.StringValue.makeStringValue(resourceURI));
            Saxon.Hej.expr.Callable fetcher = new Saxon.Hej.expr.CallableDelegate((context1, arguments) => content.getItem(context1));
            Saxon.Hej.type.FunctionItemType fetcherType = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[0], Saxon.Hej.value.SequenceType.SINGLE_ITEM);
            Saxon.Hej.functions.CallableFunction fetcherFunction = new Saxon.Hej.functions.CallableFunction(0, fetcher, fetcherType);
            map.initialPut("fetch", fetcherFunction);
            return map;
        }
    
    }
}