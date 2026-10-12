using System.Globalization;
using Mnog.Engine.Script;
using Mnog.Format;
using Mnog.Script;
using NVorbis;
using SkiaSharp;

namespace Mnog.Engine.Assets;

/// <summary>A converted movie loaded into memory: symbols ready to instantiate and draw.</summary>
public sealed class MovieAsset
{
    public required string Name;
    public required string Dir;
    public required MovieDoc Doc;
    public readonly Dictionary<int, SymbolAsset> Symbols = [];
    public TimelineAsset Timeline = null!;
    public int Version => Doc.SwfVersion;
    public readonly List<string> Errors = [];
    /// <summary>Widescreen stretches of placed objects: (sprite id, 0 for the main timeline; depth) to x scale (widescreen.json).</summary>
    public readonly Dictionary<(int Sprite, int Depth), (float ScaleX, float Anchor, float? Split, float? SplitLeft, float? AnchorX, int? Id, float ScaleY, float? SplitY, float? SplitAt, float? SplitLeftAt, float? ScaleLeft)> WidescreenStretch = [];

    public SymbolAsset? Symbol(int id) => Symbols.GetValueOrDefault(id);

    public SymbolAsset? Export(string name)
    {
        if (Doc.Exports == null) return null;
        foreach (var (k, id) in Doc.Exports)
            if (k.Equals(name, StringComparison.OrdinalIgnoreCase)) return Symbol(id);
        return null;
    }

    public static MovieAsset Load(string name, string dir)
    {
        var doc = Json.Load<MovieDoc>(Path.Combine(dir, "movie.json"));
        var m = new MovieAsset { Name = name, Dir = dir, Doc = doc };
        var stretchFile = Path.Combine(dir, "widescreen.json");
        if (File.Exists(stretchFile) && System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(stretchFile)) is System.Text.Json.Nodes.JsonArray stretches)
            foreach (var st in stretches)
                if (st?["Sprite"]?.GetValue<int>() is int sp && st["Depth"]?.GetValue<int>() is int dp && (st["ScaleX"]?.GetValue<double>() ?? 1) is double sx)
                    m.WidescreenStretch[(sp, dp)] = ((float)sx, (float)(st["Anchor"]?.GetValue<double>() ?? 0.5), (float?)st["Split"]?.GetValue<double>(),
                        (float?)st["SplitLeft"]?.GetValue<double>(), (float?)st["AnchorX"]?.GetValue<double>(),
                        // ("Id": only that symbol at the depth, not another the timeline places there later.)
                        (int?)st["Id"]?.GetValue<int>(),
                        // ("ScaleY": stretched down from its top too, e.g. to reach the vector bar's top edge.)
                        (float)(st["ScaleY"]?.GetValue<double>() ?? 1),
                        // ("SplitY": only the part below this y (its own coordinates) stretched down, from there.)
                        (float?)st["SplitY"]?.GetValue<double>(),
                        // ("SplitAt", "SplitLeftAt": the split points as x in its own coordinates, in place of fractions
                        // of its width: for one whose width changes with the game's state, as IV.4-N's background does.)
                        (float?)st["SplitAt"]?.GetValue<double>(), (float?)st["SplitLeftAt"]?.GetValue<double>(),
                        // ("ScaleLeft": the part left of SplitLeft stretched by this much instead (its art reaches less far).)
                        (float?)st["ScaleLeft"]?.GetValue<double>());
        foreach (var s in doc.Symbols)
        {
            try
            {
                m.Symbols[s.Id] = s switch
                {
                    ShapeSymbol sh => new ShapeAsset(m, sh),
                    MorphSymbol mo => new MorphAsset(m, mo),
                    ImageSymbol im => new ImageAsset(m, im),
                    SpriteSymbol sp => new SpriteAsset(m, sp),
                    ButtonSymbol b => new ButtonAsset(m, b),
                    FontSymbol f => new FontAsset(m, f),
                    TextSymbol t => new TextAsset(m, t),
                    EditTextSymbol e => new EditTextAsset(m, e),
                    SoundSymbol so => new SoundAsset(m, so),
                    _ => throw new NotSupportedException(s.GetType().Name),
                };
            }
            catch (Exception e) when (e is not OutOfMemoryException)
            {
                m.Errors.Add($"symbol {s.Id}: {e.Message}");
            }
        }
        m.Timeline = new TimelineAsset(m, doc.Timeline, "");
        return m;
    }

    /// <summary>Loads and compiles a script file; failures become an empty script plus an error.</summary>
    public Chunk? Script(string rel)
    {
        try
        {
            var file = Parser.Parse(File.ReadAllText(Path.Combine(Dir, rel)));
            return Compiler.Compile(file, $"{Name}/{rel}");
        }
        catch (Exception e) when (e is ParseException or CompileException or IOException)
        {
            Errors.Add($"{rel}: {e.Message}");
            return null;
        }
    }
}

