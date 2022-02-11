using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public delegate System.IO.TextReader UnparsedTextURIResolver (System.Uri absoluteURI, string encoding, Saxon.Hej.Configuration config);
}