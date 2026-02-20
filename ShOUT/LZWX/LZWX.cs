namespace ShOUT.LZWX
{
    public class LZWX
    {
        const byte UNLZW_BITS = 9;
        const int UNLZW_END = 256;

        private DictionaryEntry[] dict = [];
        private int dictSize = 0;
        private int dictOff = 0;
        private int dictLen = 0;
        private int dictAlloc = 0;

        private int outLen = 0;
        private int outSize = 0;
        private int outBuffPos = 0;

        private int bits = 0;

        public int Decompress(byte[] data, int decompressedSize, byte[] outBuff)
        {
            int inSize = data.Length;
            int code = 0;                   // current element
            int inlen = 0;                  // current input length
            int cl = 0;
            int dl = 0;
            int totbits = 0;
            int n = 0;                      // bytes written in the output
            int i = 0;

            code = -1;                       // useless
            inlen = 0;                       // current input length
            outLen = 0;                      // current output length
            outSize = decompressedSize;      // needed only for the global var
            totbits = 0;

            dict = null;                     // global var initialization

            if (init() < 0) { return 0; }

            inSize -= 2;
            while (inlen < inSize)
            {
                cl = (data[inlen + 1] << 8) | data[inlen];
                dl = data[inlen + 2];

                for (i = 0; i < (totbits & 7); i++)
                {
                    cl = (cl >> 1) | ((dl & 1) << 15);
                    dl >>= 1;
                }

                code = ((1 << bits) - 1) & cl;

                totbits += bits;
                inlen = totbits >> 3;

                if (code == 257) { break; }

                if (code == UNLZW_END)
                {
                    if (init() < 0) { break; }
                    continue;
                }

                if (code == dictSize)
                {
                    if (dictionary_repeating() < 0) { break; }

                    n = expand(code, outBuff);
                }
                else
                {
                    n = expand(code, outBuff);

                    if (dictionary() < 0) { break; }
                }

                if (n < 0) { break; }

                dictOff = outLen;
                dictLen = n;
                outLen += n;
            }

            dict = null;

            return outLen;
        }

        int init()
        {
            //outBuffPos = 0;
            //outLen = 0;

            bits = UNLZW_BITS;
            dictSize = UNLZW_END + 2;
            dictOff = 0;
            dictLen = 0;

            dictAlloc = 1 * (1 << (UNLZW_BITS + 3));
            dict = new DictionaryEntry[dictAlloc];

            return 0;
        }

        int dictionary_repeating()
        {
            if (dictLen++ != 0)
            {
                if ((dictOff + dictLen) > outSize)
                {
                    dictLen = outSize - dictOff;
                }

                DictionaryEntry dictEntry = new(outBuffPos + dictOff, dictLen);

                dict[dictSize] = dictEntry;
                dictSize++;

                if (((dictSize >> bits) != 0) && (bits != 12))
                {
                    bits++;
                }
            }

            return 0;
        }

        int dictionary()
        {
            if (dictLen++ != 0)
            {
                if ((dictOff + dictLen) > outSize)
                {
                    dictLen = outSize - dictOff;
                }

                DictionaryEntry dictEntry = new DictionaryEntry(outBuffPos + dictOff, dictLen);

                dict[dictSize] = dictEntry;
                dictSize++;

                if (((dictSize >> bits) != 0) && (bits != 12))
                {
                    bits++;
                }
            }

            return 0;
        }

        int expand(int code, byte[] outBuff)
        {
            if (code >= dictSize) { return 0; }

            if (code >= UNLZW_END)
            {
                if ((outLen + dict[code].Length) > outSize) { return -1; }

                unlzwx_cpy(outBuffPos + outLen, dict[code].Offset, dict[code].Length, outBuff);

                return dict[code].Length;
            }

            if ((outLen + 1) > outSize) { return -1; }

            outBuff[outLen] = (byte)code;

            return 1;
        }

        void unlzwx_cpy(int outpos, int offset, int len, byte[] outBuff)
        {
            for (int i = 0; i < len; i++)
            {
                outBuff[outpos + i] = outBuff[offset + i];
            }
        }
    }
}