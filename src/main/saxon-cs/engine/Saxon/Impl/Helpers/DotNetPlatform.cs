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


namespace Saxon.Impl.Helpers {
    
    public class DotNetPlatform : Saxon.Hej.Platform {
        private string resourceLocation;
        
        public DotNetPlatform() {
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
            config.setURIResolver(new StandardURIResolver(config, new StandardXmlResolver(config)));
            config.setCollectionFinder((cxt, uri) => 
                new Saxon.Hej.resource.StandardCollectionFinder().findCollection(cxt, uri));
            config.setUnparsedTextURIResolver(new StandardUnparsedTextResolver().resolve);
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
                return new System.Globalization.RegionInfo(culture.LCID).TwoLetterISORegionName;
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

        public bool hasICUCollator() {
            return true;
        }

        public bool hasICUNumberer() {
            return false;
        }

        // public org.xml.sax.XMLReader loadParser() {
        //     org.xml.sax.XMLReader parser;
        //     try {
        //         parser = Saxon.Ejavax.xml.parsers.SAXParserFactory.newInstance("org.apache.xerces.jaxp.SAXParserFactoryImpl", this.GetType().GetClassLoader()).newSAXParser().getXMLReader();
        //     } catch (Saxon.Ejavax.xml.parsers.ParserConfigurationException err) {
        //         throw new Saxon.Ejavax.xml.transform.TransformerFactoryConfigurationError(err);
        //     } catch (org.xml.sax.SAXException err) {
        //         throw new Saxon.Ejavax.xml.transform.TransformerFactoryConfigurationError(err);
        //     }
        //     return parser;
        // }
        // public org.xml.sax.XMLReader loadParserForXmlFragments() {
        //     return loadParser();
        // }
        public Saxon.Ejavax.xml.transform.Source getParserSource(Saxon.Hej.@event.PipelineConfiguration pipe,
            Saxon.Ejavax.xml.transform.stream.StreamSource input, int validation, bool dtdValidation) {
            return getParserSourceInternal(pipe, input, validation, dtdValidation, 0);
        }

        private Saxon.Ejavax.xml.transform.Source getParserSourceInternal(Saxon.Hej.@event.PipelineConfiguration pipe,
            Saxon.Ejavax.xml.transform.stream.StreamSource input, int validation, bool dtdValidation, int depth) {
            if (depth > 2) {
                return input;
            }

            XmlReaderSettings settings = new() {
                ConformanceLevel = ConformanceLevel.Document, 
                DtdProcessing = DtdProcessing.Parse
            };
            if (pipe.getURIResolver() is StandardURIResolver) {
                settings.XmlResolver = ((StandardURIResolver) pipe.getURIResolver()).getXmlResolver();
            }
            settings.ValidationType = dtdValidation ? ValidationType.DTD : ValidationType.None;

            Stream inputStream = input.getInputStream();
            if (inputStream != null) {
                

                XmlReader parser = XmlReader.Create(inputStream, settings);
                
                //XmlReader parser = new XmlTextReader(input.getSystemId(), inputStream);
                ((XmlTextReader) parser).EntityHandling = EntityHandling.ExpandEntities;
                ((XmlTextReader) parser).WhitespaceHandling = WhitespaceHandling.All;
                ((XmlTextReader) parser).Normalization = true;
                // if (pipe.getURIResolver() is StandardURIResolver)
                //     ((XmlTextReader) parser).XmlResolver =
                //         ((StandardURIResolver) pipe.getURIResolver()).getXmlResolver();
                // parser = new XmlValidatingReader(parser);
                // if (dtdValidation)
                //     ((XmlValidatingReader) parser).ValidationType = ValidationType.DTD;
                // else
                //     ((XmlValidatingReader) parser).ValidationType = ValidationType.None;
                Saxon.Hej.pull.PullProvider provider = new DotNetPullProvider(parser);
                var ps = new Saxon.Hej.pull.PullSource(provider);
                ps.setSystemId(input.getSystemId());
                if (validation == Saxon.Hej.lib.Validation.DEFAULT) {
                    return ps;
                } else {
                    var @as = Saxon.Hej.lib.AugmentedSource.makeAugmentedSource(ps);
                    @as.setSchemaValidationMode(validation);
                    return @as;
                }
            }

            TextReader reader = input.getReader();
            if (reader != null) {
                XmlReader parser = XmlReader.Create(reader, settings);
                
                //XmlReader parser = new XmlTextReader(input.getSystemId(), reader);
                ((XmlTextReader) parser).EntityHandling = EntityHandling.ExpandEntities;
                ((XmlTextReader) parser).WhitespaceHandling = WhitespaceHandling.All;
                ((XmlTextReader) parser).Normalization = true;
                // if (pipe.getURIResolver() is StandardURIResolver)
                //     ((XmlTextReader) parser).XmlResolver =
                //         ((StandardURIResolver) pipe.getURIResolver()).getXmlResolver();
                // parser = new XmlValidatingReader(parser);
                // if (dtdValidation)
                //     ((XmlValidatingReader) parser).ValidationType = ValidationType.DTD;
                // else
                //     ((XmlValidatingReader) parser).ValidationType = ValidationType.None;
                Saxon.Hej.pull.PullProvider provider = new DotNetPullProvider(parser);
                PullSource ps = new (provider);
                ps.setSystemId(input.getSystemId());
                if (validation == Saxon.Hej.lib.Validation.DEFAULT) {
                    return ps;
                } else {
                    var @as = Saxon.Hej.lib.AugmentedSource.makeAugmentedSource(ps);
                    @as.setSchemaValidationMode(validation);
                    return @as;
                }
            }

            var uri = input.getSystemId();
            if (uri != null)
                try {
                    var r = pipe.getURIResolver().resolve(uri, null);
                    if (r == null) {
                        return input;
                    }
                    else {
                        if (r is Saxon.Hej.lib.AugmentedSource augmentedSource) {
                            Source r2 = augmentedSource.getContainedSource();
                            if (r2 is Saxon.Ejavax.xml.transform.stream.StreamSource streamSource) {
                                r2 = getParserSourceInternal(pipe, streamSource,
                                    validation, dtdValidation, depth + 1);
                                return r2;
                            } else {
                                return r2;
                            }
                        } else {
                            if (r is Saxon.Ejavax.xml.transform.stream.StreamSource streamSource) {
                                if (isSame(streamSource, input)) return input;
                                Source r2 = getParserSourceInternal(pipe, streamSource,
                                    validation, dtdValidation, depth + 1);
                                AugmentedSource @as = Saxon.Hej.lib.AugmentedSource.makeAugmentedSource(r2);
                                @as.setPleaseCloseAfterUse(true);
                                return @as;
                            }
                            else {
                                return r;
                            }
                        }
                    }
                }
                catch (Saxon.Ejavax.xml.transform.TransformerException) {
                    return input;
                }

            return input;
        }

        private static bool isSame(Saxon.Ejavax.xml.transform.Source first, Saxon.Ejavax.xml.transform.Source second) {
            if (first == second) return true;
            if (first is Saxon.Ejavax.xml.transform.stream.StreamSource && second is Saxon.Ejavax.xml.transform.stream.StreamSource) {
                var ssFirst = (Saxon.Ejavax.xml.transform.stream.StreamSource) first;
                var ssSecond = (Saxon.Ejavax.xml.transform.stream.StreamSource) second;
                if (ssFirst.getInputStream() != null && ssSecond.getInputStream() != null)
                    return ssFirst.getInputStream() == ssSecond.getInputStream();
                if (ssFirst.getReader() != null && ssSecond.getReader() != null)
                    return ssFirst.getReader() == ssSecond.getReader();
                if (ssFirst.getSystemId() != null && ssSecond.getSystemId() != null)
                    return ssFirst.getSystemId().Equals(ssSecond.getSystemId());
            }

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
            return null;
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
                flags = flags.Substring(0, semi);
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
                return new Saxon.Hej.regex.JavaRegularExpression(regex, flags);
            }
            else {
                if (useDotNet)
                    return new Saxon.Hej.regex.JavaRegularExpression(regex, flags);
                else
                    return new Saxon.Hej.regex.ARegularExpression(regex, flags, hostLanguage, warnings, config);
            }
        }

        public Saxon.Hej.type.ExternalObjectType getExternalObjectType(Saxon.Hej.Configuration config, string uri,
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
            return new StandardModuleURIResolver(new StandardXmlResolver(config));
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

        public ActiveSource resolveSource(Source source, Configuration config) {
            if (source is ActiveSource activeSource) {
                return activeSource;
            }
            if (source is StreamSource streamSource) {
                
                Stream inputStream = streamSource.getInputStream();
                //string docTypeName = null;
                // bool hasDTD = false;
                // if (inputStream.CanSeek) {
                //     byte[] buffer = new byte[256];
                //     int size = inputStream.Read(buffer, 0, 256);
                //     string content = Encoding.Latin1.GetString(buffer);
                //     int docTypeStart = content.IndexOf("<!DOCTYPE");
                //     hasDTD = content.Contains("<!DOCTYPE");
                //     inputStream.Position = 0;
                // }
                //Console.WriteLine("Parsing " + source.getSystemId());
                
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.DtdProcessing = DtdProcessing.Parse;
                settings.MaxCharactersFromEntities = 100000; // TODO: configurable entity expansion limit
                if (source.getSystemId() != null) {
                    settings.XmlResolver =
                        new TracingXmlResolver(new Uri(source.getSystemId(), UriKind.RelativeOrAbsolute));
                }

                XmlReader xmlReader = null;
                if (inputStream != null) {
                    xmlReader = XmlReader.Create(inputStream, settings);

                    //if (hasDTD) {
                        
                    // }
                    // else {
                    //     xmlReader = XmlReader.Create(((StreamSource) source).getInputStream(),
                    //         settings, (XmlParserContext) null);
                    // }
                    // xmlReader = new XmlTextReader(((StreamSource) source).getInputStream());
                    // if (xmlReader is XmlTextReader textReader) {
                    //     textReader.DtdProcessing = DtdProcessing.Parse;
                    //     textReader.EntityHandling = EntityHandling.ExpandEntities;
                    // }
                    // Console.Error.WriteLine("CONTENTS OF STREAM SOURCE");
                    // var reader = new StreamReader(((StreamSource) source).getInputStream(), Encoding.UTF8);
                    // System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    // string line;
                    // while ((line = reader.ReadLine()) != null) {
                    //     sb.Append(line);
                    //     Console.Error.WriteLine(line);
                    // }
                    //
                    // if (sb.Length == 0) {
                    //     Console.Error.WriteLine("STREAM SOURCE IS EMPTY!!");
                    // }
                    // Console.Error.WriteLine("END OF CONTENTS OF STREAM SOURCE");
                    // xmlReader = XmlReader.Create(new StringReader(sb.ToString()),
                    //     settings, parserContext);
                }
                else if (streamSource.getReader() != null) {
                    xmlReader = XmlReader.Create(streamSource.getReader(), settings);
                }
                else {
                    string sysId = source.getSystemId();
                    if (sysId == null) {
                        throw new IllegalArgumentException("Supplied StreamSource includes no stream, reader, or URI");
                    }
                    else {
                        Uri uri = new Uri(sysId);
                        URIResolver ur = config.getURIResolver();
                        if (ur is StandardURIResolver) {
                            XmlResolver xr = ((StandardURIResolver) ur).getXmlResolver();
                            object o = xr.GetEntity(uri, "xml", typeof(Stream));
                            if (o is Stream) {
                                xmlReader = XmlReader.Create((Stream) o, settings);
                            }
                        }
                    }
                    
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
    }

    public class TracingXmlResolver : XmlUrlResolver {
        private readonly Uri baseUri;

        public TracingXmlResolver(Uri baseURI) {
            this.baseUri = baseURI;
        }
        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn) {
            //Console.Error.WriteLine("GetEntity " + absoluteUri);
            return base.GetEntity(absoluteUri, role, ofObjectToReturn);
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

// Copyright (c) 2021 Saxonica Limited

