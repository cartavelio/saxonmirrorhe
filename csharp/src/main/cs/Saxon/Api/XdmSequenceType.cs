using System;
using Saxon.Hej.expr;
using Saxon.Hej.s9api;
using Saxon.Hej.type;
using ItemType = Saxon.Hej.type.ItemType;
using SequenceType = Saxon.Hej.value.SequenceType;

namespace Saxon.Api {
    /// <summary>
    /// An instance of class <c>XdmSequenceType</c> represents a sequence type, that is, the combination
    /// of an item type and an occurrence indicator.
    /// </summary>
    /// <remarks>
    /// <para>An <c>XdmSequenceType</c> may be constructed using the method <c>Processor.ParseSequenceType()</c></para>
    /// </remarks>

    public class XdmSequenceType
    {
        private readonly XdmItemType itemType;
        private readonly char occurrenceIn;
        private readonly int occurrence;

        /// <summary>
        /// Occurrence indicator denoting a sequence of any length (zero or more items)
        /// </summary>
        public const char ZERO_OR_MORE = '*';
        /// <summary>
        /// Occurrence indicator denoting a sequence of any length other than zero (one or more items)
        /// </summary>
        public const char ONE_OR_MORE = '+';
        /// <summary>
        /// Occurrence indicator denoting a sequence of length zero or one
        /// </summary>
        public const char ZERO_OR_ONE = '?';
        /// <summary>
        /// Occurrence indicator denoting an empty sequence (zero items)
        /// </summary>
        public const char ZERO = '0';
        /// <summary>
        /// Occurrence indicator denoting a sequence of length exactly one
        /// </summary>
        public const char ONE = ' ';

        /// <summary>
        /// A <c>SequenceType</c> that matches any sequence (any kind of item, any length)
        /// </summary>
        public static readonly XdmSequenceType AnySequenceType = new(
            XdmItemType.MakeXdmItemType(AnyItemType.getInstance()), ZERO_OR_MORE);

        /// <summary>
        /// Create an <c>XdmSequenceType</c> corresponding to a given <c>XdmItemType</c> and occurrence indicator
        /// </summary>
        /// <param name="itemType">The item type</param>
        /// <param name="occurrenceIndicator">The occurrence indicator, one of '?' (zero-or-one), 
        /// '*' (zero-or-more), '+' (one-or-more), ' ' (a single space) (exactly one),
        /// or '0' (exactly zero). The type <c>empty-sequence()</c>
        /// can be represented by an occurrence indicator of 'º' with any item type.</param>

        public XdmSequenceType(XdmItemType itemType, char occurrenceIndicator)
        {
            int occ = occurrenceIndicator switch {
                ZERO_OR_MORE => StaticProperty.ALLOWS_ZERO_OR_MORE,
                ONE_OR_MORE => StaticProperty.ALLOWS_ONE_OR_MORE,
                ZERO_OR_ONE => StaticProperty.ALLOWS_ZERO_OR_ONE,
                ONE => StaticProperty.EXACTLY_ONE,
                ZERO => StaticProperty.ALLOWS_ZERO,
                _ => throw new ArgumentException("Unknown occurrence indicator")
            };
            this.itemType = itemType;
            this.occurrence = occ;
            this.occurrenceIn = occurrenceIndicator;
        }

        internal static XdmSequenceType FromSequenceType(SequenceType seqType)
        {
            XdmItemType itemType = XdmItemType.MakeXdmItemType(seqType.getPrimaryType());
            char occ;
            if (seqType.getCardinality() == StaticProperty.ALLOWS_ZERO_OR_MORE)
            {
                occ = ZERO_OR_MORE;
            }
            else if (seqType.getCardinality() == StaticProperty.ALLOWS_ONE_OR_MORE)
            {
                occ = ONE_OR_MORE;
            }
            else if (seqType.getCardinality() == StaticProperty.ALLOWS_ZERO_OR_ONE)
            {
                occ = ZERO_OR_ONE;
            }
            else if (seqType.getCardinality() == StaticProperty.ALLOWS_ZERO)
            {
                occ = ZERO;
            }
            else
            {
                occ = ONE;
            }
            if (itemType == null) {
                return null;
            }
            return new XdmSequenceType(itemType, occ);
        }
        
        /// <summary>
        /// The ItemType part of the SequenceType
        /// </summary>

        public XdmItemType ItemType => itemType;

        /// <summary>
        /// The occurrence indicator. One of "?", "*", "+", or " " (single space) meaning exactly 1,
        /// or "0" meaning exactly zero.
        /// </summary>
        public char OccurrenceIndicator => occurrenceIn;

        internal SequenceType ToSequenceType() {
            ItemType jit = itemType.Unwrap();
            return SequenceType.makeSequenceType(jit, occurrence);
        }
        
        /// <summary>
        /// Ask whether a supplied value is an instance of this SequenceType
        /// </summary>
        /// <param name="value">the value to be tested</param>
        /// <returns>true if the supplied value conforms to this SequenceType</returns>

        public bool Matches(XdmValue value) {
            OccurrenceIndicator indicator = Saxon.Hej.s9api.OccurrenceIndicator.getOccurrenceIndicator(occurrence);
            if (!indicator.allows(value.Count)) {
                return false;
            }

            return value.AllMatch<XdmItem>(item => itemType.Matches(item));
        }

        /// <summary>
        /// Get a string representation of the sequence type
        /// </summary>
        /// <returns>the string representation of the item type, followed by an occurrence indicator</returns>
        public override string ToString() {
            return (itemType.ToString() + occurrenceIn).Trim();
        }

        /// <summary>
        /// Compare two sequence types for equality
        /// </summary>
        /// <param name="obj">The object to be compared</param>
        /// <returns>The result of the equality comparison.</returns>
        public override bool Equals(object obj) {
            if (obj is XdmSequenceType other) {
                return itemType.Equals(other.itemType) && occurrence == other.occurrence;
            }

            return false;
        }

        /// <summary>
        /// Get a hash code to support equality comparison
        /// </summary>
        /// <returns>A suitable hash code</returns>
        public override int GetHashCode() {
            return itemType.GetHashCode() ^ (occurrence<<3);
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited