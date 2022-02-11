using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.resource {
    public class JSONResource : Saxon.Hej.lib.Resource {
        private readonly string href;
        private string jsonStr;
        private Saxon.Hej.om.Item item;
        private readonly Saxon.Hej.expr.XPathContext context;
        private Saxon.Hej.resource.AbstractResourceCollection.InputDetails details;
        public readonly static Saxon.Hej.lib.ResourceFactory FACTORY = (context, details) => new Saxon.Hej.resource.JSONResource(context, details);
        public JSONResource(Saxon.Hej.expr.XPathContext context, Saxon.Hej.resource.AbstractResourceCollection.InputDetails details)  {
            this.href = details.resourceUri;
            this.context = context;
            if ((details.encoding == null)) {
                details.encoding = "UTF-8";
            }
            this.details = details;
        }
        public string getResourceURI() {
            return href;
        }
        public Saxon.Hej.om.Item getItem() {
            if ((item != null)) {
                return item;
            }
            if ((jsonStr == null)) {
                jsonStr = details.obtainCharacterContent();
                if ((jsonStr == null)) {
                    return null;
                }
            }
            System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> options = new System.Collections.Generic.Dictionary<string, Saxon.Hej.om.Sequence>();
            Saxon.Impl.Helpers.MapUtils.Put(options, "liberal", Saxon.Hej.value.BooleanValue.FALSE);
            Saxon.Impl.Helpers.MapUtils.Put(options, "duplicates", Saxon.Hej.value.StringValue.bmp("use-first"));
            Saxon.Impl.Helpers.MapUtils.Put(options, "escape", Saxon.Hej.value.BooleanValue.FALSE);
            return item = Saxon.Hej.ma.json.ParseJsonFn.parse(jsonStr, options, context);
        }
        public string getContentType() {
            return "application/json";
        }
    
    }
}