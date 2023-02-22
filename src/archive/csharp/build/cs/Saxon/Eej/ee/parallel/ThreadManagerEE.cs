using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.parallel {
    public class ThreadManagerEE : Saxon.Hej.expr.XPathContextMajor.ThreadManager {
        private readonly Saxon.Eej.config.EnterpriseConfiguration config;
        private Saxon.Ejava.util.concurrent.ExecutorService executorService = null;
        private Saxon.Eej.ee.parallel.CountingCompletionService<Saxon.Eej.ee.trans.Outcome<bool>> completionService = null;
        public ThreadManagerEE(Saxon.Eej.config.EnterpriseConfiguration config)  {
            this.config = config;
        }
        public Saxon.Eej.ee.parallel.CountingCompletionService<Saxon.Eej.ee.trans.Outcome<bool>> getCompletionService() {
            if ((completionService == null)) {
                activate();
            }
            return completionService;
        }
        private void activate() { lock(this) {
            if ((completionService == null)) {
                int threads = config.getResultDocumentThreadsLimit();
                if ((threads<1)) {
                    threads = 1;
                }
                executorService = config.getMultithreadingFactory().makeExecutorService(threads);
                completionService = new Saxon.Eej.ee.parallel.CountingCompletionService<Saxon.Eej.ee.trans.Outcome<bool>>(executorService);
            }
        }}
        public override void waitForChildThreads() { lock(this) {
            if ((executorService == null)) {
                return ;
            }
            try {
                while (completionService.hasUncompletedTasks()) {
                    Saxon.Eej.ee.trans.Outcome<bool> result = completionService.take().get();
                    if (!(result.isSuccess())) {
                        System.Exception e = result.getException();
                        if (!(((e is Saxon.Hej.trans.XPathException)))) {
                            System.Console.Error.WriteLine(e);
                        }
                        throw Saxon.Hej.trans.XPathException.makeXPathException(e);
                    }
                }
            } catch (Exception e) when (e is System.Threading.ThreadInterruptedException || e is Saxon.Ejava.util.concurrent.ExecutionException) {
                System.Console.Error.WriteLine(e);
                throw new Saxon.Hej.trans.XPathException(e);
            } finally {
                executorService.shutdown();
                bool finished;
                try {
                    finished = awaitTermination();
                    if (!(finished)) {
                        System.Console.Error.WriteLine("Some child xsl:result-document threads have not finished");
                    }
                } catch (System.Threading.ThreadInterruptedException e) {
                    System.Console.Error.WriteLine(e);
                }
            }
        }}
        private bool awaitTermination() {
            return executorService.awaitTermination();
        }
    
    }
}