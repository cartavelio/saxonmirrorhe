using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public abstract class AbstractUniStringConsumer : Saxon.Hej.str.UniStringConsumer {
    
        /* Expanding default method open from interface UniStringConsumer */
        public virtual void open() {
        }

        /* Expanding default method close from interface UniStringConsumer */
        public virtual void close() {
        }

        /* Creating stub method accept to satisfy interface UniStringConsumer */
        public virtual Saxon.Hej.str.UniStringConsumer accept(Saxon.Hej.str.UnicodeString arg1) {
            throw new System.NotImplementedException();
        }

    }
}