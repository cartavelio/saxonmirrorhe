namespace Saxon.Impl.Helpers {
    public class QueueUtils {
        public static T Poll<T>(System.Collections.Concurrent.BlockingCollection<T> queue) where T : class {
            bool taken = queue.TryTake(out T item);
            return taken ? item : null;
        }

    }
}

// Copyright (c) 2021 Saxonica Limited