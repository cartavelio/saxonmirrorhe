using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class PushToReceiver : Saxon.Hej.s9api.push.Push {
        private readonly Saxon.Hej.@event.ComplexContentOutputter cco;
        private readonly Saxon.Hej.Configuration config;
        public PushToReceiver(Saxon.Hej.@event.Receiver cco)  {
            this.cco = new Saxon.Hej.@event.ComplexContentOutputter(new Saxon.Hej.@event.RegularSequenceChecker(cco, false));
            config = cco.getPipelineConfiguration().getConfiguration();
        }
        public Saxon.Hej.s9api.push.Document document(bool wellFormed) {
            try {
                cco.open();
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
            return new Saxon.Hej.@event.PushToReceiver.DocImpl(cco, wellFormed);
        }
        private abstract class ContainerImpl : Saxon.Hej.s9api.push.Container {
            private Saxon.Hej.@event.ComplexContentOutputter cco;
            private string defaultNamespace;
            private Saxon.Hej.@event.PushToReceiver.ElemImpl elementAwaitingClosure;
            private bool closed;
            public ContainerImpl(Saxon.Hej.@event.ComplexContentOutputter cco, string defaultNamespace)  {
                this.defaultNamespace = defaultNamespace;
                this.cco = cco;
            }
            public void setDefaultNamespace(string uri) {
                this.defaultNamespace = uri;
            }
            public virtual Saxon.Hej.s9api.push.Element element(Saxon.Api.QName name) {
                try {
                    implicitClose();
                    Saxon.Hej.om.FingerprintedQName fp = new Saxon.Hej.om.FingerprintedQName(name.GetStructuredQName(), getConfiguration().getNamePool());
                    getOutputter().startElement(fp, Saxon.Hej.type.Untyped.getInstance(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Api.SaxonApiException(e);
                }
                return elementAwaitingClosure = new Saxon.Hej.@event.PushToReceiver.ElemImpl(getOutputter(), defaultNamespace);
            }
            public virtual Saxon.Hej.s9api.push.Element element(string name) {
                try {
                    implicitClose();
                    Saxon.Hej.om.NodeName fp = (defaultNamespace.Length==0 ? new Saxon.Hej.om.NoNamespaceName(name) : new Saxon.Hej.om.FingerprintedQName("", Saxon.Hej.om.NamespaceUri.of(defaultNamespace), name));
                    cco.startElement(fp, Saxon.Hej.type.Untyped.getInstance(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Api.SaxonApiException(e);
                }
                return elementAwaitingClosure = new Saxon.Hej.@event.PushToReceiver.ElemImpl(getOutputter(), defaultNamespace);
            }
            public virtual Saxon.Hej.s9api.push.Container text(string value) {
                try {
                    implicitClose();
                    if (((value != null) && (value.Length>0))) {
                        getOutputter().characters(Saxon.Hej.str.StringView.of(value.ToString()), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                    }
                    return this;
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Api.SaxonApiException(e);
                }
            }
            public virtual Saxon.Hej.s9api.push.Container comment(string value) {
                try {
                    implicitClose();
                    if ((value != null)) {
                        getOutputter().comment(Saxon.Hej.str.StringView.of(value.ToString()), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                    }
                    return this;
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Api.SaxonApiException(e);
                }
            }
            public virtual Saxon.Hej.s9api.push.Container processingInstruction(string name, string value) {
                try {
                    implicitClose();
                    if ((value != null)) {
                        getOutputter().processingInstruction(name, Saxon.Hej.str.StringView.of(value.ToString()), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                    }
                    return this;
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Api.SaxonApiException(e);
                }
            }
            public void close() {
                if (!(closed)) {
                    implicitClose();
                    sendEndEvent();
                    closed = true;
                }
            }
            private void implicitClose() {
                if (closed) {
                    throw new Saxon.Api.SaxonApiException("The container has been closed");
                }
                if ((elementAwaitingClosure != null)) {
                    elementAwaitingClosure.close();
                    elementAwaitingClosure = null;
                }
            }
            abstract protected internal void sendEndEvent();
            protected internal Saxon.Hej.@event.ComplexContentOutputter getOutputter() {
                return cco;
            }
            protected internal Saxon.Hej.Configuration getConfiguration() {
                return cco.getConfiguration();
            }
        
        }
        private class DocImpl : Saxon.Hej.@event.PushToReceiver.ContainerImpl, Saxon.Hej.s9api.push.Document {
            private readonly bool wellFormed;
            private bool foundElement = false;
            internal DocImpl(Saxon.Hej.@event.ComplexContentOutputter cco, bool wellFormed) : base (cco, "") {
                try {
                    this.wellFormed = wellFormed;
                    cco.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Api.SaxonApiException(e);
                }
            }
            public override Saxon.Hej.s9api.push.Element element(Saxon.Api.QName name) {
                if ((wellFormed && foundElement)) {
                    throw new Saxon.Api.SaxonApiException("A well-formed document cannot have more than one element child");
                }
                foundElement = true;
                return base.element(name);
            }
            public override Saxon.Hej.s9api.push.Element element(string name) {
                if ((wellFormed && foundElement)) {
                    throw new Saxon.Api.SaxonApiException("A well-formed document cannot have more than one element child");
                }
                foundElement = true;
                return base.element(name);
            }
            public override Saxon.Hej.@event.PushToReceiver.DocImpl text(string value) {
                if (((wellFormed && (value != null)) && (value.Length>0))) {
                    throw new Saxon.Api.SaxonApiException("A well-formed document cannot contain text outside any element");
                }
                return (Saxon.Hej.@event.PushToReceiver.DocImpl)(base.text(value));
            }
            public override Saxon.Hej.s9api.push.Document comment(string value) {
                return (Saxon.Hej.s9api.push.Document)(base.comment(value));
            }
            public override Saxon.Hej.s9api.push.Document processingInstruction(string name, string value) {
                return (Saxon.Hej.s9api.push.Document)(base.processingInstruction(name, value));
            }
            protected internal override void sendEndEvent() {
                try {
                    if ((wellFormed && !(foundElement))) {
                        throw new Saxon.Api.SaxonApiException("A well-formed document must contain an element node");
                    }
                    getOutputter().endDocument();
                    getOutputter().close();
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Api.SaxonApiException(e);
                }
            }
        
        }
        private class ElemImpl : Saxon.Hej.@event.PushToReceiver.ContainerImpl, Saxon.Hej.s9api.push.Element {
            private bool foundChild;
            internal ElemImpl(Saxon.Hej.@event.ComplexContentOutputter cco, string defaultNamespace) : base (cco, defaultNamespace) {
            }
            public Saxon.Hej.s9api.push.Element attribute(Saxon.Api.QName name, string value) {
                checkChildNotFound();
                try {
                    if ((value != null)) {
                        Saxon.Hej.om.FingerprintedQName fp = new Saxon.Hej.om.FingerprintedQName(name.GetStructuredQName(), getConfiguration().getNamePool());
                        getOutputter().attribute(fp, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, value, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                    }
                    return this;
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Api.SaxonApiException(e);
                }
            }
            public Saxon.Hej.s9api.push.Element attribute(string name, string value) {
                checkChildNotFound();
                try {
                    if ((value != null)) {
                        Saxon.Hej.om.NodeName fp = new Saxon.Hej.om.NoNamespaceName(name);
                        getOutputter().attribute(fp, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, value, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                    }
                    return this;
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Api.SaxonApiException(e);
                }
            }
            public Saxon.Hej.s9api.push.Element @namespace(string prefix, string uri) {
                checkChildNotFound();
                try {
                    getOutputter().@namespace(prefix, Saxon.Hej.om.NamespaceUri.of(uri), Saxon.Hej.@event.ReceiverOption.NONE);
                    return this;
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Api.SaxonApiException(e);
                }
            }
            private void checkChildNotFound() {
                if (foundChild) {
                    throw new Saxon.Api.SaxonApiException("Attribute nodes must be attached to an element before any children");
                }
            }
            public override Saxon.Hej.s9api.push.Element element(Saxon.Api.QName name) {
                foundChild = true;
                return base.element(name);
            }
            public override Saxon.Hej.s9api.push.Element element(string name) {
                foundChild = true;
                return base.element(name);
            }
            public override Saxon.Hej.s9api.push.Element text(string value) {
                foundChild = true;
                return (Saxon.Hej.s9api.push.Element)(base.text(value));
            }
            public override Saxon.Hej.s9api.push.Element comment(string value) {
                foundChild = true;
                return (Saxon.Hej.s9api.push.Element)(base.comment(value));
            }
            public override Saxon.Hej.s9api.push.Element processingInstruction(string name, string value) {
                foundChild = true;
                return (Saxon.Hej.s9api.push.Element)(base.processingInstruction(name, value));
            }
            protected internal override void sendEndEvent() {
                try {
                    getOutputter().endElement();
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Api.SaxonApiException(e);
                }
            }
        
        }
    
    }
}