using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class IdentitySelector : Saxon.Eej.ee.schema.FieldOrSelector {
        public IdentitySelector(string xpath)  {
            setXPath(xpath);
        }
    
    }
}