using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.map {
    public class KeyValuePair {
        public Saxon.Hej.value.AtomicValue key;
        public Saxon.Hej.om.GroundedValue value;
        public KeyValuePair(Saxon.Hej.value.AtomicValue key, Saxon.Hej.om.GroundedValue value)  {
            this.key = key;
            this.value = value;
        }
    
    }
}