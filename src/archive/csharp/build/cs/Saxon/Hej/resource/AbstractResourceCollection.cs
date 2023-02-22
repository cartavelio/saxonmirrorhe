using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.resource {
    public abstract class AbstractResourceCollection : Saxon.Hej.lib.ResourceCollection {
        protected internal Saxon.Hej.Configuration config;
        protected internal string collectionURI;
        protected internal Saxon.Hej.functions.URIQueryParameters @params = null;
        protected internal bool noExceptions = false;
        public AbstractResourceCollection(Saxon.Hej.Configuration config)  {
            this.config = config;
        }
        public static void checkNotNull(string collectionURI, Saxon.Hej.expr.XPathContext context) {
            if ((collectionURI == null)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("No default collection has been defined");
                err.setErrorCode("FODC0002");
                err.setXPathContext(context);
                throw err;
            }
        }
        public string getCollectionURI() {
            return collectionURI;
        }
        public virtual bool isStable(Saxon.Hej.expr.XPathContext context) {
            if ((@params == null)) {
                return false;
            }
            Saxon.Ejava.util.Optional<bool> stable = @params.getStable();
            if (!(stable.isPresent())) {
                return context.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.STABLE_COLLECTION_URI);
            } else {
                return stable.get();
            }
        }
        public void registerContentType(string contentType, Saxon.Hej.lib.ResourceFactory factory) {
            config.registerMediaType(contentType, factory);
        }
        protected internal Saxon.Hej.lib.ParseOptions optionsFromQueryParameters(Saxon.Hej.functions.URIQueryParameters @params, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.lib.ParseOptions options = new Saxon.Hej.lib.ParseOptions(context.getConfiguration().getParseOptions());
            if ((@params != null)) {
                Saxon.Ejava.util.Optional<int> v = @params.getValidationMode();
                if (v.isPresent()) {
                    options.setSchemaValidationMode(v.get());
                }
                Saxon.Ejava.util.Optional<bool> xInclude = @params.getXInclude();
                if (xInclude.isPresent()) {
                    options.setXIncludeAware(xInclude.get());
                }
                Saxon.Ejava.util.Optional<Saxon.Hej.om.SpaceStrippingRule> stripSpace = @params.getSpaceStrippingRule();
                if (stripSpace.isPresent()) {
                    options.setSpaceStrippingRule(stripSpace.get());
                }
                int onError = Saxon.Hej.functions.URIQueryParameters.ON_ERROR_FAIL;
                if (@params.getOnError().isPresent()) {
                    onError = @params.getOnError().get();
                }
                Saxon.Hej.Controller controller = context.getController();
                Saxon.Hej.lib.ErrorReporter oldErrorReporter = ((controller == null) ? context.getConfiguration().makeErrorReporter() : controller.getErrorReporter());
                setupErrorHandlingForCollection(options, onError, oldErrorReporter);
            }
            return options;
        }
        public static void setupErrorHandlingForCollection(Saxon.Hej.lib.ParseOptions options, int onError, Saxon.Hej.lib.ErrorReporter oldErrorReporter) {
            if ((onError == Saxon.Hej.functions.URIQueryParameters.ON_ERROR_IGNORE)) {
                options.setErrorReporter(new Saxon.Hej.resource.AbstractResourceCollection.ErrorSuppressor());
            } else {
                if ((onError == Saxon.Hej.functions.URIQueryParameters.ON_ERROR_WARNING)) {
                    options.setErrorReporter(new Saxon.Hej.resource.AbstractResourceCollection.ErrorAsWarningReporter(oldErrorReporter));
                }
            }
        }
        private class ErrorSuppressor : Saxon.Hej.lib.ErrorReporter {
            public void report(Saxon.Hej.s9api.XmlProcessingError error) {
            }
        
        }
        private class ErrorAsWarningReporter : Saxon.Hej.lib.ErrorReporter {
            private readonly Saxon.Hej.lib.ErrorReporter originalErrorReporter;
            public ErrorAsWarningReporter(Saxon.Hej.lib.ErrorReporter originalErrorReporter)  {
                this.originalErrorReporter = originalErrorReporter;
            }
            public void report(Saxon.Hej.s9api.XmlProcessingError error) {
                if (error.isWarning()) {
                    originalErrorReporter.report(error);
                } else {
                    originalErrorReporter.report(error.asWarning());
                    Saxon.Hej.trans.XmlProcessingIncident supp = ((/*1430*/Saxon.Hej.trans.XmlProcessingIncident)new Saxon.Hej.trans.XmlProcessingIncident("The document will be excluded from the collection").asWarning());
                    supp.setLocation(error.getLocation());
                    originalErrorReporter.report(supp);
                }
            }
        
        }
        public class InputDetails {
            public string resourceUri;
            public byte[] binaryContent;
            public string characterContent;
            public string contentType;
            public string encoding;
            public Saxon.Hej.lib.ParseOptions parseOptions;
            public int onError = Saxon.Hej.functions.URIQueryParameters.ON_ERROR_FAIL;
            public System.IO.Stream getInputStream() {
                return Saxon.Impl.Overrides.ResourceLoader.urlStream(new Saxon.Ejava.net.URL(resourceUri));
            }
            public byte[] obtainBinaryContent() {
                if ((binaryContent != null)) {
                    return binaryContent;
                } else {
                    if ((characterContent != null)) {
                        string e = ((encoding != null) ? encoding : "UTF-8");
                        return Saxon.Hej.resource.BinaryResource.encode(characterContent, e);
                    } else {
                        using (System.IO.Stream stream = getInputStream()) {                            try {
                                return Saxon.Hej.resource.BinaryResource.readBinaryFromStream(stream, resourceUri);
                            } catch (System.IO.IOException e) {
                                throw new Saxon.Hej.trans.XPathException(e);
                            }
                        }                    }
                }
            }
            public string obtainCharacterContent() {
                if ((characterContent != null)) {
                    return characterContent;
                } else {
                    if (((binaryContent != null) && (encoding != null))) {
                        return Saxon.Hej.resource.BinaryResource.decode(binaryContent, encoding);
                    } else {
                        using (System.IO.Stream stream = getInputStream()) {                            try {
                                string enc = encoding;
                                if ((enc == null)) {
                                    enc = Saxon.Hej.resource.EncodingDetector.inferStreamEncoding(stream, null);
                                }
                                return characterContent = Saxon.Hej.resource.CatalogCollection.makeStringFromStream(stream, enc);
                            } catch (System.IO.IOException e) {
                                if ((onError == Saxon.Hej.functions.URIQueryParameters.ON_ERROR_FAIL)) {
                                    throw new Saxon.Hej.trans.XPathException(e);
                                } else {
                                    return null;
                                }
                            }
                        }                    }
                }
            }
        
        }
        protected internal Saxon.Hej.resource.AbstractResourceCollection.InputDetails getInputDetails(string resourceURI) {
            Saxon.Hej.resource.AbstractResourceCollection.InputDetails inputDetails = new Saxon.Hej.resource.AbstractResourceCollection.InputDetails();
            try {
                inputDetails.resourceUri = resourceURI;
                System.Uri uri = new System.Uri(resourceURI, System.UriKind.RelativeOrAbsolute);
                if ("file".Equals(uri.Scheme)) {
                    if (((@params != null) && @params.getContentType().isPresent())) {
                        inputDetails.contentType = @params.getContentType().get();
                    } else {
                        inputDetails.contentType = guessContentTypeFromName(resourceURI);
                    }
                } else {
                    Saxon.Ejava.net.URLConnection connection = Saxon.Impl.Overrides.ResourceLoader.urlConnection(Saxon.Impl.Helpers.UriUtils.ToURL(uri));
                    inputDetails.contentType = connection.getContentType();
                    inputDetails.encoding = connection.getContentEncoding();
                    foreach (string param in System.Text.RegularExpressions.Regex.Split(inputDetails.contentType.Replace(" ", ""), ";")) {
                        if (param.StartsWith("charset=")) {
                            inputDetails.encoding = param.Split("=", 2)[1];
                        } else {
                            inputDetails.contentType = param;
                        }
                    }
                }
                if (((inputDetails.contentType == null) || (config.getResourceFactoryForMediaType(inputDetails.contentType) == null))) {
                    System.IO.Stream stream;
                    if ("file".Equals(uri.Scheme)) {
                        Saxon.Ejava.io.File file = new Saxon.Ejava.io.File(uri);
                        stream = new System.IO.BufferedStream(System.IO.File.OpenRead(file.getPath()));
                        if ((file.length()<=1024)) {
                            inputDetails.binaryContent = Saxon.Hej.resource.BinaryResource.readBinaryFromStream(stream, resourceURI);
                            stream.Close();
                            stream = new System.IO.MemoryStream(inputDetails.binaryContent);
                        }
                    } else {
                        stream = Saxon.Impl.Overrides.ResourceLoader.urlStream(Saxon.Impl.Helpers.UriUtils.ToURL(uri));
                    }
                    inputDetails.contentType = guessContentTypeFromContent(stream);
                    stream.Close();
                }
                if (((@params != null) && @params.getOnError().isPresent())) {
                    inputDetails.onError = @params.getOnError().get();
                }
                return inputDetails;
            } catch (Exception e) when (e is System.UriFormatException || e is System.IO.IOException) {
                throw new Saxon.Hej.trans.XPathException(e);
            }
        }
        protected internal string guessContentTypeFromName(string resourceURI) {
            string contentTypeFromName = Saxon.Ejava.net.URLConnection.guessContentTypeFromName(resourceURI);
            string extension = null;
            if ((contentTypeFromName == null)) {
                extension = getFileExtension(resourceURI);
                if ((extension != null)) {
                    contentTypeFromName = config.getMediaTypeForFileExtension(extension);
                }
            }
            return contentTypeFromName;
        }
        protected internal string guessContentTypeFromContent(System.IO.Stream stream) {
            try {
                stream = Saxon.Hej.query.InputStreamMarker.ensureMarkSupported(stream);
                return Saxon.Ejava.net.URLConnection.guessContentTypeFromStream(stream);
            } catch (System.IO.IOException err) {
                return null;
            }
        }
        private string getFileExtension(string name) {
            int i = name.LastIndexOf('.');
            int p = System.Math.Max(name.LastIndexOf('/'), name.LastIndexOf('\\'));
            if (((i>p) && ((i+1)<name.Length))) {
                return name.Substring((i+1));
            }
            return null;
        }
        public Saxon.Hej.lib.Resource makeResource(Saxon.Hej.expr.XPathContext context, Saxon.Hej.resource.AbstractResourceCollection.InputDetails details) {
            Saxon.Hej.lib.ResourceFactory factory = null;
            string contentType = details.contentType;
            if ((contentType != null)) {
                factory = context.getConfiguration().getResourceFactoryForMediaType(contentType);
            }
            if ((factory == null)) {
                factory = Saxon.Hej.resource.BinaryResource.FACTORY;
            }
            return /*Delegate.makeResource()*/factory(context, details);
        }
        public Saxon.Hej.lib.Resource makeTypedResource(Saxon.Hej.expr.XPathContext context, Saxon.Hej.lib.Resource basicResource) {
            string mediaType = basicResource.getContentType();
            Saxon.Hej.lib.ResourceFactory factory = config.getResourceFactoryForMediaType(mediaType);
            if ((factory == null)) {
                return basicResource;
            }
            if ((basicResource is Saxon.Hej.resource.BinaryResource)) {
                Saxon.Hej.resource.AbstractResourceCollection.InputDetails details = new Saxon.Hej.resource.AbstractResourceCollection.InputDetails();
                details.binaryContent = ((Saxon.Hej.resource.BinaryResource)(basicResource)).getData();
                details.contentType = mediaType;
                details.resourceUri = basicResource.getResourceURI();
                return /*Delegate.makeResource()*/factory(context, details);
            } else {
                if ((basicResource is Saxon.Hej.resource.UnparsedTextResource)) {
                    Saxon.Hej.resource.AbstractResourceCollection.InputDetails details = new Saxon.Hej.resource.AbstractResourceCollection.InputDetails();
                    details.characterContent = ((Saxon.Hej.resource.UnparsedTextResource)(basicResource)).getContent();
                    details.contentType = mediaType;
                    details.resourceUri = basicResource.getResourceURI();
                    return /*Delegate.makeResource()*/factory(context, details);
                } else {
                    return basicResource;
                }
            }
        }
        public Saxon.Hej.lib.Resource makeResource(Saxon.Hej.expr.XPathContext context, string resourceURI) {
            Saxon.Hej.resource.AbstractResourceCollection.InputDetails details = getInputDetails(resourceURI);
            return makeResource(context, details);
        }
        public virtual bool stripWhitespace(Saxon.Hej.om.SpaceStrippingRule rules) {
            return false;
        }
    
        /* Creating stub method getResourceURIs to satisfy interface ResourceCollection */
        public virtual System.Collections.Generic.IEnumerator<string> getResourceURIs(Saxon.Hej.expr.XPathContext arg1) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getResources to satisfy interface ResourceCollection */
        public virtual System.Collections.Generic.IEnumerator<Saxon.Hej.lib.Resource> getResources(Saxon.Hej.expr.XPathContext arg1) {
            throw new System.NotImplementedException();
        }

    }
}