public abstract class SymbolAsset(MovieAsset movie, int id)
{
    public readonly MovieAsset Movie = movie;
    public readonly int Id = id;
}

public static class Geo
{
    public static SKMatrix Matrix(double[]? m) =>
        m == null ? SKMatrix.Identity : new SKMatrix((float)m[0], (float)m[2], (float)m[4], (float)m[1], (float)m[3], (float)m[5], 0, 0, 1);

    public static SKRect Rect(double[] b) => new((float)b[0], (float)b[1], (float)b[2], (float)b[3]);

    public static SKColor Color(string? hex)
    {
        if (hex == null || hex.Length < 7) return SKColors.Black;
        uint v = uint.Parse(hex.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return hex.Length >= 9
            ? new SKColor((byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)v)
            : new SKColor((byte)(v >> 16), (byte)(v >> 8), (byte)v);
    }

    public static SKPath Path(string svg)
    {
        var p = SKPath.ParseSvgPathData(svg) ?? new SKPath();
        p.FillType = SKPathFillType.EvenOdd;
        return p;
    }
}

/// <summary>A drawable piece: fill or stroke with its path. Paint shaders are created once.</summary>
public sealed class ShapePart
{
    public required SKPath Path;
    public FillDoc? Fill;
    public StrokeDoc? Stroke;
    public SKShader? Shader;
    /// <summary>A bitmap fill whose image has a higher-resolution drawing: drawn with that, with Vectorize bitmaps on.</summary>
    public SKShader? HiResShader;
    /// <summary>HiResShader drawn whatever the Vectorize bitmaps setting (ImageAsset.HiResAlways).</summary>
    public bool HiResAlways;
    public SKColor Color;
    public bool ShaderReady;
    /// <summary>An opaque colour or gradient fill: its edges are sealed (see RenderContext.Seal).</summary>
    public bool Sealed;

    SKPath? _outline;
    /// <summary>The area a stroke covers (for hit testing), computed once.</summary>
    public SKPath StrokeOutline
    {
        get
        {
            if (_outline == null)
            {
                using var paint = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = Math.Max(1, (float)(Stroke?.Width ?? 1)) };
                _outline = paint.GetFillPath(Path) ?? new SKPath();
            }
            return _outline;
        }
    }
}

public sealed class ShapeAsset : SymbolAsset
{
    public readonly SKRect Bounds;
    public readonly List<ShapePart> Parts = [];

    public ShapeAsset(MovieAsset m, ShapeSymbol s) : base(m, s.Id)
    {
        Bounds = Geo.Rect(s.Bounds);
        foreach (var p in s.Parts)
            Parts.Add(new ShapePart { Path = Geo.Path(p.Path), Fill = p.Fill, Stroke = p.Stroke });
        Detailed = Parts.Sum(p => p.Path.PointCount) > 20000;
    }

    /// <summary>
    /// Very detailed artwork (the title's traced logo, tens of thousands of points): drawn once into an image
    /// at the size it's shown and reused while that stays the same (see RenderContext.DrawShape).
    /// </summary>
    public readonly bool Detailed;

    SKRect? _vectorBounds;

    /// <summary>The bounds with vectorized images drawn: wider where an image's replacement continues below it.</summary>
    public SKRect VectorBounds
    {
        get
        {
            if (_vectorBounds is { } known) return known;
            var r = Bounds;
            foreach (var part in Parts)
                if (part.Fill is { Type: "bitmap", Bitmap: int id } fill && Movie.Symbol(id) is ImageAsset { Vector: not null, Image: { } img } image && image.VectorHeight > img.Height)
                    r = SKRect.Union(r, Geo.Matrix(fill.Matrix).MapRect(SKRect.Create(img.Width, image.VectorHeight)));
            _vectorBounds = r;
            return r;
        }
    }

    /// <summary>A copy holding only some of another shape's parts (a live backdrop leaving part of the art out).</summary>
    public ShapeAsset(ShapeAsset source, IEnumerable<int> parts) : base(source.Movie, source.Id)
    {
        Bounds = source.Bounds;
        foreach (int i in parts) if (i >= 0 && i < source.Parts.Count) Parts.Add(source.Parts[i]);
        Detailed = source.Detailed;
    }

    public SKImage? Cached;
    public SKMatrix CachedFor;
}

/// <summary>Morph shape: start/end point lists with identical command structure.</summary>
public sealed class MorphAsset : SymbolAsset
{
    public sealed class Part
    {
        public FillDoc? FillStart, FillEnd;
        public StrokeDoc? StrokeStart, StrokeEnd;
        public required char[] Commands;
        public required float[] Start, End;
    }

    public readonly List<Part> Parts = [];
    public readonly SKRect StartBounds, EndBounds;

