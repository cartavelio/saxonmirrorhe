using System;
using System.Collections.Generic;
using System.Globalization;
using System.Impl.Helpers;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Saxon.Callbacks;
using Saxon.Ejava.lang;
using Saxon.Ejava.net;
using Saxon.Ejava.util;
using Saxon.Ejavax.xml.transform;
using Saxon.Ejavax.xml.transform.stream;
using Saxon.Hej;
using Saxon.Hej.expr;
using Saxon.Hej.expr.parser;
using Saxon.Hej.expr.sort;
using Saxon.Hej.lib;
using Saxon.Hej.om;
using Saxon.Hej.pull;
using Saxon.Hej.regex;
using Saxon.Hej.resource;
using Saxon.Hej.str;
using Saxon.Hej.trans;
using Saxon.Hej.type;
using Saxon.Impl.Overrides;
using Exception = System.Exception;

namespace Saxon.Impl.Helpers {
    
    public class DotNetPlatform : Platform {
        private readonly string resourceLocation;
        private readonly string resourceRootLocation;
        
        public DotNetPlatform() {
            // Allow access to all known encodings
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            // Establish the prefix to use for loading resources. There must be a better way, but this works.
            Assembly ass = Assembly.GetExecutingAssembly();
            var resourceNames = ass.GetManifestResourceNames();
            foreach (string name in resourceNames) {
                if (name.EndsWith("casevariants.xml")) {
                    resourceLocation = name.Replace("casevariants.xml", "");
                    resourceRootLocation = resourceLocation.Substring(0, resourceLocation.Length - "/net/sf/saxon/data/".Length);
                    break;
                }
            }
        }

        public void initialize(Configuration config) {
            //config.setURIResolver(new StandardURIResolver(config, new StandardXmlResolver(config)));
            config.setCollectionFinder((cxt, uri) => 
                new StandardCollectionFinder().findCollection(cxt, uri));
            //config.setUnparsedTextURIResolver(new StandardUnparsedTextResolver(config).resolve);
            config.setLocalizerFactory(new DotNetLocalizerFactory(config));
        }
        

        public string getDefaultLanguage() {
            CultureInfo culture = CultureInfo.CurrentCulture;
            if (culture.Equals(CultureInfo.InvariantCulture)) {
                // this seems to be the case on Mac with .NET Core. It seems there's no obvious
                // way of picking up the settings from the operating system, so we default to English
                return "en";
            }
            else {
                return culture.Name;
            }
        }
        
        public string getDefaultCountry() {
            CultureInfo culture = CultureInfo.CurrentCulture;
            if (culture.Equals(CultureInfo.InvariantCulture) || culture.IsNeutralCulture) {
                // this seems to be the case on Mac with .NET Core. It seems there's no obvious
                // way of picking up the settings from the operating system, so we default to "US"
                return "US";
            }
            else {
                return new RegionInfo(culture.Name).TwoLetterISORegionName;
            }
        }

        public bool isJava() {
            return false;
        }

        public bool isDotNet() {
            return true;
        }

        public string getPlatformVersion() {
            string suffix = "";
            if (OperatingSystem.IsWindows()) {
                suffix = " on Windows " + Environment.OSVersion.Version;
            } else if (OperatingSystem.IsMacOS()) {
                suffix = " on MacOS";  // version info is not helpful
            } else if (OperatingSystem.IsLinux()) {
                suffix = " on Linux";
            }

            return ".NET " + Environment.Version + suffix;
        }

        public string getPlatformSuffix() {
            return "CS";
        }
        
        public bool isWindows() {
            return OperatingSystem.IsWindows();
        }

        public bool hasICUCollator() {
            return true;
        }

        public bool hasICUNumberer() {
            return false;
        }

        public StringCollator makeCollation(Configuration config,
                    Properties props, string uri) {
            return DotNetCollationFactory.makeCollation(config, uri, props);
        }

        public bool canReturnCollationKeys(StringCollator collation) {
            return true;
            //return collation is DotNetComparator || collation is Saxon.Hej.expr.sort.CodepointCollator;
        }

        public AtomicMatchKey getCollationKey(
                SimpleCollation namedCollation, string value) {
            // DotNetComparator c = (DotNetComparator) namedCollation.getComparator();
            // return c.getCollationKey(value);
            throw new NotImplementedException(); 
        }

        public StringCollator makeUcaCollator(string uri, Configuration config) {
            return DotNetCollationFactory.makeCollation(config, uri, null);
        }

