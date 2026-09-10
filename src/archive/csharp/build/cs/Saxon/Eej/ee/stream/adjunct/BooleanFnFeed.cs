using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public abstract class BooleanFnFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
        private int count;
        private int level;
        private bool foundAtomic;
        private bool result;
        private bool done;
        internal BooleanFnFeed(Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (exp, result, context) {
        }
        public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
            base.open(terminator);
            count = 0;
            level = 0;
            result = !(isPositive());
        }
        protected internal abstract bool isPositive();
        private void testNode() {
            if (((level == 0) && !(hasFailed()))) {
                if ((++count == 1)) {
                    getNextOutputter().append(Saxon.Hej.value.BooleanValue.get(isPositive()));
                    getNextOutputter().close();
                    done = true;
                    getTerminator().terminate();
                } else {
                    if (foundAtomic) {
                        try {
                            Saxon.Hej.expr.parser.ExpressionTool.ebvError("a sequence of two or more items starting with an atomic value", getExpression());
                        } catch (Saxon.Hej.trans.XPathException e) {
                            dynamicError(e);
                        }
                    }
                }
            }
        }
        public override void append(Saxon.Hej.om.Item item) {
            if (!(hasFailed())) {
                if ((item is Saxon.Hej.om.NodeInfo)) {
                    testNode();
                } else {
                    if ((++count == 1)) {
                        try {
                            result = (Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(item)^!(isPositive()));
                        } catch (Saxon.Hej.trans.XPathException e) {
                            dynamicError(e);
                            return ;
                        }
                        foundAtomic = (item is Saxon.Hej.value.AtomicValue);
                    } else {
                        if (foundAtomic) {
                            try {
                                Saxon.Hej.expr.parser.ExpressionTool.ebvError("a sequence of two or more items starting with an atomic value", getExpression());
                            } catch (Saxon.Hej.trans.XPathException e) {
                                dynamicError(e);
                            }
                        }
                    }
                }
            }
        }
        public override void startDocument(int properties) {
            testNode();
            level++;
        }
        public override void endDocument() {
            level--;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType typeCode, Saxon.Hej.s9api.Location location, int properties) {
            testNode();
            level++;
        }
        public override void @namespace(string prefix, string namespaceUri, int properties) {
            testNode();
        }
        public override void endElement() {
            level--;
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location location, int properties) {
            testNode();
        }
        public override void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties) {
            testNode();
        }
        public override void attribute(Saxon.Hej.om.NodeName attName, Saxon.Hej.type.SimpleType typeCode, string value, Saxon.Hej.s9api.Location location, int properties) {
            testNode();
        }
        public override void close() {
            if ((!(hasFailed()) && !(done))) {
                getNextOutputter().append(Saxon.Hej.value.BooleanValue.get(result));
                base.close();
                done = true;
            }
        }
        public class Positive : Saxon.Eej.ee.stream.adjunct.BooleanFnFeed {
            public Positive(Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (exp, result, context) {
            }
            protected internal override bool isPositive() {
                return true;
            }
        
        }
        public class Negative : Saxon.Eej.ee.stream.adjunct.BooleanFnFeed {
            internal Negative(Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (exp, result, context) {
            }
            protected internal override bool isPositive() {
                return false;
            }
        
        }
    
    }
}