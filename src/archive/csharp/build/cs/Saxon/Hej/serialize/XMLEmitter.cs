using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class XMLEmitter : Saxon.Hej.serialize.Emitter {

        static XMLEmitter() {
            specialInText = new bool[128];
            for (int i = 0; (i<=31); i++) {
                specialInText[i] = true;
            }
            for (int i = 32; (i<=127); i++) {
                specialInText[i] = false;
            }
            specialInText['\n'] = false;
            specialInText['\t'] = false;
            specialInText['\r'] = true;
            specialInText['<'] = true;
            specialInText['>'] = true;
            specialInText['&'] = true;
            specialInAtt = new bool[128];
            for (int i = 0; (i<=31); i++) {
                specialInAtt[i] = true;
            }
            for (int i = 32; (i<=127); i++) {
                specialInAtt[i] = false;
            }
            specialInAtt[(char)(0)] = true;
            specialInAtt['\r'] = true;
            specialInAtt['\n'] = true;
            specialInAtt['\t'] = true;
            specialInAtt['<'] = true;
            specialInAtt['>'] = true;
            specialInAtt['&'] = true;
            specialInAtt['\"'] = true;
            specialInAttSingle = Saxon.Ejava.util.Arrays.copyOf(specialInAtt, 128);
            specialInAttSingle['\"'] = false;
            specialInAttSingle['\''] = true;
        }
        protected internal bool canonical = false;
        protected internal bool started = false;
        protected internal bool startedElement = false;
        protected internal bool openStartTag = false;
        protected internal bool declarationIsWritten = false;
        protected internal Saxon.Hej.om.NodeName elementCode;
        protected internal int indentForNextAttribute = -1;
        protected internal bool undeclareNamespaces = false;
        protected internal bool unfailing = false;
        protected internal string internalSubset = null;
        protected internal char delimiter = '"';
        protected internal bool[] attSpecials = specialInAtt;
        protected internal System.Collections.Generic.Stack<string> elementStack = new System.Collections.Generic.Stack<string>();
        private bool indenting = false;
        private bool requireWellFormed = false;
        protected internal Saxon.Hej.serialize.CharacterReferenceGenerator characterReferenceGenerator = Saxon.Hej.serialize.HexCharacterReferenceGenerator.THE_INSTANCE;
        protected internal static bool[] specialInText;
        protected internal static bool[] specialInAtt;
        protected internal static bool[] specialInAttSingle;
        internal Saxon.Ejava.util.function.IntPredicate isSpecialInText;
        internal Saxon.Ejava.util.function.IntPredicate isSpecialInAttribute;
        public XMLEmitter()  {
        }
        public void setCharacterReferenceGenerator(Saxon.Hej.serialize.CharacterReferenceGenerator generator) {
            this.characterReferenceGenerator = generator;
        }
        public virtual void setEscapeNonAscii(bool escape) {
        }
        public override void open() {
        }
        public override void startDocument(int properties) {
        }
        public override void endDocument() {
        }
        protected internal virtual void openDocument() {
            System.Diagnostics.Debug.Assert((writer != null));
            if ((characterSet == null)) {
                characterSet = Saxon.Hej.serialize.charcode.UTF8CharacterSet.getInstance();
            }
            if ((outputProperties == null)) {
                outputProperties = new Saxon.Ejava.util.Properties();
            }
            undeclareNamespaces = "yes".Equals(outputProperties.getProperty(Saxon.Hej.lib.SaxonOutputKeys.UNDECLARE_PREFIXES));
            canonical = "yes".Equals(outputProperties.getProperty(Saxon.Hej.lib.SaxonOutputKeys.CANONICAL));
            unfailing = "yes".Equals(outputProperties.getProperty(Saxon.Hej.lib.SaxonOutputKeys.UNFAILING));
            internalSubset = outputProperties.getProperty(Saxon.Hej.lib.SaxonOutputKeys.INTERNAL_DTD_SUBSET);
            if ("yes".Equals(outputProperties.getProperty(Saxon.Hej.lib.SaxonOutputKeys.SINGLE_QUOTES))) {
                delimiter = '\'';
                attSpecials = specialInAttSingle;
            }
            if (allCharactersEncodable) {
                isSpecialInText = c => (((c<127) ? specialInText[c] : (((c<160) || (c == 0x2028)))));
                isSpecialInAttribute = c => (((c<127) ? attSpecials[c] : (((c<160) || (c == 0x2028)))));
            } else {
                isSpecialInText = c => (((c<127) ? specialInText[c] : (((((c<160) || (c == 0x2028)) || (c>65535)) || !(characterSet.inCharset(c))))));
                isSpecialInAttribute = c => (((c<127) ? attSpecials[c] : (((((c<160) || (c == 0x2028)) || (c>65535)) || !(characterSet.inCharset(c))))));
            }
            writeDeclaration();
        }
        public virtual void writeDeclaration() {
            if (declarationIsWritten) {
                return ;
            }
            declarationIsWritten = true;
            try {
                indenting = "yes".Equals(outputProperties.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.INDENT));
                string byteOrderMark = outputProperties.getProperty(Saxon.Hej.lib.SaxonOutputKeys.BYTE_ORDER_MARK);
                string encoding = outputProperties.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.ENCODING);
                if ((((encoding == null) || Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(encoding, "utf8")) || canonical)) {
                    encoding = "UTF-8";
                }
                if ((("yes".Equals(byteOrderMark) && !(canonical)) && (((Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("UTF-8", encoding) || Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("UTF-16LE", encoding)) || Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase("UTF-16BE", encoding))))) {
                    writer.writeCodePoint(0xFEFF);
                }
                string omitXMLDeclaration = outputProperties.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.OMIT_XML_DECLARATION);
                if ((omitXMLDeclaration == null)) {
                    omitXMLDeclaration = "no";
                }
                if (canonical) {
                    omitXMLDeclaration = "yes";
                }
                string version = outputProperties.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.VERSION);
                if ((version == null)) {
                    version = ((getConfiguration().getXMLVersion() == Saxon.Hej.Configuration.XML10) ? "1.0" : "1.1");
                } else {
                    if ((!(version.Equals("1.0")) && !(version.Equals("1.1")))) {
                        if (unfailing) {
                            version = "1.0";
                        } else {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("XML version must be 1.0 or 1.1");
                            err.setErrorCode("SESU0013");
                            throw err;
                        }
                    }
                    if (((!(version.Equals("1.0")) && omitXMLDeclaration.Equals("yes")) && (outputProperties.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.DOCTYPE_SYSTEM) != null))) {
                        if (!(unfailing)) {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Values of 'version', 'omit-xml-declaration', and 'doctype-system' conflict");
                            err.setErrorCode("SEPM0009");
                            throw err;
                        }
                    }
                }
                string undeclare = outputProperties.getProperty(Saxon.Hej.lib.SaxonOutputKeys.UNDECLARE_PREFIXES);
                if ("yes".Equals(undeclare)) {
                    undeclareNamespaces = true;
                }
                if ((version.Equals("1.0") && undeclareNamespaces)) {
                    if (unfailing) {
                        undeclareNamespaces = false;
                    } else {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot undeclare namespaces with XML version 1.0");
                        err.setErrorCode("SEPM0010");
                        throw err;
                    }
                }
                string standalone = outputProperties.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.STANDALONE);
                if ("omit".Equals(standalone)) {
                    standalone = null;
                }
                if ((standalone != null)) {
                    requireWellFormed = true;
                    if ((omitXMLDeclaration.Equals("yes") && !(unfailing))) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Values of 'standalone' and 'omit-xml-declaration' conflict");
                        err.setErrorCode("SEPM0009");
                        throw err;
                    }
                }
                string systemId = outputProperties.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.DOCTYPE_SYSTEM);
                if (((systemId != null) && !("".Equals(systemId)))) {
                    requireWellFormed = true;
                }
                if (omitXMLDeclaration.Equals("no")) {
                    writer.writeAscii(XML_DECL_VERSION);
                    writer.write(version);
                    writer.writeAscii(QUOTE_SPACE);
                    writer.writeAscii(XML_DECL_ENCODING);
                    writer.write(encoding);
                    writer.writeCodePoint('"');
                    if ((standalone != null)) {
                        writer.writeAscii(XML_DECL_STANDALONE);
                        writer.write(standalone);
                        writer.writeCodePoint('"');
                    }
                    writer.writeAscii(Saxon.Hej.str.StringConstants.PI_END);
                }
            } catch (System.IO.IOException err) {
                throw new Saxon.Hej.trans.XPathException(("Failure writing to "+getSystemId()), err);
            }
        }
        private static readonly byte[] XML_DECL_VERSION = Saxon.Hej.str.StringConstants.bytes("<?xml version=\"");
        private static readonly byte[] XML_DECL_ENCODING = Saxon.Hej.str.StringConstants.bytes("encoding=\"");
        private static readonly byte[] XML_DECL_STANDALONE = Saxon.Hej.str.StringConstants.bytes(" standalone=\"");
        private static readonly byte[] QUOTE_SPACE = Saxon.Hej.str.StringConstants.bytes("\" ");
        protected internal static readonly byte[] DOCTYPE = Saxon.Hej.str.StringConstants.bytes("<!DOCTYPE ");
        private static readonly byte[] SYSTEM = Saxon.Hej.str.StringConstants.bytes("  SYSTEM ");
        private static readonly byte[] PUBLIC = Saxon.Hej.str.StringConstants.bytes("  PUBLIC \"");
        protected internal static readonly byte[] RIGHT_ANGLE_NEWLINE = Saxon.Hej.str.StringConstants.bytes(">\n");
        protected internal virtual void writeDocType(Saxon.Hej.om.NodeName name, string displayName, string systemId, string publicId) {
            try {
                if (!(canonical)) {
                    if ((declarationIsWritten && !(indenting))) {
                        writer.writeCodePoint(0x0A);
                    }
                    writer.writeAscii(DOCTYPE);
                    writer.write(displayName);
                    writer.writeCodePoint(0x0A);
                    string quotedSystemId = null;
                    if ((systemId != null)) {
                        if (systemId.Contains("\"")) {
                            quotedSystemId = (("'"+systemId)+"'");
                        } else {
                            quotedSystemId = (('"'+systemId)+'"');
                        }
                    }
                    if (((systemId != null) && (publicId == null))) {
                        writer.writeAscii(SYSTEM);
                        writer.write(quotedSystemId);
                    } else {
                        if (((systemId == null) && (publicId != null))) {
                            writer.writeAscii(PUBLIC);
                            writer.write(publicId);
                            writer.writeCodePoint('"');
                        } else {
                            if ((publicId != null)) {
                                writer.writeAscii(PUBLIC);
                                writer.write(publicId);
                                writer.writeAscii(QUOTE_SPACE);
                                writer.write(quotedSystemId);
                            }
                        }
                    }
                    if ((internalSubset != null)) {
                        writer.writeCodePoint('[');
                        writer.writeCodePoint(0x0A);
                        writer.write(internalSubset);
                        writer.writeCodePoint(0x0A);
                        writer.writeCodePoint(']');
                    }
                    writer.writeAscii(RIGHT_ANGLE_NEWLINE);
                }
            } catch (System.IO.IOException err) {
                throw new Saxon.Hej.trans.XPathException(("Failure writing to "+getSystemId()), err);
            }
        }
        public override void close() {
            if (!(started)) {
                openDocument();
            }
            try {
                if ((writer != null)) {
                    writer.flush();
                }
            } catch (System.IO.IOException err) {
                throw new Saxon.Hej.trans.XPathException(("Failure writing to "+getSystemId()), err);
            }
            base.close();
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            previousAtomic = false;
            if (!(started)) {
                openDocument();
            } else {
                if ((((requireWellFormed && (elementStack.Count==0)) && startedElement) && !(unfailing))) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("When 'standalone' or 'doctype-system' is specified, "+"the document must be well-formed; but this document contains more than one top-level element"));
                    err.setErrorCode("SEPM0004");
                    throw err;
                }
            }
            startedElement = true;
            string displayName = elemName.getDisplayName();
            if (!(allCharactersEncodable)) {
                int badchar = testCharacters(Saxon.Hej.str.StringView.of(displayName));
                if ((badchar != 0)) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Element name contains a character (decimal + "+badchar)+") not available in the selected encoding"));
                    err.setErrorCode("SERE0008");
                    throw err;
                }
            }
            elementStack.Push(displayName);
            elementCode = elemName;
            try {
                if (!(started)) {
                    string systemId = outputProperties.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.DOCTYPE_SYSTEM);
                    string publicId = outputProperties.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.DOCTYPE_PUBLIC);
                    if ("".Equals(systemId)) {
                        systemId = null;
                    }
                    if ("".Equals(publicId)) {
                        publicId = null;
                    }
                    if ((systemId != null)) {
                        requireWellFormed = true;
                        writeDocType(elemName, displayName, systemId, publicId);
                    } else {
                        if (writeDocTypeWithNullSystemId()) {
                            writeDocType(elemName, displayName, null, publicId);
                        }
                    }
                    started = true;
                }
                if (openStartTag) {
                    closeStartTag();
                }
                writer.writeCodePoint('<');
                writer.write(displayName);
                if ((indentForNextAttribute>=0)) {
                    indentForNextAttribute += displayName.Length;
                }
                bool isFirst = true;
                foreach (Saxon.Hej.om.NamespaceBinding ns in namespaces) {
                    @namespace(ns.getPrefix(), ns.getNamespaceUri(), isFirst);
                    isFirst = false;
                }
                foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                    attribute(att.getNodeName(), att.getValue(), att.getProperties(), isFirst);
                    isFirst = false;
                }
                openStartTag = true;
                indentForNextAttribute = -1;
            } catch (System.IO.IOException err) {
                throw new Saxon.Hej.trans.XPathException(("Failure writing to "+getSystemId()), err);
            }
        }
        protected internal virtual bool writeDocTypeWithNullSystemId() {
            return (internalSubset != null);
        }
        public void @namespace(string nsprefix, Saxon.Hej.om.NamespaceUri nsuri, bool isFirst) {
            try {
                if (nsprefix.Length==0) {
                    if (isFirst) {
                        writer.writeCodePoint(' ');
                    } else {
                        writeAttributeIndentString();
                    }
                    writeAttribute(elementCode, "xmlns", nsuri.ToString(), Saxon.Hej.@event.ReceiverOption.NONE);
                } else {
                    if (nsprefix.Equals("xml")) {
                    } else {
                        int badchar = testCharacters(Saxon.Hej.str.StringView.of(nsprefix));
                        if ((badchar != 0)) {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Namespace prefix contains a character (decimal + "+badchar)+") not available in the selected encoding"));
                            err.setErrorCode("SERE0008");
                            throw err;
                        }
                        if ((undeclareNamespaces || !(nsuri.isEmpty()))) {
                            if (isFirst) {
                                writer.writeCodePoint(' ');
                            } else {
                                writeAttributeIndentString();
                            }
                            writeAttribute(elementCode, ("xmlns:"+nsprefix), nsuri.ToString(), Saxon.Hej.@event.ReceiverOption.NONE);
                        }
                    }
                }
            } catch (System.IO.IOException err) {
                throw new Saxon.Hej.trans.XPathException(("Failure writing to "+getSystemId()), err);
            }
        }
        public void setIndentForNextAttribute(int indent) {
            indentForNextAttribute = indent;
        }
        private void attribute(Saxon.Hej.om.NodeName nameCode, string value, int properties, bool isFirst) {
            string displayName = nameCode.getDisplayName();
            if (!(allCharactersEncodable)) {
                int badchar = testCharacters(Saxon.Hej.str.StringView.of(displayName));
                if ((badchar != 0)) {
                    if (unfailing) {
                        displayName = convertToAscii(Saxon.Hej.str.StringView.of(displayName)).ToString();
                    } else {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Attribute name contains a character (decimal + "+badchar)+") not available in the selected encoding"));
                        err.setErrorCode("SERE0008");
                        throw err;
                    }
                }
            }
            try {
                if (isFirst) {
                    writer.writeCodePoint(' ');
                } else {
                    writeAttributeIndentString();
                }
            } catch (System.IO.IOException err) {
                throw new Saxon.Hej.trans.XPathException(("Failure writing to "+getSystemId()), err);
            }
            writeAttribute(elementCode, displayName, value, properties);
        }
        protected internal void writeAttributeIndentString() {
            if ((indentForNextAttribute<0)) {
                writer.writeCodePoint(' ');
            } else {
                writer.writeCodePoint('\n');
                writer.writeRepeatedAscii((byte)(0x20), indentForNextAttribute);
            }
        }
        public void closeStartTag() {
            try {
                if (openStartTag) {
                    writer.writeCodePoint('>');
                    openStartTag = false;
                }
            } catch (System.IO.IOException err) {
                throw new Saxon.Hej.trans.XPathException(("Failure writing to "+getSystemId()), err);
            }
        }
        protected internal virtual void writeEmptyElementTagCloser(string displayName, Saxon.Hej.om.NodeName nameCode) {
            if (canonical) {
                writer.writeCodePoint('>');
                writer.writeAscii(Saxon.Hej.str.StringConstants.END_TAG_START);
                writer.write(displayName);
                writer.writeCodePoint('>');
            } else {
                writer.writeAscii(Saxon.Hej.str.StringConstants.EMPTY_TAG_END);
            }
        }
        protected internal virtual void writeAttribute(Saxon.Hej.om.NodeName elCode, string attname, string value, int properties) {
            try {
                writer.write(attname);
                if (Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.NO_SPECIAL_CHARS)) {
                    writer.writeCodePoint('=');
                    writer.writeCodePoint(delimiter);
                    writer.write(value);
                    writer.writeCodePoint(delimiter);
                } else {
                    if (Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.USE_NULL_MARKERS)) {
                        writer.writeCodePoint('=');
                        char delim = (((value.IndexOf('"')>=0) && (value.IndexOf('\'')<0)) ? '\'' : delimiter);
                        writer.writeCodePoint(delim);
                        writeEscape(Saxon.Hej.str.StringView.tidy(value), true);
                        writer.writeCodePoint(delim);
                    } else {
                        writer.writeCodePoint('=');
                        writer.writeCodePoint(delimiter);
                        if (Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.DISABLE_ESCAPING)) {
                            writer.write(value);
                        } else {
                            writeEscape(Saxon.Hej.str.StringView.tidy(value), true);
                        }
                        writer.writeCodePoint(delimiter);
                    }
                }
            } catch (System.IO.IOException err) {
                throw new Saxon.Hej.trans.XPathException(("Failure writing to "+getSystemId()), err);
            }
        }
        protected internal int testCharacters(Saxon.Hej.str.UnicodeString chars) {
            long foundInvalid = chars.indexWhere(ch => ((ch>127) && !(characterSet.inCharset(ch))), 0);
            if ((foundInvalid>=0)) {
                return chars.codePointAt(foundInvalid);
            }
            return 0;
        }
        protected internal Saxon.Hej.str.UnicodeString convertToAscii(Saxon.Hej.str.UnicodeString chars) {
            Saxon.Hej.str.UnicodeBuilder buff = new Saxon.Hej.str.UnicodeBuilder();
            Saxon.Hej.z.IntIterator iter = chars.codePoints();
            while (iter.hasNext()) {
                int c = iter.next();
                if (((c>=20) && (c<127))) {
                    buff.append(c);
                } else {
                    buff.append((("_"+c)+"_"));
                }
            }
            return buff.toUnicodeString();
        }
        public override void endElement() {
            string displayName = elementStack.Pop();
            try {
                if (openStartTag) {
                    writeEmptyElementTagCloser(displayName, elementCode);
                    openStartTag = false;
                } else {
                    writer.writeAscii(Saxon.Hej.str.StringConstants.END_TAG_START);
                    writer.write(displayName);
                    writer.writeCodePoint('>');
                }
            } catch (System.IO.IOException err) {
                throw new Saxon.Hej.trans.XPathException(("Failure writing to "+getSystemId()), err);
            }
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if (!(started)) {
                openDocument();
            }
            if ((((requireWellFormed && (elementStack.Count==0)) && !(Saxon.Hej.value.Whitespace.isAllWhite(chars))) && !(unfailing))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("When 'standalone' or 'doctype-system' is specified, "+"the document must be well-formed; but this document contains a top-level text node"));
                err.setErrorCode("SEPM0004");
                throw err;
            }
            try {
                if (openStartTag) {
                    closeStartTag();
                }
                if ((chars is Saxon.Hej.str.WhitespaceString)) {
                    ((Saxon.Hej.str.WhitespaceString)(chars)).write(writer);
                } else {
                    if (Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.NO_SPECIAL_CHARS)) {
                        writer.write(chars);
                    } else {
                        if (!(Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.DISABLE_ESCAPING))) {
                            writeEscape(chars, false);
                        } else {
                            if ((testCharacters(chars) == 0)) {
                                if (!(Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.USE_NULL_MARKERS))) {
                                    writer.write(chars);
                                } else {
                                    Saxon.Hej.z.IntIterator iter = chars.codePoints();
                                    while (iter.hasNext()) {
                                        int c = iter.next();
                                        if ((c != 0)) {
                                            writer.writeCodePoint(c);
                                        }
                                    }
                                }
                            } else {
                                Saxon.Hej.z.IntIterator iter = chars.codePoints();
                                while (iter.hasNext()) {
                                    int c = iter.next();
                                    if ((c != 0)) {
                                        if (characterSet.inCharset(c)) {
                                            writer.writeCodePoint(c);
                                        } else {
                                            writeEscape(new Saxon.Hej.str.UnicodeChar(c), false);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (System.IO.IOException err) {
                throw new Saxon.Hej.trans.XPathException(("Failure writing to "+getSystemId()), err);
            }
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            if (!(started)) {
                openDocument();
            }
            int x = testCharacters(Saxon.Hej.str.StringView.of(target));
            if ((x != 0)) {
                if (unfailing) {
                    target = convertToAscii(Saxon.Hej.str.StringView.of(target)).ToString();
                } else {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(((("Character in processing instruction name cannot be represented "+"in the selected encoding (code ")+x)+')'));
                    err.setErrorCode("SERE0008");
                    throw err;
                }
            }
            x = testCharacters(data);
            if ((x != 0)) {
                if (unfailing) {
                    data = convertToAscii(data);
                } else {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(((("Character in processing instruction data cannot be represented "+"in the selected encoding (code ")+x)+')'));
                    err.setErrorCode("SERE0008");
                    throw err;
                }
            }
            try {
                if (openStartTag) {
                    closeStartTag();
                }
                writer.writeAscii(Saxon.Hej.str.StringConstants.PI_START);
                writer.write(target);
                if (!(data.isEmpty())) {
                    writer.writeCodePoint(0x20);
                    writer.write(data);
                }
                writer.writeAscii(Saxon.Hej.str.StringConstants.PI_END);
            } catch (System.IO.IOException err) {
                throw new Saxon.Hej.trans.XPathException(("Failure writing to "+getSystemId()), err);
            }
        }
        protected internal virtual void writeEscape(Saxon.Hej.str.UnicodeString chars, bool inAttribute) {
            long segstart = 0;
            bool disabled = false;
            bool[] specialChars = (inAttribute ? attSpecials : specialInText);
            if ((chars is Saxon.Hej.str.WhitespaceString)) {
                ((Saxon.Hej.str.WhitespaceString)(chars)).writeEscape(specialChars, writer);
                return ;
            }
            Saxon.Ejava.util.function.IntPredicate special = (inAttribute ? isSpecialInAttribute : isSpecialInText);
            long clength = chars.length();
            while ((segstart<clength)) {
                long found = chars.indexWhere(special, segstart);
                long i = ((found == -1) ? clength : found);
                if ((found<0)) {
                    if ((segstart == 0)) {
                        writer.write(chars);
                    } else {
                        writer.write(chars.substring(segstart, clength));
                    }
                    return ;
                }
                if ((i>segstart)) {
                    writer.write(chars.substring(segstart, i));
                }
                int c = chars.codePointAt(i);
                if ((c == 0)) {
                    disabled = !(disabled);
                } else {
                    if (disabled) {
                        if (((c>127) && !(characterSet.inCharset(c)))) {
                            Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException((((("Character "+c)+" (x")+Saxon.Impl.Helpers.Utils.intToHexString(c))+") is not available in the chosen encoding"));
                            de.setErrorCode("SERE0008");
                            throw de;
                        }
                        writeCodePoint(c);
                    } else {
                        if ((c<127)) {
                            switch (c) {
                              case '<':
                                writer.writeAscii(Saxon.Hej.str.StringConstants.ESCAPE_LT);
                                break ;
                              case '>':
                                writer.writeAscii(Saxon.Hej.str.StringConstants.ESCAPE_GT);
                                break ;
                              case '&':
                                writer.writeAscii(Saxon.Hej.str.StringConstants.ESCAPE_AMP);
                                break ;
                              case '\"':
                                writer.writeAscii(Saxon.Hej.str.StringConstants.ESCAPE_QUOT);
                                break ;
                              case '\'':
                                writer.writeAscii(Saxon.Hej.str.StringConstants.ESCAPE_APOS);
                                break ;
                              case '\n':
                                writer.writeAscii(Saxon.Hej.str.StringConstants.ESCAPE_NL);
                                break ;
                              case '\r':
                                writer.writeAscii(Saxon.Hej.str.StringConstants.ESCAPE_CR);
                                break ;
                              case '\t':
                                writer.writeAscii(Saxon.Hej.str.StringConstants.ESCAPE_TAB);
                                break ;
                              default:
                                characterReferenceGenerator.outputCharacterReference(c, writer);
                                break ;
                            }
                        } else {
                            if (((c<160) || (c == 0x2028))) {
                                characterReferenceGenerator.outputCharacterReference(c, writer);
                            } else {
                                if ((c>65535)) {
                                    if (characterSet.inCharset(c)) {
                                        writeCodePoint(c);
                                    } else {
                                        characterReferenceGenerator.outputCharacterReference(c, writer);
                                    }
                                } else {
                                    characterReferenceGenerator.outputCharacterReference(c, writer);
                                }
                            }
                        }
                    }
                }
                segstart = ++i;
            }
        }
        protected internal void writeCodePoint(int c) {
            writer.writeCodePoint(c);
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if (!(started)) {
                openDocument();
            }
            int x = testCharacters(chars);
            if ((x != 0)) {
                if (unfailing) {
                    chars = convertToAscii(chars);
                } else {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(((("Character in comment cannot be represented "+"in the selected encoding (code ")+x)+')'));
                    err.setErrorCode("SERE0008");
                    throw err;
                }
            }
            try {
                if (openStartTag) {
                    closeStartTag();
                }
                writer.writeAscii(Saxon.Hej.str.StringConstants.COMMENT_START);
                writer.write(chars);
                writer.writeAscii(Saxon.Hej.str.StringConstants.COMMENT_END);
            } catch (System.IO.IOException err) {
                throw new Saxon.Hej.trans.XPathException(("Failure writing to "+getSystemId()), err);
            }
        }
        public override bool usesTypeAnnotations() {
            return false;
        }
        public bool isStarted() {
            return started;
        }
    
    }
}