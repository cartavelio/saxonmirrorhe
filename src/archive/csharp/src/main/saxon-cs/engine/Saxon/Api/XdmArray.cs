using System;
using System.Collections.Generic;
using System.Linq;
using Saxon.Hej.ma.arrays;
using Saxon.Hej.om;

namespace Saxon.Api {
    /// <summary inherits="XdmFunctionItem">
    /// The class <c>XdmArray</c> represents an array item in an XDM 3.1 sequence:
    /// this is a new kind of item in the XDM data model. An array is a list of zero or 
    /// more members, each of which is an arbitrary XDM value. An array is also a function:
    /// it maps a positive integer to the array member found at that position in the array.
    /// </summary>
    [Serializable]
    public class XdmArray : XdmFunctionItem {
        ///<summary> Constructor to create an empty <c>XdmArray</c></summary>
        public XdmArray() : base(SimpleArrayItem.EMPTY_ARRAY) {
        }

        ///<summary>Create an <c>XdmArray</c> whose members are single items, corresponding
        ///one-to-one with the items making up a supplied sequence.</summary>
        ///<param name="value">A sequence of items; each item becomes a member of the array.</param>
        public XdmArray(XdmValue value) : base(_init(value)) {
        }

        private static SimpleArrayItem _init(XdmValue value) {
            int length = value.Count;
            List<GroundedValue> list = new List<GroundedValue>(length);
            foreach (XdmItem item in value.GetList()) {
                list.Add(item.Unwrap());
            }

            return new SimpleArrayItem(list);
        }

        ///<summary> Create an <c>XdmArray</c> supplying the members as an array of <c>XdmValue</c> objects.</summary>
        /// <param name="members">An array of <c>XdmValue</c> objects. Note that subsequent changes 
        /// to the array will have no effect on the <c>XdmArray</c>.</param>
        public XdmArray(XdmValue[] members) : base(_init(members)) {
        }

        private static SimpleArrayItem _init(XdmValue[] members) {
            List<GroundedValue> list = new List<GroundedValue>(members.Length);
            list.AddRange(members.Select(v => v.value));
            return new SimpleArrayItem(list);
        }
        

        internal XdmArray(ArrayItem array) : base(array) {
        }


        /// <summary>Create an <c>XdmArray</c> supplying the members as a list of <c>XdmValue</c> objects</summary>
        /// <param name="members">A sequence of <c>XdmValue</c> objects. Note that if this is supplied as 
        /// a list or similar collection, subsequent changes to the list/collection will have no effect on 
        /// the <c>XdmValue</c>.</param>
        /// <remarks>Note that the argument can be a single <c>XdmValue</c> representing a sequence, in which case the
        ///  constructed array will have one member for each item in the supplied sequence.</remarks>
        public XdmArray(List<XdmValue> members) : base(_init(members)) {
        }

        private static SimpleArrayItem _init(List<XdmValue> members) {
            List<GroundedValue> list = new List<GroundedValue>(members.Count);
            foreach (XdmValue v in members) {
                list.Add(v.Unwrap());
            }

            return new SimpleArrayItem(list);
        }

        /// <summary>
        /// Get the number of members in the array
        /// </summary>
        /// <returns>the number of members in the array.</returns> 
        /// <remarks>(Note that the <see cref="XdmValue.Count"/> property returns 1 (one),
        /// because an XDM array is an item.)</remarks>
        public int ArrayLength() {
            return ((ArrayItem) value).arrayLength();
        }

        /// <summary>
        /// Get the n'th member in the array, counting from zero.
        /// </summary>
        /// <param name="n">the position of the member that is required, counting the first member in 
        /// the array as member zero</param>
        /// <returns>the n'th member in the sequence making up the array, counting from zero</returns>
        public XdmValue Get(int n) {
            try {
                return Wrap(((ArrayItem) value).get(n));
            }
            catch (Exception) {
                throw new IndexOutOfRangeException();
            }
        }


        /// <summary>
        /// Create a new array in which one member is replaced with a new value.
        /// </summary>
        /// <param name="n">the position of the member that is to be replaced, counting the first member
        /// in the array as member zero</param>
        /// <param name="valuei">the new value</param>
        /// <returns>the new array</returns>
        public XdmArray Put(int n, XdmValue valuei) {
            try {
                return (XdmArray) Wrap(((ArrayItem) value).put(n, valuei.value));
            }
            catch (Exception) {
                throw new IndexOutOfRangeException();
            }
        }


        /// <summary>
        /// Append a new member to an array
        /// </summary>
        /// <param name="value">the new member</param>
        /// <returns>a new array, one item longer than the original</returns>
        public XdmArray AppendMember(XdmValue value) {
            try {
                GroundedValue member = value.value;
                ArrayItem newArray = ((ArrayItem)this.value).append(member);
                return (XdmArray) Wrap(newArray);
            }
            catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }

        /// <summary>
        /// Concatenate another array
        /// </summary>
        /// <param name="value">the other array</param>
        /// <returns>a new array, containing the members of this array followed by the members 
        /// of the other array</returns>
        public XdmArray Concat(XdmArray value) {
            try {
                ArrayItem other = (ArrayItem) value.value;
                ArrayItem newArray = ((ArrayItem) this.value).concat(other);
                return (XdmArray) Wrap(newArray);
            }
            catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }


        /// <summary>
        /// Get the members of the array in the form of a list.
        /// </summary>
        /// <returns>A list of the members of this array.</returns>
        public List<XdmValue> AsList() {
            ArrayItem val = ((ArrayItem) value);
            List<XdmValue> result = new List<XdmValue>(val.getLength());
            foreach (GroundedValue mem in val.members()) {
                result.Add(Wrap(mem));
            }
            return result;
        }

        /// <summary>
        /// Make an XDM array from an object array. Each member of the supplied array
        /// is converted to a single member in the result array using the method
        /// <see cref="XdmValue.MakeValue(Object)"/>        
        /// </summary>
        /// <param name="objectArray">the array of objects</param>
        /// <returns>the result of the conversion if successful</returns>
        public static XdmArray MakeArray(object[] objectArray) {
            List<XdmValue> list = new List<XdmValue>(objectArray.Length);
            foreach (object obj in objectArray) {
                list.Add(MakeValue(obj));
            }
            return new XdmArray(list);
        }

        /// <summary>
        /// Make an <c>XdmArray</c> whose members are <c>xs:boolean</c> values       
        /// </summary>
        /// <param name="booleanArray">the input array of booleans</param>
        /// <returns>an <c>XdmArray</c> whose members are <c>xs:boolean</c> values corresponding one-to-one with the input</returns>
        public static XdmArray MakeArray(bool[] booleanArray) {
            List<XdmValue> list = new List<XdmValue>(booleanArray.Length);
            foreach (bool b in booleanArray) {
                list.Add(new XdmAtomicValue(b));
            }
            return new XdmArray(list);
        }


        /// <summary>
        /// Make an <c>XdmArray</c> whose members are <c>xs:integer</c> values      
        /// </summary>
        /// <param name="longArray">the input array of long values</param>
        /// <returns>an <c>XdmArray</c> whose members are <c>xs:integer</c> values corresponding one-to-one with the input</returns>
        public static XdmArray MakeArray(long[] longArray) {
            List<XdmValue> list = new List<XdmValue>(longArray.Length);
            foreach (long lng in longArray) {
                list.Add(new XdmAtomicValue(lng));
            }
            return new XdmArray(list);
        }


        /// <summary>
        /// Make an <c>XdmArray</c> whose members are <c>xs:integer</c> values      
        /// </summary>
        /// <param name="intArray">the input array of int values</param>
        /// <returns>an <c>XdmArray</c> whose members are <c>xs:integer</c> values corresponding one-to-one with the input</returns>
        public static XdmArray MakeArray(int[] intArray) {
            List<XdmValue> list = new List<XdmValue>(intArray.Length);
            foreach (int i in intArray) {
                list.Add(new XdmAtomicValue(i));
            }
            return new XdmArray(list);
        }

        /// <summary>
        /// Make an <c>XdmArray</c> whose members are <c>xs:integer</c> values      
        /// </summary>
        /// <param name="byteArray">the input array of byte values</param>
        /// <returns>an <c>XdmArray</c> whose members are <c>xs:integer</c> values corresponding one-to-one with the input</returns>
        public static XdmArray MakeArray(byte[] byteArray) {
            List<XdmValue> list = new List<XdmValue>(byteArray.Length);
            foreach (byte i in byteArray) {
                list.Add(new XdmAtomicValue(i));
            }
            return new XdmArray(list);
        }

    }
}

// Copyright (c) 2021 Saxonica Limited