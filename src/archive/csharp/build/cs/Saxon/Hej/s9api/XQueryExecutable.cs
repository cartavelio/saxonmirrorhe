using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    internal class XQueryExecutable {
        internal Saxon.Hej.s9api.Processor processor;
        internal Saxon.Hej.query.XQueryExpression exp;
        protected internal XQueryExecutable(Saxon.Hej.s9api.Processor processor, Saxon.Hej.query.XQueryExpression exp)  {
            this.processor = processor;
            this.exp = exp;
        }
        public Saxon.Hej.s9api.XQueryEvaluator load() {
            return new Saxon.Hej.s9api.XQueryEvaluator(processor, exp);
        }
        public Saxon.Api.XdmItemType getResultItemType() {
            Saxon.Hej.type.ItemType it = exp.getExpression().getItemType();
            return new Saxon.Hej.s9api.ConstructedItemType(it, processor.getUnderlyingConfiguration());
        }
        public Saxon.Hej.s9api.OccurrenceIndicator getResultCardinality() {
            int card = exp.getExpression().getCardinality();
            return Saxon.Hej.s9api.OccurrenceIndicator.getOccurrenceIndicator(card);
        }
        public bool isUpdateQuery() {
            return exp.isUpdateQuery();
        }
        public void explain(Saxon.Hej.s9api.Destination destination) {
            Saxon.Hej.Configuration config = processor.getUnderlyingConfiguration();
            try {
                Saxon.Hej.@event.PipelineConfiguration pipe = config.makePipelineConfiguration();
                exp.explain(new Saxon.Hej.trace.ExpressionPresenter(config, destination.getReceiver(pipe, config.obtainDefaultSerializationProperties())));
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Hej.query.XQueryExpression getUnderlyingCompiledQuery() {
            return exp;
        }
    
    }
}