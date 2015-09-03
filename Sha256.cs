using System;
using System.Collections.Generic;
using System.Text;

namespace Bitcoin.Sha26
{
    /// <summary>
    /// SHA256 implementation with midstate optimization
    /// http://crypto.stackexchange.com/questions/1862/sha-256-midstate
    /// </summary>
    public class Sha256
    {
        private long count = 64;
        private readonly object syncLock = new object();

        private const int BLOCK_SIZE = 64;
        private uint[] H = new uint[] { 0x6A09E667, 0xBB67AE85, 0x3C6EF372, 0xA54FF53A, 0x510E527F, 0x9B05688C, 0x1F83D9AB, 0x5BE0CD19 };
        private readonly uint[] midstate = new uint[] { 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000 };

        private readonly uint[] K =
            new uint[] {
			0x428A2F98, 0x71374491, 0xB5C0FBCF, 0xE9B5DBA5,
			0x3956C25B, 0x59F111F1, 0x923F82A4, 0xAB1C5ED5,
			0xD807AA98, 0x12835B01, 0x243185BE, 0x550C7DC3,
			0x72BE5D74, 0x80DEB1FE, 0x9BDC06A7, 0xC19BF174,
			0xE49B69C1, 0xEFBE4786, 0x0FC19DC6, 0x240CA1CC,
			0x2DE92C6F, 0x4A7484AA, 0x5CB0A9DC, 0x76F988DA,
			0x983E5152, 0xA831C66D, 0xB00327C8, 0xBF597FC7,
			0xC6E00BF3, 0xD5A79147, 0x06CA6351, 0x14292967,
			0x27B70A85, 0x2E1B2138, 0x4D2C6DFC, 0x53380D13,
			0x650A7354, 0x766A0ABB, 0x81C2C92E, 0x92722C85,
			0xA2BFE8A1, 0xA81A664B, 0xC24B8B70, 0xC76C51A3,
			0xD192E819, 0xD6990624, 0xF40E3585, 0x106AA070,
			0x19A4C116, 0x1E376C08, 0x2748774C, 0x34B0BCB5,
			0x391C0CB3, 0x4ED8AA4A, 0x5B9CCA4F, 0x682E6FF3,
			0x748F82EE, 0x78A5636F, 0x84C87814, 0x8CC70208,
			0x90BEFFFA, 0xA4506CEB, 0xBEF9A3F7, 0xC67178F2
		};

        private readonly byte[] block1 = new byte[64];
        private readonly byte[] block2 = new byte[64];

        /// <summary>The number of bytes that have been processed.</summary>
        /// <remarks>This number does NOT include the bytes that are waiting in the buffer.</remarks>
        public long Count
        {
            get { return count; }
        }

        private byte[] HashCore(byte[] array)
        {
            lock (syncLock)
            {
                int i;
                int bufferCount = 0;
                byte[] buffer = new byte[BLOCK_SIZE];
                Reset();
                // For as long as we have full blocks, process them.
                for (i = 0; i < (array.Length - (array.Length % BLOCK_SIZE)); i += BLOCK_SIZE)
                {
                    ProcessBlock(array, i);
                    count += (long)BLOCK_SIZE;
                }

                // If we still have some bytes left, store them for later.
                int bytesLeft = array.Length % BLOCK_SIZE;
                if (bytesLeft != 0)
                {
                    Array.Copy(array, ((array.Length - bytesLeft)), buffer, 0, bytesLeft);
                    bufferCount = bytesLeft;
                }
                return ProcessFinalBlock(buffer, 0, bufferCount);
            }
        }

        private void Reset()
        {
            count = 0;
            H = new uint[] { 0x6A09E667, 0xBB67AE85, 0x3C6EF372, 0xA54FF53A, 0x510E527F, 0x9B05688C, 0x1F83D9AB, 0x5BE0CD19 };
        }

        public uint[] GetMidstate(byte[] data)
        {
            Reset();
            byte[] m1 = new byte[64];

            Array.Copy(data, m1, 64);
            ProcessBlock(m1, 0); //This will initialize h to hash of 1st block = MIDSTATE
            Array.Copy(H, midstate, 8); //midstate is initialized
            return midstate;
        }

        private void ProcessBlock(byte[] inputBuffer, int inputOffset)
        {
            lock (syncLock)
            {
                uint[] workBuffer = new uint[64];
                uint a = H[0];
                uint b = H[1];
                uint c = H[2];
                uint d = H[3];
                uint e = H[4];
                uint f = H[5];
                uint g = H[6];
                uint h = H[7];
                uint T1, T2;
                int i;

                //System.Diagnostics.Debug.WriteLine("Input Buffer - " + Conversions.ByteArrayToCsv(inputBuffer));
                //System.Diagnostics.Debug.WriteLine("H - " + Conversions.UintArrayToCsv(H));

                uint[] temp = Conversions.ByteToUInt(inputBuffer, inputOffset, BLOCK_SIZE, EndianType.BigEndian);
                Array.Copy(temp, 0, workBuffer, 0, temp.Length);
                for (i = 16; i < 64; i++)
                {

                    workBuffer[i] = Gamma1256(workBuffer[i - 2]) + workBuffer[i - 7] + Gamma0256(workBuffer[i - 15]) +
                                    workBuffer[i - 16];
                }

                for (i = 0; i < 64; i++)
                {

                    T1 = h + Sigma1256(e) + Ch(e, f, g) + K[i] + workBuffer[i];
                    T2 = Sigma0256(a) + Maj(a, b, c);

                    h = g;
                    g = f;
                    f = e;
                    e = d + T1;
                    d = c;
                    c = b;
                    b = a;
                    a = T1 + T2;
                }

                H[0] += a;
                H[1] += b;
                H[2] += c;
                H[3] += d;
                H[4] += e;
                H[5] += f;
                H[6] += g;
                H[7] += h;
            }
        }