    public MorphAsset(MovieAsset m, MorphSymbol s) : base(m, s.Id)
    {
        StartBounds = Geo.Rect(s.StartBounds);
        EndBounds = Geo.Rect(s.EndBounds);
        foreach (var p in s.Parts)
        {
            var (cmds, a) = Tokens(p.Start);
            var (_, b) = Tokens(p.End);
            if (a.Length != b.Length) throw new InvalidDataException("morph paths differ in structure");
            Parts.Add(new Part
            {
                FillStart = p.FillStart, FillEnd = p.FillEnd,
                StrokeStart = p.StrokeStart, StrokeEnd = p.StrokeEnd,
                Commands = cmds, Start = a, End = b,
            });
        }
    }

    static (char[], float[]) Tokens(string path)
    {
        var cmds = new List<char>();
        var nums = new List<float>();
        foreach (var tok in path.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            int i = 0;
            if (char.IsLetter(tok[0])) { cmds.Add(tok[0]); i = 1; }
            if (i < tok.Length) nums.Add(float.Parse(tok.AsSpan(i), CultureInfo.InvariantCulture));
        }
        return (cmds.ToArray(), nums.ToArray());
    }

    public SKPath Build(Part p, float t)
    {
        var path = new SKPath { FillType = SKPathFillType.EvenOdd };
        int k = 0;
        float V() { float v = p.Start[k] + (p.End[k] - p.Start[k]) * t; k++; return v; }
        foreach (var c in p.Commands)
        {
            switch (c)
            {
                case 'M': path.MoveTo(V(), V()); break;
                case 'L': path.LineTo(V(), V()); break;
                case 'Q': { float cx = V(), cy = V(), x = V(), y = V(); path.QuadTo(cx, cy, x, y); break; }
                case 'Z': path.Close(); break;
            }
        }
        return path;
    }
}

public sealed class ImageAsset(MovieAsset m, ImageSymbol s) : SymbolAsset(m, s.Id)
{
    SKImage? _image;
    bool _tried;
    // (Its decoding and reading are done once, maybe ahead in the background: Player.PreloadNeighbours.)
    readonly object _load = new();

    /// <summary>Decodes the image and reads its replacements now (in the background, ahead of the scene).</summary>
    public void Preload(bool vector)
    {
        _ = Image;
        _ = HiRes;
        if (vector) _ = Vector;
    }

    public SKImage? Image
    {
        get
        {
            if (!_tried) lock (_load) if (!_tried)
            {
                // (Decoded now, not when first drawn: a large picture would stall that frame.)
                try { _image = SKImage.FromEncodedData(System.IO.Path.Combine(Movie.Dir, s.File))?.ToRasterImage(true); }
                catch (Exception e) when (e is not OutOfMemoryException) { Movie.Errors.Add($"image {Id}: {e.Message}"); }
                _tried = true;
            }
            return _image;
        }
    }

    SKImage? _hiRes;
    bool _hiResTried;

    /// <summary>HiRes is drawn whatever the Vectorize bitmaps setting (an upscaled photo, not a redrawing).</summary>
    public bool HiResAlways { get; private set; }

    /// <summary>A higher-resolution drawing of the image (hires-images/&lt;id&gt;.png in the movie, e.g. the loading
    /// island): drawn in its place at the image's size, so its extra pixels show on large screens. With Vectorize
    /// bitmaps on; an upscaled photo (upscaled-images/&lt;id&gt;.png, e.g. the skies, or a .ref naming another movie's) always
    /// (HiResAlways).</summary>
    public SKImage? HiRes
    {
        get
        {
            if (!_hiResTried) lock (_load) if (!_hiResTried)
            {
                var file = System.IO.Path.Combine(Movie.Dir, "upscaled-images", $"{Id}.png");
                // (Or another movie's: upscaled-images/<id>.ref names it, e.g. "sky1/upscaled-images/1.png", for a
                // copy of the same picture embedded in another movie, so it's stored once.)
                var redirect = System.IO.Path.ChangeExtension(file, ".ref");
                if (!File.Exists(file) && File.Exists(redirect))
                    file = System.IO.Path.Combine(Movie.Dir, "..", File.ReadAllText(redirect).Trim());
                if (File.Exists(file)) HiResAlways = true;
                else file = System.IO.Path.Combine(Movie.Dir, "hires-images", $"{Id}.png");
                if (File.Exists(file))
                    try { _hiRes = SKImage.FromEncodedData(file)?.ToRasterImage(true); }
                    catch (Exception e) when (e is not OutOfMemoryException) { Movie.Errors.Add($"hires image {Id}: {e.Message}"); }
                _hiResTried = true;
            }
            return _hiRes;
        }
    }

    List<VectorPart>? _vector;
    bool _vectorTried;

    /// <summary>The area the vector replacement covers (in the image's pixels); the rest stays the image.</summary>
    public SKPath? VectorClip;

    /// <summary>A repeating texture over the replacement: one tile of vector shapes, and its size (image pixels).</summary>
    public SKPicture? VectorTexture;
    public float VectorTextureSize;

