using System;
using net.sf.saxon.@event;
using net.sf.saxon.om;
using net.sf.saxon.trace;
using net.sf.saxon.type;

namespace Saxon.Api {
    internal class SpaceStrippingRule : net.sf.saxon.om.SpaceStrippingRule
    {
        private System.Predicate<QName> elementTest;

        public SpaceStrippingRule(System.Predicate<QName> elementTest)
        {
            this.elementTest = elementTest;
        }

        public void export(ExpressionPresenter ep)
        {
            throw new NotImplementedException(); 
        }


        public int isSpacePreserving(NodeName nn, SchemaType st)
        {
            return elementTest(new QName(nn.getStructuredQName().ToString())) ?
                Stripper.ALWAYS_STRIP :
                Stripper.ALWAYS_PRESERVE;
        }

        public ProxyReceiver makeStripper(Receiver r)
        {
            return new net.sf.saxon.@event.Stripper(this, r);
        }
    }
}