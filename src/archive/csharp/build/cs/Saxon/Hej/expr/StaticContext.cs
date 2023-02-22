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
        void issueWarning(string message, Saxon.Hej.s9api.Location locator);
        string getSystemId();
        string getStaticBaseURI();
        Saxon.Hej.expr.Expression bindVariable(Saxon.Hej.om.StructuredQName qName);
        Saxon.Hej.functions.FunctionLibrary getFunctionLibrary();
        string getDefaultCollationName();
        string getDefaultElementNamespace();
        Saxon.Api.UnprefixedElementMatchingPolicy getUnprefixedElementMatchingPolicy();
        string getDefaultFunctionNamespace();
        bool isInBackwardsCompatibleMode();
        bool isImportedSchema(string @namespace);
        System.Collections.Generic.ISet<string> getImportedSchemaNamespaces();
        Saxon.Hej.om.NamespaceResolver getNamespaceResolver();
        Saxon.Hej.type.ItemType getRequiredContextItemType();
        Saxon.Hej.trans.DecimalFormatManager getDecimalFormatManager();
        int getXPathVersion();
        Saxon.Hej.trans.KeyManager getKeyManager();
        Saxon.Hej.type.ItemType resolveTypeAlias(Saxon.Hej.om.StructuredQName typeName);
        Saxon.Hej.expr.parser.OptimizerOptions getOptimizerOptions();
    
    }
}