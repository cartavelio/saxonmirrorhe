using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.serialize {
    public class Base64BinaryEmitter : Saxon.Eej.serialize.BinaryEmitter {
        public Base64BinaryEmitter(Saxon.Ejavax.xml.transform.stream.StreamResult stream, Saxon.Hej.@event.PipelineConfiguration pipe) : base (stream, pipe) {
        }
        protected internal override byte[] textNodeToBinary(Saxon.Hej.str.UnicodeString chars) {
            return new Saxon.Hej.value.Base64BinaryValue(Saxon.Hej.value.Whitespace.normalizeWhitespace(chars)).getBinaryValue();
        }
    
    }
}