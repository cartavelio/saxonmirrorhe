using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.registry {
    public class XSLT30FunctionSet : Saxon.Hej.functions.registry.BuiltInFunctionSet {
        private static readonly Saxon.Hej.functions.registry.XSLT30FunctionSet THE_INSTANCE = new Saxon.Hej.functions.registry.XSLT30FunctionSet();
        public static Saxon.Hej.functions.registry.XSLT30FunctionSet getInstance() {
            return THE_INSTANCE;
        }
        private XSLT30FunctionSet()  {
            init();
        }
        private void init() {
            importFunctionSet(Saxon.Hej.functions.registry.XPath40FunctionSet.getInstance());
            register("accumulator-after", 1, typeof(Saxon.Hej.functions.AccumulatorFn.AccumulatorAfter), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|CITEM)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("accumulator-before", 1, typeof(Saxon.Hej.functions.AccumulatorFn.AccumulatorBefore), Saxon.Hej.type.AnyItemType.getInstance(), STAR, (LATE|CITEM)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("available-system-properties", 0, typeof(Saxon.Hej.functions.AvailableSystemProperties), Saxon.Hej.type.BuiltInAtomicType.QNAME, STAR, LATE);
            register("current", 0, typeof(Saxon.Hej.functions.Current), Saxon.Hej.type.Type.ITEM_TYPE, ONE, LATE);
            register("current-group", 0, typeof(Saxon.Hej.functions.CurrentGroup), Saxon.Hej.type.Type.ITEM_TYPE, STAR, LATE);
            register("current-grouping-key", 0, typeof(Saxon.Hej.functions.CurrentGroupingKey), Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, STAR, LATE);
            register("current-merge-group", 0, typeof(Saxon.Hej.functions.CurrentMergeGroup), Saxon.Hej.type.AnyItemType.getInstance(), STAR, LATE);
            register("current-merge-group", 1, typeof(Saxon.Hej.functions.CurrentMergeGroup), Saxon.Hej.type.AnyItemType.getInstance(), STAR, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("current-merge-key", 0, typeof(Saxon.Hej.functions.CurrentMergeKey), Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, STAR, LATE);
            register("current-output-uri", 0, typeof(Saxon.Hej.functions.CurrentOutputUri), Saxon.Hej.type.BuiltInAtomicType.ANY_URI, OPT, LATE);
            register("document", 1, typeof(Saxon.Hej.functions.DocumentFn), Saxon.Hej.type.Type.NODE_TYPE, STAR, ((BASE|LATE)|UO)).arg(0, Saxon.Hej.type.Type.ITEM_TYPE, STAR, null);
            register("document", 2, typeof(Saxon.Hej.functions.DocumentFn), Saxon.Hej.type.Type.NODE_TYPE, STAR, ((BASE|LATE)|UO)).arg(0, Saxon.Hej.type.Type.ITEM_TYPE, STAR, null).arg(1, Saxon.Hej.type.Type.NODE_TYPE, ONE, null);
            register("element-available", 1, typeof(Saxon.Hej.functions.ElementAvailable), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, NS).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("function-available", 1, typeof(Saxon.Hej.functions.FunctionAvailable), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, (NS|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("function-available", 2, typeof(Saxon.Hej.functions.FunctionAvailable), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, (NS|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null);
            register("key", 2, typeof(Saxon.Hej.functions.KeyFn), Saxon.Hej.type.Type.NODE_TYPE, STAR, ((CDOC|NS)|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, STAR, EMPTY);
            register("key", 3, typeof(Saxon.Hej.functions.KeyFn), Saxon.Hej.type.Type.NODE_TYPE, STAR, (NS|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, STAR, EMPTY).arg(2, Saxon.Hej.type.Type.NODE_TYPE, ONE, null);
            register("regex-group", 1, typeof(Saxon.Hej.functions.RegexGroup), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, (LATE|SIDE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null);
            register("stream-available", 1, typeof(Saxon.Hej.functions.StreamAvailable), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, LATE).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, OPT, null);
            register("system-property", 1, typeof(Saxon.Hej.functions.SystemProperty), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, (NS|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("type-available", 1, typeof(Saxon.Hej.functions.TypeAvailable), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, NS).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("unparsed-entity-public-id", 1, typeof(Saxon.Hej.functions.UnparsedEntity.UnparsedEntityPublicId), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, (CDOC|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("unparsed-entity-public-id", 2, typeof(Saxon.Hej.functions.UnparsedEntity.UnparsedEntityPublicId), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.Type.NODE_TYPE, ONE, null);
            register("unparsed-entity-uri", 1, typeof(Saxon.Hej.functions.UnparsedEntity.UnparsedEntityUri), Saxon.Hej.type.BuiltInAtomicType.ANY_URI, ONE, (CDOC|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
            register("unparsed-entity-uri", 2, typeof(Saxon.Hej.functions.UnparsedEntity.UnparsedEntityUri), Saxon.Hej.type.BuiltInAtomicType.ANY_URI, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.Type.NODE_TYPE, ONE, null);
        }
    
    }
}