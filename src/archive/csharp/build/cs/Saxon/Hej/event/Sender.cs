using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public abstract class Sender {
        private Sender()  {
        }
        public static void send(Saxon.Ejavax.xml.transform.Source source, Saxon.Hej.@event.Receiver receiver, Saxon.Hej.lib.ParseOptions options) {
            Saxon.Hej.@event.PipelineConfiguration pipe = receiver.getPipelineConfiguration();
            if ((options == null)) {
                options = new Saxon.Hej.lib.ParseOptions(pipe.getParseOptions());
            } else {
                options = new Saxon.Hej.lib.ParseOptions(options);
            }
            string systemId = source.getSystemId();
            Saxon.Hej.lib.AugmentedSource originalAugmentedSource = null;
            if ((source is Saxon.Hej.lib.AugmentedSource)) {
                originalAugmentedSource = (Saxon.Hej.lib.AugmentedSource)(source);
                options.merge(((Saxon.Hej.lib.AugmentedSource)(source)).getParseOptions());
                systemId = source.getSystemId();
                source = ((Saxon.Hej.lib.AugmentedSource)(source)).getContainedSource();
            }
            Saxon.Hej.Configuration config = pipe.getConfiguration();
            options.applyDefaults(config);
            receiver.setSystemId(systemId);
            Saxon.Hej.@event.Receiver next = receiver;
            System.Collections.Generic.IList<Saxon.Hej.@event.FilterFactory> filters = options.getFilters();
            if ((filters != null)) {
                for (int i = (filters.Count-1); (i>=0); i--) {
                    Saxon.Hej.@event.FilterFactory ff = filters[i];
                    Saxon.Hej.@event.Receiver filter = /*Delegate.makeFilter()*/ff(next);
                    filter.setSystemId(source.getSystemId());
                    next = filter;
                }
            }
            next = makeValidator(next, source.getSystemId(), options);
            Saxon.Hej.om.SpaceStrippingRule strippingRule = options.getSpaceStrippingRule();
            if (((strippingRule != null) && !(((strippingRule is Saxon.Hej.om.NoElementsSpaceStrippingRule))))) {
                next = strippingRule.makeStripper(next);
            }
            if (!(((source is Saxon.Hej.lib.ActiveSource)))) {
                source = config.resolveSource(source, config);
            }
            if ((source == null)) {
                throw new Saxon.Hej.trans.XPathException((("A source of type "+source.GetType().Name)+" is not supported in this environment"));
            }
            ((Saxon.Hej.lib.ActiveSource)(source)).deliver(next, options);
            if (((originalAugmentedSource != null) && originalAugmentedSource.isPleaseCloseAfterUse())) {
                originalAugmentedSource.close();
            }
        }
        public static void sendDocumentInfo(Saxon.Hej.om.NodeInfo top, Saxon.Hej.@event.Receiver receiver, Saxon.Hej.s9api.Location location) {
            Saxon.Hej.@event.PipelineConfiguration pipe = receiver.getPipelineConfiguration();
            Saxon.Hej.om.NamePool targetNamePool = pipe.getConfiguration().getNamePool();
            if ((top.getConfiguration().getNamePool() != targetNamePool)) {
                receiver = new Saxon.Hej.@event.NamePoolConverter(receiver, top.getConfiguration().getNamePool(), targetNamePool);
            }
            Saxon.Hej.@event.LocationCopier copier = new Saxon.Hej.@event.LocationCopier((top.getNodeKind() == Saxon.Hej.type.Type.DOCUMENT), location.getSystemId());
            pipe.setComponent(typeof(Saxon.Hej.@event.CopyInformee).Name, copier);
            pipe.setCopyInformee(element => copier.notifyElementNode(element));
            receiver.open();
            switch (top.getNodeKind()) {
              case Saxon.Hej.type.Type.DOCUMENT:
                top.copy(receiver, (Saxon.Hej.om.CopyOptions.ALL_NAMESPACES|Saxon.Hej.om.CopyOptions.TYPE_ANNOTATIONS), location);
                break ;
              case Saxon.Hej.type.Type.ELEMENT:
                receiver.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
                top.copy(receiver, (Saxon.Hej.om.CopyOptions.ALL_NAMESPACES|Saxon.Hej.om.CopyOptions.TYPE_ANNOTATIONS), location);
                receiver.endDocument();
                break ;
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException("Expected document or element node");
            }
            receiver.close();
        }
        public static Saxon.Hej.@event.Receiver makeValidator(Saxon.Hej.@event.Receiver receiver, string systemId, Saxon.Hej.lib.ParseOptions options) {
            Saxon.Hej.@event.PipelineConfiguration pipe = receiver.getPipelineConfiguration();
            Saxon.Hej.Configuration config = pipe.getConfiguration();
            int sv = options.getSchemaValidationMode();
            if (((sv != Saxon.Hej.lib.Validation.PRESERVE) && (sv != Saxon.Hej.lib.Validation.DEFAULT))) {
                Saxon.Hej.Controller controller = pipe.getController();
                if ((((controller != null) && !(controller.getExecutable().isSchemaAware())) && (sv != Saxon.Hej.lib.Validation.STRIP))) {
                    throw new Saxon.Hej.trans.XPathException("Cannot use schema-validated input documents when the query/stylesheet is not schema-aware");
                }
                if (!(((receiver is Saxon.Hej.@event.DocumentValidator)))) {
                    receiver = config.getDocumentValidator(receiver, systemId, options, null);
                }
            }
            return receiver;
        }
    
    }
}