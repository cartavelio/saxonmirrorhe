using System;
using System.IO;
using javax.xml.transform;
using javax.xml.transform.stream;
using net.sf.saxon;

namespace Saxon.Api {
    /// <summary>
    /// internal class that wraps a (.NET) QueryResolver to create a (Java) SchemaURIResolver
    /// </summary>

    internal class DotNetSchemaURIResolver : net.sf.saxon.lib.SchemaURIResolver
    {

        internal SchemaResolver resolver;
        internal Configuration config;

        /// <summary>
        /// Initializes a new instance of the <see cref="Saxon.Api.DotNetSchemaURIResolver"/> class.
        /// </summary>
        /// <param name="resolver">Resolver.</param>
        public DotNetSchemaURIResolver(SchemaResolver resolver)
        {
            this.resolver = resolver;
        }

        public void setConfiguration(Configuration config)
        {
            this.config = config;
        }

        /// <summary>
        /// Resolve the specified targetNamespace, baseURI and locations.
        /// </summary>
        /// <param name="targetNamespace">Target namespace.</param>
        /// <param name="baseURI">BaseURI.</param>
        /// <param name="locations">Locations.</param>
        public Source[] resolve(String targetNamespace, String baseURI, String[] locations)
        {
            if (config.isSchemaAvailable(targetNamespace) && !(java.lang.Boolean.valueOf(((java.lang.Object)config.getConfigurationProperty(net.sf.saxon.lib.FeatureKeys.MULTIPLE_SCHEMA_IMPORTS)).toString()).booleanValue()))
            {
                return new Source[0];
            }
            Uri baseU = (baseURI == null ? null : new Uri(baseURI));
            Uri[] modules = resolver.GetSchemaDocuments(targetNamespace, baseU, locations);
            StreamSource[] ss = new StreamSource[modules.Length];
            for (int i = 0; i < ss.Length; i++)
            {
                ss[i] = new StreamSource();
                ss[i].setSystemId(modules[i].ToString());
                Object doc = resolver.GetEntity(modules[i]);
                if (doc is Stream)
                {
                    ss[i].setInputStream(new JDotNetInputStream((Stream)doc));
                }
                else if (doc is String)
                {
                    ss[i].setReader(new JDotNetReader(new StringReader((String)doc)));
                }
                else
                {
                    throw new ArgumentException("Invalid response from GetEntity()");
                }
            }
            return ss;
        }
    }
}