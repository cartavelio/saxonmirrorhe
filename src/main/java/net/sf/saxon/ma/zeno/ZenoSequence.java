package net.sf.saxon.ma.zeno;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.str.EmptyUniString;
import net.sf.saxon.str.StringConstants;
import net.sf.saxon.str.UnicodeBuilder;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpModifiers;
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
    @CSharpModifiers(code = {"public", "override"})
    public UnicodeString getUnicodeStringValue() throws XPathException {
        switch (getLength()) {
            case 0:
                return EmptyUniString.getInstance();
            case 1:
                return this.head().getUnicodeStringValue();
            default:
                UnicodeBuilder builder = new UnicodeBuilder();
                UnicodeString separator = EmptyUniString.getInstance();
                for (Item item : chain) {
                    builder.append(separator);
                    separator = StringConstants.SINGLE_SPACE;
                    builder.append(item.getStringValue());
                }
                return builder.toUnicodeString();
        }
    }

    @Override
    @CSharpModifiers(code = {"public", "override"})
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
            if (this.getLength() == 1) {
                chain = ((ZenoSequence)items).chain.prepend(head());
            } else {
                chain = chain.addAll(((ZenoSequence) items).chain);
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

    public static class ZenoSequenceIterator implements GroundedIterator, LookaheadIterator {

        private ZenoSequence sequence;
        private Iterator<Item> chainIterator;
        private int position = 0;

        public ZenoSequenceIterator(ZenoSequence sequence) {
            this.sequence = sequence;
            this.chainIterator = sequence.chain.iterator();;
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
        public boolean supportsHasNext() {
            return true;
        }

        @Override
        public boolean hasNext() {
            return chainIterator.hasNext();
        }
    }

}

