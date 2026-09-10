using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.resource {
    public class DirectoryCollection : Saxon.Hej.resource.AbstractResourceCollection {
        private readonly Saxon.Ejava.io.File dirFile;
        private Saxon.Hej.om.SpaceStrippingRule whitespaceRules;
        public DirectoryCollection(Saxon.Hej.Configuration config, string collectionURI, Saxon.Ejava.io.File file, Saxon.Hej.functions.URIQueryParameters @params) : base (config) {
            if ((collectionURI == null)) {
                throw new Saxon.Ejava.lang.NullPointerException();
            }
            this.collectionURI = collectionURI;
            dirFile = file;
            if ((@params == null)) {
                this.@params = new Saxon.Hej.functions.URIQueryParameters("", config);
            } else {
                this.@params = @params;
            }
        }
        public override bool stripWhitespace(Saxon.Hej.om.SpaceStrippingRule rules) {
            this.whitespaceRules = rules;
            return true;
        }
        public override System.Collections.Generic.IEnumerator<string> getResourceURIs(Saxon.Hej.expr.XPathContext context) {
            return directoryContents(dirFile, @params);
        }
        public override System.Collections.Generic.IEnumerator<Saxon.Hej.lib.Resource> getResources(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.lib.ParseOptions options = optionsFromQueryParameters(@params, context).withSpaceStrippingRule(whitespaceRules);
            Saxon.Ejava.util.Optional<bool> metadataParam = @params.getMetaData();
            bool metadata = (metadataParam.isPresent() && metadataParam.get());
            System.Collections.Generic.IEnumerator<string> resourceURIs = getResourceURIs(context);
            return new Saxon.Impl.Overrides.MappingEnumerator<string,Saxon.Hej.lib.Resource>(resourceURIs, @in => {
                try {
                    Saxon.Hej.resource.AbstractResourceCollection.InputDetails details = getInputDetails(@in);
                    details.resourceUri = @in;
                    details.parseOptions = options;
                    if (@params.getContentType().isPresent()) {
                        details.contentType = @params.getContentType().get();
                    }
                    Saxon.Hej.lib.Resource resource = makeResource(context, details);
                    if ((resource != null)) {
                        if (metadata) {
                            return makeMetadataResource(resource, details, context);
                        } else {
                            return resource;
                        }
                    }
                    return null;
                } catch (Saxon.Hej.trans.XPathException e) {
                    Saxon.Ejava.util.Optional<int> onError = @params.getOnError();
                    if ((onError.isPresent() && (onError.get() == Saxon.Hej.functions.URIQueryParameters.ON_ERROR_FAIL))) {
                        return new Saxon.Hej.resource.FailedResource(@in, e);
                    } else {
                        if ((onError.isPresent() && (onError.get() == Saxon.Hej.functions.URIQueryParameters.ON_ERROR_WARNING))) {
                            context.getController().warning(((("collection(): failed to parse "+@in)+": ")+e.Message), e.getErrorCodeLocalPart(), null);
                            return null;
                        } else {
                            return null;
                        }
                    }
                }
            } );
        }
        private Saxon.Hej.resource.MetadataResource makeMetadataResource(Saxon.Hej.lib.Resource resource, Saxon.Hej.resource.AbstractResourceCollection.InputDetails details, Saxon.Hej.expr.XPathContext context) {
            System.Collections.Generic.IDictionary<string,Saxon.Hej.om.GroundedValue> properties = new System.Collections.Generic.Dictionary<string, Saxon.Hej.om.GroundedValue>();
            try {
                System.Uri uri = new System.Uri(resource.getResourceURI(), System.UriKind.RelativeOrAbsolute);
                if ((details.contentType != null)) {
                    Saxon.Impl.Helpers.MapUtils.Put(properties, "content-type", Saxon.Hej.value.StringValue.makeStringValue(details.contentType));
                }
                if ((details.encoding != null)) {
                    Saxon.Impl.Helpers.MapUtils.Put(properties, "encoding", Saxon.Hej.value.StringValue.makeStringValue(details.encoding));
                }
                if ("file".Equals(uri.Scheme)) {
                    Saxon.Ejava.io.File file = new Saxon.Ejava.io.File(uri);
                    Saxon.Impl.Helpers.MapUtils.Put(properties, "path", Saxon.Hej.value.StringValue.makeStringValue(file.getPath()));
                    Saxon.Impl.Helpers.MapUtils.Put(properties, "absolute-path", Saxon.Hej.value.StringValue.makeStringValue(file.getAbsolutePath()));
                    Saxon.Impl.Helpers.MapUtils.Put(properties, "canonical-path", Saxon.Hej.value.StringValue.makeStringValue(file.getCanonicalPath()));
                    Saxon.Impl.Helpers.MapUtils.Put(properties, "can-read", Saxon.Hej.value.BooleanValue.get(file.canRead()));
                    Saxon.Impl.Helpers.MapUtils.Put(properties, "can-write", Saxon.Hej.value.BooleanValue.get(file.canWrite()));
                    Saxon.Impl.Helpers.MapUtils.Put(properties, "can-execute", Saxon.Hej.value.BooleanValue.get(file.canExecute()));
                    Saxon.Impl.Helpers.MapUtils.Put(properties, "is-hidden", Saxon.Hej.value.BooleanValue.get(file.isHidden()));
                    try {
                        Saxon.Impl.Helpers.MapUtils.Put(properties, "last-modified", Saxon.Hej.value.DateTimeValue.fromJavaTime(file.lastModified()));
                    } catch (Saxon.Hej.trans.XPathException e) {
                    }
                    Saxon.Impl.Helpers.MapUtils.Put(properties, "length", new Saxon.Hej.value.Int64Value(file.length()));
                }
            } catch (Exception e) when (e is System.UriFormatException || e is System.IO.IOException) {
            }
            return new Saxon.Hej.resource.MetadataResource(resource.getResourceURI(), resource, properties, context);
        }
        protected internal System.Collections.Generic.IEnumerator<string> directoryContents(Saxon.Ejava.io.File directory, Saxon.Hej.functions.URIQueryParameters @params) {
            Saxon.Ejava.io.FilenameFilter filter = null;
            bool recurse = false;
            if ((@params != null)) {
                Saxon.Ejava.util.Optional<Saxon.Ejava.io.FilenameFilter> f = @params.getFilenameFilter();
                if (f.isPresent()) {
                    filter = f.get();
                }
                Saxon.Ejava.util.Optional<bool> r = @params.getRecurse();
                if (r.isPresent()) {
                    recurse = r.get();
                }
            }
            System.Collections.Generic.Stack<System.Collections.Generic.IEnumerator<Saxon.Ejava.io.File>> directories = new System.Collections.Generic.Stack<System.Collections.Generic.IEnumerator<Saxon.Ejava.io.File>>();
            directories.Push(Saxon.Ejava.util.Arrays.asList(directory.listFiles(filter)).GetEnumerator());
            return new Saxon.Hej.resource.DirectoryCollection.DirectoryIterator(directories, recurse, filter);
        }
        private class DirectoryIterator : System.Collections.Generic.IEnumerator<string> {
            private readonly System.Collections.Generic.Stack<System.Collections.Generic.IEnumerator<Saxon.Ejava.io.File>> directories;
            private readonly Saxon.Ejava.io.FilenameFilter filter;
            private readonly bool recurse;
            private string nextItem = null;
            public DirectoryIterator(System.Collections.Generic.Stack<System.Collections.Generic.IEnumerator<Saxon.Ejava.io.File>> directories, bool recurse, Saxon.Ejava.io.FilenameFilter filter)  {
                this.directories = directories;
                this.recurse = recurse;
                this.filter = filter;
                advance();
            }
            public bool hasNext() {
                return (nextItem != null);
            }
            public string next() {
                string s = nextItem;
                advance();
                return s;
            }
            public void remove() {
                throw new Saxon.Ejava.lang.UnsupportedOperationException();
            }
            private void advance() {
                if ((directories.Count==0)) {
                    nextItem = null;
                } else {
                    System.Collections.Generic.IEnumerator<Saxon.Ejava.io.File> files = directories.Peek();
                    while (!(files.MoveNext())) {
                        directories.Pop();
                        if ((directories.Count==0)) {
                            nextItem = null;
                            return ;
                        }
                        files = directories.Peek();
                    }
                    Saxon.Ejava.io.File nextFile = files.Current;
                    if (nextFile.isDirectory()) {
                        if (recurse) {
                            directories.Push(Saxon.Ejava.util.Arrays.asList(nextFile.listFiles(filter)).GetEnumerator());
                        }
                        advance();
                    } else {
                        nextItem = nextFile.toURI().ToString();
                    }
                }
            }
                
        /* Add methods to implement IEnumerable */

            object System.Collections.IEnumerator.Current => Current;
            public string Current { get; private set; }
            public bool MoveNext() {
                if (hasNext()) {Current = next(); return true;} else return false;
            }
            public void Reset() {throw new NotImplementedException();}
            public void Dispose() {}
      
        }
    
    }
}