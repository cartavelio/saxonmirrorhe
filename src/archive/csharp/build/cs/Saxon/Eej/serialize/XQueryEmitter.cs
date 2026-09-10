using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.serialize {
    public class XQueryEmitter : Saxon.Hej.serialize.XMLEmitter {
        protected internal override void writeEscape(Saxon.Hej.str.UnicodeString chars, bool inAttribute) {
            bool inBraces = false;
            chars = chars.tidy();
            Saxon.Hej.str.UnicodeBuilder buff = new Saxon.Hej.str.UnicodeBuilder();
            for (int i = 0; (i<chars.length()); i++) {
                int c = chars.codePointAt(i);
                if (((!(inBraces) && (c == '{')) && (chars.codePointAt((i+1)) != '{'))) {
                    inBraces = true;
                    buff.append((char)(0));
                } else {
                    if ((inBraces && (c == '}'))) {
                        inBraces = false;
                        buff.append((char)(0));
                    } else {
                        if ((inBraces && (c == '"'))) {
                            buff.append((char)(0));
                            i++;
                            do {
                                buff.append(c);
                                c = chars.codePointAt(i++);
                            } while ((c != '"'));
                            buff.append((char)(0));
                            i--;
                        } else {
                            if ((inBraces && (c == '\''))) {
                                buff.append((char)(0));
                                i++;
                                do {
                                    buff.append(c);
                                    c = chars.codePointAt(i++);
                                } while ((c != '\''));
                                buff.append((char)(0));
                                i--;
                            }
                        }
                    }
                }
                buff.append(c);
            }
            base.writeEscape(buff.toUnicodeString(), inAttribute);
        }
    
    }
}