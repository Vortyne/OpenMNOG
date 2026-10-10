using System.Text;

namespace Mnog.Engine;

/// <summary>Windows-1252, the code page the original Flash 4/5 content used for text and ord()/chr().</summary>
public static class Ansi
{
    public static readonly Encoding Encoding = Create();

    static Encoding Create()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding(1252);
    }
}
