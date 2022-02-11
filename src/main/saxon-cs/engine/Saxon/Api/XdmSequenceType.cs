using System;
using Saxon.Hej.expr;
using Saxon.Hej.s9api;
using ItemType = Saxon.Hej.type.ItemType;
using SequenceType = Saxon.Hej.value.SequenceType;

namespace Saxon.Api {
    /// <summary>
    /// An instance of class <c>XdmSequenceType</c> represents a sequence type, that is, the combination
    /// of an item type and an occurrence indicator.
    /// </summary>

    public class XdmSequenceType
    {

        internal XdmItemType itemType;
        internal char occurrenceIn;
        internal int occurrence;


        public const char ZERO_OR_MORE='*';
        public const char ONE_OR_MORE = '+';
        public const char ZERO_OR_ONE = '?';
        public const char ZERO = '0'; 
        public const char ONE = ' ';

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
            int occ;
            switch (occurrenceIndicator)
            {
                case ZERO_OR_MORE:
                    occ = StaticProperty.ALLOWS_ZERO_OR_MORE;
                    break;
                case ONE_OR_MORE:
                    occ = StaticProperty.ALLOWS_ONE_OR_MORE;
                    break;
                case ZERO_OR_ONE:
                    occ = StaticProperty.ALLOWS_ZERO_OR_ONE;
                    break;
                case ONE:
                    occ = StaticProperty.EXACTLY_ONE;
                    break;
                case ZERO:
                    occ = StaticProperty.ALLOWS_ZERO;
                    break;
                default:
                    throw new ArgumentException("Unknown occurrence indicator");
            }
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

       

        internal SequenceType ToSequenceType() {
            ItemType jit = itemType.Unwrap();
            return SequenceType.makeSequenceType(jit, occurrence);
        }

        
 

    }
}