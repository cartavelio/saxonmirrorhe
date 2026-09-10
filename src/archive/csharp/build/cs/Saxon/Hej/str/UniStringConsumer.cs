using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.str {
    public interface UniStringConsumer {
        void open();
        Saxon.Hej.str.UniStringConsumer accept(Saxon.Hej.str.UnicodeString chars);
        void close();
    
    }
}