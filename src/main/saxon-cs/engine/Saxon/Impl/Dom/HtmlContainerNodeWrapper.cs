using net.sf.saxon.om;
using net.sf.saxon.tree.wrapper;

namespace SaxonCS3.Impl.Dom {
    /// <summary>
    /// Marker interface representing a wrapper for a document or element node in the HTML tree
    /// </summary>
    /// <remarks>In fact, it is also used for text and comment nodes, but that's just careless implementation...</remarks>
    public interface HtmlContainerNodeWrapper : NodeInfo {
        
    }
}