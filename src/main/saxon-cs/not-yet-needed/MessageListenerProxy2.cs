using System;
using javax.xml.transform;

namespace Saxon.Api {
    /// <summary>
    /// Message listener proxy. This class implements a <c>net.sf.saxon.s9api.MessageListener2</c> that can receive 
    /// <c>xsl:message</c> output and send it to a user-supplied <c>MessageListener</c>
    /// </summary>
    [Serializable]
    internal class MessageListenerProxy2 : net.sf.saxon.s9api.MessageListener2
    {

        public IMessageListener2 listener;

        /// <summary>
        /// Initializes a new instance of the <see cref="Saxon.Api.MessageListenerProxy"/> class.
        /// </summary>
        /// <param name="ml">ml.</param>
        public MessageListenerProxy2(IMessageListener2 ml)
        {
            listener = ml;
        }


        public void message(XdmNode content, QName errorCode, bool terminate, SourceLocator locator)
        {
            IXmlLocation location = new XmlLocation();
            location.BaseUri = new Uri(locator.getSystemId());
            location.LineNumber = locator.getLineNumber();
            listener.Message((XdmNode)XdmValue.Wrap(content.getUnderlyingValue()), new QName(errorCode),terminate, location);
        }
    }
}