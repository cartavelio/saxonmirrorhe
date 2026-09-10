using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class MemoSequence : Saxon.Hej.om.Sequence {
        private readonly Saxon.Hej.om.SequenceIterator inputIterator;
        private Saxon.Hej.om.Item[] reservoir = null;
        private int used;
        private enum State {
            UNREAD,
            MAYBE_MORE,
            ALL_READ,
            BUSY,
            EMPTY,
        }
        private Saxon.Hej.om.MemoSequence.State state = Saxon.Hej.om.MemoSequence.State.UNREAD;
        public MemoSequence(Saxon.Hej.om.SequenceIterator iterator)  {
            this.inputIterator = iterator;
        }
        public Saxon.Hej.om.Item head() {
            return iterate().next();
        }
        public Saxon.Hej.om.SequenceIterator iterate() { lock(this) {
            switch (state) {
              case Saxon.Hej.om.MemoSequence.State.UNREAD:
                state = Saxon.Hej.om.MemoSequence.State.BUSY;
                if ((inputIterator is Saxon.Hej.tree.iter.EmptyIterator)) {
                    state = Saxon.Hej.om.MemoSequence.State.EMPTY;
                    return inputIterator;
                }
                reservoir = new Saxon.Hej.om.Item[50];
                used = 0;
                state = Saxon.Hej.om.MemoSequence.State.MAYBE_MORE;
                return new Saxon.Hej.om.MemoSequence.ProgressiveIterator(this);
              case Saxon.Hej.om.MemoSequence.State.MAYBE_MORE:
                return new Saxon.Hej.om.MemoSequence.ProgressiveIterator(this);
              case Saxon.Hej.om.MemoSequence.State.ALL_READ:
                switch (used) {
                  case 0:
                    state = Saxon.Hej.om.MemoSequence.State.EMPTY;
                    return Saxon.Hej.tree.iter.EmptyIterator.emptyIterator();
                  case 1:
                    System.Diagnostics.Debug.Assert((reservoir != null));
                    return Saxon.Hej.tree.iter.SingletonIterator.makeIterator(reservoir[0]);
                  default:
                    return new Saxon.Hej.tree.iter.ArrayIterator.Of<Saxon.Hej.om.Item>(reservoir, 0, used);
                }
              case Saxon.Hej.om.MemoSequence.State.BUSY:
                Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException("Attempt to access a variable while it is being evaluated");
                de.setErrorCode("XTDE0640");
                throw new Saxon.Hej.trans.UncheckedXPathException(de);
              case Saxon.Hej.om.MemoSequence.State.EMPTY:
                return Saxon.Hej.tree.iter.EmptyIterator.emptyIterator();
              default:
                throw new Saxon.Ejava.lang.IllegalStateException("Unknown iterator state");
            }
        }}
        public Saxon.Hej.om.Item itemAt(int n) { lock(this) {
            if ((n<0)) {
                return null;
            }
            if (((reservoir != null) && (n<used))) {
                return reservoir[n];
            }
            if (((state == Saxon.Hej.om.MemoSequence.State.ALL_READ) || (state == Saxon.Hej.om.MemoSequence.State.EMPTY))) {
                return null;
            }
            if ((state == Saxon.Hej.om.MemoSequence.State.UNREAD)) {
                Saxon.Hej.om.Item item = inputIterator.next();
                if ((item == null)) {
                    state = Saxon.Hej.om.MemoSequence.State.EMPTY;
                    return null;
                } else {
                    state = Saxon.Hej.om.MemoSequence.State.MAYBE_MORE;
                    reservoir = new Saxon.Hej.om.Item[50];
                    append(item);
                    if ((n == 0)) {
                        return item;
                    }
                }
            }
            int diff = ((n-used)+1);
            while ((diff-->0)) {
                Saxon.Hej.om.Item i = inputIterator.next();
                if ((i == null)) {
                    state = Saxon.Hej.om.MemoSequence.State.ALL_READ;
                    condense();
                    return null;
                }
                append(i);
                state = Saxon.Hej.om.MemoSequence.State.MAYBE_MORE;
            }
            return reservoir[n];
        }}
        private void append(Saxon.Hej.om.Item item) {
            System.Diagnostics.Debug.Assert((reservoir != null));
            if ((used>=reservoir.Length)) {
                reservoir = Saxon.Ejava.util.Arrays.copyOf(reservoir, (used*2));
            }
            reservoir[used++] = item;
        }
        private void condense() {
            if (((reservoir != null) && ((reservoir.Length-used)>30))) {
                reservoir = Saxon.Ejava.util.Arrays.copyOf(reservoir, used);
            }
        }
        public sealed class ProgressiveIterator : Saxon.Hej.om.SequenceIterator, Saxon.Hej.expr.LastPositionFinder, Saxon.Hej.tree.iter.GroundedIterator {
            private Saxon.Hej.om.MemoSequence container;
            private int position = -1;
            public ProgressiveIterator(Saxon.Hej.om.MemoSequence container)  {
                this.container = container;
            }
            public Saxon.Hej.om.MemoSequence getMemoSequence() {
                return container;
            }
            public Saxon.Hej.om.Item next() {
                lock (container) {
                    if ((position == -2)) {
                        return null;
                    }
                    if ((++position<container.used)) {
                        System.Diagnostics.Debug.Assert((container.reservoir != null));
                        return container.reservoir[position];
                    } else {
                        if ((container.state == Saxon.Hej.om.MemoSequence.State.ALL_READ)) {
                            position = -2;
                            return null;
                        } else {
                            System.Diagnostics.Debug.Assert((container.inputIterator != null));
                            Saxon.Hej.om.Item i = container.inputIterator.next();
                            if ((i == null)) {
                                container.state = Saxon.Hej.om.MemoSequence.State.ALL_READ;
                                container.condense();
                                position = -2;
                                return null;
                            }
                            position = container.used;
                            container.append(i);
                            container.state = Saxon.Hej.om.MemoSequence.State.MAYBE_MORE;
                            return i;
                        }
                    }
                }
            }
            public bool supportsGetLength() {
                return true;
            }
            public int getLength() {
                if ((container.state == Saxon.Hej.om.MemoSequence.State.ALL_READ)) {
                    return container.used;
                } else {
                    if ((container.state == Saxon.Hej.om.MemoSequence.State.EMPTY)) {
                        return 0;
                    } else {
                        int savePos = position;
                        while ((next() != null)) {
                        }
                        position = savePos;
                        return container.used;
                    }
                }
            }
            public bool isActuallyGrounded() {
                return true;
            }
            public Saxon.Hej.om.GroundedValue materialize() {
                if ((container.state == Saxon.Hej.om.MemoSequence.State.ALL_READ)) {
                    return makeExtent();
                } else {
                    if ((container.state == Saxon.Hej.om.MemoSequence.State.EMPTY)) {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    } else {
                        int savePos = position;
                        while ((next() != null)) {
                        }
                        position = savePos;
                        return makeExtent();
                    }
                }
            }
            private Saxon.Hej.om.GroundedValue makeExtent() {
                if ((container.used == container.reservoir.Length)) {
                    if ((container.used == 0)) {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    } else {
                        if ((container.used == 1)) {
                            return container.reservoir[0];
                        } else {
                            return new Saxon.Hej.value.SequenceExtent.Of<Saxon.Hej.om.Item>(container.reservoir);
                        }
                    }
                } else {
                    return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(Saxon.Impl.Helpers.ListUtils.SubList(Saxon.Ejava.util.Arrays.asList(container.reservoir), 0, container.used));
                }
            }
            public Saxon.Hej.om.GroundedValue getResidue() {
                if ((((container.state == Saxon.Hej.om.MemoSequence.State.EMPTY) || (position>=container.used)) || (position == -2))) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                } else {
                    if ((container.state == Saxon.Hej.om.MemoSequence.State.ALL_READ)) {
                        return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(Saxon.Impl.Helpers.ListUtils.SubList(Saxon.Ejava.util.Arrays.asList(container.reservoir), (position+1), container.used));
                    } else {
                        int savePos = position;
                        while ((next() != null)) {
                        }
                        position = savePos;
                        return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(Saxon.Impl.Helpers.ListUtils.SubList(Saxon.Ejava.util.Arrays.asList(container.reservoir), (position+1), container.used));
                    }
                }
            }
        
            /* Expanding default method close from interface SequenceIterator */
            public void close() {
            }

            /* Expanding default method discharge from interface SequenceIterator */
            public void discharge() {
            }

        }
    
        /* Expanding default method materialize from interface Sequence */
        public virtual Saxon.Hej.om.GroundedValue materialize() {
            return Saxon.Hej.om.SequenceTool.toGroundedValue(iterate());
        }

        /* Expanding default method makeRepeatable from interface Sequence */
        public virtual Saxon.Hej.om.Sequence makeRepeatable() {
            return this;
        }

    }
}