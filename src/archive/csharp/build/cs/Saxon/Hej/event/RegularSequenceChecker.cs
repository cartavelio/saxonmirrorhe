using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class RegularSequenceChecker : Saxon.Hej.@event.ProxyReceiver {

        static RegularSequenceChecker() {
            edge(Saxon.Hej.@event.RegularSequenceChecker.State.INITIAL, Saxon.Hej.@event.RegularSequenceChecker.Transition.OPEN, Saxon.Hej.@event.RegularSequenceChecker.State.OPEN);
            edge(Saxon.Hej.@event.RegularSequenceChecker.State.OPEN, Saxon.Hej.@event.RegularSequenceChecker.Transition.APPEND, Saxon.Hej.@event.RegularSequenceChecker.State.OPEN);
            edge(Saxon.Hej.@event.RegularSequenceChecker.State.OPEN, Saxon.Hej.@event.RegularSequenceChecker.Transition.TEXT, Saxon.Hej.@event.RegularSequenceChecker.State.OPEN);
            edge(Saxon.Hej.@event.RegularSequenceChecker.State.OPEN, Saxon.Hej.@event.RegularSequenceChecker.Transition.COMMENT, Saxon.Hej.@event.RegularSequenceChecker.State.OPEN);
            edge(Saxon.Hej.@event.RegularSequenceChecker.State.OPEN, Saxon.Hej.@event.RegularSequenceChecker.Transition.PI, Saxon.Hej.@event.RegularSequenceChecker.State.OPEN);
            edge(Saxon.Hej.@event.RegularSequenceChecker.State.OPEN, Saxon.Hej.@event.RegularSequenceChecker.Transition.START_DOCUMENT, Saxon.Hej.@event.RegularSequenceChecker.State.CONTENT);
            edge(Saxon.Hej.@event.RegularSequenceChecker.State.OPEN, Saxon.Hej.@event.RegularSequenceChecker.Transition.START_ELEMENT, Saxon.Hej.@event.RegularSequenceChecker.State.CONTENT);
            edge(Saxon.Hej.@event.RegularSequenceChecker.State.CONTENT, Saxon.Hej.@event.RegularSequenceChecker.Transition.TEXT, Saxon.Hej.@event.RegularSequenceChecker.State.CONTENT);
            edge(Saxon.Hej.@event.RegularSequenceChecker.State.CONTENT, Saxon.Hej.@event.RegularSequenceChecker.Transition.COMMENT, Saxon.Hej.@event.RegularSequenceChecker.State.CONTENT);
            edge(Saxon.Hej.@event.RegularSequenceChecker.State.CONTENT, Saxon.Hej.@event.RegularSequenceChecker.Transition.PI, Saxon.Hej.@event.RegularSequenceChecker.State.CONTENT);
            edge(Saxon.Hej.@event.RegularSequenceChecker.State.CONTENT, Saxon.Hej.@event.RegularSequenceChecker.Transition.START_ELEMENT, Saxon.Hej.@event.RegularSequenceChecker.State.CONTENT);
            edge(Saxon.Hej.@event.RegularSequenceChecker.State.CONTENT, Saxon.Hej.@event.RegularSequenceChecker.Transition.END_ELEMENT, Saxon.Hej.@event.RegularSequenceChecker.State.CONTENT);
            edge(Saxon.Hej.@event.RegularSequenceChecker.State.CONTENT, Saxon.Hej.@event.RegularSequenceChecker.Transition.END_DOCUMENT, Saxon.Hej.@event.RegularSequenceChecker.State.OPEN);
            edge(Saxon.Hej.@event.RegularSequenceChecker.State.OPEN, Saxon.Hej.@event.RegularSequenceChecker.Transition.CLOSE, Saxon.Hej.@event.RegularSequenceChecker.State.FINAL);
            edge(Saxon.Hej.@event.RegularSequenceChecker.State.FAILED, Saxon.Hej.@event.RegularSequenceChecker.Transition.CLOSE, Saxon.Hej.@event.RegularSequenceChecker.State.FAILED);
        }
        private readonly System.Collections.Generic.Stack<System.Int16> stack = new System.Collections.Generic.Stack<System.Int16>();
        public enum State {
            INITIAL,
            OPEN,
            START_TAG,
            CONTENT,
            FINAL,
            FAILED,
        }
        private enum Transition {
            OPEN,
            APPEND,
            TEXT,
            COMMENT,
            PI,
            START_DOCUMENT,
            START_ELEMENT,
            END_ELEMENT,
            END_DOCUMENT,
            CLOSE,
        }
        private Saxon.Hej.@event.RegularSequenceChecker.State state;
        private bool fullChecking = false;
        private static readonly System.Collections.Generic.IDictionary<Saxon.Hej.@event.RegularSequenceChecker.State,System.Collections.Generic.IDictionary<Saxon.Hej.@event.RegularSequenceChecker.Transition,Saxon.Hej.@event.RegularSequenceChecker.State>> machine = new System.Collections.Generic.Dictionary<Saxon.Hej.@event.RegularSequenceChecker.State, System.Collections.Generic.IDictionary<Saxon.Hej.@event.RegularSequenceChecker.Transition, Saxon.Hej.@event.RegularSequenceChecker.State>>();
        private static void edge(Saxon.Hej.@event.RegularSequenceChecker.State from, Saxon.Hej.@event.RegularSequenceChecker.Transition @event, Saxon.Hej.@event.RegularSequenceChecker.State to) {
            System.Collections.Generic.IDictionary<Saxon.Hej.@event.RegularSequenceChecker.Transition,Saxon.Hej.@event.RegularSequenceChecker.State> edges = Saxon.Impl.Helpers.MapUtils.ComputeIfAbsent(machine, from, s => new System.Collections.Generic.Dictionary<Saxon.Hej.@event.RegularSequenceChecker.Transition,Saxon.Hej.@event.RegularSequenceChecker.State>());
            Saxon.Impl.Helpers.MapUtils.Put(edges, @event, to);
        }
        private void transition(Saxon.Hej.@event.RegularSequenceChecker.Transition @event) {
            System.Collections.Generic.IDictionary<Saxon.Hej.@event.RegularSequenceChecker.Transition,Saxon.Hej.@event.RegularSequenceChecker.State> map = Saxon.Impl.Helpers.MapUtils.Get(machine, state);
            System.Diagnostics.Debug.Assert((map != null));
            if (map.ContainsKey(@event)) {
                state = Saxon.Impl.Helpers.MapUtils.Get(map, @event);
            } else {
                throw new Saxon.Ejava.lang.IllegalStateException(((("Event "+@event)+" is not permitted in state ")+state));
            }
        }
        public RegularSequenceChecker(Saxon.Hej.@event.Receiver nextReceiver, bool fullChecking) : base (nextReceiver) {
            state = Saxon.Hej.@event.RegularSequenceChecker.State.INITIAL;
            this.fullChecking = fullChecking;
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            try {
                transition(Saxon.Hej.@event.RegularSequenceChecker.Transition.APPEND);
                nextReceiver.append(item, locationId, copyNamespaces);
            } catch (Saxon.Hej.trans.XPathException e) {
                state = Saxon.Hej.@event.RegularSequenceChecker.State.FAILED;
                throw;
            }
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            transition(Saxon.Hej.@event.RegularSequenceChecker.Transition.TEXT);
            if ((chars.isEmpty() && !((stack.Count==0)))) {
                throw new Saxon.Ejava.lang.IllegalStateException("Zero-length text nodes not allowed within document/element content");
            }
            try {
                nextReceiver.characters(chars, locationId, properties);
            } catch (Saxon.Hej.trans.XPathException e) {
                state = Saxon.Hej.@event.RegularSequenceChecker.State.FAILED;
                throw;
            }
        }
        public override void close() {
            if (((state != Saxon.Hej.@event.RegularSequenceChecker.State.FINAL) && (state != Saxon.Hej.@event.RegularSequenceChecker.State.FAILED))) {
                if (!((stack.Count==0))) {
                    throw new Saxon.Ejava.lang.IllegalStateException("Unclosed element or document nodes at end of stream");
                }
                nextReceiver.close();
                state = Saxon.Hej.@event.RegularSequenceChecker.State.FINAL;
            }
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            transition(Saxon.Hej.@event.RegularSequenceChecker.Transition.COMMENT);
            try {
                nextReceiver.comment(chars, locationId, properties);
            } catch (Saxon.Hej.trans.XPathException e) {
                state = Saxon.Hej.@event.RegularSequenceChecker.State.FAILED;
                throw;
            }
        }
        public override void endDocument() {
            transition(Saxon.Hej.@event.RegularSequenceChecker.Transition.END_DOCUMENT);
            if (((stack.Count==0) || (stack.Pop() != Saxon.Hej.type.Type.DOCUMENT))) {
                throw new Saxon.Ejava.lang.IllegalStateException("Unmatched endDocument() call");
            }
            try {
                nextReceiver.endDocument();
            } catch (Saxon.Hej.trans.XPathException e) {
                state = Saxon.Hej.@event.RegularSequenceChecker.State.FAILED;
                throw;
            }
        }
        public override void endElement() {
            transition(Saxon.Hej.@event.RegularSequenceChecker.Transition.END_ELEMENT);
            if (((stack.Count==0) || (stack.Pop() != Saxon.Hej.type.Type.ELEMENT))) {
                throw new Saxon.Ejava.lang.IllegalStateException("Unmatched endElement() call");
            }
            if ((stack.Count==0)) {
                state = Saxon.Hej.@event.RegularSequenceChecker.State.OPEN;
            }
            try {
                nextReceiver.endElement();
            } catch (Saxon.Hej.trans.XPathException e) {
                state = Saxon.Hej.@event.RegularSequenceChecker.State.FAILED;
                throw;
            }
        }
        public override void open() {
            transition(Saxon.Hej.@event.RegularSequenceChecker.Transition.OPEN);
            try {
                nextReceiver.open();
            } catch (Saxon.Hej.trans.XPathException e) {
                state = Saxon.Hej.@event.RegularSequenceChecker.State.FAILED;
                throw;
            }
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            transition(Saxon.Hej.@event.RegularSequenceChecker.Transition.PI);
            try {
                nextReceiver.processingInstruction(target, data, locationId, properties);
            } catch (Saxon.Hej.trans.XPathException e) {
                state = Saxon.Hej.@event.RegularSequenceChecker.State.FAILED;
                throw;
            }
        }
        public override void startDocument(int properties) {
            transition(Saxon.Hej.@event.RegularSequenceChecker.Transition.START_DOCUMENT);
            stack.Push(Saxon.Hej.type.Type.DOCUMENT);
            try {
                nextReceiver.startDocument(properties);
            } catch (Saxon.Hej.trans.XPathException e) {
                state = Saxon.Hej.@event.RegularSequenceChecker.State.FAILED;
                throw;
            }
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            transition(Saxon.Hej.@event.RegularSequenceChecker.Transition.START_ELEMENT);
            stack.Push(Saxon.Hej.type.Type.ELEMENT);
            if (fullChecking) {
                attributes.verify();
                string prefix = elemName.getPrefix();
                if (prefix.Length==0) {
                    string declaredDefaultUri = namespaces.getDefaultNamespace();
                    if (!(declaredDefaultUri.Equals(elemName.getURI()))) {
                        throw new Saxon.Ejava.lang.IllegalStateException((((((("URI of element Q{"+elemName.getURI())+"}")+elemName.getLocalPart())+" does not match declared default namespace {")+declaredDefaultUri)+"}"));
                    }
                } else {
                    string declaredUri = namespaces.getURI(prefix);
                    if ((declaredUri == null)) {
                        throw new Saxon.Ejava.lang.IllegalStateException((("Prefix "+prefix)+" has not been declared"));
                    } else {
                        if (!(declaredUri.Equals(elemName.getURI()))) {
                            throw new Saxon.Ejava.lang.IllegalStateException((("Prefix "+prefix)+" is bound to the wrong namespace"));
                        }
                    }
                }
                foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                    Saxon.Hej.om.NodeName name = att.getNodeName();
                    if (!(name.getURI().Length==0)) {
                        string attPrefix = name.getPrefix();
                        string declaredUri = namespaces.getURI(attPrefix);
                        if ((declaredUri == null)) {
                            throw new Saxon.Ejava.lang.IllegalStateException(((("Prefix "+attPrefix)+" has not been declared for attribute ")+att.getNodeName().getDisplayName()));
                        } else {
                            if (!(declaredUri.Equals(name.getURI()))) {
                                throw new Saxon.Ejava.lang.IllegalStateException((((("Prefix "+prefix)+" is bound to the wrong namespace {")+declaredUri)+"}"));
                            }
                        }
                    }
                }
            }
            try {
                nextReceiver.startElement(elemName, type, attributes, namespaces, location, properties);
            } catch (Saxon.Hej.trans.XPathException e) {
                state = Saxon.Hej.@event.RegularSequenceChecker.State.FAILED;
                throw;
            }
        }
    
    }
}