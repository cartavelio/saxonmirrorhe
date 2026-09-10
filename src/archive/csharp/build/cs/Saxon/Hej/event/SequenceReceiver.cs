using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public abstract class SequenceReceiver : Saxon.Hej.@event.Receiver {
        protected internal bool previousAtomic = false;
        protected internal Saxon.Hej.@event.PipelineConfiguration pipelineConfiguration;
        protected internal string systemId = null;
        public SequenceReceiver(Saxon.Hej.@event.PipelineConfiguration pipe)  {
            this.pipelineConfiguration = pipe;
        }
        public Saxon.Hej.@event.PipelineConfiguration getPipelineConfiguration() {
            return pipelineConfiguration;
        }
        public virtual void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration pipelineConfiguration) {
            this.pipelineConfiguration = pipelineConfiguration;
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return pipelineConfiguration.getConfiguration();
        }
        public virtual void setSystemId(string systemId) {
            this.systemId = systemId;
        }
        public string getSystemId() {
            return systemId;
        }
        public virtual void setUnparsedEntity(string name, string systemID, string publicID) {
        }
        public virtual void open() {
            previousAtomic = false;
        }
        public abstract void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int properties);
        public void append(Saxon.Hej.om.Item item) {
            append(item, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES);
        }
        public virtual Saxon.Hej.om.NamePool getNamePool() {
            return pipelineConfiguration.getConfiguration().getNamePool();
        }
        protected internal void flatten(Saxon.Hej.ma.arrays.ArrayItem array, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            foreach (Saxon.Hej.om.Sequence member in array.members()) {
                Saxon.Hej.om.SequenceTool.supply(member.iterate(), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((it => append(it, locationId, copyNamespaces))));
            }
        }
        protected internal void decompose(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            if ((item != null)) {
                switch (item.getGenre()) {
                  case Saxon.Hej.om.Genre.ATOMIC:
                  case Saxon.Hej.om.Genre.EXTERNAL:
                    if (previousAtomic) {
                        characters(Saxon.Hej.str.StringConstants.SINGLE_SPACE, locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                    }
                    characters(item.getUnicodeStringValue(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                    previousAtomic = true;
                    break ;
                  case Saxon.Hej.om.Genre.ARRAY:
                    flatten((Saxon.Hej.ma.arrays.ArrayItem)(item), locationId, copyNamespaces);
                    break ;
                  case Saxon.Hej.om.Genre.MAP:
                  case Saxon.Hej.om.Genre.FUNCTION:
                    string thing = ((item is Saxon.Hej.ma.map.MapItem) ? "map" : "function item");
                    string errorCode = getErrorCodeForDecomposingFunctionItems();
                    if (errorCode.StartsWith("SENR")) {
                        throw new Saxon.Hej.trans.XPathException((("Cannot serialize a "+thing)+" using this output method"), errorCode, locationId);
                    } else {
                        throw new Saxon.Hej.trans.XPathException((("Cannot add a "+thing)+" to an XDM node tree"), errorCode, locationId);
                    }
                  case Saxon.Hej.om.Genre.NODE:
                  default:
                    Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
                    int kind = node.getNodeKind();
                    if (((node is Saxon.Hej.tree.util.Orphan) && ((Saxon.Hej.tree.util.Orphan)(node)).isDisableOutputEscaping())) {
                        characters(item.getUnicodeStringValue(), locationId, Saxon.Hej.@event.ReceiverOption.DISABLE_ESCAPING);
                        previousAtomic = false;
                    } else {
                        if ((kind == Saxon.Hej.type.Type.DOCUMENT)) {
                            startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
                            foreach (Saxon.Hej.om.NodeInfo child in node.children()) {
                                append(child, locationId, copyNamespaces);
                            }
                            previousAtomic = false;
                            endDocument();
                        } else {
                            if (((kind == Saxon.Hej.type.Type.ATTRIBUTE) || (kind == Saxon.Hej.type.Type.NAMESPACE))) {
                                string description = ((kind == Saxon.Hej.type.Type.ATTRIBUTE) ? "attribute" : "namespace");
                                throw new Saxon.Hej.trans.XPathException((((("Sequence normalization: Cannot process free-standing "+description)+" node (")+node.getDisplayName())+")"), "SENR0001", locationId);
                            } else {
                                int copyOptions = Saxon.Hej.om.CopyOptions.TYPE_ANNOTATIONS;
                                if (Saxon.Hej.@event.ReceiverOption.contains(copyNamespaces, Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES)) {
                                    copyOptions |= Saxon.Hej.om.CopyOptions.ALL_NAMESPACES;
                                }
                                ((Saxon.Hej.om.NodeInfo)(item)).copy(this, copyOptions, locationId);
                                previousAtomic = false;
                            }
                        }
                    }
                    break ;
                }
            }
        }
        protected internal virtual string getErrorCodeForDecomposingFunctionItems() {
            return (getPipelineConfiguration().isXSLT() ? "XTDE0450" : "XQTY0105");
        }
        public bool handlesAppend() {
            return true;
        }
    
        /* Expanding default method usesTypeAnnotations from interface Receiver */
        public virtual bool usesTypeAnnotations() {
            return false;
        }

        /* Creating stub method startDocument to satisfy interface Receiver */
        public virtual void startDocument(int arg1) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method endDocument to satisfy interface Receiver */
        public virtual void endDocument() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method startElement to satisfy interface Receiver */
        public virtual void startElement(Saxon.Hej.om.NodeName arg1, Saxon.Hej.type.SchemaType arg2, Saxon.Hej.om.AttributeMap arg3, Saxon.Hej.om.NamespaceMap arg4, Saxon.Hej.s9api.Location arg5, int arg6) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method endElement to satisfy interface Receiver */
        public virtual void endElement() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method characters to satisfy interface Receiver */
        public virtual void characters(Saxon.Hej.str.UnicodeString arg1, Saxon.Hej.s9api.Location arg2, int arg3) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method processingInstruction to satisfy interface Receiver */
        public virtual void processingInstruction(string arg1, Saxon.Hej.str.UnicodeString arg2, Saxon.Hej.s9api.Location arg3, int arg4) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method comment to satisfy interface Receiver */
        public virtual void comment(Saxon.Hej.str.UnicodeString arg1, Saxon.Hej.s9api.Location arg2, int arg3) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method close to satisfy interface Receiver */
        public virtual void close() {
            throw new System.NotImplementedException();
        }

    }
}