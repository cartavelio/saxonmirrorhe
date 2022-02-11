using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.serialize {
    public class HexBinaryEmitter : Saxon.Eej.serialize.BinaryEmitter {
        public HexBinaryEmitter(Saxon.Ejavax.xml.transform.stream.StreamResult stream, Saxon.Hej.@event.PipelineConfiguration pipe) : base (stream, pipe) {
        }
        protected internal override byte[] textNodeToBinary(Saxon.Hej.str.UnicodeString chars) {
            return new Saxon.Hej.value.HexBinaryValue(Saxon.Hej.value.Whitespace.trim(chars)).getBinaryValue();
        }
    
    }
}