using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public abstract class TextLinesIterator : Saxon.Hej.om.SequenceIterator {
        protected internal Saxon.Impl.Helpers.LineNumberReader reader;
        protected internal Saxon.Hej.z.IntPredicateProxy checker;
        internal Saxon.Hej.value.StringValue current = null;
        internal int position = 0;
        protected internal Saxon.Hej.s9api.Location location;
        protected internal System.Uri uri;
        private Saxon.Hej.om.SequenceIterator dischargedIterator;
        protected internal TextLinesIterator()  {
        }
        public TextLinesIterator(Saxon.Impl.Helpers.LineNumberReader reader, Saxon.Hej.s9api.Location location, System.Uri uri, Saxon.Hej.z.IntPredicateProxy checker)  {
            this.reader = reader;
            this.location = location;
            this.uri = uri;
            this.checker = checker;
        }
        public virtual Saxon.Hej.om.Item next() {
            if ((dischargedIterator != null)) {
                return (Saxon.Hej.value.StringValue)(dischargedIterator.next());
            }
            if ((position<0)) {
                close();
                return null;
            }
            try {
                string s = reader.ReadLine();
                if ((s == null)) {
                    current = null;
                    position = -1;
                    close();
                    return null;
                }
                if ((((position == 0) && (s.Length>0)) && (s[0] == '\ufeff'))) {
                    s = s.Substring(1);
                }
                checkLine(checker, s);
                current = new Saxon.Hej.value.StringValue(s);
                position++;
                return current;
            } catch (System.IO.IOException err) {
                close();
                Saxon.Hej.trans.XPathException e = handleIOError(uri, err);
                if ((location != null)) {
                    e.setLocator(location);
                }
                throw new Saxon.Hej.trans.UncheckedXPathException(e);
            } catch (Saxon.Hej.trans.XPathException err) {
                throw new Saxon.Hej.trans.UncheckedXPathException(err);
            }
        }
        protected internal virtual Saxon.Hej.trans.XPathException handleIOError(System.Uri uri, System.IO.IOException err) {
            return Saxon.Hej.functions.UnparsedTextFunction.handleIOError(uri, err);
        }
        public void close() {
            try {
                reader.Close();
            } catch (System.IO.IOException err) {
            }
        }
        public void discharge() {
            dischargedIterator = Saxon.Hej.om.SequenceTool.toGroundedValue(this).iterate();
            close();
        }
        private void checkLine(Saxon.Hej.z.IntPredicateProxy checker, string buffer) {
            for (int c = 0; (c<buffer.Length); ) {
                int ch32 = buffer[c++];
                if (Saxon.Hej.serialize.charcode.UTF16CharacterSet.isHighSurrogate(ch32)) {
                    char low = buffer[c++];
                    ch32 = Saxon.Hej.serialize.charcode.UTF16CharacterSet.combinePair((char)(ch32), low);
                }
                if (!(checker.test(ch32))) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((((((("The unparsed-text file contains a character that is illegal in XML (line="+position)+" column=")+((c+1)))+" value=hex ")+Saxon.Impl.Helpers.Utils.intToHexString(ch32))+')'));
                    err.setErrorCode("FOUT1190");
                    err.setLocator(location);
                    throw err;
                }
            }
        }
    
    }
}