    /// <summary>The replacement drawn at the screen's resolution (RenderContext.DrawVectorImage), by scale (each kept
    /// while it's still drawn: one image filled at two sizes, or the save slot's small picture, mustn't make the
    /// other be redrawn), and when each scale was last drawn (ms).</summary>
    public readonly Dictionary<float, SKImage> VectorCache = [];
    public readonly Dictionary<float, long> VectorUsed = [];

    /// <summary>Scales being drawn in the background (RenderContext.DrawVectorImage), and the finished ones to pick up.</summary>
    public readonly HashSet<float> VectorPending = [];
    public readonly System.Collections.Concurrent.ConcurrentQueue<(float Scale, SKImage? Image)> VectorDone = new();

    /// <summary>How the texture is drawn over the replacement ("blend": "overlay" shifts it lighter and darker).</summary>
    public SKBlendMode VectorTextureBlend = SKBlendMode.SrcOver;

    /// <summary>The texture's strength down the image ("fade": rows and opacities), as a vertical gradient; null for full.</summary>
    public SKShader? VectorTextureFade;

    /// <summary>The whole replacement's strength down the image (its own "fade"; Mt Ihu fades into the photo's mist).</summary>
    public SKShader? VectorFade;

    /// <summary>"seal": each shape's edge is also drawn as a hairline in its colour (flat tones meeting leave no seams).</summary>
    public bool VectorSeal;

    /// <summary>The replacement's height ("height", image pixels): taller than the image where it continues below it
    /// (the title screen's beach, over the sand strip added beneath).</summary>
    public float VectorHeight;

    /// <summary>"hideImage": the image isn't drawn while vectorized (another image's replacement covers its place).</summary>
    public bool VectorHidesImage;

    /// <summary>
    /// "layers": more replacements over other parts of the image, each drawn on its own after the main one, with its
    /// own "clip", "parts" and "texture" (a beach strip's water beside its sand, each with its own grain).
    /// </summary>
    public readonly List<VectorLayer> VectorLayers = [];

    /// <summary>
    /// A shape's gradient ("gradient"), in image pixels; null for its flat colour. Linear: {"from": [x, y], "to": [x, y],
    /// "stops": [[0..1, colour], ...]}; radial: {"type": "radial", "center": [x, y], "radius": r, "focal": [x, y]?, ...}.
    /// Either may carry a "matrix" [a, b, c, d, e, f] (as SVG's) placing the gradient's coordinates in the image.
    /// </summary>
    internal static SKShader? GradientFill(System.Text.Json.Nodes.JsonObject? g)
    {
        // A mesh gradient: a grid of colours ("data": base64 RGBA, "cols" x "rows", one every "step" pixels from
        // "origin"), blended smoothly (bicubic) between them: a soft field of colour (beach haze) with no bands.
        if (g?["type"]?.GetValue<string>() == "mesh") return MeshFill(g);
        if (g?["stops"] is not System.Text.Json.Nodes.JsonArray { Count: > 0 } stops) return null;
        var list = stops.OfType<System.Text.Json.Nodes.JsonArray>().Select(st => ((float)st[0]!.GetValue<double>(), Geo.Color(st[1]!.GetValue<string>()))).ToList();
        if (list.Count == 1) list.Add(list[0]);
        var colors = list.Select(st => st.Item2).ToArray();
        var pos = list.Select(st => st.Item1).ToArray();
        static SKPoint Pt(System.Text.Json.Nodes.JsonNode? n) => n is System.Text.Json.Nodes.JsonArray a ? new((float)a[0]!.GetValue<double>(), (float)a[1]!.GetValue<double>()) : default;
        var m = g["matrix"] is System.Text.Json.Nodes.JsonArray mx && mx.Count == 6
            ? new SKMatrix((float)mx[0]!.GetValue<double>(), (float)mx[2]!.GetValue<double>(), (float)mx[4]!.GetValue<double>(),
                           (float)mx[1]!.GetValue<double>(), (float)mx[3]!.GetValue<double>(), (float)mx[5]!.GetValue<double>(), 0, 0, 1)
            : SKMatrix.Identity;
        if (g["type"]?.GetValue<string>() == "radial")
        {
            var c = Pt(g["center"]);
            float r = (float)(g["radius"]?.GetValue<double>() ?? 0);
            var f = g["focal"] is { } fn ? Pt(fn) : c;
            return f == c
                ? SKShader.CreateRadialGradient(c, r, colors, pos, SKShaderTileMode.Clamp, m)
                : SKShader.CreateTwoPointConicalGradient(f, 0, c, r, colors, pos, SKShaderTileMode.Clamp, m);
        }
        if (g["from"] is null || g["to"] is null) return null;
        return SKShader.CreateLinearGradient(Pt(g["from"]), Pt(g["to"]), colors, pos, SKShaderTileMode.Clamp, m);
    }

