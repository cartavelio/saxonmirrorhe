using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public class UnparsedTextIterator : Saxon.Hej.tree.iter.TextLinesIterator {
        internal Saxon.Hej.expr.XPathContext context;
        internal string encoding = null;
        public UnparsedTextIterator(System.Uri absoluteURI, Saxon.Hej.expr.XPathContext context, string encoding, Saxon.Hej.s9api.Location location)  {
            Saxon.Hej.Configuration config = context.getConfiguration();
            System.IO.TextReader reader = context.getController().getUnparsedTextURIResolver().resolve(absoluteURI, encoding, config);
            this.reader = new Saxon.Impl.Helpers.LineNumberReader(reader);
            this.uri = absoluteURI;
            this.context = context;
            this.checker = context.getConfiguration().getValidCharacterChecker();
            this.encoding = encoding;
            this.location = location;
        }
        public UnparsedTextIterator(Saxon.Impl.Helpers.LineNumberReader reader, System.Uri absoluteURI, Saxon.Hej.expr.XPathContext context, string encoding)  {
            this.reader = reader;
            this.uri = absoluteURI;
            this.context = context;
            this.checker = context.getConfiguration().getValidCharacterChecker();
            this.encoding = encoding;
            this.location = null;
        }
    
    }
}