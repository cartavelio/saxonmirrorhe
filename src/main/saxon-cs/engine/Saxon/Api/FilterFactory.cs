using net.sf.saxon.@event;

namespace Saxon.Api {
    internal class FilterFactory : net.sf.saxon.@event.FilterFactory
    {
        net.sf.saxon.om.SpaceStrippingRule stripperRules;
        public FilterFactory(net.sf.saxon.om.SpaceStrippingRule sr)
        {
            stripperRules = sr;
        }
        public ProxyReceiver makeFilter(Receiver r)
        {
            return new net.sf.saxon.@event.Stripper(stripperRules, r);
        }

        Receiver net.sf.saxon.@event.FilterFactory.makeFilter(Receiver r)
        {
            return (Receiver)new Stripper(stripperRules, r);
        }
    }
}