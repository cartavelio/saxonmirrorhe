using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pull {
    public class PullSource : Saxon.Hej.lib.ActiveSource {
        private string systemId;
        private readonly Saxon.Hej.pull.PullProvider provider;
        public PullSource(Saxon.Hej.pull.PullProvider provider)  {
            this.provider = provider;
            if ((provider.getSourceLocator() != null)) {
                systemId = provider.getSourceLocator().getSystemId();
            }
        }
        public Saxon.Hej.pull.PullProvider getPullProvider() {
            return provider;
        }
        public void setSystemId(string systemId) {
            this.systemId = systemId;
        }
        public string getSystemId() {
            return systemId;
        }
        public void deliver(Saxon.Hej.@event.Receiver receiver, Saxon.Hej.lib.ParseOptions options) {
            Saxon.Hej.@event.PipelineConfiguration pipe = receiver.getPipelineConfiguration();
            bool xInclude = options.isXIncludeAware();
            if (xInclude) {
                throw new Saxon.Hej.trans.XPathException("XInclude processing is not supported with a pull parser");
            }
            receiver = Saxon.Hej.@event.Sender.makeValidator(receiver, getSystemId(), options);
            Saxon.Hej.pull.PullProvider provider = getPullProvider();
            provider.setPipelineConfiguration(pipe);
            receiver.setPipelineConfiguration(pipe);
            Saxon.Hej.pull.PullPushCopier copier = new Saxon.Hej.pull.PullPushCopier(provider, receiver);
            try {
                copier.copy();
            } finally {
                if (options.isPleaseCloseAfterUse()) {
                    provider.close();
                }
            }
        }
    
    }
}