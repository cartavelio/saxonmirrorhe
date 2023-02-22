using System;
using javax.xml.transform;

namespace Saxon.Api {
    /// <summary>
    /// Message listener proxy. This class implements a <c>net.sf.saxon.s9api.MessageListener</c> that can receive 
    /// <c>xsl:message</c> output and send it to a user-supplied <c>MessageListener</c>
    /// </summary>
    [Serializable]
    internal class MessageListenerProxy : net.sf.saxon.s9api.MessageListener
    {

        public IMessageListener listener;

        /// <summary>
        /// Initializes a new instance of the <see cref="Saxon.Api.MessageListenerProxy"/> class.
        /// </summary>
        /// <param name="ml">ml.</param>
        public MessageListenerProxy(IMessageListener ml)
        {
            listener = ml;
        }


        public void message(net.sf.saxon.s9api.XdmNode xn, bool b, SourceLocator sl)
        {
            IXmlLocation location = new XmlLocation();
            location.BaseUri = new Uri(sl.getSystemId());
            location.LineNumber = sl.getLineNumber();
            listener.Message((XdmNode)XdmValue.Wrap(xn.getUnderlyingValue()), b, location);
        }
    }
}