using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class CharacterMap {
        private Saxon.Hej.om.StructuredQName name;
        private readonly Saxon.Hej.z.IntHashMap<string> charMap;
        private int min = System.Int32.MaxValue;
        private int max = 0;
        private bool mapsWhitespace = false;
        public CharacterMap(Saxon.Hej.om.StructuredQName name, Saxon.Hej.z.IntHashMap<string> map)  {
            this.name = name;
            this.charMap = map;
            init();
        }
        public CharacterMap(System.Collections.Generic.IEnumerable<Saxon.Hej.serialize.CharacterMap> list)  {
            charMap = new Saxon.Hej.z.IntHashMap<string>(64);
            foreach (Saxon.Hej.serialize.CharacterMap map in list) {
                Saxon.Hej.z.IntIterator keys = map.charMap.keyIterator();
                while (keys.hasNext()) {
                    int next = keys.next();
                    charMap.put(next, map.charMap.get(next));
                }
            }
            init();
        }
        private void init() {
            Saxon.Hej.z.IntIterator keys = charMap.keyIterator();
            while (keys.hasNext()) {
                int next = keys.next();
                if ((next<min)) {
                    min = next;
                }
                if ((next>max)) {
                    max = next;
                }
                if ((!(mapsWhitespace) && Saxon.Hej.value.Whitespace.isWhite(next))) {
                    mapsWhitespace = true;
                }
            }
            if ((min>0xD800)) {
                min = 0xD800;
            }
        }
        public Saxon.Hej.om.StructuredQName getName() {
            return name;
        }
        public Saxon.Hej.z.IntHashMap<string> getMap() {
            return charMap;
        }
        public Saxon.Hej.str.UnicodeString map(Saxon.Hej.str.UnicodeString @in, bool insertNulls) {
            if ((!(mapsWhitespace) && (@in is Saxon.Hej.str.WhitespaceString))) {
                return @in;
            }
            bool move = false;
            Saxon.Hej.z.IntIterator iter = @in.codePoints();
            while (iter.hasNext()) {
                int c = iter.next();
                if (((c>=min) && (c<=max))) {
                    move = true;
                    break ;
                }
            }
            if (!(move)) {
                return @in;
            }
            Saxon.Hej.str.UnicodeBuilder buffer = new Saxon.Hej.str.UnicodeBuilder();
            iter = @in.codePoints();
            while (iter.hasNext()) {
                int c = iter.next();
                if (((c>=min) && (c<=max))) {
                    string rep = charMap.get(c);
                    if ((rep == null)) {
                        buffer.append(c);
                    } else {
                        if (insertNulls) {
                            buffer.append((char)(0));
                            buffer.append(rep);
                            buffer.append((char)(0));
                        } else {
                            buffer.append(rep);
                        }
                    }
                } else {
                    buffer.append(c);
                }
            }
            return buffer.toUnicodeString();
        }
        public void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("charMap");
            @out.emitAttribute("name", name);
            for (Saxon.Hej.z.IntIterator iter = charMap.keyIterator(); iter.hasNext(); ) {
                int c = iter.next();
                string s = charMap.get(c);
                @out.startElement("m");
                @out.emitAttribute("c", (c+""));
                @out.emitAttribute("s", s);
                @out.endElement();
            }
            @out.endElement();
        }
    
    }
}