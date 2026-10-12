namespace Mnog.Engine.Assets;

/// <summary>
/// A WAV reader (the whole file to 32-bit float interleaved samples): PCM at 8, 16, 24 or 32 bits, or 32-bit float,
/// in RIFF (or WAVE_FORMAT_EXTENSIBLE) files. WAV needs no decoding, so it loads as fast as the file can be read: the
/// high-quality replacements (game/audio/hq) and lossless sound replacements (sounds/N.wav beside sounds/N.ogg) use it.
/// </summary>
public static class WavDecoder
{
    public static (float[] Samples, int Rate, int Channels) Decode(string path)
    {
        var b = File.ReadAllBytes(path);
        if (b.Length < 12 || BitConverter.ToUInt32(b, 0) != 0x46464952 || BitConverter.ToUInt32(b, 8) != 0x45564157)
            throw new InvalidDataException("not a WAV file");
        int format = 0, channels = 0, rate = 0, bits = 0, pos = 12, dataStart = -1, dataLen = 0;
        while (pos + 8 <= b.Length)
        {
            uint id = BitConverter.ToUInt32(b, pos);
            int len = BitConverter.ToInt32(b, pos + 4);
            int body = pos + 8;
            if (id == 0x20746D66) // "fmt "
            {
                format = BitConverter.ToUInt16(b, body);
                channels = BitConverter.ToUInt16(b, body + 2);
                rate = BitConverter.ToInt32(b, body + 4);
                bits = BitConverter.ToUInt16(b, body + 14);
                // WAVE_FORMAT_EXTENSIBLE: the real format is the first two bytes of its sub-format GUID.
                if (format == 0xFFFE && len >= 26) format = BitConverter.ToUInt16(b, body + 24);
            }
            else if (id == 0x61746164) // "data"
            {
                dataStart = body;
                dataLen = Math.Min(len < 0 ? int.MaxValue : len, b.Length - body);
                break;
            }
            pos = body + len + (len & 1);
        }
        if (dataStart < 0 || channels == 0 || rate == 0) throw new InvalidDataException("no audio in the WAV file");
        int bytes = bits / 8;
        int n = dataLen / bytes;
        var s = new float[n];
        var d = b.AsSpan(dataStart, n * bytes);
        if (format == 3 && bits == 32)
            for (int i = 0; i < n; i++) s[i] = BitConverter.ToSingle(d.Slice(i * 4, 4));
        else if (format == 1 && bits == 16)
            for (int i = 0; i < n; i++) s[i] = (short)(d[i * 2] | d[i * 2 + 1] << 8) / 32768f;
        else if (format == 1 && bits == 24)
            for (int i = 0; i < n; i++) s[i] = ((d[i * 3] << 8 | d[i * 3 + 1] << 16 | d[i * 3 + 2] << 24) >> 8) / 8388608f;
        else if (format == 1 && bits == 32)
            for (int i = 0; i < n; i++) s[i] = BitConverter.ToInt32(d.Slice(i * 4, 4)) / 2147483648f;
        else if (format == 1 && bits == 8)
            for (int i = 0; i < n; i++) s[i] = (d[i] - 128) / 128f;
        else
            throw new InvalidDataException($"unsupported WAV format {format}, {bits} bits");
        return (s, rate, channels);
    }
}
