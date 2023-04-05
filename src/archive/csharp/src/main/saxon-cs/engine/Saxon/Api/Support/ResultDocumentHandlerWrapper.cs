using System;
using System.Collections;
using Saxon.Ejavax.xml.transform;
using Saxon.Hej.@event;
using Saxon.Hej.lib;

namespace Saxon.Api.Support {
    ///<summary>Internal wrapper class for <c>IResultDocumentHandler</c></summary>
    internal class ResultDocumentHandlerWrapper : ResultDocumentResolver
    {

        private readonly ResultDocumentHandler handler;
        private readonly ArrayList resultList = new ();
        private readonly ArrayList destinationList = new();
        private readonly PipelineConfiguration pipe;

        /// <summary>
        /// Initializes a new instance of the <see cref="Saxon.Api.Support.ResultDocumentHandlerWrapper"/> class.
        /// </summary>
        /// <param name="handler">Handler.</param>
        /// <param name="pipe">Pipe.</param>
        public ResultDocumentHandlerWrapper(ResultDocumentHandler handler, PipelineConfiguration pipe)
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
                if (ReferenceEquals(resultList[i], result))
                {
                    ((IDestination)destinationList[i]).UnderlyingDestination.close();
                    resultList.RemoveAt(i);
                    destinationList.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Get a Receiver for the specified secondary result document.
        /// </summary>
        /// <param name="xpc">XPath dynamic context</param>
        /// <param name="href">Relative URI</param>
        /// <param name="baseString">Base URI.</param>
        /// <param name="sp">Serialization parameters</param>
        public Receiver resolve(Saxon.Hej.expr.XPathContext xpc, string href, string baseString, Saxon.Hej.serialize.SerializationProperties sp)
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
            IDestination destination = handler(href, baseUri);
            Receiver result = destination.UnderlyingDestination.getReceiver(pipe, sp);
            resultList.Add(result);
            destinationList.Add(destination);
            return result;
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited