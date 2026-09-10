using System;
using System.Xml;
using Saxon.Api;

namespace SaxonCS_NUnit.ModelTests;

public class TinyTreeTests : ObjectModelTests {
    protected override string getTreeModel() {
        return "tiny";
    }

    protected override XdmNode buildExternalDocument(Processor processor, string fileName) {
        DocumentBuilder builder = processor.NewDocumentBuilder();
        builder.TreeModel = TreeModel.TinyTree;
        return builder.Build(new Uri(ConfigTest.DataDirUri + fileName));
    }
    
    
}