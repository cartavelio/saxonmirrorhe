using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    public interface Location : Saxon.Ejavax.xml.transform.SourceLocator {
        Saxon.Hej.s9api.Location saveLocation();
    
    }
}