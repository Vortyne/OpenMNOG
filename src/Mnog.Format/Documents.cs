using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mnog.Format;

// The on-disk format is documented in docs/FORMAT.md. All geometry is in pixels;
// matrices are [a, b, c, d, tx, ty]; colors are "#RRGGBBAA"; frame numbers are 1-based.

public sealed class GameManifest
{
    public string Format { get; set; } = "mnog-game/1";
    public string Title { get; set; } = "";
    public string EntryMovie { get; set; } = "";
    public int Width { get; set; }
    public int Height { get; set; }
    public double FrameRate { get; set; }
}

public sealed class MovieDoc
{
    public string Format { get; set; } = "mnog-movie/1";
    public string Source { get; set; } = "";
    /// <summary>Script semantics follow this SWF version (4 = Flash 4 numeric operators).</summary>
    public int SwfVersion { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double FrameRate { get; set; }
    public string Background { get; set; } = "#FFFFFFFF";
    public List<SymbolDoc> Symbols { get; set; } = [];
    public TimelineDoc Timeline { get; set; } = new();
    public Dictionary<string, int>? Exports { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(ShapeSymbol), "shape")]
[JsonDerivedType(typeof(MorphSymbol), "morph")]
[JsonDerivedType(typeof(ImageSymbol), "image")]
[JsonDerivedType(typeof(SpriteSymbol), "sprite")]
[JsonDerivedType(typeof(ButtonSymbol), "button")]
[JsonDerivedType(typeof(FontSymbol), "font")]
[JsonDerivedType(typeof(TextSymbol), "text")]
[JsonDerivedType(typeof(EditTextSymbol), "edittext")]
[JsonDerivedType(typeof(SoundSymbol), "sound")]
public abstract class SymbolDoc
{
    [JsonPropertyOrder(-1)]
    public int Id { get; set; }
}

// ---- Vector graphics ----

public sealed class FillDoc
{
    /// <summary>solid | linear | radial | focal | bitmap</summary>
    public string Type { get; set; } = "solid";
    public string? Color { get; set; }
    /// <summary>Gradients: maps the unit gradient square [-1,1]² to shape space. Bitmaps: maps image pixels to shape space.</summary>
    public double[]? Matrix { get; set; }
    public List<GradientStopDoc>? Stops { get; set; }
    /// <summary>pad | reflect | repeat</summary>
    public string? Spread { get; set; }
    public bool? LinearRgb { get; set; }
    public double? Focal { get; set; }
    public int? Bitmap { get; set; }
    public bool? Repeat { get; set; }
    public bool? Smooth { get; set; }
}

public sealed class GradientStopDoc
{
    /// <summary>Position along the gradient, 0..1.</summary>
    public double At { get; set; }
    public string Color { get; set; } = "";
}

public sealed class StrokeDoc
{
    public double Width { get; set; }
    public string? Color { get; set; }
    public FillDoc? Fill { get; set; }
    /// <summary>round | none | square</summary>
    public string? Cap { get; set; }
    /// <summary>round | bevel | miter</summary>
    public string? Join { get; set; }
    public double? MiterLimit { get; set; }
}

/// <summary>One filled or stroked path. Fills use the even-odd rule. Paths are SVG path data (M/L/Q/Z).</summary>
public sealed class ShapePartDoc
{
    public FillDoc? Fill { get; set; }
    public StrokeDoc? Stroke { get; set; }
    public string Path { get; set; } = "";
}

public sealed class ShapeSymbol : SymbolDoc
{
    /// <summary>[xMin, yMin, xMax, yMax]</summary>
    public double[] Bounds { get; set; } = [];
    /// <summary>Drawn in order.</summary>
    public List<ShapePartDoc> Parts { get; set; } = [];
}

/// <summary>A shape tween. Start and end paths have identical command sequences; interpolate by ratio.</summary>
public sealed class MorphPartDoc
{
    public FillDoc? FillStart { get; set; }
    public FillDoc? FillEnd { get; set; }
    public StrokeDoc? StrokeStart { get; set; }
    public StrokeDoc? StrokeEnd { get; set; }
    public string Start { get; set; } = "";
    public string End { get; set; } = "";
}

public sealed class MorphSymbol : SymbolDoc
{
    public double[] StartBounds { get; set; } = [];
    public double[] EndBounds { get; set; } = [];
    public List<MorphPartDoc> Parts { get; set; } = [];
}

public sealed class ImageSymbol : SymbolDoc
{
    public string File { get; set; } = "";
    public int Width { get; set; }
    public int Height { get; set; }
}

// ---- Fonts and text ----

public sealed class GlyphDoc
{
    /// <summary>The character this glyph draws (may be empty if unknown).</summary>
    public string Char { get; set; } = "";
    /// <summary>Outline in font units (see UnitsPerEm), even-odd fill.</summary>
    public string Path { get; set; } = "";
    public double? Advance { get; set; }
}

public sealed class FontSymbol : SymbolDoc
{
    public string Name { get; set; } = "";
    public bool? Bold { get; set; }
    public bool? Italic { get; set; }
    public double UnitsPerEm { get; set; } = 1024;
    public double? Ascent { get; set; }
    public double? Descent { get; set; }
    public double? Leading { get; set; }
    public List<GlyphDoc> Glyphs { get; set; } = [];
}

public sealed class TextRunDoc
{
    public int Font { get; set; }
    public double Size { get; set; }
    public string Color { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
    /// <summary>Readable copy of the run's characters (informational).</summary>
    public string Text { get; set; } = "";
    /// <summary>Glyph indices into the font.</summary>
    public List<int> Glyphs { get; set; } = [];
    /// <summary>Advance after each glyph, in pixels.</summary>
    public List<double> Advances { get; set; } = [];
}

public sealed class TextSymbol : SymbolDoc
{
    public double[] Bounds { get; set; } = [];
    public double[] Matrix { get; set; } = [];
    public List<TextRunDoc> Runs { get; set; } = [];
}

public sealed class EditTextSymbol : SymbolDoc
{
    public double[] Bounds { get; set; } = [];
    public string? Variable { get; set; }
    public string? Text { get; set; }
    public int? Font { get; set; }
    public double Size { get; set; }
    public string? Color { get; set; }
    /// <summary>left | right | center | justify</summary>
    public string Align { get; set; } = "left";
    public double LeftMargin { get; set; }
    public double RightMargin { get; set; }
    public double Indent { get; set; }
    public double Leading { get; set; }
    public int? MaxLength { get; set; }
    public bool? WordWrap { get; set; }
    public bool? Multiline { get; set; }
    public bool? Password { get; set; }
    public bool? ReadOnly { get; set; }
    public bool? Selectable { get; set; }
    public bool? Border { get; set; }
    public bool? Html { get; set; }
    public bool? UseOutlines { get; set; }
    public bool? AutoSize { get; set; }
}

// ---- Sound ----

public sealed class SoundSymbol : SymbolDoc
{
    /// <summary>Ogg Vorbis file, already trimmed to the exact sample range Flash would play.</summary>
    public string File { get; set; } = "";
    public int Rate { get; set; }
    public int Channels { get; set; }
    public long Samples { get; set; }
    /// <summary>Codec in the original SWF (informational).</summary>
    public string Original { get; set; } = "";
}

public sealed class SoundRefDoc
{
    public int Id { get; set; }
    /// <summary>Stop all instances of this sound instead of starting it.</summary>
    public bool? Stop { get; set; }
    /// <summary>Don't start if already playing.</summary>
    public bool? NoMultiple { get; set; }
    /// <summary>In/out points in 44.1 kHz samples.</summary>
    public long? In { get; set; }
    public long? Out { get; set; }
    public int? Loops { get; set; }
    /// <summary>[pos44, leftLevel(0..32768), rightLevel] triples.</summary>
    public List<long[]>? Envelope { get; set; }
}

public sealed class StreamDoc
{
    public string File { get; set; } = "";
    /// <summary>First timeline frame (1-based) carrying stream data.</summary>
    public int StartFrame { get; set; }
    public int Rate { get; set; }
    public int Channels { get; set; }
}

// ---- Sprites, buttons and timelines ----

public sealed class SpriteSymbol : SymbolDoc
{
    public TimelineDoc Timeline { get; set; } = new();
}

public sealed class ButtonRecordDoc
{
    /// <summary>Subset of: up, over, down, hit.</summary>
    public List<string> States { get; set; } = [];
    public int Id { get; set; }
    public int Depth { get; set; }
    public double[]? Matrix { get; set; }
    public double[]? Cxform { get; set; }
}

public sealed class ButtonActionDoc
{
    /// <summary>Transition names, e.g. release, press, rollOver, rollOut, dragOver, dragOut, releaseOutside.</summary>
    public List<string> On { get; set; } = [];
    public int? Key { get; set; }
    public string Script { get; set; } = "";
}

public sealed class ButtonSymbol : SymbolDoc
{
    public bool? TrackAsMenu { get; set; }
    public List<ButtonRecordDoc> Records { get; set; } = [];
    public List<ButtonActionDoc> Actions { get; set; } = [];
    /// <summary>Keys: rollOut, rollOver, press, release.</summary>
    public Dictionary<string, SoundRefDoc>? Sounds { get; set; }
}

public sealed class ClipEventDoc
{
    /// <summary>load, enterFrame, unload, mouseMove, mouseDown, mouseUp, keyDown, keyUp, data, ...</summary>
    public List<string> On { get; set; } = [];
    public int? Key { get; set; }
    public string Script { get; set; } = "";
}

/// <summary>A display-list change. Exactly one of Place/Remove is set.</summary>
public sealed class OpDoc
{
    /// <summary>Depth to place at.</summary>
    public int? Place { get; set; }
    /// <summary>Depth to clear.</summary>
    public int? Remove { get; set; }
    /// <summary>Symbol to place (omitted when only modifying the existing object).</summary>
    public int? Id { get; set; }
    /// <summary>True when modifying (or replacing) the object already at this depth.</summary>
    public bool? Move { get; set; }
    public double[]? Matrix { get; set; }
    /// <summary>[rMul, gMul, bMul, aMul, rAdd, gAdd, bAdd, aAdd]; adds are 0..255.</summary>
    public double[]? Cxform { get; set; }
    /// <summary>Morph ratio, 0..1.</summary>
    public double? Ratio { get; set; }
    public string? Name { get; set; }
    public int? ClipDepth { get; set; }
    public List<ClipEventDoc>? Events { get; set; }
}

public sealed class FrameDoc
{
    public List<string>? Labels { get; set; }
    public List<OpDoc>? Ops { get; set; }
    /// <summary>Frame script files, run in order after the display list is updated.</summary>
    public List<string>? Scripts { get; set; }
    public List<SoundRefDoc>? Sounds { get; set; }
}

public sealed class TimelineDoc
{
    public int FrameCount { get; set; }
    public List<FrameDoc> Frames { get; set; } = [];
    public StreamDoc? Stream { get; set; }
}

public static class Json
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    static readonly JsonSerializerOptions CompactOptions = new(Options) { WriteIndented = false };

    /// <summary>Writes readable JSON: nested structure is indented, but short arrays/objects stay on one line.</summary>
    public static void Save<T>(string path, T value)
    {
        var node = JsonSerializer.SerializeToNode(value, CompactOptions);
        var sb = new System.Text.StringBuilder();
        Format(node, sb, 0);
        sb.Append('\n');
        File.WriteAllText(path, sb.ToString());
    }

    const int InlineLimit = 110;

    static void Format(System.Text.Json.Nodes.JsonNode? node, System.Text.StringBuilder sb, int indent)
    {
        var compact = node?.ToJsonString(CompactOptions) ?? "null";
        bool isContainer = node is System.Text.Json.Nodes.JsonObject or System.Text.Json.Nodes.JsonArray;
        // Arrays of plain values (paths, matrices, glyph lists) always stay on one line.
        bool primitiveArray = node is System.Text.Json.Nodes.JsonArray arr &&
            arr.All(x => x is not (System.Text.Json.Nodes.JsonObject or System.Text.Json.Nodes.JsonArray));
        if (!isContainer || primitiveArray || compact.Length + indent * 2 <= InlineLimit)
        {
            sb.Append(Spaced(compact));
            return;
        }
        string pad = new(' ', (indent + 1) * 2);
        if (node is System.Text.Json.Nodes.JsonObject obj)
        {
            sb.Append("{\n");
            int i = 0;
            foreach (var (k, v) in obj)
            {
                sb.Append(pad).Append(JsonSerializer.Serialize(k, CompactOptions)).Append(": ");
                Format(v, sb, indent + 1);
                sb.Append(++i < obj.Count ? ",\n" : "\n");
            }
            sb.Append(' ', indent * 2).Append('}');
        }
        else
        {
            var a = (System.Text.Json.Nodes.JsonArray)node!;
            sb.Append("[\n");
            for (int i = 0; i < a.Count; i++)
            {
                sb.Append(pad);
                Format(a[i], sb, indent + 1);
                sb.Append(i < a.Count - 1 ? ",\n" : "\n");
            }
            sb.Append(' ', indent * 2).Append(']');
        }
    }

    /// <summary>Adds a space after ':' and ',' outside of strings, for readability.</summary>
    static string Spaced(string json)
    {
        var sb = new System.Text.StringBuilder(json.Length + json.Length / 8);
        bool inString = false;
        for (int i = 0; i < json.Length; i++)
        {
            char c = json[i];
            sb.Append(c);
            if (inString)
            {
                if (c == '\\') sb.Append(json[++i]);
                else if (c == '"') inString = false;
            }
            else if (c == '"') inString = true;
            else if (c is ',' or ':') sb.Append(' ');
        }
        return sb.ToString();
    }

    public static T Load<T>(string path) =>
        JsonSerializer.Deserialize<T>(File.ReadAllText(path), Options) ?? throw new InvalidDataException(path);
}
