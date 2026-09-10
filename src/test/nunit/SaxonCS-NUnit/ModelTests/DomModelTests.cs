using System.IO;
using System.Xml;
using Saxon.Api;

namespace SaxonCS_NUnit.ModelTests; 

public class DomModelTests : ObjectModelTests {
    protected override string getTreeModel() {
        return "dom";
    }

    protected override XdmNode buildExternalDocument(Processor processor, string fileName) {
        XmlDocument doc = new() {
            PreserveWhitespace = true
        };
        doc.Load(ConfigTest.DATA_DIR + fileName);
        return processor.NewDocumentBuilder().Wrap(doc);
    }
    
    
}