    static SKShader? MeshFill(System.Text.Json.Nodes.JsonObject g)
    {
        int cols = g["cols"]!.GetValue<int>(), rows = g["rows"]!.GetValue<int>();
        float step = (float)g["step"]!.GetValue<double>();
        var o = g["origin"] as System.Text.Json.Nodes.JsonArray;
        float ox = (float)(o?[0]?.GetValue<double>() ?? 0), oy = (float)(o?[1]?.GetValue<double>() ?? 0);
        var bytes = Convert.FromBase64String(g["data"]!.GetValue<string>());
        if (bytes.Length != cols * rows * 4) return null;
        var info = new SKImageInfo(cols, rows, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        using var raw = SKImage.FromPixelCopy(info, bytes);
        if (raw == null) return null;
        // (Kept in floating point, so the blend between neighbouring grid colours is as smooth as it can be.)
        using var surface = SKSurface.Create(new SKImageInfo(cols, rows, SKColorType.RgbaF16, SKAlphaType.Premul));
        surface.Canvas.Clear(SKColors.Transparent);
        surface.Canvas.DrawImage(raw, 0, 0);
        using var grid = surface.Snapshot();
        // Grid point (i, j) at origin + step * (i, j): the image's pixel centres.
        var m = SKMatrix.CreateScale(step, step).PostConcat(SKMatrix.CreateTranslation(ox - step / 2, oy - step / 2));
        return grid.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, new SKSamplingOptions(SKCubicResampler.Mitchell), m);
    }

    /// <summary>A vertical gradient of opacity from [row, opacity] stops.</summary>
    static SKShader? FadeShader(System.Text.Json.Nodes.JsonArray? stops, float h)
    {
        if (stops is not { Count: > 1 }) return null;
        var list = stops.OfType<System.Text.Json.Nodes.JsonArray>().Select(st => ((float)st[0]!.GetValue<double>(), (float)st[1]!.GetValue<double>())).ToList();
        return SKShader.CreateLinearGradient(new SKPoint(0, 0), new SKPoint(0, h),
            list.Select(st => SKColors.Black.WithAlpha((byte)Math.Round(Math.Clamp(st.Item2, 0, 1) * 255))).ToArray(),
            list.Select(st => Math.Clamp(st.Item1 / h, 0, 1)).ToArray(), SKShaderTileMode.Clamp);
    }

    /// <summary>
    /// A vector replacement drawn over part of the image (Misc > Vector sand: vector-images/&lt;id&gt;.json beside the
    /// movie, from assets/sand), inside VectorClip, in the image's pixels; null when there's none.
    /// </summary>
    public List<VectorPart>? Vector
    {
        get
        {
            if (_vectorTried) return _vector;
            lock (_load)
            {
                if (!_vectorTried) ReadVector();
                return _vector;
            }
        }
    }

