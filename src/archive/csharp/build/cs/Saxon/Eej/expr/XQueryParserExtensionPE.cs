using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.expr {
    public class XQueryParserExtensionPE : Saxon.Eej.expr.SaxonSyntaxExtension {
        public XQueryParserExtensionPE()  {
        }
        public override void parseItemTypeDeclaration(Saxon.Hej.query.XQueryParser p) {
            p.nextToken();
            p.expect(Saxon.Hej.expr.parser.Token.NAME);
            Saxon.Hej.om.StructuredQName typeName = p.makeStructuredQName(p.getTokenizer().currentTokenValue, Saxon.Hej.om.NamespaceUri.NULL);
            p.nextToken();
            p.expect(Saxon.Hej.expr.parser.Token.AS);
            p.nextToken();
            Saxon.Hej.type.ItemType typeDef = p.parseItemType();
            p.getStaticContext().getPackageData().obtainTypeAliasManager().registerTypeAlias(typeName, typeDef);
        }
    
    }
}