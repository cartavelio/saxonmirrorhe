using System;
using System.Collections;
using System.Collections.Generic;
using Saxon.Hej.ma.map;
using Saxon.Hej.om;
using Saxon.Hej.value;
using Saxon.Impl.DataModel;
using KeyValuePair = Saxon.Hej.ma.map.KeyValuePair;

namespace Saxon.Api {
    /// <summary inherits="XdmFunctionItem">
    /// The class <c>XdmMap</c> represents a map item in an XPath 3.1 sequence:
    /// this is a new kind of item in the XDM data model. A map is a list of zero or more entries, each of which
    /// is a pair comprising a key (which is an atomic value) and a value (which is an arbitrary value).
    /// </summary>
    [Serializable]
    public class XdmMap : XdmFunctionItem {
        /// <summary>
        /// Create an empty <c>XdmMap</c>
        /// </summary>
        public XdmMap() : base(new ImmMap()) {
        }

        // Wrap a supplied Saxon map

		internal XdmMap(Saxon.Hej.ma.map.MapItem map) : base(map) {
        }


        /// <summary>
        /// Get the number of entries in the map
        /// </summary>
        /// <remarks>(Note that the <see cref="XdmValue.Count"/> method returns 1 (one),
        /// because an XDM map is an item.)</remarks>

        public int Size => ((MapItem) value).size();


        /// <summary>
        /// Ask whether the <c>XdmMap</c> is empty
        /// </summary>
        /// <returns>Returns <code>true</code> if this map contains no key-value pairs, that is
        /// if the <c>Size</c> property is zero.</returns>
        public bool IsEmpty() {
            return ((MapItem) value).isEmpty();
        }

        /// <summary>
        /// Create a new map containing an additional (key, value) pair.
        /// If there is an existing entry with the same key, it is removed.
        /// </summary>
        /// <param name="key">The key of the new entry.</param>
        /// <param name="value">The value part of the new entry.</param>
        /// <returns>A new map containing the additional entry (or replaced entry). The original map is unchanged.</returns>
        public XdmMap Put(XdmAtomicValue key, XdmValue value) {
            MapItem jMap = (MapItem) this.value;
            AtomicValue jKey = (AtomicValue) key.value;
            GroundedValue jValue = value.value;
            return (XdmMap)Wrap(jMap.put(jKey, jValue));
        }


        /// <summary>
        /// Create a new map in which the entry for a given key has been removed.
        /// If there is no entry with the same key, the new map has the same content as the old (it may or may not
        /// be the same .NET object)
        /// </summary>
        /// <param name="key">The key of the entry that is to be removed</param>
        /// <returns>A map without the specified entry. The original map is unchanged.</returns>
        public XdmMap Remove(XdmAtomicValue key) {
            MapItem jMap = (MapItem) this.value;
            AtomicValue jKey = (AtomicValue) key.value;
            return (XdmMap)Wrap(jMap.remove(jKey));
        }


        /// <summary>
        /// Return a corresponding .NET Dictionary collection of keys and values.
        /// </summary>
        /// <returns>A mutable Dictionary from atomic values to (sequence) values, containing the
        /// same entries as this map</returns>
        public Dictionary<XdmAtomicValue, XdmValue> AsDictionary() {
            Dictionary<XdmAtomicValue, XdmValue> map = new Dictionary<XdmAtomicValue, XdmValue>();
            MapItem jmap = (MapItem) value;
            foreach (Saxon.Hej.ma.map.KeyValuePair pair in jmap.keyValuePairs()) {
                map.Add((XdmAtomicValue) XdmValue.Wrap(pair.key), XdmValue.Wrap(pair.value));
            }
            return map;
        }


        /// <summary>
        /// Get the keys present in the map in the form of a set.
        /// </summary>
        /// <returns>a set of the keys present in this map, with no defined ordering.</returns>
        public HashSet<XdmAtomicValue> KeySet() {
            HashSet<XdmAtomicValue> result = new HashSet<XdmAtomicValue>();
            foreach (Saxon.Hej.ma.map.KeyValuePair pair in ((MapItem) value).keyValuePairs()) {
                result.Add((XdmAtomicValue) Wrap(pair.key));
            }
            return result;
        }


        /// <summary>
        /// Returns <code>true</code> if this map contains a mapping for the specified
        /// key. More formally, returns <code>true</code> if and only if
        /// this map contains a mapping for a key <code>k</code> such that
        /// <code>(key==null ? k==null : key.Equals(k))</code>.  (There can be
        /// at most one such mapping.)
        /// </summary>
        /// <param name="key">the key whose presence in this map is to be tested</param>
        /// <returns><c>true</c> if this map contains a mapping for the specified key</returns>
        public bool ContainsKey(XdmAtomicValue key) {
            return ((MapItem) value).get((AtomicValue) key.value) != null;
        }


