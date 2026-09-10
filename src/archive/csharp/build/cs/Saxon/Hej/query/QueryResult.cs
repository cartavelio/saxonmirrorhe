using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.query {
    public class QueryResult {
        public static string RESULT_NS = "http://saxon.sf.net/xquery-results";
        private QueryResult()  {
        }
        public static string serialize(Saxon.Hej.om.NodeInfo nodeInfo) {
            System.IO.StringWriter sw = new System.IO.StringWriter();
            Saxon.Ejava.util.Properties props = new Saxon.Ejava.util.Properties();
            props.setProperty("method", "xml");
            props.setProperty("indent", "yes");
            props.setProperty("omit-xml-declaration", "yes");
            serialize(nodeInfo, new Saxon.Ejavax.xml.transform.stream.StreamResult(sw), props);
            return sw.ToString();
        }
        public static Saxon.Hej.om.NodeInfo wrap(Saxon.Hej.om.SequenceIterator iterator, Saxon.Hej.Configuration config) {
            Saxon.Hej.@event.PipelineConfiguration pipe = config.makePipelineConfiguration();
            Saxon.Hej.tree.tiny.TinyBuilder builder = new Saxon.Hej.tree.tiny.TinyBuilder(pipe);
            builder.setStatistics(config.getTreeStatistics().SOURCE_DOCUMENT_STATISTICS);
            sendWrappedSequence(iterator, builder);
            return builder.getCurrentRoot();
        }
        public static void sendWrappedSequence(Saxon.Hej.om.SequenceIterator iterator, Saxon.Hej.@event.Receiver destination) {
            Saxon.Hej.lib.SerializerFactory sf = destination.getPipelineConfiguration().getConfiguration().getSerializerFactory();
            Saxon.Hej.@event.SequenceCopier.copySequence(iterator, sf.newSequenceWrapper(destination));
        }
        public static void serialize(Saxon.Hej.om.NodeInfo node, Saxon.Ejavax.xml.transform.Result destination, Saxon.Ejava.util.Properties outputProperties) {
            Saxon.Hej.Configuration config = node.getConfiguration();
            serializeSequence(Saxon.Hej.tree.iter.SingletonIterator.makeIterator(node), config, destination, outputProperties);
        }
        public static void serialize(Saxon.Hej.om.NodeInfo node, Saxon.Ejavax.xml.transform.Result destination, Saxon.Hej.serialize.SerializationProperties properties) {
            Saxon.Hej.Configuration config = node.getConfiguration();
            serializeSequence(Saxon.Hej.tree.iter.SingletonIterator.makeIterator(node), config, destination, properties);
        }
        public static void serializeSequence(Saxon.Hej.om.SequenceIterator iterator, Saxon.Hej.Configuration config, System.IO.Stream destination, Saxon.Ejava.util.Properties outputProps) {
            serializeSequence(iterator, config, new Saxon.Ejavax.xml.transform.stream.StreamResult(destination), outputProps);
            try {
                destination.Flush();
            } catch (System.IO.IOException err) {
                throw new Saxon.Hej.trans.XPathException(err);
            }
        }
        public static void serializeSequence(Saxon.Hej.om.SequenceIterator iterator, Saxon.Hej.Configuration config, System.IO.TextWriter writer, Saxon.Ejava.util.Properties outputProps) {
            serializeSequence(iterator, config, new Saxon.Ejavax.xml.transform.stream.StreamResult(writer), outputProps);
            try {
                writer.Flush();
            } catch (System.IO.IOException err) {
                throw new Saxon.Hej.trans.XPathException(err);
            }
        }
        public static void serializeSequence(Saxon.Hej.om.SequenceIterator iterator, Saxon.Hej.Configuration config, Saxon.Ejavax.xml.transform.Result result, Saxon.Ejava.util.Properties outputProperties) {
            Saxon.Hej.lib.SerializerFactory sf = config.getSerializerFactory();
            Saxon.Hej.@event.Receiver tr = sf.getReceiver(result, new Saxon.Hej.serialize.SerializationProperties(outputProperties));
            Saxon.Hej.@event.SequenceCopier.copySequence(iterator, tr);
        }
        public static void serializeSequence(Saxon.Hej.om.SequenceIterator iterator, Saxon.Hej.Configuration config, Saxon.Ejavax.xml.transform.Result result, Saxon.Hej.serialize.SerializationProperties properties) {
            Saxon.Hej.lib.SerializerFactory sf = config.getSerializerFactory();
            Saxon.Hej.@event.Receiver tr = sf.getReceiver(result, properties);
            Saxon.Hej.@event.SequenceCopier.copySequence(iterator, tr);
        }
        public static void rewriteToDisk(Saxon.Hej.om.NodeInfo doc, Saxon.Ejava.util.Properties outputProperties, bool backup, Saxon.Ejava.io.PrintStream log) {
            switch (doc.getNodeKind()) {
              case Saxon.Hej.type.Type.DOCUMENT:
                break ;
              case Saxon.Hej.type.Type.ELEMENT:
                Saxon.Hej.om.NodeInfo parent = doc.getParent();
                if (((parent != null) && (parent.getNodeKind() != Saxon.Hej.type.Type.DOCUMENT))) {
                    throw new Saxon.Hej.trans.XPathException("Cannot rewrite an element node unless it is top-level");
                }
                break ;
              default:
                throw new Saxon.Hej.trans.XPathException("Node to be rewritten must be a document or element node");
            }
            string uri = doc.getSystemId();
            if (((uri == null) || uri.Length==0)) {
                throw new Saxon.Hej.trans.XPathException("Cannot rewrite a document with no known URI");
            }
            System.Uri u;
            try {
                u = new System.Uri(uri, System.UriKind.RelativeOrAbsolute);
            } catch (System.UriFormatException e) {
                throw new Saxon.Hej.trans.XPathException(("SystemId of updated document is not a valid URI: "+uri));
            }
            Saxon.Ejava.io.File existingFile = new Saxon.Ejava.io.File(u);
            Saxon.Ejava.io.File dir = existingFile.getParentFile();
            if ((backup && existingFile.exists())) {
                Saxon.Ejava.io.File backupFile = new Saxon.Ejava.io.File(dir, (existingFile.getName()+".bak"));
                if ((log != null)) {
                    log.Println(("Creating backup file "+backupFile));
                }
                bool success = existingFile.renameTo(backupFile);
                if (!(success)) {
                    throw new Saxon.Hej.trans.XPathException(("Failed to create backup file of "+backupFile));
                }
            }
            if (!(existingFile.exists())) {
                if ((log != null)) {
                    log.Println(("Creating file "+existingFile));
                }
                try {
                    existingFile.createNewFile();
                } catch (System.IO.IOException e) {
                    throw new Saxon.Hej.trans.XPathException(("Failed to create new file "+existingFile));
                }
            } else {
                if ((log != null)) {
                    log.Println(("Overwriting file "+existingFile));
                }
            }
            Saxon.Hej.Configuration config = doc.getConfiguration();
            Saxon.Hej.lib.SerializerFactory factory = config.getSerializerFactory();
            Saxon.Hej.@event.Receiver r = factory.getReceiver(new Saxon.Ejavax.xml.transform.stream.StreamResult(existingFile), new Saxon.Hej.serialize.SerializationProperties(outputProperties));
            doc.copy(r, Saxon.Hej.om.CopyOptions.ALL_NAMESPACES, Saxon.Hej.expr.parser.Loc.NONE);
            r.close();
        }
    
    }
}