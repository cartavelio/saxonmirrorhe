using System;
using System.IO;
using System.Xml;
using Saxon.Ejava.lang;
using Saxon.Ejava.net;
using Saxon.Ejavax.xml.transform;
using Saxon.Ejavax.xml.transform.stream;
using File = Saxon.Ejava.io.File;

namespace Saxon.Api {
    /// <summary>
    /// An <c>XsdCompiler</c> object allows XSD schema documents to be compiled.
    /// The compiler holds information options controlling how the schema is compiled.
    /// </summary>
    /// <remarks>
    /// <para>To construct an <c>XsdCompiler</c>, use the factory method
    /// <c>NewXsdCompiler</c> on the <see cref="Processor"/> object.</para>
    /// <para>An <c>XsdCompiler</c> may be used repeatedly to compile multiple
    /// schemas. Any changes made to the <c>XsdCompiler</c> 
    /// do not affect schemas that have already been compiled.
    /// An <c>XsdCompiler</c> may be used concurrently in multiple threads, but
    /// it should not then be modified once initialized.</para>
    /// <para>The abstract <c>XsdCompiler</c> class is included in all Saxon editions,
    /// but the concrete subclass that does real schema processing is available
    /// only in Saxon-EE.</para>
    /// </remarks>
    
    public abstract class XsdCompiler {
        /// <summary>
        /// The version of the W3C XML Schema Specification handled by this <c>XsdCompiler</c>.
        /// </summary>
        /// <remarks>
        /// <para>The value must be "1.0" (indicating XML Schema 1.0) or "1.1" (indicating XML Schema 1.1).
        /// The default is "1.0". New constructs defined in XSD 1.1 are rejected unless this property
        /// is set to "1.1" before compiling the schema.
        /// </para>
        /// </remarks>
        /// 
        public abstract string XsdVersion { get; set; }

        /// <summary>The <c>ErrorReporter</c> to be used when compiling schema documents.</summary>

        public abstract ErrorReporter ErrorReporter { get; set; }

        public abstract SchemaResolver SchemaResolver { set; }

        ///<summary>
        /// Load a schema document from a given SchemaSource, or from a set of SchemaSources.
        /// The schema components are  returned
        /// by the method as an <c>XsdSchema</c> object.
        ///</summary>
        public abstract XsdSchema Compile(params SchemaSource[] sources);
        
        /// <summary>
        /// Compile a schema supplied as a <c>Stream</c>. The resulting schema components are added
        /// to the cache. The base URI of the schema document is taken as the current working directory.
        /// </summary>
        /// <param name="input">A stream containing the source text of the schema. This method
        /// will consume the supplied stream. It is the caller's responsibility to close the stream
        /// after use.</param>
        /// <returns>The schema constructed from this input document.</returns>

        public XsdSchema Compile(Stream input) {
            return Compile (input, new Uri(Environment.CurrentDirectory));
        }

        /// <summary>
        /// Compile a schema supplied as a <c>Stream</c>. The resulting schema components are added
        /// to the cache.
        /// </summary>
        /// <param name="input">A stream containing the source text of the schema. This method
        /// will consume the supplied stream. It is the caller's responsibility to close the stream
        /// after use.</param>
        /// <param name="baseUri">The base URI of the schema document, for resolving any references to other
        /// schema documents</param>
        /// <returns>The schema constructed from this input document.</returns> 
        public XsdSchema Compile(System.IO.Stream input, System.Uri baseUri) {
            return Compile(new SchemaSource(input, baseUri));
        }
        
        /// <summary>
        /// Compile a schema supplied as a <c>TextReader</c>. The resulting schema components are added
        /// to the cache.
        /// </summary>
        /// <param name="input">A <c>TextReader</c> that supplies the source text of the schema. This method
        /// will consume the supplied stream. It is the caller's responsibility to close the reader
        /// after use.</param>
        /// <param name="baseUri">The base URI of the schema document, for resolving any references to other
        /// schema documents</param>
        /// <returns>The schema constructed from this input document.</returns>
        public XsdSchema Compile(System.IO.TextReader input, System.Uri baseUri) {
            return Compile(new SchemaSource(input, baseUri));
        }
        
        /// <summary>
        /// Compile a schema supplied as a <c>TextReader</c>. The base URI of the schema document is
        /// taken as the current working directory.
        /// </summary>
        /// <param name="reader">A <c>TextReader</c> that supplies the source text of the schema. This method
        /// will consume the supplied stream. It is the caller's responsibility to close the reader
        /// after use.</param>
        /// <returns>The schema constructed from this input document.</returns>
        public XsdSchema Compile(System.IO.TextReader reader) {
            return Compile(new SchemaSource(reader, new Uri(Environment.CurrentDirectory)));
        }
        
        /// <summary>
        /// Compile a schema, retrieving the source using a URI. 
        /// </summary>
        /// <remarks>
        /// The document located via the URI is parsed using the <c>System.Xml</c> parser.
        /// </remarks>
        /// <param name="uri">The URI identifying the location where the schema document can be
        /// found</param>
        /// <returns>The schema constructed from this input document.</returns>
        /// 
        public XsdSchema Compile(System.Uri uri) {
            return Compile(new SchemaSource(uri));
        }
        
        /// <summary>
        /// Compile a schema, retrieving the source from a given file. 
        /// </summary>
        /// <remarks>
        /// The content of the file is parsed using the <c>System.Xml</c> parser.
        /// </remarks>
        /// <param name="file">The file where the schema document can be
        /// found</param>
        /// <returns>The schema constructed from this input document.</returns>
        /// 
        public XsdSchema Compile(System.IO.FileInfo file) {
            return Compile(new SchemaSource(file));
        }
        
        
        
        /// <summary>
        /// Compile a schema, retrieving the source from a supplied XML reader
        /// (that is, an XML parser). The base URI of the document is taken
        /// from the <c>BaseUri</c> property of the <c>XmlReader</c>.
        /// </summary>
        /// <param name="reader">The XML reader supplying
        /// the schema document</param>
        /// <returns>The schema constructed from this input document.</returns>
        /// 
        public XsdSchema Compile(XmlReader reader) {
            return Compile(new SchemaSource(reader));
        }
        
        /// <summary>
        /// Compile a schema, retrieving the source from a supplied XDM node.
        /// This may be a document node whose
        /// child is an <c>xs:schema</c> element, or it may be
        /// the <c>xs:schema</c> element itself. 
        /// The base URI of the document is taken
        /// from the <c>BaseUri</c> property of the <c>XdmNode</c>.
        /// </summary>
        /// <param name="node">The XML reader supplying
        /// the schema document</param>
        /// <returns>The schema constructed from this input document.</returns>
        /// 
        public XsdSchema Compile(XdmNode node) {
            return Compile(new SchemaSource(node));
        }

        /// <summary>
        /// Combine a set of schemas into one
        /// </summary>
        /// <param name="schemas">The schemas to be combined</param>
        /// <returns>The combined schema</returns>
        public abstract XsdSchema Combine (params XsdSchema[] schemas); 
            
        /// <summary>
        /// Return an empty schema. This may be useful for validating instance documents when all
        /// the actual schema definitions are linked from the instance document using {@code xsi:schemaLocation}
        /// and {@code xsi:noNamespaceSchemaLocation} attributes
        /// </summary>
        /// <returns>An empty schema</returns>

        public abstract XsdSchema EmptySchema();

        /// <summary>
        /// Import a precompiled Schema Component Model from a given Source. The schema components derived from this schema
        /// document are added to the cache of schema components maintained by this XsdCompiler
        /// </summary>
        /// <param name="source">The XML file containing the schema component model, as generated by a previous call on
        ///  <c>XsdSchema#exportComponents</c>
        /// </param>
        /// <returns>The schema produced by processing the supplied schema component model export file</returns>

        public abstract XsdSchema ImportComponents(SchemaSource source);
        
        /// <summary>
        /// Embedded class defining the source for a particular schema document.
        /// </summary>
        public class SchemaSource {
            
            readonly Source jSource;

            /// <summary>
            /// A schema document supplied in the form of a stream, together with a Uri that defines
            /// the base URI for resolving any <c>xs:include</c> or <c>xs:import</c> declarations.
            /// </summary>
            /// <param name="stream">The input stream</param>
            /// <param name="uri">The base URI</param>
            public SchemaSource(System.IO.Stream stream, System.Uri uri) {
                jSource = new StreamSource(stream, uri.AbsoluteUri);
            }
            
            /// <summary>
            /// A schema document supplied in the form of a text reader, together with a Uri that defines
            /// the base URI for resolving any <c>xs:include</c> or <c>xs:import</c> declarations.
            /// </summary>
            /// <param name="reader">The text reader</param>
            /// <param name="uri">The base URI</param>
            public SchemaSource(System.IO.TextReader reader, System.Uri uri) {
                jSource = new StreamSource(reader, uri.AbsoluteUri);
            }

            /// <summary>
            /// A schema document supplied in the form of a Uri, which is dereferenced directly
            /// (without using any resolver)
            /// </summary>
            /// <param name="uri">The base URI</param>
            public SchemaSource(System.Uri uri) {
                jSource = new StreamSource(uri.AbsoluteUri);
            }

            /// <summary>
            /// A schema document supplied in the form of a <c>FileInfo</c>
            /// </summary>

            public SchemaSource(System.IO.FileInfo file) {
                jSource = new StreamSource(file.OpenRead(), new System.Uri(file.FullName).AbsoluteUri);
            }
            
            /// <summary>
            /// A schema document supplied in the form of an <c>XdmNode</c>
            /// </summary>

            public SchemaSource(XdmNode node) {
                jSource = node.Implementation.asActiveSource();
            }
            
            /// <summary>
            /// A schema document supplied in the form of an <c>XmlReader</c>
            /// </summary>

            public SchemaSource(XmlReader parser) {
                throw new UnsupportedOperationException(); // TODO
                //jSource = node.Implementation.asActiveSource();
            }

            internal Source getSource() {
                return jSource;
            }
        }
    }
}