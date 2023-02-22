using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.config {
    public class EvaluationNuisance : Saxon.Hej.@event.ProxyReceiver {
        public EvaluationNuisance(Saxon.Hej.@event.Receiver next) : base (next) {
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            Saxon.Hej.str.UnicodeBuilder fsb = new Saxon.Hej.str.UnicodeBuilder((chars.length32()+20));
            Saxon.Hej.z.IntIterator iter = chars.codePoints();
            int i = 0;
            while (iter.hasNext()) {
                fsb.append(iter.next());
                if (((i++%10) == 9)) {
                    fsb.append('*');
                }
            }
            base.characters(fsb.toUnicodeString(), locationId, properties);
        }
    
    }
}