        private byte[] ProcessFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            lock (syncLock)
            {
                StandardDigestPadding(inputBuffer, inputOffset, inputCount);
                return Conversions.UIntToByte(H, 0, H.Length, EndianType.BigEndian);
            }
        }

        private void StandardDigestPadding(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            // Figure out how much padding is needed between the last byte and the size.
            int paddingSize = (int)(((ulong)inputCount + (ulong)Count) % (ulong)BLOCK_SIZE);
            paddingSize = (BLOCK_SIZE - 8) - paddingSize;
            if (paddingSize < 1) { paddingSize += BLOCK_SIZE; }

            // Create the final, padded block(s).
            byte[] temp = new byte[inputCount + paddingSize + 8];
            Array.Copy(inputBuffer, inputOffset, temp, 0, inputCount);
            temp[inputCount] = 0x80;
            ulong size = ((ulong)Count + (ulong)inputCount) * 8;
            Array.Copy(Conversions.ULongToByte(size, EndianType.BigEndian), 0, temp, (temp.Length - 8), 8);
            if (inputCount == 16)
            {
                System.Diagnostics.Debug.WriteLine(Conversions.ByteArrayToCsv(temp));
            }
            // Push the final block(s) into the calculation.        
            ProcessBlock(temp, 0);
            if (temp.Length == (BLOCK_SIZE * 2))
            {
                ProcessBlock(temp, BLOCK_SIZE);
            }
        }

        /// <summary>
        /// Initialize the two blocks & midstate. Only nonce gets incremented every run.
        /// </summary>
        /// <param name="data"></param>
        public void Initialize(byte[] data)
        {
            Array.Copy(data, block1, 64);
            Array.Copy(data, 64, block2, 0, 12);

            //http://crypto.stackexchange.com/questions/1862/sha-256-midstate
            //Bitcoin data is 72 bytes. So there are two blocks to process M1 and M2.
            //For nonce 0 to n, hashes are calculated. Since the nonce is in M2, we do not need to calculate M1 again.
            //Midstate = Hash(M1);            
            ProcessBlock(block1, 0); //This will initialize h to hash of 1st block = MIDSTATE
            Array.Copy(H, midstate, 8); //midstate is initialized

            //Padding
            block2[16] = 0x80; //1
            block2[63] = 0x2;

            //Append size
            count = 64;
            ulong size = ((ulong)Count + (ulong)16) * 8;
            Array.Copy(Conversions.ULongToByte(size, EndianType.BigEndian), 0, block2, (block2.Length - 8), 8);
        }

        public byte[] CalculateHash(byte[] nonce)
        {
            Array.Copy(nonce, 0, block2, 12, 4);
            ProcessBlock(block2, 0);
            byte[] hash = Conversions.UIntToByte(H, 0, H.Length, EndianType.BigEndian);
            byte[] finalHash = HashCore(hash);
            Array.Copy(midstate, H, 8);
            return finalHash;
        }

        #region helper functions
        /// <summary>
        /// ROTL^n(value). Circular shift left
        /// </summary>        
        public uint RotateLeft(uint value, int bits)
        {
            return (value << bits) | (value >> (32 - bits));
        }

        /// <summary>
        /// ROTR^n(value). Circular shift right
        /// </summary>        
        public uint RotateRight(uint value, int bits)
        {
            return (value >> bits) | (value << (32 - bits));
        }

        public uint ShiftRight(uint value, int bits)
        {
            return (value >> bits);
        }

        public uint Ch(uint x, uint y, uint z)
        {
            return ((x & y) ^ ((~x) & z));
        }

        public uint Maj(uint x, uint y, uint z)
        {
            return ((x & y) ^ (x & z) ^ (y & z));
        }

        public uint Sigma0256(uint x)
        {
            return (RotateRight(x, 2) ^ RotateRight(x, 13) ^ RotateRight(x, 22));
        }

        public uint Sigma1256(uint x)
        {
            return (RotateRight(x, 6) ^ RotateRight(x, 11) ^ RotateRight(x, 25));
        }

        public uint Gamma0256(uint x)
        {
            return (RotateRight(x, 7) ^ RotateRight(x, 18) ^ ShiftRight(x, 3));
        }

        public uint Gamma1256(uint x)
        {
            return (RotateRight(x, 17) ^ RotateRight(x, 19) ^ ShiftRight(x, 10));
        }
        #endregion
    }
}