    void ReadVector()
    {
        {
            var file = System.IO.Path.Combine(Movie.Dir, "vector-images", $"{Id}.json");
            if (!File.Exists(file)) { _vectorTried = true; return; }
            try
            {
                if (System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(file)) is System.Text.Json.Nodes.JsonObject o && o["parts"] is System.Text.Json.Nodes.JsonArray parts)
                {
                    if (o["clip"]?.GetValue<string>() is { Length: > 0 } clip) VectorClip = Geo.Path(clip);
                    VectorSeal = o["seal"]?.GetValue<bool>() == true;
                    VectorHidesImage = o["hideImage"]?.GetValue<bool>() == true;
                    VectorHeight = Math.Max(Image?.Height ?? 0, (float)(o["height"]?.GetValue<double>() ?? 0));
                    if (Image is { } whole) VectorFade = FadeShader(o["fade"] as System.Text.Json.Nodes.JsonArray, VectorHeight);
                    (VectorTexture, VectorTextureSize, VectorTextureBlend, VectorTextureFade) = ReadTexture(o["texture"] as System.Text.Json.Nodes.JsonObject, VectorHeight);
                    if (o["layers"] is System.Text.Json.Nodes.JsonArray layers)
                        foreach (var l in layers.OfType<System.Text.Json.Nodes.JsonObject>())
                        {
                            var lt = l["texture"] as System.Text.Json.Nodes.JsonObject;
                            var (t, size, blend, fade) = ReadTexture(lt, VectorHeight);
                            // (A layer's tile may be wider than tall: "height", else square.)
                            float th = (float)(lt?["height"]?.GetValue<double>() ?? size);
                            VectorLayers.Add(new VectorLayer(l["clip"]?.GetValue<string>() is { Length: > 0 } lc ? Geo.Path(lc) : null,
                                (l["parts"] as System.Text.Json.Nodes.JsonArray ?? []).OfType<System.Text.Json.Nodes.JsonObject>().Select(VectorPart.Read).ToList(),
                                t, size, blend, fade, th));
                        }
                    _vector = parts.OfType<System.Text.Json.Nodes.JsonObject>().Select(VectorPart.Read).ToList();
                }
            }
            catch (Exception e) when (e is System.Text.Json.JsonException or IOException) { Movie.Errors.Add($"vector image {Id}: {e.Message}"); }
            _vectorTried = true;
        }
    }

    /// <summary>A replacement's texture ("texture": {"size", "parts", "blend", "fade"}): one tile recorded as a picture.</summary>
    (SKPicture? Tile, float Size, SKBlendMode Blend, SKShader? Fade) ReadTexture(System.Text.Json.Nodes.JsonObject? tex, float height)
    {
        if (tex?["parts"] is not System.Text.Json.Nodes.JsonArray tparts) return (null, 0, SKBlendMode.SrcOver, null);
        float size = (float)tex["size"]!.GetValue<double>();
        var blend = tex["blend"]?.GetValue<string>() == "overlay" ? SKBlendMode.Overlay : SKBlendMode.SrcOver;
        SKShader? fadeShader = null;
        if (tex["fade"] is System.Text.Json.Nodes.JsonArray { Count: > 1 } fade && Image != null)
        {
            var stops = fade.OfType<System.Text.Json.Nodes.JsonArray>().Select(st => ((float)st[0]!.GetValue<double>(), (float)st[1]!.GetValue<double>())).ToList();
            fadeShader = SKShader.CreateLinearGradient(new SKPoint(0, 0), new SKPoint(0, height),
                stops.Select(st => SKColors.Black.WithAlpha((byte)Math.Round(Math.Clamp(st.Item2, 0, 1) * 255))).ToArray(),
                stops.Select(st => Math.Clamp(st.Item1 / height, 0, 1)).ToArray(), SKShaderTileMode.Clamp);
        }
        var tile = new SKRect(0, 0, size, (float)(tex["height"]?.GetValue<double>() ?? size));
        using var recorder = new SKPictureRecorder();
        var c = recorder.BeginRecording(tile);
        using var paint = new SKPaint { IsAntialias = true, IsDither = true };
        foreach (var p in tparts.OfType<System.Text.Json.Nodes.JsonObject>())
        {
            // (A texture's shape may be a gradient too, e.g. a ripple's soft highlight fading out at its edges.)
            using var fill = GradientFill(p["gradient"] as System.Text.Json.Nodes.JsonObject);
            paint.Shader = fill;
            paint.Color = fill != null ? SKColors.Black : Geo.Color(p["color"]!.GetValue<string>());
            using var tp = Geo.Path(p["path"]!.GetValue<string>());
            c.DrawPath(tp, paint);
        }
        paint.Shader = null;
        return (recorder.EndRecording(), size, blend, fadeShader);
    }

    SKBitmap? _alpha;

    /// <summary>Opacity (0..255) of the image at a pixel, clamped to its edges (as a non-repeating bitmap fill is).</summary>
    public byte AlphaAt(float x, float y)
    {
        if (Image is not { } image) return 0;
        _alpha ??= SKBitmap.FromImage(image);
        int px = Math.Clamp((int)x, 0, _alpha.Width - 1), py = Math.Clamp((int)y, 0, _alpha.Height - 1);
        return _alpha.GetPixel(px, py).Alpha;
    }
}

public sealed class FontAsset : SymbolAsset
{
    public readonly FontSymbol Doc;
    public readonly SKPath[] Glyphs;
    public readonly Dictionary<char, int> ByChar = [];

    public FontAsset(MovieAsset m, FontSymbol f) : base(m, f.Id)
    {
        Doc = f;
        Glyphs = f.Glyphs.Select(g => Geo.Path(g.Path)).ToArray();
        for (int i = 0; i < f.Glyphs.Count; i++)
            if (f.Glyphs[i].Char.Length == 1) ByChar.TryAdd(f.Glyphs[i].Char[0], i);
    }
}

public sealed class TextAsset(MovieAsset m, TextSymbol t) : SymbolAsset(m, t.Id)
{
    public readonly TextSymbol Doc = t;
    public readonly SKMatrix Matrix = Geo.Matrix(t.Matrix);
    public readonly SKRect Bounds = Geo.Rect(t.Bounds);
}

public sealed class EditTextAsset(MovieAsset m, EditTextSymbol e) : SymbolAsset(m, e.Id)
{
    public readonly EditTextSymbol Doc = e;
    public readonly SKRect Bounds = Geo.Rect(e.Bounds);
}

/// <summary>A sound decoded to 32-bit float interleaved samples on first use.</summary>
public sealed class SoundAsset(MovieAsset m, SoundSymbol s) : SymbolAsset(m, s.Id)
{
    public readonly SoundSymbol Doc = s;
    float[]? _samples;
    public int Rate { get; private set; } = s.Rate;
    public int Channels { get; private set; } = s.Channels;

    readonly object _load = new();

    /// <summary>Whether it's decoded yet (Samples returns at once).</summary>
    public bool Decoded => _samples != null;

    public float[] Samples
    {
        get
        {
            if (_samples != null) return _samples;
            lock (_load) return _samples ?? Decode();
        }
    }

    float[] Decode()
    {
        {
            try
            {
                // A lossless replacement next to it is played instead: sounds/N.wav (loads fastest: no decoding), or
                // sounds/N.flac, beside sounds/N.ogg. (A replacement file given directly is read the same way.)
                var wav = System.IO.Path.ChangeExtension(System.IO.Path.Combine(Movie.Dir, Doc.File), ".wav");
                if (File.Exists(wav))
                {
                    (_samples, var rate, var channels) = WavDecoder.Decode(wav);
                    (Rate, Channels) = (rate, channels);
                    return _samples;
                }
                var flac = System.IO.Path.ChangeExtension(System.IO.Path.Combine(Movie.Dir, Doc.File), ".flac");
                if (File.Exists(flac))
                {
                    (_samples, var rate, var channels) = FlacDecoder.Decode(flac);
                    (Rate, Channels) = (rate, channels);
                    return _samples;
                }
                using var vorbis = new VorbisReader(System.IO.Path.Combine(Movie.Dir, Doc.File));
                Rate = vorbis.SampleRate;
                Channels = vorbis.Channels;
                var list = new List<float>();
                var buf = new float[4096 * Channels];
                int n;
                while ((n = vorbis.ReadSamples(buf, 0, buf.Length)) > 0) list.AddRange(buf.AsSpan(0, n));
                _samples = list.ToArray();
            }
            catch (Exception e) when (e is not OutOfMemoryException)
            {
                Movie.Errors.Add($"sound {Id}: {e.Message}");
                _samples = [];
            }
            return _samples;
        }
    }

    /// <summary>Decodes a standalone file (stream sounds) the same way.</summary>
    public static SoundAsset FromFile(MovieAsset m, string file, int id) =>
        new(m, new SoundSymbol { Id = id, File = file });
}

public sealed class SpriteAsset(MovieAsset m, SpriteSymbol s) : SymbolAsset(m, s.Id)
{
    public readonly TimelineAsset Timeline = new(m, s.Timeline, $"sprite{s.Id}");
}

public sealed class ButtonAsset : SymbolAsset
{
    public sealed class Record
    {
        public bool Up, Over, Down, Hit;
        public int Id, Depth;
        public SKMatrix Matrix;
        public CxForm Cx = CxForm.Identity;
    }

    public sealed class ActionEntry
    {
        public required HashSet<string> On;
        public int? Key;
        public Chunk? Script;
    }

    public readonly List<Record> Records = [];
    public readonly List<ActionEntry> Actions = [];
    public readonly Dictionary<string, SoundRefDoc> Sounds;
    public readonly bool TrackAsMenu;
    /// <summary>A scene's turn-around button: rolling over it shows the turn cursor (mousedrag "mouseturn...").</summary>
    public readonly bool TurnsAround;

    public ButtonAsset(MovieAsset m, ButtonSymbol b) : base(m, b.Id)
    {
        TrackAsMenu = b.TrackAsMenu == true;
        foreach (var r in b.Records.OrderBy(r => r.Depth))
            Records.Add(new Record
            {
                Up = r.States.Contains("up"), Over = r.States.Contains("over"),
                Down = r.States.Contains("down"), Hit = r.States.Contains("hit"),
                Id = r.Id, Depth = r.Depth, Matrix = Geo.Matrix(r.Matrix), Cx = CxForm.From(r.Cxform),
            });
        foreach (var a in b.Actions)
        {
            Actions.Add(new ActionEntry { On = [.. a.On], Key = a.Key, Script = m.Script(a.Script) });
            if (a.On.Contains("rollOver") && a.Script is { } rel && File.Exists(Path.Combine(m.Dir, rel)) &&
                File.ReadAllText(Path.Combine(m.Dir, rel)).Contains("\"mouseturn", StringComparison.OrdinalIgnoreCase))
                TurnsAround = true;
        }
        Sounds = b.Sounds ?? [];
    }
}

public sealed class FrameAsset
{
    public List<OpDoc> Ops = [];
    public List<Chunk> Scripts = [];
    public List<SoundRefDoc> Sounds = [];
    public List<string> Labels = [];
}

public sealed class TimelineAsset
{
    public readonly int FrameCount;
    public readonly FrameAsset[] Frames;
    public readonly Dictionary<string, int> Labels = new(StringComparer.OrdinalIgnoreCase);
    public readonly StreamDoc? Stream;
    public readonly Dictionary<(int Frame, int Depth), List<(HashSet<string> On, int? Key, Chunk? Script)>> ClipEvents = [];

