#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines
{
    public abstract class SerpentEngineBase
        : IBlockCipher
    {
        internal const int ROUNDS = 32;
        internal const int PHI = unchecked((int)0x9E3779B9); // (sqrt(5) - 1) * 2**31
        protected static readonly int BlockSize = 16;

        protected bool encrypting;
        protected int[] wKey;

        protected int X0, X1, X2, X3; // registers

        /**
         * initialise a Serpent cipher.
         * 
         * @param encrypting whether or not we are for encryption.
         * @param params     the parameters required to set up the cipher.
         * @throws IllegalArgumentException if the params argument is
         * inappropriate.
         */
        public virtual void Init(bool encrypting, ICipherParameters parameters)
        {
            if (!(parameters is KeyParameter))
                throw new ArgumentException("invalid parameter passed to " + AlgorithmName + " init - " +
                                            Platform.GetTypeName(parameters));

            this.encrypting = encrypting;
            wKey = MakeWorkingKey(((KeyParameter)parameters).GetKey());
        }

        public virtual string AlgorithmName => "Serpent";

        public virtual bool IsPartialBlockOkay => false;

        public virtual int GetBlockSize()
        {
            return BlockSize;
        }

        /**
         * Process one block of input from the array in and write it to
         * the out array.
         * 
         * @param in     the array containing the input data.
         * @param inOff  offset into the in array the data starts at.
         * @param out    the array the output data will be copied into.
         * @param outOff the offset into the out array the output will start at.
         * @return the number of bytes processed and produced.
         * @throws DataLengthException if there isn't enough data in in, or
         * space in out.
         * @throws IllegalStateException if the cipher isn't initialised.
         */
        public int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
        {
            if (wKey == null)
                throw new InvalidOperationException(AlgorithmName + " not initialised");

            Check.DataLength(input, inOff, BlockSize, "input buffer too short");
            Check.OutputLength(output, outOff, BlockSize, "output buffer too short");

            if (encrypting)
                EncryptBlock(input, inOff, output, outOff);
            else
                DecryptBlock(input, inOff, output, outOff);

            return BlockSize;
        }

        public virtual void Reset()
        {
        }

        protected static int RotateLeft(int x, int bits)
        {
            return (x << bits) | (int)((uint)x >> (32 - bits));
        }

        private static int RotateRight(int x, int bits)
        {
            return (int)((uint)x >> bits) | (x << (32 - bits));
        }

        /*
         * The sboxes below are based on the work of Brian Gladman and
         * Sam Simpson, whose original notice appears below.
         * <p>
         * For further details see:
         *      http://fp.gladman.plus.com/cryptography_technology/serpent/
         * </p>
         */

        /* Partially optimised Serpent S Box boolean functions derived  */
        /* using a recursive descent analyser but without a full search */
        /* of all subtrees. This set of S boxes is the result of work    */
        /* by Sam Simpson and Brian Gladman using the spare time on a    */
        /* cluster of high capacity servers to search for S boxes with    */
        /* this customised search engine. There are now an average of    */
        /* 15.375 terms    per S box.                                        */
        /*                                                              */
        /* Copyright:   Dr B. R Gladman (gladman@seven77.demon.co.uk)   */
        /*                and Sam Simpson (s.simpson@mia.co.uk)            */
        /*              17th December 1998                                */
        /*                                                              */
        /* We hereby give permission for information in this file to be */
        /* used freely subject only to acknowledgement of its origin.    */

        /*
         * S0 - { 3, 8,15, 1,10, 6, 5,11,14,13, 4, 2, 7, 0, 9,12 } - 15 terms.
         */
        protected void Sb0(int a, int b, int c, int d)
        {
            var t1 = a ^ d;
            var t3 = c ^ t1;
            var t4 = b ^ t3;
            X3 = (a & d) ^ t4;
            var t7 = a ^ (b & t1);
            X2 = t4 ^ (c | t7);
            var t12 = X3 & (t3 ^ t7);
            X1 = ~t3 ^ t12;
            X0 = t12 ^ ~t7;
        }

        /**
        * InvSO - {13, 3,11, 0,10, 6, 5,12, 1,14, 4, 7,15, 9, 8, 2 } - 15 terms.
        */
        protected void Ib0(int a, int b, int c, int d)
        {
            var t1 = ~a;
            var t2 = a ^ b;
            var t4 = d ^ (t1 | t2);
            var t5 = c ^ t4;
            X2 = t2 ^ t5;
            var t8 = t1 ^ (d & t2);
            X1 = t4 ^ (X2 & t8);
            X3 = (a & t4) ^ (t5 | X1);
            X0 = X3 ^ t5 ^ t8;
        }

        /**
        * S1 - {15,12, 2, 7, 9, 0, 5,10, 1,11,14, 8, 6,13, 3, 4 } - 14 terms.
        */
        protected void Sb1(int a, int b, int c, int d)
        {
            var t2 = b ^ ~a;
            var t5 = c ^ (a | t2);
            X2 = d ^ t5;
            var t7 = b ^ (d | t2);
            var t8 = t2 ^ X2;
            X3 = t8 ^ (t5 & t7);
            var t11 = t5 ^ t7;
            X1 = X3 ^ t11;
            X0 = t5 ^ (t8 & t11);
        }

        /**
        * InvS1 - { 5, 8, 2,14,15, 6,12, 3,11, 4, 7, 9, 1,13,10, 0 } - 14 steps.
        */
        protected void Ib1(int a, int b, int c, int d)
        {
            var t1 = b ^ d;
            var t3 = a ^ (b & t1);
            var t4 = t1 ^ t3;
            X3 = c ^ t4;
            var t7 = b ^ (t1 & t3);
            var t8 = X3 | t7;
            X1 = t3 ^ t8;
            var t10 = ~X1;
            var t11 = X3 ^ t7;
            X0 = t10 ^ t11;
            X2 = t4 ^ (t10 | t11);
        }

        /**
        * S2 - { 8, 6, 7, 9, 3,12,10,15,13, 1,14, 4, 0,11, 5, 2 } - 16 terms.
        */
        protected void Sb2(int a, int b, int c, int d)
        {
            var t1 = ~a;
            var t2 = b ^ d;
            var t3 = c & t1;
            X0 = t2 ^ t3;
            var t5 = c ^ t1;
            var t6 = c ^ X0;
            var t7 = b & t6;
            X3 = t5 ^ t7;
            X2 = a ^ ((d | t7) & (X0 | t5));
            X1 = t2 ^ X3 ^ X2 ^ (d | t1);
        }

        /**
        * InvS2 - {12, 9,15, 4,11,14, 1, 2, 0, 3, 6,13, 5, 8,10, 7 } - 16 steps.
        */
        protected void Ib2(int a, int b, int c, int d)
        {
            var t1 = b ^ d;
            var t2 = ~t1;
            var t3 = a ^ c;
            var t4 = c ^ t1;
            var t5 = b & t4;
            X0 = t3 ^ t5;
            var t7 = a | t2;
            var t8 = d ^ t7;
            var t9 = t3 | t8;
            X3 = t1 ^ t9;
            var t11 = ~t4;
            var t12 = X0 | X3;
            X1 = t11 ^ t12;
            X2 = (d & t11) ^ t3 ^ t12;
        }

        /**
        * S3 - { 0,15,11, 8,12, 9, 6, 3,13, 1, 2, 4,10, 7, 5,14 } - 16 terms.
        */
        protected void Sb3(int a, int b, int c, int d)
        {
            var t1 = a ^ b;
            var t2 = a & c;
            var t3 = a | d;
            var t4 = c ^ d;
            var t5 = t1 & t3;
            var t6 = t2 | t5;
            X2 = t4 ^ t6;
            var t8 = b ^ t3;
            var t9 = t6 ^ t8;
            var t10 = t4 & t9;
            X0 = t1 ^ t10;
            var t12 = X2 & X0;
            X1 = t9 ^ t12;
            X3 = (b | d) ^ t4 ^ t12;
        }

        /**
        * InvS3 - { 0, 9,10, 7,11,14, 6,13, 3, 5,12, 2, 4, 8,15, 1 } - 15 terms
        */
        protected void Ib3(int a, int b, int c, int d)
        {
            var t1 = a | b;
            var t2 = b ^ c;
            var t3 = b & t2;
            var t4 = a ^ t3;
            var t5 = c ^ t4;
            var t6 = d | t4;
            X0 = t2 ^ t6;
            var t8 = t2 | t6;
            var t9 = d ^ t8;
            X2 = t5 ^ t9;
            var t11 = t1 ^ t9;
            var t12 = X0 & t11;
            X3 = t4 ^ t12;
            X1 = X3 ^ X0 ^ t11;
        }

        /**
        * S4 - { 1,15, 8, 3,12, 0,11, 6, 2, 5, 4,10, 9,14, 7,13 } - 15 terms.
        */
        protected void Sb4(int a, int b, int c, int d)
        {
            var t1 = a ^ d;
            var t2 = d & t1;
            var t3 = c ^ t2;
            var t4 = b | t3;
            X3 = t1 ^ t4;
            var t6 = ~b;
            var t7 = t1 | t6;
            X0 = t3 ^ t7;
            var t9 = a & X0;
            var t10 = t1 ^ t6;
            var t11 = t4 & t10;
            X2 = t9 ^ t11;
            X1 = a ^ t3 ^ (t10 & X2);
        }

        /**
        * InvS4 - { 5, 0, 8, 3,10, 9, 7,14, 2,12,11, 6, 4,15,13, 1 } - 15 terms.
        */
        protected void Ib4(int a, int b, int c, int d)
        {
            var t1 = c | d;
            var t2 = a & t1;
            var t3 = b ^ t2;
            var t4 = a & t3;
            var t5 = c ^ t4;
            X1 = d ^ t5;
            var t7 = ~a;
            var t8 = t5 & X1;
            X3 = t3 ^ t8;
            var t10 = X1 | t7;
            var t11 = d ^ t10;
            X0 = X3 ^ t11;
            X2 = (t3 & t11) ^ X1 ^ t7;
        }

        /**
        * S5 - {15, 5, 2,11, 4,10, 9,12, 0, 3,14, 8,13, 6, 7, 1 } - 16 terms.
        */
        protected void Sb5(int a, int b, int c, int d)
        {
            var t1 = ~a;
            var t2 = a ^ b;
            var t3 = a ^ d;
            var t4 = c ^ t1;
            var t5 = t2 | t3;
            X0 = t4 ^ t5;
            var t7 = d & X0;
            var t8 = t2 ^ X0;
            X1 = t7 ^ t8;
            var t10 = t1 | X0;
            var t11 = t2 | t7;
            var t12 = t3 ^ t10;
            X2 = t11 ^ t12;
            X3 = b ^ t7 ^ (X1 & t12);
        }

        /**
        * InvS5 - { 8,15, 2, 9, 4, 1,13,14,11, 6, 5, 3, 7,12,10, 0 } - 16 terms.
        */
        protected void Ib5(int a, int b, int c, int d)
        {
            var t1 = ~c;
            var t2 = b & t1;
            var t3 = d ^ t2;
            var t4 = a & t3;
            var t5 = b ^ t1;
            X3 = t4 ^ t5;
            var t7 = b | X3;
            var t8 = a & t7;
            X1 = t3 ^ t8;
            var t10 = a | d;
            var t11 = t1 ^ t7;
            X0 = t10 ^ t11;
            X2 = (b & t10) ^ (t4 | (a ^ c));
        }

        /**
        * S6 - { 7, 2,12, 5, 8, 4, 6,11,14, 9, 1,15,13, 3,10, 0 } - 15 terms.
        */
        protected void Sb6(int a, int b, int c, int d)
        {
            var t1 = ~a;
            var t2 = a ^ d;
            var t3 = b ^ t2;
            var t4 = t1 | t2;
            var t5 = c ^ t4;
            X1 = b ^ t5;
            var t7 = t2 | X1;
            var t8 = d ^ t7;
            var t9 = t5 & t8;
            X2 = t3 ^ t9;
            var t11 = t5 ^ t8;
            X0 = X2 ^ t11;
            X3 = ~t5 ^ (t3 & t11);
        }

        /**
        * InvS6 - {15,10, 1,13, 5, 3, 6, 0, 4, 9,14, 7, 2,12, 8,11 } - 15 terms.
        */
        protected void Ib6(int a, int b, int c, int d)
        {
            var t1 = ~a;
            var t2 = a ^ b;
            var t3 = c ^ t2;
            var t4 = c | t1;
            var t5 = d ^ t4;
            X1 = t3 ^ t5;
            var t7 = t3 & t5;
            var t8 = t2 ^ t7;
            var t9 = b | t8;
            X3 = t5 ^ t9;
            var t11 = b | X3;
            X0 = t8 ^ t11;
            X2 = (d & t1) ^ t3 ^ t11;
        }

        /**
        * S7 - { 1,13,15, 0,14, 8, 2,11, 7, 4,12,10, 9, 3, 5, 6 } - 16 terms.
        */
        protected void Sb7(int a, int b, int c, int d)
        {
            var t1 = b ^ c;
            var t2 = c & t1;
            var t3 = d ^ t2;
            var t4 = a ^ t3;
            var t5 = d | t1;
            var t6 = t4 & t5;
            X1 = b ^ t6;
            var t8 = t3 | X1;
            var t9 = a & t4;
            X3 = t1 ^ t9;
            var t11 = t4 ^ t8;
            var t12 = X3 & t11;
            X2 = t3 ^ t12;
            X0 = ~t11 ^ (X3 & X2);
        }

        /**
        * InvS7 - { 3, 0, 6,13, 9,14,15, 8, 5,12,11, 7,10, 1, 4, 2 } - 17 terms.
        */
        protected void Ib7(int a, int b, int c, int d)
        {
            var t3 = c | (a & b);
            var t4 = d & (a | b);
            X3 = t3 ^ t4;
            var t6 = ~d;
            var t7 = b ^ t4;
            var t9 = t7 | (X3 ^ t6);
            X1 = a ^ t9;
            X0 = c ^ t7 ^ (d | X1);
            X2 = t3 ^ X1 ^ X0 ^ (a & X3);
        }

        /**
        * Apply the linear transformation to the register set.
        */
        protected void LT()
        {
            var x0 = RotateLeft(X0, 13);
            var x2 = RotateLeft(X2, 3);
            var x1 = X1 ^ x0 ^ x2;
            var x3 = X3 ^ x2 ^ (x0 << 3);

            X1 = RotateLeft(x1, 1);
            X3 = RotateLeft(x3, 7);
            X0 = RotateLeft(x0 ^ X1 ^ X3, 5);
            X2 = RotateLeft(x2 ^ X3 ^ (X1 << 7), 22);
        }

        /**
        * Apply the inverse of the linear transformation to the register set.
        */
        protected void InverseLT()
        {
            var x2 = RotateRight(X2, 22) ^ X3 ^ (X1 << 7);
            var x0 = RotateRight(X0, 5) ^ X1 ^ X3;
            var x3 = RotateRight(X3, 7);
            var x1 = RotateRight(X1, 1);
            X3 = x3 ^ x2 ^ (x0 << 3);
            X1 = x1 ^ x0 ^ x2;
            X2 = RotateRight(x2, 3);
            X0 = RotateRight(x0, 13);
        }

        protected abstract int[] MakeWorkingKey(byte[] key);

        protected abstract void EncryptBlock(byte[] input, int inOff, byte[] output, int outOff);

        protected abstract void DecryptBlock(byte[] input, int inOff, byte[] output, int outOff);
    }
}

#endif