        /// <summary>
        /// Returns the value to which the specified key is mapped,
        /// or <c>null</c> if this map contains no mapping for the key.
        /// </summary>
        /// <param name="key">the key whose associated value is to be returned. If this is
        /// not an <c>XdmAtomicValue</c>, the method attempts to construct an
        /// <c>XdmAtomicValue</c> using the method <see cref="XdmAtomicValue.MakeAtomicValue(Object)"/>;
        /// it is therefore possible to pass a simple key such as a string or integer.</param>
        /// <returns>the value to which the specified key is mapped, or
        /// <c>null</c> if this map contains no mapping for the key</returns>
        public XdmValue Get(XdmAtomicValue key) {
            if (key == null) {
                throw new ArgumentNullException();
            }

            AtomicValue k = (AtomicValue) (key).value;
            Sequence v = ((MapItem) value).get(k);
            return v == null ? null : Wrap(v);
        }

        /// <summary>
        /// Returns the value to which the specified key is mapped,
        /// or <c>null</c> if this map contains no mapping for the key.
        /// </summary>
        /// <param name="key">the key whose associated value is to be returned.</param>
        /// <returns>the value to which the specified key is mapped, or
        /// <c>null</c> if this map contains no mapping for the key</returns>
        public XdmValue Get(String key) {
            if (key == null) {
                throw new ArgumentNullException();
            }

            AtomicValue k = (AtomicValue) (new XdmAtomicValue(key)).value;
            Sequence v = ((MapItem) value).get(k);
            return v == null ? null : Wrap(v);
        }


        /// <summary>
        /// Returns the value to which the specified key is mapped,
        /// or <c>null</c> if this map contains no mapping for the key.
        /// </summary>
        /// <param name="key">the key whose associated value is to be returned.</param>
        /// <returns>the value to which the specified key is mapped, or
        /// <c>null</c> if this map contains no mapping for the key</returns>
        public XdmValue Get(long key) {
            AtomicValue k = (AtomicValue) (new XdmAtomicValue(key)).value;
            Sequence v = ((MapItem) value).get(k);
            return v == null ? null : XdmValue.Wrap(v);
        }

        /// <summary>
        /// Returns a <c>Collection</c> containing the values found in this map.
        /// </summary>
        /// <returns>A collection of the values found in this map, that is, the value
        /// parts of the key-value pairs. The result may contain duplicates, and the
        /// order of results is unpredictable.</returns>
        public ICollection<XdmValue> Values() {
            List<XdmValue> result = new List<XdmValue>();
            foreach (Saxon.Hej.ma.map.KeyValuePair pair in ((MapItem) value).keyValuePairs()) {
                result.Add((XdmAtomicValue) Wrap(pair.value));
            }
            return result;
        }


        /// <summary>
        /// Returns a <c>HashSet</c> view of the mappings contained in this map.
        /// </summary>
        /// <returns>a set view of the mappings contained in this map</returns>
        public HashSet<DictionaryEntry> EntrySet() {
            HashSet<DictionaryEntry> result = new HashSet<DictionaryEntry>();
            foreach (Saxon.Hej.ma.map.KeyValuePair pair in ((MapItem) value).keyValuePairs()) {
                result.Add(new DictionaryEntry(pair.key, pair.value));
            }
            return result;
        }

        /// <summary>
        /// Static factory method to construct an XDM map by converting each entry
        /// in a supplied generic collection of key/value pairs; <code>IDictionary</code>. The keys in the 
        /// Dictionary must be convertible to XDM atomic values using the 
        /// <see cref="XdmAtomicValue.MakeAtomicValue(Object)"/> method. The associated values 
        /// must be convertible to XDM sequences
        /// using the <see cref="XdmValue.MakeValue(Object)"/> method.
        /// </summary>
        /// <param name="input">the supplied map</param>
        /// <returns>the resulting <c>XdmMap</c></returns>
        public static XdmMap MakeMap(IDictionary input) {
            ImmMap result = new ImmMap();
            XdmAtomicValue key;
            XdmValue value;

            foreach (object keyi in input.Keys) {
                key = XdmAtomicValue.MakeAtomicValue(keyi);
                value = MakeValue(input[keyi]);
                result.initialPut((AtomicValue) key.Unwrap(), value.value);
            }

            return new XdmMap(result);
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited