using System.Globalization;
using System.Impl.Helpers;
using System.IO;
using System.Xml;
using java.lang;
using javax.xml.transform;
using javax.xml.transform.stream;
using net.sf.saxon;
using net.sf.saxon.lib;
using net.sf.saxon.pull;
using Saxon.Callbacks;
using SaxonCS3.Saxon.Impl.Helpers;

namespace Saxon.Impl.Helpers {
    using System;
    using System.Collections.Generic;

    public class DotNetPlatform : net.sf.saxon.Platform {
        public DotNetPlatform() {
        }

        public void initialize(net.sf.saxon.Configuration config) {
            config.setURIResolver(new StandardURIResolver(config, new StandardXmlResolver()));
            config.setCollectionFinder((cxt, uri) => 
                new net.sf.saxon.resource.StandardCollectionFinder().findCollection(cxt, uri));
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
            return ".NET " + Environment.Version.ToString() + " on " + Environment.OSVersion.ToString();
        }

        public string getPlatformSuffix() {
            return "N";
        }

        public System.IO.Stream locateResource(string filename, java.util.List<string> messages) {
            throw new java.lang.UnsupportedOperationException();
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
        //         parser = javax.xml.parsers.SAXParserFactory.newInstance("org.apache.xerces.jaxp.SAXParserFactoryImpl", this.GetType().GetClassLoader()).newSAXParser().getXMLReader();
        //     } catch (javax.xml.parsers.ParserConfigurationException err) {
        //         throw new javax.xml.transform.TransformerFactoryConfigurationError(err);
        //     } catch (org.xml.sax.SAXException err) {
        //         throw new javax.xml.transform.TransformerFactoryConfigurationError(err);
        //     }
        //     return parser;
        // }
        // public org.xml.sax.XMLReader loadParserForXmlFragments() {
        //     return loadParser();
        // }
        public javax.xml.transform.Source getParserSource(net.sf.saxon.@event.PipelineConfiguration pipe,
            javax.xml.transform.stream.StreamSource input, int validation, bool dtdValidation) {
            return getParserSourceInternal(pipe, input, validation, dtdValidation, 0);
        }

        private javax.xml.transform.Source getParserSourceInternal(net.sf.saxon.@event.PipelineConfiguration pipe,
            javax.xml.transform.stream.StreamSource input, int validation, bool dtdValidation, int depth) {
            if (depth > 2) return input;
            var config = pipe.getConfiguration();
            var @is = input.getInputStream();
            if (@is != null) {
                XmlReader parser = new XmlTextReader(input.getSystemId(), @is);
                ((XmlTextReader) parser).EntityHandling = EntityHandling.ExpandEntities;
                ((XmlTextReader) parser).WhitespaceHandling = WhitespaceHandling.All;
                ((XmlTextReader) parser).Normalization = true;
                if (pipe.getURIResolver() is StandardURIResolver)
                    ((XmlTextReader) parser).XmlResolver =
                        ((StandardURIResolver) pipe.getURIResolver()).getXmlResolver();
                parser = new XmlValidatingReader(parser);
                if (dtdValidation)
                    ((XmlValidatingReader) parser).ValidationType = ValidationType.DTD;
                else
                    ((XmlValidatingReader) parser).ValidationType = ValidationType.None;
                net.sf.saxon.pull.PullProvider provider = new DotNetPullProvider(parser);
                var ps = new net.sf.saxon.pull.PullSource(provider);
                ps.setSystemId(input.getSystemId());
                if (validation == net.sf.saxon.lib.Validation.DEFAULT) {
                    return ps;
                } else {
                    var @as = net.sf.saxon.lib.AugmentedSource.makeAugmentedSource(ps);
                    @as.setSchemaValidationMode(validation);
                    return @as;
                }
            }

            var reader = input.getReader();
            if (reader != null) {
                XmlReader parser = new XmlTextReader(input.getSystemId(), reader);
                ((XmlTextReader) parser).EntityHandling = EntityHandling.ExpandEntities;
                ((XmlTextReader) parser).WhitespaceHandling = WhitespaceHandling.All;
                ((XmlTextReader) parser).Normalization = true;
                if (pipe.getURIResolver() is StandardURIResolver)
                    ((XmlTextReader) parser).XmlResolver =
                        ((StandardURIResolver) pipe.getURIResolver()).getXmlResolver();
                parser = new XmlValidatingReader(parser);
                if (dtdValidation)
                    ((XmlValidatingReader) parser).ValidationType = ValidationType.DTD;
                else
                    ((XmlValidatingReader) parser).ValidationType = ValidationType.None;
                net.sf.saxon.pull.PullProvider provider = new DotNetPullProvider(parser);
                var ps = new net.sf.saxon.pull.PullSource(provider);
                ps.setSystemId(input.getSystemId());
                if (validation == net.sf.saxon.lib.Validation.DEFAULT) {
                    return ps;
                } else {
                    var @as = net.sf.saxon.lib.AugmentedSource.makeAugmentedSource(ps);
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
                        if (r is net.sf.saxon.lib.AugmentedSource) {
                            var r2 = ((net.sf.saxon.lib.AugmentedSource) r).getContainedSource();
                            if (r2 is javax.xml.transform.stream.StreamSource) {
                                r2 = getParserSourceInternal(pipe, (javax.xml.transform.stream.StreamSource) r2,
                                    validation, dtdValidation, depth + 1);
                                return r2;
                            } else {
                                return r2;
                            }
                        } else {
                            if (r is javax.xml.transform.stream.StreamSource) {
                                if (isSame(r, input)) return input;
                                var r2 = getParserSourceInternal(pipe, (javax.xml.transform.stream.StreamSource) r,
                                    validation, dtdValidation, depth + 1);
                                var @as = net.sf.saxon.lib.AugmentedSource.makeAugmentedSource(r2);
                                @as.setPleaseCloseAfterUse(true);
                                return @as;
                            }
                            else {
                                return r;
                            }
                        }
                    }
                }
                catch (javax.xml.transform.TransformerException err) {
                    return input;
                }

            return input;
        }

        private static bool isSame(javax.xml.transform.Source first, javax.xml.transform.Source second) {
            if (first == second) return true;
            if (first is javax.xml.transform.stream.StreamSource && second is javax.xml.transform.stream.StreamSource) {
                var ssFirst = (javax.xml.transform.stream.StreamSource) first;
                var ssSecond = (javax.xml.transform.stream.StreamSource) second;
                if (ssFirst.getInputStream() != null && ssSecond.getInputStream() != null)
                    return ssFirst.getInputStream() == ssSecond.getInputStream();
                if (ssFirst.getReader() != null && ssSecond.getReader() != null)
                    return ssFirst.getReader() == ssSecond.getReader();
                if (ssFirst.getSystemId() != null && ssSecond.getSystemId() != null)
                    return ssFirst.getSystemId().Equals(ssSecond.getSystemId());
            }

            return false;
        }

        public net.sf.saxon.lib.StringCollator makeCollation(net.sf.saxon.Configuration config,
                    java.util.Properties props, string uri) {
            return DotNetCollationFactory.makeCollation(config, uri, props);
        }

        public bool canReturnCollationKeys(net.sf.saxon.lib.StringCollator collation) {
            return true;
            //return collation is DotNetComparator || collation is net.sf.saxon.expr.sort.CodepointCollator;
        }

        public net.sf.saxon.expr.sort.AtomicMatchKey getCollationKey(
                net.sf.saxon.expr.sort.SimpleCollation namedCollation, string value) {
            // DotNetComparator c = (DotNetComparator) namedCollation.getComparator();
            // return c.getCollationKey(value);
            throw new NotImplementedException(); 
        }

        public net.sf.saxon.lib.StringCollator makeUcaCollator(string uri, net.sf.saxon.Configuration config) {
            return null;
        }

        public net.sf.saxon.regex.RegularExpression compileRegularExpression(net.sf.saxon.Configuration config,
            net.sf.saxon.str.UnicodeString regex, string flags, string hostLanguage, IList<string> warnings) {
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
                return new net.sf.saxon.regex.JavaRegularExpression(regex, flags);
            }
            else {
                if (useDotNet)
                    return new net.sf.saxon.regex.JavaRegularExpression(regex, flags);
                else
                    return new net.sf.saxon.regex.ARegularExpression(regex, flags, hostLanguage, warnings, config);
            }
        }

        public net.sf.saxon.type.ExternalObjectType getExternalObjectType(net.sf.saxon.Configuration config, string uri,
                    string localName) {
            // if (uri.Equals(net.sf.saxon.lib.NamespaceConstant.DOT_NET_TYPE))
            //     return new DotNetExternalObjectType(System.Type.GetType(localName), config);
            // else
                throw new java.lang.IllegalArgumentException("Type is not in .NET namespace");
        }

        public string getInstallationDirectory(string edition, net.sf.saxon.Configuration config) {
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

        public void registerAllBuiltInObjectModels(net.sf.saxon.Configuration config) {
        }

        public void setDefaultSAXParserFactory(net.sf.saxon.Configuration config) {
            // var editionCode = "he";
            // java.lang.System.setProperty("javax.xml.parsers.SAXParserFactory",
            //     "org.apache.xerces.jaxp.SAXParserFactoryImpl, saxon9" + editionCode + ", Version=" +
            //     net.sf.saxon.Version.getProductVersion() + ", Culture=neutral, PublicKeyToken=e1fdd002d5083fe6");
        }

        public bool JAXPStaticContextCheck(net.sf.saxon.expr.parser.RetainedStaticContext retainedStaticContext,
            net.sf.saxon.expr.StaticContext sc) {
            return false;
        }

        public net.sf.saxon.lib.ModuleURIResolver makeStandardModuleURIResolver(net.sf.saxon.Configuration config) {
            return new StandardModuleURIResolver(new StandardXmlResolver());
        }

        public IDynamicLoader getDefaultDynamicLoader() {
            return new DynamicLoader();
        }

        public Stream locateResource(string filename, IList<string> messages) {
            System.Reflection.Assembly ass = System.Reflection.Assembly.GetExecutingAssembly();
            // var resourceNames = ass.GetManifestResourceNames();
            // foreach (string name in resourceNames) {
            //     Console.WriteLine("Resource: " + name);
            // }
            // TODO: project name hard-coded here, but will change?
            return ass.GetManifestResourceStream(@"SaxonCS3.resources.net.sf.saxon.data." + filename.Replace('/', '.'));
        }

        public ActiveSource resolveSource(Source source, Configuration config) {
            if (source is ActiveSource activeSource) {
                return activeSource;
            }
            if (source is StreamSource) {
                
                Stream inputStream = ((StreamSource) source).getInputStream();
                string docTypeName = null;
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
                else if (((StreamSource) source).getReader() != null) {
                    xmlReader = XmlReader.Create(((StreamSource) source).getReader(), settings);
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
        public override object? GetEntity(Uri absoluteUri, string? role, Type? ofObjectToReturn) {
            Console.Error.WriteLine("GetEntity " + absoluteUri);
            return base.GetEntity(absoluteUri, role, ofObjectToReturn);
        }

        public override Uri ResolveUri(Uri? baseUri, string? relativeUri) {
            Console.Error.WriteLine("Resolve " + baseUri + " .. " + relativeUri);
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

