using System;

namespace Echo.Memory
{
    public readonly ref partial struct BitVectorSpan
    {
        /// <summary>
        /// Inverts all the bits in the vector.
        /// </summary>
        public void Not() => Bits.Not();

        /// <summary>
        /// Performs a bitwise AND operation with another bit vector.
        /// </summary>
        /// <param name="other">The other bit vector.</param>
        public void And(BitVectorSpan other)
        {
            AssertSameBitSize(other);
            
            for (int i = 0; i < KnownMask.Length; i++)
            {
                (byte bitsA, byte knownA) = (Bits[i], KnownMask[i]);
                (byte bitsB, byte knownB) = (other.Bits[i], other.KnownMask[i]);

                KnownMask[i] = (byte) ((knownA & knownB) | (knownA & ~bitsA) | (knownB & ~bitsB));
            }

            Bits.And(other.Bits);
        }
        
        /// <summary>
        /// Performs a bitwise OR operation with another bit vector.
        /// </summary>
        /// <param name="other">The other bit vector.</param>
        public void Or(BitVectorSpan other)
        {
            AssertSameBitSize(other);
            
            for (int i = 0; i < KnownMask.Length; i++)
            {
                (byte bitsA, byte knownA) = (Bits[i], KnownMask[i]);
                (byte bitsB, byte knownB) = (other.Bits[i], other.KnownMask[i]);

                KnownMask[i] = (byte) (bitsA | bitsB | (knownA & knownB));
            }

            Bits.Or(other.Bits);
        }
        
        /// <summary>
        /// Performs a bitwise XOR operation with another bit vector.
        /// </summary>
        /// <param name="other">The other bit vector.</param>
        public void Xor(BitVectorSpan other)
        {
            AssertSameBitSize(other);
            
            Bits.Xor(other.Bits);
            KnownMask.And(other.KnownMask);
        }

        /// <summary>
        /// Shift all bits in the vector to the left, filling the least significant bits with zeroes.
        /// </summary>
        /// <param name="count">The number of bits to shift with.</param>
        public void ShiftLeft(int count)
        {
            if (Count > 64 || !IsFullyKnown)
            {
                ShiftLeftLle(count);
                return;
            }

            switch (Count)
            {
                case 8:
                    U8 <<= count;
                    break;

                case 16:
                    U16 <<= count;
                    break;

                case 32:
                    U32 <<= count;
                    break;

                case 64:
                    U64 <<= count;
                    break;

                default:
                    ShiftLeftLle(count);
                    break;
            }
        }

        private void ShiftLeftLle(int count)
        {
            count = Math.Min(Count, count);

            for (int i = Count - count - 1; i >= 0; i--)
                this[i + count] = this[i];

            for (int i = 0; i < count; i++)
                this[i] = Trilean.False;
        }

        /// <summary>
        /// Shift all bits in the vector to the right, and either sign- or zero-extends the value.
        /// </summary>
        /// <param name="count">The number of bits to shift with.</param>
        /// <param name="signExtend">Gets a value indicating whether the bits should be sign- or zero-extended.</param>
        public void ShiftRight(int count, bool signExtend)
        {
            if (Count > 64 || !IsFullyKnown)
            {
                ShiftRightLle(count, signExtend);
                return;
            }

            if (signExtend)
            {
                switch (Count)
                {
                    case 8 :
                        I8 >>= count;
                        return;
                    case 16:
                        I16 >>= count;
                        return;
                    case 32:
                        I32 >>= count;
                        return;
                    case 64:
                        I64 >>= count;
                        return;
                }
            }
            else
            {
                switch (Count)
                {
                    case 8:
                        U8 >>= count;
                        return;
                    case 16:
                        U16 >>= count;
                        return;
                    case 32:
                        U32 >>= count;
                        return;
                    case 64:
                        U64 >>= count;
                        return;
                }
            }

            ShiftRightLle(count, signExtend);
        }

        private void ShiftRightLle(int count, bool signExtend)
        {
            count = Math.Min(Count * 8, count);
            var sign = signExtend
                ? this[Count - 1]
                : Trilean.False;

            for (int i = count; i < Count; i++)
                this[i - count] = this[i];

            for (int i = 0; i < count; i++)
                this[Count - i - 1] = sign;
        }
    }
}