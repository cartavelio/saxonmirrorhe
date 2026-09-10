using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class FeatureIndex {

        static FeatureIndex() {
            Saxon.Hej.lib.FeatureData.init();
            foreach (Saxon.Hej.lib.FeatureData data in Saxon.Hej.lib.FeatureData.featureList) {
                Saxon.Impl.Helpers.MapUtils.Put(byName, data.uri, data);
                byCode.put(data.code, data);
            }
        }
        private readonly static System.Collections.Generic.IDictionary<string,Saxon.Hej.lib.FeatureData> byName = new System.Collections.Generic.Dictionary<string, Saxon.Hej.lib.FeatureData>();
        private readonly static Saxon.Hej.z.IntHashMap<Saxon.Hej.lib.FeatureData> byCode = new Saxon.Hej.z.IntHashMap<Saxon.Hej.lib.FeatureData>();
        public static System.Collections.Generic.IEnumerable<string> getNames() {
            return new System.Collections.Generic.SortedSet<string>(Saxon.Impl.Helpers.MapUtils.KeySet(byName));
        }
        public static bool exists(string featureName) {
            return byName.ContainsKey(featureName);
        }
        public static Saxon.Hej.lib.FeatureData getData(string featureName) {
            return Saxon.Impl.Helpers.MapUtils.Get(byName, featureName);
        }
        public static Saxon.Hej.lib.FeatureData getData(int code) {
            return byCode.get(code);
        }
    
    }
}