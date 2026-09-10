using System.Globalization;
using System.Impl.Helpers;
using System.IO;
using System.Xml;
using SysCollGen = System.Collections.Generic;
using Saxon.Ejava.lang;
using Saxon.Ejavax.xml.transform;
using Saxon.Ejavax.xml.transform.stream;
using Saxon.Hej;
using Saxon.Hej.lib;
using Saxon.Hej.pull;
using Saxon.Callbacks;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using AngleSharp.Dom;
using Saxon.Ejava.net;
using XmlResolver.Features;
using Saxon.Hej.om;
using Saxon.Hej.trans;
using Saxon.Hej.type;
using Type = System.Type;


namespace Saxon.Impl.Helpers {
    
    public class DotNetPlatform : Saxon.Hej.Platform {
        private string resourceLocation;
        
        public DotNetPlatform() {
            // Allow access to all known encodings
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            // Establish the prefix to use for loading resources. There must be a better way, but this works.
            System.Reflection.Assembly ass = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceNames = ass.GetManifestResourceNames();
            foreach (string name in resourceNames) {
                if (name.EndsWith("casevariants.xml")) {
                    resourceLocation = name.Replace("casevariants.xml", "");
                    break;
                }
            }
        }

        public void initialize(Saxon.Hej.Configuration config) {
            //config.setURIResolver(new StandardURIResolver(config, new StandardXmlResolver(config)));
            config.setCollectionFinder((cxt, uri) => 
                new Saxon.Hej.resource.StandardCollectionFinder().findCollection(cxt, uri));
            //config.setUnparsedTextURIResolver(new StandardUnparsedTextResolver(config).resolve);
            config.setLocalizerFactory(new Saxon.Impl.Overrides.DotNetLocalizerFactory(config));
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
                return new System.Globalization.RegionInfo(culture.Name).TwoLetterISORegionName;
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
            return System.OperatingSystem.IsWindows();
        }

        public bool hasICUCollator() {
            return true;
        }

        public bool hasICUNumberer() {
            return false;
        }

        public Saxon.Hej.lib.StringCollator makeCollation(Saxon.Hej.Configuration config,
                    Saxon.Ejava.util.Properties props, string uri) {
            return DotNetCollationFactory.makeCollation(config, uri, props);
        }

        public bool canReturnCollationKeys(Saxon.Hej.lib.StringCollator collation) {
            return true;
            //return collation is DotNetComparator || collation is Saxon.Hej.expr.sort.CodepointCollator;
        }

        public Saxon.Hej.expr.sort.AtomicMatchKey getCollationKey(
                Saxon.Hej.expr.sort.SimpleCollation namedCollation, string value) {
            // DotNetComparator c = (DotNetComparator) namedCollation.getComparator();
            // return c.getCollationKey(value);
            throw new NotImplementedException(); 
        }

        public Saxon.Hej.lib.StringCollator makeUcaCollator(string uri, Saxon.Hej.Configuration config) {
            return DotNetCollationFactory.makeCollation(config, uri, null);
        }

        public Saxon.Hej.regex.RegularExpression compileRegularExpression(Saxon.Hej.Configuration config,
            Saxon.Hej.str.UnicodeString regex, string flags, string hostLanguage, IList<string> warnings) {
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
                return new Saxon.Hej.regex.JavaRegularExpression(regex, flags);
            }
            else {
                if (useDotNet) {
                    flags = flags.Substring(0, semi);
                    return new Saxon.Hej.regex.JavaRegularExpression(regex, flags);
                }
                else {
                    return new Saxon.Hej.regex.ARegularExpression(regex, flags, hostLanguage, warnings, config);
                }
            }
        }

        public Saxon.Hej.type.ExternalObjectType getExternalObjectType(Saxon.Hej.Configuration config, NamespaceUri uri,
                    string localName) {
            // if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.DOT_NET_TYPE))
            //     return new DotNetExternalObjectType(System.Type.GetType(localName), config);
            // else
                throw new Saxon.Ejava.lang.IllegalArgumentException("Type is not in .NET namespace");
        }

        public string getInstallationDirectory(string edition, Saxon.Hej.Configuration config) {
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

        public void registerAllBuiltInObjectModels(Saxon.Hej.Configuration config) {
        }

        public void setDefaultSAXParserFactory(Saxon.Hej.Configuration config) {
            // var editionCode = "he";
            // Saxon.Ejava.lang.System.setProperty("Saxon.Ejavax.xml.parsers.SAXParserFactory",
            //     "org.apache.xerces.jaxp.SAXParserFactoryImpl, saxon9" + editionCode + ", Version=" +
            //     Saxon.Hej.Version.getProductVersion() + ", Culture=neutral, PublicKeyToken=e1fdd002d5083fe6");
        }

        public bool JAXPStaticContextCheck(Saxon.Hej.expr.parser.RetainedStaticContext retainedStaticContext,
            Saxon.Hej.expr.StaticContext sc) {
            return false;
        }

        public Saxon.Hej.lib.ModuleURIResolver makeStandardModuleURIResolver(Saxon.Hej.Configuration config) {
            return new StandardModuleURIResolver(config);
        }

        public IDynamicLoader getDefaultDynamicLoader() {
            return new DynamicLoader();
        }

        public void showEmbeddedResources() {
            System.Reflection.Assembly ass = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceNames = ass.GetManifestResourceNames();
            foreach (string name in resourceNames) {
                Console.WriteLine("Resource: " + name);
            }
        }

        public Stream locateResource(string filename, IList<string> messages) {
            System.Reflection.Assembly ass = System.Reflection.Assembly.GetExecutingAssembly();
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

        public ActiveSource resolveSource(Source source, ParseOptions options, Configuration config) {
            if (source is ActiveSource activeSource) {
                return activeSource;
            }
            
            if (source is StreamSource streamSource) {
                
                Stream inputStream = streamSource.getInputStream();
            
                XmlReaderSettings settings = new XmlReaderSettings() {
                    DtdProcessing = DtdProcessing.Parse,
                    MaxCharactersFromEntities = 100000,
                    XmlResolver = new XmlResolverWrappingResourceResolver(config.getResourceResolver())
                };

                if (options != null && options.getDTDValidationMode() == Validation.STRICT) {
                    settings.ValidationType = ValidationType.DTD;
                }
            
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
                    catch (System.Exception e) {
                        throw new XPathException("Unable to retrieve URI " + sysId);
                    }
                    xmlReader = XmlReader.Create(stream, settings,source.getSystemId());
                }
            
                if (xmlReader == null) {
                    throw new IllegalArgumentException("Unable to establish an XmlReader for supplied Source");
                }
            
                DotNetPullProvider provider = new DotNetPullProvider(xmlReader);
                provider.setBaseURI(source.getSystemId());
                PullSource pullSource = new PullSource(provider);
                pullSource.setSystemId(source.getSystemId());
                return pullSource;
            }
            return null;
        }
        
        public static System.Xml.XmlResolver getXmlResolverFromResourceResolver(Saxon.Hej.lib.JJ_ResourceResolver resourceResolver) {
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