        public RegularExpression compileRegularExpression(Configuration config,
            UnicodeString regex, string flags, string hostLanguage, IList<string> warnings) {
            var useJava = false;
            var useDotNet = false;
            var useSaxon = false;
            var semi = flags.IndexOf(';');
            if (semi >= 0) {
                useJava = flags.IndexOf('j', semi) >= 0;
                useDotNet = flags.IndexOf('n', semi) >= 0;
                useSaxon = flags.IndexOf('s', semi) >= 0;
            }

            if (!useJava && !useDotNet && !useSaxon) {
                var def = config.getDefaultRegexEngine();
                if ("N".Equals(def)) {
                    useDotNet = true;
                }
                else {
                    if ("J".Equals(def)) useJava = true;
                }
            }

            if (useJava) {
                flags = flags.Substring(0, semi);
                return new JavaRegularExpression(regex, flags);
            }
            else {
                if (useDotNet) {
                    flags = flags.Substring(0, semi);
                    return new JavaRegularExpression(regex, flags);
                }
                else {
                    return new ARegularExpression(regex, flags, hostLanguage, warnings, config);
                }
            }
        }

        public ExternalObjectType getExternalObjectType(Configuration config, NamespaceUri uri,
                    string localName) {
            // if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.DOT_NET_TYPE))
            //     return new DotNetExternalObjectType(System.Type.GetType(localName), config);
            // else
                throw new IllegalArgumentException("Type is not in .NET namespace");
        }

        public string getInstallationDirectory(string edition, Configuration config) {
            // RegistryKey[] bases = {Registry.LocalMachine, Registry.CurrentUser};
            // string[] paths = {"Software\\Saxonica\\Saxon", "Software\\Wow6432Node\\Saxonica\\Saxon"};
            // foreach (RegistryKey @base in bases)
            // foreach (var path in paths)
            //     if (@base != null) {
            //         RegistryKey regKey = @base.OpenSubKey(path + edition + "-N\\Settings", false);
            //         if (regKey != null) {
            //             if (config.isTiming()) config.getLogger().info("Found registry key at " + regKey.ToString());
            //             var installPath = (string) regKey.GetValue("InstallPath");
            //             if (config.isTiming()) config.getLogger().info("Software installation path: " + installPath);
            //             return installPath;
            //         }
            //     }

            return null;
        }

        public void registerAllBuiltInObjectModels(Configuration config) {
        }

        public void setDefaultSAXParserFactory(Configuration config) {
            // var editionCode = "he";
            // Saxon.Ejava.lang.System.setProperty("Saxon.Ejavax.xml.parsers.SAXParserFactory",
            //     "org.apache.xerces.jaxp.SAXParserFactoryImpl, saxon9" + editionCode + ", Version=" +
            //     Saxon.Hej.Version.getProductVersion() + ", Culture=neutral, PublicKeyToken=e1fdd002d5083fe6");
        }

        public bool JAXPStaticContextCheck(RetainedStaticContext retainedStaticContext,
            StaticContext sc) {
            return false;
        }

        public ModuleURIResolver makeStandardModuleURIResolver(Configuration config) {
            return new StandardModuleURIResolver(config);
        }

        public IDynamicLoader getDefaultDynamicLoader() {
            return new DynamicLoader();
        }

        public void showEmbeddedResources() {
            Assembly ass = Assembly.GetExecutingAssembly();
            var resourceNames = ass.GetManifestResourceNames();
            foreach (string name in resourceNames) {
                Console.WriteLine("Resource: " + name);
            }
        }

        public Stream locateResource(string filename, IList<string> messages) {
            Assembly ass = Assembly.GetExecutingAssembly();
            Stream firstAttempt = ass.GetManifestResourceStream(resourceLocation + filename);
            if (firstAttempt != null) {
                //Console.Error.WriteLine("First attempt at " + filename + " worked");
                return firstAttempt;
            }

            if (OperatingSystem.IsWindows() && filename.Contains("/")) {
                // On Windows, if the filename contains a "/", it won't find the file.
                // the filename has to use "\" as the separator.
                string bsfilename = filename.Replace("/", "\\");
                firstAttempt = ass.GetManifestResourceStream(resourceLocation + bsfilename);
                if (firstAttempt != null) {
                    //Console.Error.WriteLine("First attempt at " + filename + " worked");
                    return firstAttempt;
                }
            }
            
            // The second attempt is needed if the embedded resources have a logical name containing a "." rather than a "/"
            // for the directory separator. This happens with embedded resources added to the project automatically by Rider:
            // see bug 5122. The second attempt is unnecessary if the Rider project has the embedded resources added
            // manually, but it does no harm so we'll keep the code:
            Stream secondAttempt = ass.GetManifestResourceStream(resourceLocation + filename.Replace('/', '.'));
            if (secondAttempt != null) {
                //Console.Error.WriteLine("Second attempt at " + filename + " worked");
                return secondAttempt;
            }

            return null;
        }

