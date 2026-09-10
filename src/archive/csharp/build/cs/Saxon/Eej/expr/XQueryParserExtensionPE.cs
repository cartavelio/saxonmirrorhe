using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.expr {
    public class XQueryParserExtensionPE : Saxon.Eej.expr.SaxonSyntaxExtension {
        public XQueryParserExtensionPE()  {
        }
        protected internal bool isReservedInQuery(string uri) {
            return Saxon.Hej.lib.NamespaceConstant.isReservedInQuery31(uri);
        }
        public override void parseTypeAliasDeclaration(Saxon.Hej.query.XQueryParser p) {
            p.nextToken();
            p.expect(Saxon.Hej.expr.parser.Token.NAME);
            Saxon.Hej.om.StructuredQName typeName = p.makeStructuredQName(p.getTokenizer().currentTokenValue, "");
            p.nextToken();
            p.expect(Saxon.Hej.expr.parser.Token.EQUALS);
            p.nextToken();
            Saxon.Hej.type.ItemType typeDef = p.parseItemType();
            p.getStaticContext().getPackageData().obtainTypeAliasManager().registerTypeAlias(typeName, typeDef);
        }
    
    }
}