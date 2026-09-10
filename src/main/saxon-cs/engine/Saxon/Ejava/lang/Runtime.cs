using System;

namespace Saxon.Ejava.lang {
    public class Runtime {
        public static Runtime getRuntime() {
            return new Runtime();
        }

        public long totalMemory() {
            return 0L;
        }
        
        public long freeMemory() {
            return 0L;
        }

        public int availableProcessors() {
            return Environment.ProcessorCount;
        }
    }
}