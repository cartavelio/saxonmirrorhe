using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class FunctionAnnotations : Saxon.Hej.functions.SystemFunction {
        public readonly static Saxon.Hej.ma.map.RecordTest RETURN_TYPE = Saxon.Hej.ma.map.RecordTest.nonExtensible(new Saxon.Hej.ma.map.RecordTest.Field("name", Saxon.Hej.value.SequenceType.SINGLE_QNAME, false), new Saxon.Hej.ma.map.RecordTest.Field("params", Saxon.Hej.value.SequenceType.ATOMIC_SEQUENCE, false));
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.FunctionItem fn = (Saxon.Hej.om.FunctionItem)(arguments[0].head());
            Saxon.Hej.query.AnnotationList annotations = fn.getAnnotations();
            System.Collections.Generic.IList<Saxon.Hej.om.Item> result = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
            foreach (Saxon.Hej.query.Annotation ann in annotations) {
                Saxon.Hej.ma.map.DictionaryMap map = new Saxon.Hej.ma.map.DictionaryMap();
                map.initialPut("name", new Saxon.Hej.value.QNameValue(ann.getAnnotationQName(), Saxon.Hej.type.BuiltInAtomicType.QNAME));
                map.initialPut("params", new Saxon.Hej.om.AtomicArray(ann.getAnnotationParameters()));
                result.Add(map);
            }
            return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(result);
        }
    
    }
}