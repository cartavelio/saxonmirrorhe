using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public interface StaticContext {
        Saxon.Hej.Configuration getConfiguration();
        Saxon.Hej.expr.PackageData getPackageData();
        Saxon.Hej.expr.XPathContext makeEarlyEvaluationContext();
        Saxon.Hej.expr.parser.RetainedStaticContext makeRetainedStaticContext();
        Saxon.Hej.s9api.Location getContainingLocation();
        void issueWarning(string message, string errorCode, Saxon.Hej.s9api.Location locator);
        string getSystemId();
        string getStaticBaseURI();
        Saxon.Hej.expr.Expression bindVariable(Saxon.Hej.om.StructuredQName qName);
        Saxon.Hej.functions.FunctionLibrary getFunctionLibrary();
        string getDefaultCollationName();
        Saxon.Hej.om.NamespaceUri getDefaultElementNamespace();
        Saxon.Api.UnprefixedElementMatchingPolicy getUnprefixedElementMatchingPolicy();
        Saxon.Hej.om.NamespaceUri getDefaultFunctionNamespace();
        bool isInBackwardsCompatibleMode();
        bool isImportedSchema(Saxon.Hej.om.NamespaceUri @namespace);
        System.Collections.Generic.ISet<Saxon.Hej.om.NamespaceUri> getImportedSchemaNamespaces();
        Saxon.Hej.om.NamespaceResolver getNamespaceResolver();
        Saxon.Hej.type.ItemType getRequiredContextItemType();
        Saxon.Hej.trans.DecimalFormatManager getDecimalFormatManager();
        int getXPathVersion();
        Saxon.Hej.trans.KeyManager getKeyManager();
        Saxon.Hej.type.ItemType resolveTypeAlias(Saxon.Hej.om.StructuredQName typeName);
        Saxon.Hej.expr.parser.OptimizerOptions getOptimizerOptions();
    
    }
}