        public Stream locateResourcePath(string path, IList<string> messages) {
            Assembly ass = Assembly.GetExecutingAssembly();
            Stream firstAttempt = ass.GetManifestResourceStream(resourceRootLocation + path);
            if (firstAttempt != null) {
                //Console.Error.WriteLine("First attempt at " + filename + " worked");
                return firstAttempt;
            }

            if (OperatingSystem.IsWindows() && path.Contains("/")) {
                // On Windows, if the filename contains a "/", it won't find the file.
                // the filename has to use "\" as the separator.
                string bspath = path.Replace("/", "\\");
                firstAttempt = ass.GetManifestResourceStream(resourceRootLocation + bspath);
                if (firstAttempt != null) {
                    //Console.Error.WriteLine("First attempt at " + filename + " worked");
                    return firstAttempt;
                }
            }
            
            // The second attempt is needed if the embedded resources have a logical name containing a "." rather than a "/"
            // for the directory separator. This happens with embedded resources added to the project automatically by Rider:
            // see bug 5122. The second attempt is unnecessary if the Rider project has the embedded resources added
            // manually, but it does no harm so we'll keep the code:
            Stream secondAttempt = ass.GetManifestResourceStream(resourceRootLocation + path.Replace('/', '.'));
            if (secondAttempt != null) {
                //Console.Error.WriteLine("Second attempt at " + filename + " worked");
                return secondAttempt;
            }

            return null;
        }

        public ActiveSource resolveSource(Source source, ParseOptions options, Configuration config) {
            if (source is ActiveSource activeSource) {
                return activeSource;
            }
            
            if (source is StreamSource streamSource) {
                
                Stream inputStream = streamSource.getInputStream();

                if (options != null && options.isXIncludeAware()) {
                    throw new XPathException("No XInclude processor is available in SaxonCS", "FODC0013");
                }
            
                XmlReaderSettings settings = new XmlReaderSettings() {
                    DtdProcessing = DtdProcessing.Parse,
                    MaxCharactersFromEntities = 100000,
                    XmlResolver = new XmlResolverWrappingResourceResolver(config.getResourceResolver())
                };

                if (options != null && options.getDTDValidationMode() == Validation.STRICT) {
                    settings.ValidationType = ValidationType.DTD;
                }

                // Commented out because "trusted" option is not fully implemented
                // if (!options.isTrusted()) {
                //     settings.XmlResolver = System.Xml.XmlResolver.ThrowingResolver;
                // }
            
                // if (source.getSystemId() != null) {
                //     settings.XmlResolver =
                //         new TracingXmlResolver(new Uri(source.getSystemId(), UriKind.RelativeOrAbsolute));
                // }
            
                XmlReader xmlReader;
                if (inputStream != null) {
                    xmlReader = XmlReader.Create(inputStream, settings,source.getSystemId());
                }
                else if (streamSource.getReader() != null) {
                    xmlReader = XmlReader.Create(streamSource.getReader(), settings, source.getSystemId());
                }
                else {
                    string sysId = source.getSystemId();
                    if (sysId == null) {
                        throw new IllegalArgumentException("Supplied StreamSource includes no stream, reader, or URI");
                    }

                    Stream stream;
                    try {
                        stream = new URL(new Uri(sysId)).openStream();
                    }
                    catch (Exception e) {
                        throw new XPathException("Unable to retrieve URI " + sysId);
                    }
                    xmlReader = XmlReader.Create(stream, settings,source.getSystemId());
                }
            
                if (xmlReader == null) {
                    throw new IllegalArgumentException("Unable to establish an XmlReader for supplied Source");
                }
            
                DotNetPullProvider provider = new DotNetPullProvider(xmlReader);
                provider.setBaseURI(source.getSystemId());
                if (options != null && options.getDTDValidationMode() == Validation.STRICT) {
                    provider.IgnoreIgnorableWhitespace = true;
                    provider.DocTypeOk = false; // Need to check manually that there is a DOCTYPE declaration
                } 
                PullSource pullSource = new PullSource(provider);
                pullSource.setSystemId(source.getSystemId());
                return pullSource;
            }
            return null;
        }
        
        public static System.Xml.XmlResolver getXmlResolverFromResourceResolver(JJ_ResourceResolver resourceResolver) {
            if (resourceResolver is CatalogResourceResolver catalogResolver) {
                return catalogResolver.GetEntityResolver().GetXmlResolver();
            }

            return new XmlResolverWrappingResourceResolver(resourceResolver);
        }
    }

    public class TracingXmlResolver : XmlUrlResolver {
        private readonly Uri baseUri;

        public TracingXmlResolver(Uri baseURI) {
            this.baseUri = baseURI;
        }

        public override Uri ResolveUri(Uri baseUri, string relativeUri) {
            //Console.Error.WriteLine("Resolve " + baseUri + " .. " + relativeUri);
            if (relativeUri.StartsWith("-//")) {
                // Looks like a Public ID: this isn't documented, but the parser calls us with a public ID!
                return null;
            }
            if (baseUri == null) {
                baseUri = this.baseUri;
            }
            return base.ResolveUri(baseUri, relativeUri);
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited

