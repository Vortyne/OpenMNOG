namespace Mnog.Engine.Assets;

/// <summary>
/// A FLAC decoder (the whole file to 32-bit float interleaved samples), for lossless replacement audio: a sound's
/// sounds/N.flac next to its sounds/N.ogg is played instead. Follows the FLAC format specification (RFC 9639):
/// STREAMINFO, then frames of constant, verbatim, fixed or LPC subframes with Rice-coded residuals and the stereo
/// decorrelation modes. Checksums aren't verified.
/// </summary>
public static class FlacDecoder
{
    public static (float[] Samples, int Rate, int Channels) Decode(string path)
    {
        var r = new Bits(File.ReadAllBytes(path));
        if (r.Read(32) != 0x664C6143) throw new InvalidDataException("not a FLAC file");
        int rate = 0, channels = 0, bps = 0;
        long total = 0;
        // Metadata blocks: only STREAMINFO (the first) matters.
        for (bool last = false; !last;)
        {
            last = r.Read(1) == 1;
            int type = (int)r.Read(7), length = (int)r.Read(24);
            if (type == 0)
            {
                r.Read(16); r.Read(16); r.Read(24); r.Read(24); // block and frame sizes
                rate = (int)r.Read(20);
                channels = (int)r.Read(3) + 1;
                bps = (int)r.Read(5) + 1;
                total = (long)r.Read(36);
                r.Skip(16 * 8); // MD5
            }
            else r.Skip(length * 8L);
        }
        if (rate == 0) throw new InvalidDataException("FLAC: no STREAMINFO");
        var output = new List<float>(total > 0 ? (int)Math.Min(total * channels, int.MaxValue) : 1 << 20);
        var block = new long[channels][];
        while (r.BytesLeft >= 2)
        {
            if (!SyncFrame(r)) break;
            DecodeFrame(r, channels, bps, block, output);
        }
        return (output.ToArray(), rate, channels);
    }

    /// <summary>Finds the next frame's sync code (14 bits 11111111111110), byte-aligned.</summary>
    static bool SyncFrame(Bits r)
    {
        r.Align();
        while (r.BytesLeft >= 2)
        {
            if (r.PeekByte(0) == 0xFF && (r.PeekByte(1) & 0xFE) == 0xF8) return true;
            r.Skip(8);
        }
        return false;
    }

    static readonly int[] BlockSizes = [0, 192, 576, 1152, 2304, 4608, 0, 0, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768];
    static readonly int[] SampleSizes = [0, 8, 12, 0, 16, 20, 24, 32];

    static void DecodeFrame(Bits r, int streamChannels, int streamBps, long[][] block, List<float> output)
    {
        r.Read(15); r.Read(1); // sync, blocking strategy
        int bsCode = (int)r.Read(4), rateCode = (int)r.Read(4), chanCode = (int)r.Read(4), sizeCode = (int)r.Read(3);
        r.Read(1);
        // The frame or sample number (UTF-8-like coding): skipped.
        int first = (int)r.Read(8);
        int extra = first < 0x80 ? 0 : first < 0xE0 ? 1 : first < 0xF0 ? 2 : first < 0xF8 ? 3 : first < 0xFC ? 4 : first < 0xFE ? 5 : 6;
        for (int i = 0; i < extra; i++) r.Read(8);
        int blockSize = bsCode switch
        {
            6 => (int)r.Read(8) + 1,
            7 => (int)r.Read(16) + 1,
            _ => BlockSizes[bsCode],
        };
        if (rateCode == 12) r.Read(8); else if (rateCode is 13 or 14) r.Read(16);
        r.Read(8); // CRC-8
        int bps = sizeCode == 0 ? streamBps : SampleSizes[sizeCode];
        int channels = chanCode < 8 ? chanCode + 1 : 2;
        for (int c = 0; c < channels; c++)
        {
            if (block[c] == null || block[c].Length < blockSize) block[c] = new long[Math.Max(blockSize, 4096)];
            // The side channel carries one more bit.
            int cbps = bps + ((chanCode == 8 && c == 1) || (chanCode == 9 && c == 0) || (chanCode == 10 && c == 1) ? 1 : 0);
            DecodeSubframe(r, cbps, blockSize, block[c]);
        }
        r.Align();
        r.Read(16); // CRC-16
        // Stereo decorrelation.
        if (chanCode == 8) // left, side
            for (int i = 0; i < blockSize; i++) block[1][i] = block[0][i] - block[1][i];
        else if (chanCode == 9) // side, right
            for (int i = 0; i < blockSize; i++) block[0][i] = block[0][i] + block[1][i];
        else if (chanCode == 10) // mid, side
            for (int i = 0; i < blockSize; i++)
            {
                long side = block[1][i], mid = (block[0][i] << 1) | (side & 1);
                block[0][i] = (mid + side) >> 1;
                block[1][i] = (mid - side) >> 1;
            }
        float scale = 1f / (1L << (bps - 1));
        for (int i = 0; i < blockSize; i++)
            for (int c = 0; c < Math.Min(channels, streamChannels); c++)
                output.Add(block[c][i] * scale);
    }

    static void DecodeSubframe(Bits r, int bps, int n, long[] s)
    {
        r.Read(1);
        int type = (int)r.Read(6);
        int wasted = 0;
        if (r.Read(1) == 1)
        {
            wasted = 1;
            while (r.Read(1) == 0) wasted++;
            bps -= wasted;
        }
        if (type == 0)
        {
            long v = r.ReadSigned(bps);
            for (int i = 0; i < n; i++) s[i] = v;
        }
        else if (type == 1)
            for (int i = 0; i < n; i++) s[i] = r.ReadSigned(bps);
        else if (type is >= 8 and <= 12)
        {
            int order = type & 7;
            for (int i = 0; i < order; i++) s[i] = r.ReadSigned(bps);
            Residual(r, n, order, s);
            for (int i = order; i < n; i++)
                s[i] += order switch
                {
                    0 => 0,
                    1 => s[i - 1],
                    2 => 2 * s[i - 1] - s[i - 2],
                    3 => 3 * s[i - 1] - 3 * s[i - 2] + s[i - 3],
                    _ => 4 * s[i - 1] - 6 * s[i - 2] + 4 * s[i - 3] - s[i - 4],
                };
        }
        else if (type >= 32)
        {
            int order = (type & 31) + 1;
            for (int i = 0; i < order; i++) s[i] = r.ReadSigned(bps);
            int precision = (int)r.Read(4) + 1;
            int shift = (int)r.ReadSigned(5);
            var coefs = new long[order];
            for (int i = 0; i < order; i++) coefs[i] = r.ReadSigned(precision);
            Residual(r, n, order, s);
            for (int i = order; i < n; i++)
            {
                long sum = 0;
                for (int j = 0; j < order; j++) sum += coefs[j] * s[i - 1 - j];
                s[i] += sum >> shift;
            }
        }
        else throw new InvalidDataException($"FLAC: reserved subframe type {type}");
        if (wasted > 0)
            for (int i = 0; i < n; i++) s[i] <<= wasted;
    }

    /// <summary>The Rice-coded residual, written into s from the predictor order on.</summary>
    static void Residual(Bits r, int n, int order, long[] s)
    {
        int method = (int)r.Read(2);
        int paramBits = method == 0 ? 4 : 5, escape = method == 0 ? 15 : 31;
        int partitionOrder = (int)r.Read(4);
        int partitions = 1 << partitionOrder;
        int i = order;
        for (int p = 0; p < partitions; p++)
        {
            int count = (n >> partitionOrder) - (p == 0 ? order : 0);
            int param = (int)r.Read(paramBits);
            if (param == escape)
            {
                int raw = (int)r.Read(5);
                for (int k = 0; k < count; k++) s[i++] = raw == 0 ? 0 : r.ReadSigned(raw);
            }
            else
                for (int k = 0; k < count; k++)
                {
                    long q = r.ReadUnary();
                    long u = (q << param) | (long)r.Read(param);
                    s[i++] = (u >> 1) ^ -(u & 1);
                }
        }
    }

    /// <summary>Reads big-endian bit fields from a byte array.</summary>
    sealed class Bits(byte[] data)
    {
        long _pos; // in bits

        public long BytesLeft => data.Length - (_pos + 7) / 8;
        public int PeekByte(int k) => data[_pos / 8 + k];
        public void Align() => _pos = (_pos + 7) & ~7L;
        public void Skip(long bits) => _pos += bits;

        public ulong Read(int n)
        {
            ulong v = 0;
            while (n > 0)
            {
                long index = _pos >> 3;
                if (index >= data.Length) throw new EndOfStreamException();
                int bitInByte = (int)(_pos & 7), avail = 8 - bitInByte, take = Math.Min(avail, n);
                int bits = (data[index] >> (avail - take)) & ((1 << take) - 1);
                v = (v << take) | (uint)bits;
                _pos += take;
                n -= take;
            }
            return v;
        }

        public long ReadSigned(int n)
        {
            if (n == 0) return 0;
            ulong v = Read(n);
            return (long)(v << (64 - n)) >> (64 - n);
        }

        /// <summary>The number of 0 bits before the next 1.</summary>
        public long ReadUnary()
        {
            long count = 0;
            while (true)
            {
                long index = _pos >> 3;
                if (index >= data.Length) throw new EndOfStreamException();
                int bitInByte = (int)(_pos & 7);
                int rest = (data[index] << bitInByte) & 0xFF;
                if (rest == 0)
                {
                    count += 8 - bitInByte;
                    _pos += 8 - bitInByte;
                    continue;
                }
                int lead = System.Numerics.BitOperations.LeadingZeroCount((uint)rest) - 24;
                count += lead;
                _pos += lead + 1;
                return count;
            }
        }
    }
}
