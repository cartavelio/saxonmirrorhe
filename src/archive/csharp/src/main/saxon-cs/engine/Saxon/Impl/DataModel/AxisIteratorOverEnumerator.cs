namespace Saxon.Impl.DataModel {

    public class AxisIteratorOverEnumerator<J>  : Saxon.Hej.tree.iter.AxisIterator {
        private readonly System.Collections.Generic.IEnumerator<J> baseEnumerator;
        private readonly System.Func<J,Saxon.Hej.om.Item> mapper;
        public AxisIteratorOverEnumerator(System.Collections.Generic.IEnumerator<J> baseEnumerator, System.Func<J,Saxon.Hej.om.Item> mapper)  {
            this.baseEnumerator = baseEnumerator;
            this.mapper = mapper;
        }
        public Saxon.Hej.om.Item next() {
            if (baseEnumerator.MoveNext()) {
                return mapper(baseEnumerator.Current);
            } else {
                return null;
            }
        }

        /* Expanding default method close from interface SequenceIterator */
        public virtual void close() {
        }

        /* Expanding default method discharge from interface SequenceIterator */
        public virtual void discharge() {
        }

    }
    

}