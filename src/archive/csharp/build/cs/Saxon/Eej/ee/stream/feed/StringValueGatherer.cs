using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class StringValueGatherer : Saxon.Eej.ee.validate.AbstractReceiver {
        private readonly Saxon.Hej.str.UnicodeBuilder buffer;
        public StringValueGatherer(Saxon.Hej.str.UnicodeBuilder buffer)  {
            this.buffer = buffer;
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            buffer.accept(chars);
        }
    
    }
}