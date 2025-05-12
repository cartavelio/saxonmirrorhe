using System;
using Saxon.Api;

namespace SaxonCS_NUnit.ModelTests;

public class LinkedTreeTests : ObjectModelTests {
    protected override string getTreeModel() {
        return "linked";
    }

    protected override XdmNode buildExternalDocument(Processor processor, string fileName) {
        DocumentBuilder builder = processor.NewDocumentBuilder();
        builder.TreeModel = TreeModel.LinkedTree;
        return builder.Build(new Uri(ConfigTest.DataDirUri + fileName));
    }


    
    
}