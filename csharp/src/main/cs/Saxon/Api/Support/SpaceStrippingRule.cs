using System;
using Saxon.Hej.@event;
using Saxon.Hej.om;
using Saxon.Hej.trace;
using Saxon.Hej.type;

namespace Saxon.Api.Support {
    internal class SpaceStrippingRule : Saxon.Hej.om.SpaceStrippingRule
    {
        private readonly System.Predicate<QName> elementTest;

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
            return new Saxon.Hej.@event.Stripper(this, r);
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited