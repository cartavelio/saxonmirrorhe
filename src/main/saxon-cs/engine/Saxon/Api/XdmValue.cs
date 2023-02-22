using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Saxon.Ejava.lang;
using Saxon.Hej.ma.arrays;
using Saxon.Hej.ma.map;
using Saxon.Hej.om;
using Saxon.Hej.value;
using Saxon.Hej.expr;
using Exception = System.Exception;

namespace Saxon.Api {
    /// <summary>
    /// A value in the XDM data model. A value is a sequence of zero or more
    /// items, each item being an atomic value, a node, or a function item.
    /// </summary>
    /// <remarks>
    /// <para>An <c>XdmValue</c> is immutable.</para>
    /// <para>An <c>XdmValue</c> is indexed, so you can get the n'th item in the value (starting from zero) by
    /// writing <c>value[n]</c>. Alternatively, use the method <c>value.ItemAt(n)</c>.</para>
    /// <para>A sequence consisting of a single item <i>may</i> be represented
    /// as an instance of <see cref="XdmItem"/>, which is a subtype of <c>XdmValue</c>. However,
    /// there is no guarantee that all single-item sequences will be instances of
    /// <c>XdmItem</c>: if you want to ensure this, use the <see cref="Simplify()"/> property.</para>
    /// <para>There are various ways of creating an <c>XdmValue</c>. To create an atomic
    /// value, use one of the constructors on <see cref="XdmAtomicValue"/> (which is a subtype of <c>XdmValue</c>).
    /// To construct an <see cref="XdmNode"/> (another subtype) by parsing an XML document, or by wrapping a DOM document,
    /// use a <see cref="DocumentBuilder"/>. To create a sequence of values, use the <see cref="Append(XdmValue)"/>
    /// method on this class to form a list from individual items or sublists.</para>
    /// <para>An <c>XdmValue</c> is also returned as the result of evaluating a query
    /// using the XQuery and XPath interfaces.</para>
    /// <para>The subtype <see cref="XdmEmptySequence"/> represents an empty sequence: an
    /// <c>XdmValue</c> of length zero. Again, there is no guarantee that every empty sequence
    /// will be represented as an instance of <c>XdmEmptySequence</c>, unless you use
    /// the <c>Simplify()</c> method.</para>
    /// </remarks>
    [Serializable]
    public class XdmValue : IEnumerable<XdmItem> {
        internal readonly GroundedValue value;

        // Internal constructor

        internal XdmValue() {
        }

        /// <summary>
        /// Create an <c>XdmValue</c> from an enumerator of <c>XdmItem</c> objects.
        /// </summary>
        /// <param name="items">An enumerator of <c>XdmItem</c> objects</param>
        public XdmValue(IEnumerator<XdmItem> items) {
            List<Item> list = new List<Item>();
            while (items.MoveNext()) {
                list.Add((Item) items.Current.Unwrap());
            }

            value = SequenceExtent.makeSequenceExtent(list);
        }


        /// <summary>
        /// Create a value from a collection of items.
        /// </summary>
        /// <param name="items">An enumerable collection providing the items to make up the sequence. Every
        /// member of this collection must be an instance of <c>XdmItem</c>
        /// </param>
        public XdmValue(IEnumerable<XdmItem> items) {
            List<Item> list = new List<Item>();
            foreach (XdmItem c in items) {
                list.Add((Item) c.Unwrap());
            }

            value = SequenceExtent.makeSequenceExtent(list);
        }

        protected XdmValue(GroundedValue groundedValue) {
            this.value = groundedValue;
        }

        /// <summary>
        /// Get an <c>IEnumerable</c> by applying a <c>Step</c> to the items in this value. This operation
        /// is analogous to the <c>SelectMany</c> operation in C#, or to the "!" operator
        /// in XPath.
        /// </summary>
        /// <typeparam name="TInput">Input items to the <c>Step</c> function, which can be subclass of <c>XdmItem</c></typeparam>
        /// <typeparam name="TResult">Result items to the <c>Step</c> function, which can be subclass of <c>XdmItem</c></typeparam>
        /// <param name="step">The <c>Step</c> to be applied to the items in this value.</param>
        /// <returns>An <c>IEnumerable</c> of items obtained by replacing each item X in this value by the items obtained
        /// by applying the <c>Step</c> function to X.</returns>
        public IEnumerable<TResult> Select<TInput, TResult>(Step<TInput, TResult> step)
            where TInput : XdmItem
            where TResult : XdmItem {
            foreach (XdmItem item in this) {
                if (item is TInput) {
                    foreach (TResult tresult in step.Invoke((TInput) item)) {
                        yield return tresult;
                    }
                }
            }
        }


        /// <summary>
        /// Concatenate two <c>IEnumerable</c> objects of <c>XdmItem</c> objects or items of its subclass.
        /// </summary>
        /// <typeparam name="TInput">The types of object to enumerate. The object must be an <c>XdmItem</c> or a derived type.</typeparam>
        /// <param name="first">The first enumerable object</param>
        /// <param name="second">The second enumerable object</param>
        /// <returns>The enumerable object as a result of the concatenation.</returns>
        public static IEnumerable<TInput> Concat<TInput>(IEnumerable<TInput> first, IEnumerable<TInput> second)
            where TInput : XdmItem {
            if (first == null) {
                throw new ArgumentNullException(nameof(first));
            }

            if (second == null) {
                throw new ArgumentNullException(nameof(second));
            }

            return ConcatImpl(first, second);
        }

        private static IEnumerable<TInput> ConcatImpl<TInput>(
            IEnumerable<TInput> first,
            IEnumerable<TInput> second)
            where TInput : XdmItem {
            foreach (TInput item in first) {
                yield return item;
            }

            first = null;
            foreach (TInput item in second) {
                yield return item;
            }
        }

        /// <summary>
        /// Get the collection of items that satisfy a supplied <c>Predicate</c>.
        /// </summary>
        /// <typeparam name="T">The types of object to enumerate. The object must be an <c>XdmItem</c> or a derived type.</typeparam>
        /// <param name="predicate">The predicate to be applied</param>
        /// <returns>An enumerable of items that satisfy the suppplied <c>Predicate</c>.</returns>
        public IEnumerable<T> Where<T>(Predicate<T> predicate)
            where T : XdmItem {
            if (predicate == null) {
                throw new ArgumentNullException(nameof(predicate));
            }

            return this.Select(new Step<T, T>(x => {
                if (!(x is T)) {
                    throw new Exception("Type error for item in XdmValue");
                }

                return Enumerable.Repeat((T) x, predicate.Invoke((T) x) ? 1 : 0);
            }));
        }

        /// <summary>
        /// Ask whether any item in this <c>XdmValue</c> matches the provided predicate.
        /// Might not evaluate the predicate on all items if not necessary for 
        /// determining the result. 
        /// </summary>
        /// <typeparam name="T">The types of object in the enumerable. The object must be an <c>XdmItem</c> or
        /// a derived type.</typeparam>
        /// <param name="predicate">The predicate to apply to items of this <c>XdmValue</c></param>
        /// <returns>True if any items of the <c>XdmValue</c> match the provided predicate, otherwise false.</returns>
        public bool AnyMatch<T>(Predicate<T> predicate)
            where T : XdmItem {
            if (predicate == null) {
                throw new ArgumentNullException(nameof(predicate));
            }

            foreach (XdmItem item in this) {
                if (item is T && predicate.Invoke((T) item)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Ask whether all items of this <c>XdmValue</c> match the provided predicate.
        /// Might not evaluate the predicate on all items if not necessary for determining the result.
        /// </summary>
        /// <typeparam name="T">The types of object in the enumerable. The object must be an <c>XdmItem</c> or a derived type.</typeparam>
        /// <param name="predicate">The predicate to apply to items of this <c>XdmValue</c></param>
        /// <returns><c>true</c> if either all items of the <c>XdmValue</c> match the provided predicate or the <c>XdmValue</c> is empty, otherwise <c>false</c></returns>
        public bool AllMatch<T>(Predicate<T> predicate)
            where T : XdmItem {
            if (predicate == null) {
                throw new ArgumentNullException(nameof(predicate));
            }

            foreach (XdmItem item in this) {
                if (item is not T xdmItem) {
                    return false;
                }

                if (!predicate.Invoke(xdmItem)) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Create a new <c>XdmValue</c> by concatenating the sequences of items in 
        /// this <c>XdmValue</c> and another <c>XdmValue</c>.
        /// </summary>
        /// <remarks>
        /// Neither of the input <c>XdmValue</c> objects is modified by this operation.
        /// </remarks>
        /// <param name="otherValue">
        /// The other <c>XdmValue</c>, whose items are to be appended to the items from this <c>XdmValue</c>.
        /// </param>
        public XdmValue Append(XdmValue otherValue) {
            List<Item> list = new List<Item>();
            foreach (XdmItem item in this) {
                list.Add((Item) item.Unwrap());
            }

            foreach (XdmItem item in otherValue) {
                list.Add((Item) item.Unwrap());
            }

            GroundedValue gv = SequenceExtent.makeSequenceExtent(list);
            return FromGroundedValue(gv);
        }

        /// <summary>
        /// Atomize a value (that is, replace nodes by their typed value)
        /// </summary>
        /// <returns>
        /// The atomized value
        /// </returns>
        public XdmValue Atomize() {
            List<Item> list = new List<Item>();
            foreach (XdmItem item in this) {
                foreach (AtomicValue val in Atomizer.atomize(item.Unwrap())) {
                    list.Add(val);
                }
            }
            return FromGroundedValue(SequenceExtent.makeSequenceExtent(list));
        }

        /// <summary>
        /// Create an <c>XdmValue</c> from an underlying Saxon <c>Sequence</c> object.
        /// </summary>
        /// <param name="value">An object representing an XDM value in the
        /// underlying Saxon implementation. If the parameter is null,
        /// the method returns null.</param>
        /// <returns>An <c>XdmValue</c> that wraps the underlying Saxon XDM value
        /// representation.</returns>
        public static XdmValue Wrap(Sequence value) {
            switch (value) {
                case null:
                    return XdmEmptySequence.Instance;
                case GroundedValue grounded:
                    return FromGroundedValue(grounded);
            }

            GroundedValue gv;
            try {
                gv = value.materialize();
            }
            catch (Exception e) {
                throw new SaxonApiException(e);
            }

            if (gv.getLength() == 0) {
                return XdmEmptySequence.Instance;
            }
            if (gv.getLength() == 1) {
                Item first = gv.head();
                return first switch {
                    AtomicValue atom => new XdmAtomicValue(atom),
                    NodeInfo node => new XdmNode(node),
                    MapItem map => new XdmMap(map),
                    ArrayItem array => new XdmArray(array),
                    Function fn => new XdmFunctionItem(fn),
                    _ => new XdmValue(first)
                };
            }
            return FromGroundedValue(gv);
            
        }

        static internal XdmValue FromGroundedValue(GroundedValue value) {
            switch (value.getLength()) {
                case 0:
                    return XdmEmptySequence.Instance;
                case 1:
                    Item first = value.head();

                    return first.getGenre() switch {
                        Genre.NODE => new XdmNode((NodeInfo)first),
                        Genre.ATOMIC => new XdmAtomicValue((AtomicValue)first),
                        Genre.MAP => new XdmMap((MapItem)first),
                        Genre.ARRAY => new XdmArray((ArrayItem)first),
                        Genre.FUNCTION => new XdmFunctionItem((Function)first),
                        Genre.EXTERNAL => new XdmExternalObject(((Saxon.Hej.value.AnyExternalObject)first).getWrappedObject()),
                        _ => throw new IllegalArgumentException()
                    };
                default:
                    return new XdmValue(value);
            }
        }

        /// <summary>
        /// Make an XDM value from a .NET object. 
        /// </summary>
        /// <remarks>
        /// The supplied object may be any of the following:
        /// <list>
        /// <item>An instance of <c>XdmValue</c> (for example an <c>XdmAtomicValue</c>, 
        /// <c>XdmMap</c>, <c>XdmArray</c> or <c>XdmNode</c>), which is returned unchanged</item>
        /// <item>An instance of Saxon's Java class <c>Saxon.Hej.om.Sequence</c>, which is wrapped
        /// as an <c>XdmValue</c></item>
        /// <item>An instance of <c>IDictionary</c> (which is wrapped as an <c>XdmMap</c> using the method <see cref="XdmMap.MakeMap"/>)</item>
        /// <item>An array of objects, which are converted by applying these rules recursively,
        /// and then wrapped as an <c>XdmArray</c>.</item>
        /// </list>
        /// </remarks>
        /// <param name="o">The supplied object</param>
        /// <returns>The result of conversion if successful.</returns>
        public static XdmValue MakeValue(object o) {
            switch (o) {
                case null:
                    return null;
                case GroundedValue groundedValue:
                    return FromGroundedValue(groundedValue);
                case Sequence sequence:
                    return new XdmValue(SequenceTool.toGroundedValue(sequence.iterate()));
                case XdmValue xdmValue:
                    return xdmValue;
                case IDictionary dictionary:
                    return XdmMap.MakeMap(dictionary);
                default: {
                    if (o.GetType().IsArray) {
                        return XdmArray.MakeArray((object[]) o);
                    }

                    if (o is IEnumerable enumerable) {
                        return MakeSequence(enumerable);
                    }

                    return XdmAtomicValue.MakeAtomicValue(o);
                }
            }
        }

        private static XdmValue MakeSequence(IEnumerable o) {
            List<Saxon.Hej.om.Item> list = new List<Saxon.Hej.om.Item>();

            if (o is string) {
                return XdmAtomicValue.MakeAtomicValue(o);
            }

            foreach (object oi in o) {
                XdmValue v = MakeValue(oi);
                if (v is XdmItem item) {
                    list.Add((Saxon.Hej.om.Item)item.Unwrap());
                }
                else {
                    GroundedValue gv = v.Unwrap();
                    list.Add(SimpleArrayItem.makeSimpleArrayItem(gv.iterate()));
                }
            }

            Sequence value = new SequenceExtent.Of<Saxon.Hej.om.Item>(list);
            return Wrap(value);
        }


        /// <summary>
        /// Extract the underlying Saxon <c>Sequence</c> object from an <c>XdmValue</c>.
        /// This method is provided for the benefit of applications that need to mix
        /// use of the Saxon .NET API with direct use of the underlying objects
        /// and methods offered by the Java implementation.
        /// </summary>
        /// <returns>An object representing the XDM value in the
        /// underlying Saxon implementation.</returns>
		public Saxon.Hej.om.GroundedValue Unwrap() {
            return value;
        }

        /// <summary>
        /// Get the sequence of items in the form of an <c>IList</c>.
        /// </summary>
        /// <returns>
        /// The list of items making up this XDM value. Each item in the list
        /// will be an object of type <c>XdmItem</c>.
        /// </returns>        
        public IList<XdmItem> GetList() {
            List<XdmItem> list = new List<XdmItem>(value.getLength());
            foreach (Item it in value.asIterable()) {
                list.Add(XdmItem.WrapItem(it));
            }
            return list;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <summary>
        /// Get the sequence of items in the form of an <c>IEnumerator</c>.
        /// </summary>
        /// <returns>
        /// An enumeration over the list of items making up this XDM value. Each item in the list
        /// will be an object of type <c>XdmItem</c>.
        /// </returns>    
        IEnumerator<XdmItem> IEnumerable<XdmItem>.GetEnumerator() {
            return GetEnumerator();
        }

        /// <summary>
        /// Get the sequence of items in the form of an <c>IEnumerator</c>.
        /// </summary>
        /// <returns>
        /// An enumeration over the list of items making up this XDM value. Each item in the list
        /// will be an object of type <c>XdmItem</c>.
        /// </returns>    
        public IEnumerator<XdmItem> GetEnumerator() {
            return GetList().GetEnumerator();
        }

        /// <summary>
        /// Get the i'th item in the value, counting from zero.
        /// </summary>
        /// <remarks>
        /// Retained for compatibility; but <c>xdmValue.ItemAt(i)</c> can also be written <c>xdmValue[i]</c>
        /// </remarks>
        /// <param name="i">The item that is required, counting the first item in the sequence as item zero.</param>
        /// <returns>The i'th item in the sequence making up the value, counting from zero.</returns>
        public XdmItem ItemAt(int i) {
            if (i < 0 || i >= Count) {
                throw new IndexOutOfRangeException("" + i);
            }

            try {
                return (XdmItem) Wrap(value.itemAt(i));
            }
            catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        
        /// <summary>
        /// Get a selected item in the sequence by position, counting from zero.
        /// </summary>
        /// <remarks>
        /// Retained for compatibility; but <c>xdmValue.ItemAt(i)</c> can also be written <c>xdmValue[i]</c>
        /// </remarks>
        /// <param name="index">The item that is required, counting the first item in the sequence as item zero.</param>
        /// <returns>The item in the sequence at position <c>index</c>, counting from zero.</returns>
        
        // Indexer declaration
        public XdmItem this[int index] => ItemAt(index);

        /// <summary>
        /// Create a string representation of the value. The is the result of serializing
        /// the value using the adaptive serialization method.
        /// </summary>
        /// <returns>A string representation of the value.</returns>
        public override string ToString() {
            return value.ToString();
        }


        /// <summary>
        /// Return a new <c>XdmValue</c> containing the nodes present in this <c>XdmValue</c>,
        /// with duplicates eliminated, and sorted into document order.
        /// </summary>
        /// <returns>The same nodes, sorted into document order, with duplicates eliminated.</returns>
        public XdmValue DocumentOrder() {
            try {
                SequenceIterator iter = value.iterate();
                Saxon.Hej.expr.sort.DocumentOrderIterator sorted = new(iter, Saxon.Hej.expr.sort.GlobalOrderComparer.getInstance());
                return Wrap(SequenceTool.toGroundedValue(sorted));
            }
            catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }

        /// <summary>
        /// Get the number of items in the sequence.
        /// </summary>
        /// <returns>
        /// The number of items in the sequence. Note that for a single item (including
        /// a map or an array) this always returns 1 (one).
        /// </returns> 

        public int Count => value.getLength();

        /// <summary>
        /// Ask whether the sequence is empty.
        /// </summary>
        /// <returns>
        /// True if the length of the sequence is zero. Note: there is no guarantee that an empty
        /// sequence will always be represented by an <code>XdmEmptySequence</code> object.
        /// </returns>

        public bool Empty => value.getLength() == 0;

        /// <summary>
        /// Simplify an XDM value: that is, reduce it to the simplest possible form.
        /// </summary>
        /// <remarks>
        /// <list>
        /// <item>If the sequence is empty, the result will be an instance of <c>XdmEmptySequence</c></item>
        /// <item>If the sequence is a single node, the result will be an instance of <c>XdmNode</c></item>
        /// <item>If it is a single atomic value, it will be an instance of <c>XdmAtomicValue</c></item>
        /// <item>If it is a map, it will be an instance of <c>XdmMap</c></item>
        /// <item>If it is an array, it will be an instance of <c>XdmArray</c></item>
        /// <item>If it is any other kind of function, it will be an instance of <c>XdmFunctionItem</c></item>
        /// <item>If it is a wrapper around a .NET object, it will be an instance of <c>XdmExternalObjectValue</c></item>
        /// </list>
        /// </remarks>
        /// <returns>The XDM value reduced to the simplest possible form.</returns>

        public XdmValue Simplify() {
            return value.getLength() switch {
                0 => this is XdmEmptySequence ? this : XdmEmptySequence.Instance,
                1 => this is XdmItem ? this : Wrap(value),
                _ => this
            };
        }
    }
}

// Copyright (c) 2021 Saxonica Limited