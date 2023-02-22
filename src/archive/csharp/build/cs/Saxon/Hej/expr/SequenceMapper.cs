using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class SequenceMapper : Saxon.Hej.expr.MappingFunction {
        public delegate Saxon.Hej.om.SequenceIterator Lambda (Saxon.Hej.om.Item item);
        private readonly Saxon.Hej.expr.SequenceMapper.Lambda lambda;
        private SequenceMapper(Saxon.Hej.expr.SequenceMapper.Lambda lambda)  {
            this.lambda = lambda;
        }
        public static Saxon.Hej.expr.SequenceMapper of(Saxon.Hej.expr.SequenceMapper.Lambda lambda) {
            return new Saxon.Hej.expr.SequenceMapper(lambda);
        }
        public Saxon.Hej.om.SequenceIterator map(Saxon.Hej.om.Item item) {
            return /*Delegate.map()*/lambda(item);
        }
    
    }
}