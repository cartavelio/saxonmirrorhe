using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej {
    public interface Platform {
        void initialize(Saxon.Hej.Configuration config);
        bool isJava();
        bool isDotNet();
        string getPlatformVersion();
        string getPlatformSuffix();
        Saxon.Hej.lib.IDynamicLoader getDefaultDynamicLoader();
        string getDefaultLanguage();
        string getDefaultCountry();
        System.IO.Stream locateResource(string filename, System.Collections.Generic.IList<string> messages);
        void showEmbeddedResources();
        Saxon.Hej.lib.ActiveSource resolveSource(Saxon.Ejavax.xml.transform.Source source, Saxon.Hej.Configuration config);
        Saxon.Hej.lib.StringCollator makeCollation(Saxon.Hej.Configuration config, Saxon.Ejava.util.Properties props, string uri);
        bool canReturnCollationKeys(Saxon.Hej.lib.StringCollator collation);
        Saxon.Hej.expr.sort.AtomicMatchKey getCollationKey(Saxon.Hej.expr.sort.SimpleCollation namedCollation, string value);
        bool hasICUCollator();
        bool hasICUNumberer();
        Saxon.Hej.lib.StringCollator makeUcaCollator(string uri, Saxon.Hej.Configuration config);
        Saxon.Hej.regex.RegularExpression compileRegularExpression(Saxon.Hej.Configuration config, Saxon.Hej.str.UnicodeString regex, string flags, string hostLanguage, System.Collections.Generic.IList<string> warnings);
        Saxon.Hej.type.ExternalObjectType getExternalObjectType(Saxon.Hej.Configuration config, string uri, string localName);
        string getInstallationDirectory(string edition, Saxon.Hej.Configuration config);
        void registerAllBuiltInObjectModels(Saxon.Hej.Configuration config);
        void setDefaultSAXParserFactory(Saxon.Hej.Configuration config);
        bool JAXPStaticContextCheck(Saxon.Hej.expr.parser.RetainedStaticContext retainedStaticContext, Saxon.Hej.expr.StaticContext sc);
        Saxon.Hej.lib.ModuleURIResolver makeStandardModuleURIResolver(Saxon.Hej.Configuration config);
    
    }
}