    public TimelineAsset(MovieAsset m, TimelineDoc doc, string name)
    {
        FrameCount = Math.Max(1, doc.FrameCount);
        Frames = new FrameAsset[Math.Max(FrameCount, doc.Frames.Count)];
        for (int i = 0; i < Frames.Length; i++)
        {
            var f = new FrameAsset();
            if (i < doc.Frames.Count)
            {
                var fd = doc.Frames[i];
                f.Ops = fd.Ops ?? [];
                f.Sounds = fd.Sounds ?? [];
                f.Labels = fd.Labels ?? [];
                foreach (var s in fd.Scripts ?? [])
                    if (m.Script(s) is { } c) f.Scripts.Add(c);
                foreach (var l in f.Labels) Labels.TryAdd(l, i + 1);
                foreach (var op in f.Ops)
                {
                    if (op.Events == null || op.Place is not int depth) continue;
                    ClipEvents[(i + 1, depth)] = op.Events.Select(e => (new HashSet<string>(e.On), e.Key, m.Script(e.Script))).ToList();
                }
            }
            Frames[i] = f;
        }
        Stream = doc.Stream;
    }

    public FrameAsset Frame(int frame) => Frames[Math.Clamp(frame, 1, Frames.Length) - 1];
}

/// <summary>Flash color transform: c' = c * mul + add (per channel, add in 0..255).</summary>
public readonly record struct CxForm(float RM, float GM, float BM, float AM, float RA, float GA, float BA, float AA)
{
    public static readonly CxForm Identity = new(1, 1, 1, 1, 0, 0, 0, 0);

    public bool IsIdentity => this == Identity;

    /// <summary>Leaves opaque colours opaque (alpha untouched or only raised).</summary>
    public bool IsOpaque => AM >= 1 && AA >= 0;

    public static CxForm From(double[]? c) =>
        c == null ? Identity : new((float)c[0], (float)c[1], (float)c[2], (float)c[3], (float)c[4], (float)c[5], (float)c[6], (float)c[7]);

    /// <summary>Applies <paramref name="inner"/> first, then this.</summary>
    public CxForm Concat(CxForm inner) => new(
        RM * inner.RM, GM * inner.GM, BM * inner.BM, AM * inner.AM,
        RM * inner.RA + RA, GM * inner.GA + GA, BM * inner.BA + BA, AM * inner.AA + AA);

    public SKColor Apply(SKColor c) => new(
        Clamp(c.Red * RM + RA), Clamp(c.Green * GM + GA), Clamp(c.Blue * BM + BA), Clamp(c.Alpha * AM + AA));

    static byte Clamp(float v) => (byte)Math.Clamp((int)MathF.Round(v), 0, 255);

    /// <summary>Equivalent Skia color filter (operates on unpremultiplied colors, as Flash does).</summary>
    public SKColorFilter ToFilter() => SKColorFilter.CreateColorMatrix(
    [
        RM, 0, 0, 0, RA / 255f,
        0, GM, 0, 0, GA / 255f,
        0, 0, BM, 0, BA / 255f,
        0, 0, 0, AM, AA / 255f,
    ]);
}


/// <summary>
/// One shape of a vector image: filled (its colour or gradient), or stroked ("stroke": width in image pixels, with
/// "cap" and "join" as SVG's). "rule": "nonzero" fills as SVG's default does (else even-odd, as traced shapes need).
/// </summary>
/// <summary>One more replacement over part of an image (ImageAsset.VectorLayers).</summary>
public sealed record VectorLayer(SKPath? Clip, List<VectorPart> Parts, SKPicture? Texture, float TextureSize, SKBlendMode TextureBlend, SKShader? TextureFade,
                                 float TextureHeight = 0);

public sealed record VectorPart(SKPath Path, SKColor Color, SKShader? Fill, float Stroke, SKStrokeCap Cap, SKStrokeJoin Join, float Miter)
{
    /// <summary>"clip": drawn only inside this outline (e.g. the chronicle screen's scanlines, inside the screen).</summary>
    public SKPath? Clip { get; init; }

    /// <summary>"blur": its edge softened by this much (a Gaussian's sigma, in the image's pixels), e.g. distant clouds.</summary>
    public float Blur { get; init; }

    public static VectorPart Read(System.Text.Json.Nodes.JsonObject p)
    {
        var path = Geo.Path(p["path"]!.GetValue<string>());
        if (p["rule"]?.GetValue<string>() == "nonzero") path.FillType = SKPathFillType.Winding;
        float stroke = (float)(p["stroke"]?.GetValue<double>() ?? 0);
        var cap = p["cap"]?.GetValue<string>() switch { "round" => SKStrokeCap.Round, "square" => SKStrokeCap.Square, _ => SKStrokeCap.Butt };
        var join = p["join"]?.GetValue<string>() switch { "round" => SKStrokeJoin.Round, "bevel" => SKStrokeJoin.Bevel, _ => SKStrokeJoin.Miter };
        return new VectorPart(path, Geo.Color(p["color"]!.GetValue<string>()), ImageAsset.GradientFill(p["gradient"] as System.Text.Json.Nodes.JsonObject),
            stroke, cap, join, (float)(p["miter"]?.GetValue<double>() ?? 4))
        {
            Clip = p["clip"]?.GetValue<string>() is { Length: > 0 } clip ? Geo.Path(clip) : null,
            Blur = (float)(p["blur"]?.GetValue<double>() ?? 0),
        };
    }
}
