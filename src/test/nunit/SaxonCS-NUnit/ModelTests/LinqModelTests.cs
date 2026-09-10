using System.IO;
using System.Xml;
using System.Xml.Linq;
using Saxon.Api;

namespace SaxonCS_NUnit.ModelTests;

public class LinqModelTests : ObjectModelTests {
    protected override string getTreeModel() {
        return "linq";
    }

    protected override XdmNode buildExternalDocument(Processor processor, string fileName) {
        XDocument doc = XDocument.Load(
            new FileStream(ConfigTest.DATA_DIR + fileName, FileMode.Open),
            LoadOptions.PreserveWhitespace);
        return processor.NewDocumentBuilder().Wrap(doc);
    }
    
    protected override bool mergesAdjacentTextNodes() {
        return false;
    }
    
    
}