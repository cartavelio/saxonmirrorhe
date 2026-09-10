using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class UnicodeNormalizer : Saxon.Hej.@event.ProxyReceiver {
        private readonly Saxon.Ejava.text.Normalizer.Form normForm;
        public UnicodeNormalizer(string form, Saxon.Hej.@event.Receiver next) : base (next) {
            switch (form) {
              case "NFC":
                normForm = Saxon.Ejava.text.Normalizer.Form.NFC;
                break ;
              case "NFD":
                normForm = Saxon.Ejava.text.Normalizer.Form.NFD;
                break ;
              case "NFKC":
                normForm = Saxon.Ejava.text.Normalizer.Form.NFKC;
                break ;
              case "NFKD":
                normForm = Saxon.Ejava.text.Normalizer.Form.NFKD;
                break ;
              default:
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Unknown normalization form "+form));
                err.setErrorCode("SESU0011");
                throw err;
            }
        }
        public Saxon.Ejava.text.Normalizer.Form getNormalizationForm() {
            return normForm;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            Saxon.Hej.om.AttributeMap am2 = attributes.apply(attInfo => {
                string newValue = normalize(Saxon.Hej.str.StringView.of(attInfo.getValue()), Saxon.Hej.@event.ReceiverOption.contains(attInfo.getProperties(), Saxon.Hej.@event.ReceiverOption.USE_NULL_MARKERS)).ToString();
                return new Saxon.Hej.om.AttributeInfo(attInfo.getNodeName(), attInfo.getType(), newValue, attInfo.getLocation(), attInfo.getProperties());
            } );
            nextReceiver.startElement(elemName, type, am2, namespaces, location, properties);
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if (Saxon.Hej.value.Whitespace.isAllWhite(chars)) {
                nextReceiver.characters(chars, locationId, properties);
            } else {
                nextReceiver.characters(normalize(chars, Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.USE_NULL_MARKERS)), locationId, properties);
            }
        }
        public Saxon.Hej.str.UnicodeString normalize(Saxon.Hej.str.UnicodeString @in, bool containsNullMarkers) {
            if ((@in is Saxon.Hej.str.WhitespaceString)) {
                return @in;
            }
            Saxon.Hej.str.UnicodeString t = @in.tidy();
            if (containsNullMarkers) {
                Saxon.Ejava.lang.StringBuilder @out = new Saxon.Ejava.lang.StringBuilder(t.length32());
                string s = @in.ToString();
                int start = 0;
                int nextNull = s.IndexOf((char)(0));
                while ((nextNull>=0)) {
                    @out.append(Saxon.Ejava.text.Normalizer.normalize(s.Substring(start, nextNull - start), normForm));
                    @out.append((char)(0));
                    start = (nextNull+1);
                    nextNull = s.IndexOf((char)(0), start);
                    @out.append(s.Substring(start, nextNull - start));
                    @out.append((char)(0));
                    start = (nextNull+1);
                    nextNull = s.IndexOf((char)(0), start);
                }
                @out.append(Saxon.Ejava.text.Normalizer.normalize(s.Substring(start), normForm));
                return Saxon.Hej.str.StringView.tidy(@out.ToString());
            } else {
                return Saxon.Hej.str.StringView.tidy(Saxon.Ejava.text.Normalizer.normalize(@in.ToString(), normForm));
            }
        }
    
    }
}