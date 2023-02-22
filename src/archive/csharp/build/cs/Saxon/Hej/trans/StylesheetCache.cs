using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    internal class StylesheetCache {
        private readonly System.Collections.Generic.IDictionary<string,Saxon.Hej.s9api.XsltExecutable> cacheByText = new System.Collections.Concurrent.ConcurrentDictionary<string,Saxon.Hej.s9api.XsltExecutable>();
        private readonly System.Collections.Generic.IDictionary<string,Saxon.Hej.s9api.XsltExecutable> cacheByLocation = new System.Collections.Concurrent.ConcurrentDictionary<string,Saxon.Hej.s9api.XsltExecutable>();
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.om.NodeInfo,Saxon.Hej.s9api.XsltExecutable> cacheByNode = new System.Collections.Concurrent.ConcurrentDictionary<Saxon.Hej.om.NodeInfo,Saxon.Hej.s9api.XsltExecutable>();
        public Saxon.Hej.s9api.XsltExecutable getStylesheetByText(string style) {
            return Saxon.Impl.Helpers.MapUtils.Get(cacheByText, style);
        }
        public Saxon.Hej.s9api.XsltExecutable getStylesheetByLocation(string style) {
            return Saxon.Impl.Helpers.MapUtils.Get(cacheByLocation, style);
        }
        public Saxon.Hej.s9api.XsltExecutable getStylesheetByNode(Saxon.Hej.om.NodeInfo style) {
            return Saxon.Impl.Helpers.MapUtils.Get(cacheByNode, style);
        }
        public void setStylesheetByText(string style, Saxon.Hej.s9api.XsltExecutable xsltExecutable) {
            Saxon.Impl.Helpers.MapUtils.Put(cacheByText, style, xsltExecutable);
        }
        public void setStylesheetByLocation(string style, Saxon.Hej.s9api.XsltExecutable xsltExecutable) {
            Saxon.Impl.Helpers.MapUtils.Put(cacheByLocation, style, xsltExecutable);
        }
        public void setStylesheetByNode(Saxon.Hej.om.NodeInfo style, Saxon.Hej.s9api.XsltExecutable xsltExecutable) {
            Saxon.Impl.Helpers.MapUtils.Put(cacheByNode, style, xsltExecutable);
        }
    
    }
}