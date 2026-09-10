using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.config {
    public class MultithreadingFactory {
        private readonly Saxon.Eej.config.EnterpriseConfiguration config;
        public MultithreadingFactory(Saxon.Eej.config.EnterpriseConfiguration config)  {
            this.config = config;
        }
        public Saxon.Hej.om.SequenceIterator makeMultithreadedItemMappingIterator(Saxon.Hej.om.SequenceIterator @base, Saxon.Hej.expr.ItemMappingFunction action) {
            return new Saxon.Impl.Parallel.MultithreadedItemMappingIterator(@base, action, this);
        }
        public Saxon.Hej.expr.XPathContextMajor.ThreadManager makeThreadManager() {
            return new Saxon.Eej.ee.parallel.ThreadManagerEE(config);
        }
        public Saxon.Ejava.util.concurrent.ExecutorService makeExecutorService(int threads) {
            return null;
        }
        public Saxon.Ejava.lang.Thread makeThread(Saxon.Ejava.lang.Runnable runnable) {
            return new Saxon.Ejava.lang.Thread(runnable);
        }
    
    }
}