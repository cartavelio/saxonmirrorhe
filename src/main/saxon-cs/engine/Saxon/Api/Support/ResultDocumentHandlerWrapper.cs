using System;
using System.Collections;
using javax.xml.transform;
using net.sf.saxon.@event;
using net.sf.saxon.lib;

namespace Saxon.Api.Support {
    ///<summary>Internal wrapper class for <c>IResultDocumentHandler</c></summary>
    internal class ResultDocumentHandlerWrapper : ResultDocumentResolver
    {

        private IResultDocumentHandler handler;
        private ArrayList resultList = new ArrayList();
        private ArrayList destinationList = new ArrayList();
        private PipelineConfiguration pipe;

        /// <summary>
        /// Initializes a new instance of the <see cref="Saxon.Api.ResultDocumentHandlerWrapper"/> class.
        /// </summary>
        /// <param name="handler">Handler.</param>
        /// <param name="pipe">Pipe.</param>
        public ResultDocumentHandlerWrapper(IResultDocumentHandler handler, PipelineConfiguration pipe)
        {
            this.handler = handler;
            this.pipe = pipe;
        }




        /// <summary>
        /// Close the specified result.
        /// </summary>
        /// <param name="result">Result.</param>
        public void close(Result result)
        {
            for (int i = 0; i < resultList.Count; i++)
            {
                if (Object.ReferenceEquals(resultList[i], result))
                {
                    ((XmlDestination)destinationList[i]).GetUnderlyingDestination().close();
                    resultList.RemoveAt(i);
                    destinationList.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Resolve the specified href and baseString.
        /// </summary>
        /// <param name="href">Href.</param>
        /// <param name="baseString">Base string.</param>
        public Receiver resolve(net.sf.saxon.expr.XPathContext xpc, string href, string baseString, net.sf.saxon.serialize.SerializationProperties sp)
        {
            Uri baseUri;
            try
            {
                baseUri = new Uri(baseString);
            }
            catch (System.UriFormatException err)
            {
                throw new TransformerException("Invalid base output URI " + baseString, err);
            }
            XmlDestination destination = handler.HandleResultDocument(href, baseUri);
            Receiver result = destination.GetUnderlyingDestination().getReceiver(pipe, sp);
            resultList.Add(result);
            destinationList.Add(destination);
            return result;
        }
    }
}