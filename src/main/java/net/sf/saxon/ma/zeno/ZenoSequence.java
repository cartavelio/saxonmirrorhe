package net.sf.saxon.ma.zeno;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.str.EmptyUnicodeString;
import net.sf.saxon.str.StringConstants;
import net.sf.saxon.str.UnicodeBuilder;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharp;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.tree.iter.GroundedIterator;
import net.sf.saxon.tree.iter.LookaheadIterator;

import java.util.Iterator;
import java.util.List;

/**
 * An XDM sequence implemented as a ZenoChain
 */

public class ZenoSequence implements GroundedValue {

    private ZenoChain<Item> chain;

    public ZenoSequence() {
        chain = new ZenoChain<>();
    }

    public static ZenoSequence fromList(List<Item> items) {
        ZenoChain<Item> chain = new ZenoChain<Item>().addAll(items);
        return new ZenoSequence(chain);
    }

    private ZenoSequence(ZenoChain<Item> chain) {
        this.chain = chain;
    }

    @Override
    public SequenceIterator iterate() {
        return new ZenoSequenceIterator(this);
    }

    @Override
    public Item itemAt(int n) {
        try {
            return chain.get(n);
        } catch (IndexOutOfBoundsException e) {
            return null;
        }
    }

    @Override
    public Item head() {
        return chain.isEmpty() ? null : chain.get(0);
    }

    @Override
    public GroundedValue subsequence(int start, int length) {
        if (length == 1) {
            return itemAt(start);
        }
        if (start < 0) {
            start = 0;
        }
        return new ZenoSequence(chain.subList(start, start + length));
    }

    @Override
    public int getLength() {
        return chain.size();
    }

    @Override
    public UnicodeString getUnicodeStringValue() throws XPathException {
        switch (getLength()) {
            case 0:
                return EmptyUnicodeString.getInstance();
            case 1:
                return this.head().getUnicodeStringValue();
            default:
                UnicodeBuilder builder = new UnicodeBuilder();
                UnicodeString separator = EmptyUnicodeString.getInstance();
                for (Item item : chain) {
                    builder.append(separator);
                    separator = StringConstants.SINGLE_SPACE;
                    builder.append(item.getStringValue());
                }
                return builder.toUnicodeString();
        }
    }

    @Override
    public String getStringValue() throws XPathException {
        switch (getLength()) {
            case 0:
                return "";
            case 1:
                return this.head().getStringValue();
            default:
                StringBuilder builder = new StringBuilder();
                String separator = "";
                for (Item item : chain) {
                    builder.append(separator);
                    separator = " ";
                    builder.append(item.getStringValue());
                }
                return builder.toString();
        }
    }

    public void append(Item item) {
        chain = chain.add(item);
    }

    public void appendSequence(GroundedValue items) {
        if (items instanceof ZenoSequence) {
            int len = this.getLength();
            switch (len) {
                case 0:
                    chain = ((ZenoSequence) items).chain;
                    return;
                case 1:
                    chain = ((ZenoSequence) items).chain.prepend(head());
                    return;
                default:
                    chain = chain.addAll(((ZenoSequence) items).chain);
                    return;
            }
        } else {
            chain = chain.addAll(items.asIterable());
        }
    }

    public static ZenoSequence join(List<GroundedValue> segments) {
        ZenoChain<Item> list = new ZenoChain<>();
        for (GroundedValue val : segments) {
            if (val instanceof ZenoSequence) {
                list = list.concat(((ZenoSequence)val).chain);
            } else {
                for (Item item : val.asIterable()) {
                    list = list.add(item);
                }
            }
        }
        return new ZenoSequence(list);
    }

    public static class ZenoSequenceIterator implements GroundedIterator
            //#if CSHARP==false
            , LookaheadIterator
            //#endif
    {

        // This class is not a LookAheadIterator on C#, because the underlying C# enumerator has no side-effect-free
        // hasNext() operation.

        private ZenoSequence sequence;
        private Iterator<Item> chainIterator;
        private int position = 0;

        public ZenoSequenceIterator(ZenoSequence sequence) {
            this.sequence = sequence;
            //#if CSHARP==false
            this.chainIterator = sequence.chain.iterator();
            //#endif
            CSharp.emitCode("this.chainIterator = sequence.chain.GetEnumerator();");
        }

        @Override
        public Item next() {
            position++;
            return chainIterator.hasNext() ? chainIterator.next() : null;
        }

        @Override
        public boolean isActuallyGrounded() {
            return true;
        }

        @Override
        public GroundedValue getResidue() {
            return sequence.subsequence(position, Integer.MAX_VALUE);
        }

        @Override
        public GroundedValue materialize() {
            return sequence;
        }

        @Override
        @CSharpReplaceBody(code="return false;")
        public boolean supportsHasNext() {
            return true;
        }

        @Override
        @CSharpReplaceBody(code="throw new InvalidOperationException();")
        public boolean hasNext() {
            return chainIterator.hasNext();
        }
    }

}

