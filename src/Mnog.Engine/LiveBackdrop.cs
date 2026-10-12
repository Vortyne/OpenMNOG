using System.Text.Json.Nodes;
using Mnog.Engine.Assets;
using Mnog.Engine.Display;
using SkiaSharp;

namespace Mnog.Engine;

/// <summary>
/// A dialog backdrop drawn live from the scene it was made from (Vectorize bitmaps): the original backdrops are
/// blurred, zoomed-in screenshots of a scene, so instead the scene itself is shown there, zoomed and blurred by the
/// engine, sharp at any resolution and still animating. Set up by a scene's fix file ("backdrop", backdrop.json):
/// <code>{ "image": 1, "scene": "III.1-N", "frame": 2, "layers": { "Middle-Ground": 390, ... },
///   "sky": ["Back-Ground/Sky-holder", "sky2"], "hide": ["Middle-Ground/Kapura", "Buttons"],
///   "zoom": 2, "x": 124.5, "y": 130.5, "blur": 6.5 }</code>
/// x/y: the scene's stage point at the image's top left; zoom: image pixels per stage pixel; blur: the Gaussian
/// blur in image pixels. The scene copy is inert (no scripts, sounds or clicks; see MovieClip.Inert).
/// </summary>
public sealed class LiveBackdrop
{
    public required int Image;
    public required string Scene;
    public int Frame = 1;
    public readonly Dictionary<string, double> Layers = [];
    /// <summary>Movies the scene loads into holders by script ("sky": one, "loads": more), loaded here instead.</summary>
    public readonly List<(string Path, string Movie)> Loads = [];
    public readonly List<string> Hide = [];
    public float Zoom = 1, X, Y, Blur;
    /// <summary>"wide": shown past the original image's sides (the scene continues there), to the view's edges.</summary>
    public bool Wide;
    /// <summary>"vertical": the layer positions are their y (a scene that pans up and down), not their x.</summary>
    public bool Vertical;
    /// <summary>"mirror": the painting shows the scene flipped left to right (x is then the scene point at its left edge).</summary>
    public bool Mirror;
    /// <summary>"frost": the dialog's text box blurs the backdrop behind it (frosted glass), where its text needs it.</summary>
    public bool Frost;
    /// <summary>"replaces": other images in the dialog scene that the live scene supersedes (drawn as nothing then),
    /// e.g. bitmap frames of a fire the scene itself animates.</summary>
    public readonly HashSet<int> Replaces = [];
    /// <summary>"sharp": objects kept in focus (drawn unblurred over the blurred scene: a depth-of-field backdrop).</summary>
    public readonly List<string> Sharp = [];
    /// <summary>The sharp objects, found when the scene copy is made.</summary>
    public List<DisplayObject> SharpObjects = [];
    /// <summary>"softer": objects blurred more than the rest ({"Back-Ground": 3}: extra blur in image pixels), e.g. distant
    /// rocks the painting had hazier. Drawn first, beneath the rest of the scene (so for back layers).</summary>
    public readonly Dictionary<string, float> Softer = [];
    /// <summary>"sharpAt": objects less blurred than the rest, each at its own blur ({"Middle-Ground/#9": 1}: image pixels),
    /// drawn over the blurred scene and beneath the "sharp" ones.</summary>
    public readonly Dictionary<string, float> SharpAt = [];
    public List<(DisplayObject Obj, float Blur)> SharpAtObjects = [];
    public List<(DisplayObject Obj, float Blur)> SofterObjects = [];
    /// <summary>"sharpBlur": a lighter blur for the sharp objects (image pixels; 0 = fully sharp).</summary>
    public float SharpBlur;
    /// <summary>"slow": clips played slower ({"Middle-Ground/#13": 0.5}: half speed), e.g. a boat bobbing more gently.</summary>
    public readonly Dictionary<string, float> Slow = [];

    /// <summary>"hold": clips held at a frame ({"Middle-Ground/Rope1": 1}), e.g. a cable at rest rather than swinging.</summary>
    public readonly Dictionary<string, int> Hold = [];
    /// <summary>"hideParts": parts of a shape left out ({"Middle-Ground/#32": [0, 1, 2]}), e.g. a jetty drawn in one
    /// shape with a sign the backdrop keeps.</summary>
    public readonly Dictionary<string, int[]> HideParts = [];
    /// <summary>"copies": more of the scene's own art, to fill where something was left out: [{"of": "Middle-Ground/#27",
    /// "parts": [1, 2], "depth": 28, "dx": 200, "dy": 40}] places those parts of that shape again, moved by dx/dy (in its
    /// parent's coordinates), at that depth of the same parent.</summary>
    public readonly List<(string Of, int[]? Parts, int Depth, float Dx, float Dy, float Scale, bool Flip)> Copies = [];
    /// <summary>"move": objects moved by [dx, dy] (in their parent's coordinates), where the original painting placed
    /// them a little differently from the scene ({"Middle-Ground/#39": [0, -25]}); a third number scales it about its
    /// own origin ([0, -25, 1.1]).</summary>
    public readonly Dictionary<string, (float Dx, float Dy, float Scale)> Move = [];
    /// <summary>"alpha": objects' opacity set (0..1), e.g. a room's darkness overlay as light as the painting had it
    /// ({"Darkness": 0.63}).</summary>
    public readonly Dictionary<string, float> Alpha = [];
    /// <summary>"depth": objects moved to another depth of their parent ({"Middle-Ground/#75": 100}), e.g. a torch the
    /// painting has in front of a crab the scene draws over it.</summary>
    public readonly Dictionary<string, int> Depth = [];
    /// <summary>"text": static text drawn with other lines than the scene's ({"Middle-Ground/#8": [runs as in movie.json]}),
    /// e.g. a wall of glyphs the painting spelled differently.</summary>
    public readonly Dictionary<string, List<Mnog.Format.TextRunDoc>> Text = [];
    /// <summary>"bloom": a soft glow from the scene's lit parts, as the painting had ({"threshold": 20, "radius": 6,
    /// "strength": 1, "tint": [1.25, 1, 0.7]}): what's brighter than threshold (0..255), blurred by radius (image
    /// pixels), tinted and added on top.</summary>
    public (float Threshold, float Radius, float Strength, float[] Tint)? Bloom;
    /// <summary>"tone": a contrast curve per colour channel, [[gain, gamma] for red, green, blue] (out = gain * in^gamma,
    /// 0..1), where the painting's lighting was moodier than the scene's ([[0.52, 1.59], [0.51, 1.49], [0.49, 1.29]]).</summary>
    public byte[][]? Tone;
    /// <summary>"light": light the painting had that the scene doesn't, added on as vertical gradients across the view
    /// ([{"top": 285, "bottom": 363, "color": "#B76E06"}]: none at top, rising to color at bottom; image pixels).</summary>
    public readonly List<(float Top, float Bottom, SKColor Color)> Light = [];
    /// <summary>"glows": distant lightstones the painting had, drawn as the lightstone item's glow ([{"x": 405, "y": 64, "r": 30}]:
    /// centre and radius in image pixels).</summary>
    public readonly List<(float X, float Y, float R, SKColor Color)> Glows = [];
    /// <summary>"shapes": painted light the scene doesn't have, e.g. rays: [{"points": [[x, y], ..], "from": [x, y], "to": [x, y],
    /// "color": "#5A2E04"}]: the polygon filled with color at "from" fading out by "to" (image pixels), blurred as the scene.</summary>
    /// "toColor" (default: color, clear) ends the gradient in another colour; "under": drawn beneath the scene (lighting a
    /// wall whose own lighting is hidden, its glyphs staying on top).
    /// "colors": [..] evenly spaced gradient colours instead; "radial": from is the centre and to a point on the edge.
    public readonly List<(SKPoint[] Points, SKPoint From, SKPoint To, SKColor[] Colors, bool Radial, bool Under)> Shapes = [];
    /// <summary>"flares": a lens flare's pieces, added on top ([{"x": 185, "y": 22, "r": 50, "color": "#FFFFF0", "ring": false}]:
    /// a soft disc of color fading out at radius r, or with "ring" a thin soft circle; image pixels).</summary>
    public readonly List<(float X, float Y, float R, SKColor Color, bool Ring)> Flares = [];

    /// <summary>The scene copy (made on first use) and the tick it was last drawn.</summary>
    public MovieClip? Root;
    public long LastDrawn;

    /// <summary>"still": the frame held as it is (the scene copy isn't run on: a picture, as the chronicle's episode stills).</summary>
    public bool Still;
    /// <summary>"overlay": the image's vector replacement (vector-images/&lt;image&gt;.json) drawn over the live scene, e.g. the
    /// chronicle orbs' glass and scanlines.</summary>
    public bool Overlay;
    /// <summary>"clip": the live scene shown only inside this outline (an SVG path in the image's pixels), e.g. the
    /// chronicle's round orbs (their images are round, their shapes square).</summary>
    public SKPath? Clip;
    /// <summary>"played": the frame reached by playing the scene from its start (its clips' animations run along), not
    /// by jumping there (which starts every clip in it afresh): the chronicle pictures taken from episodes as they play.</summary>
    public bool Played;
    /// <summary>"unbar": the episode's letterbox (a black frame bigger than the stage, drawn over the scene) left out,
    /// so a picture framed past its edge shows the art that goes on beneath it.</summary>
    public bool Unbar;
    /// <summary>"if": shown only while the game's variables (on _level1) have these values ({"OnuKoroLava": "1"}); the first
    /// backdrop for an image whose "if" holds is used, so a later one without it is the fallback (e.g. XII.6A's
    /// lightstone glows only once Onu-Koro's lava is fixed).</summary>
    public readonly Dictionary<string, string> If = [];

    /// <summary>Reads a movie's backdrop.json: one backdrop, or several ("backdrops": [..], one per image, e.g. the
    /// chronicle's pictures). Empty when it has none or it can't be read.</summary>
    public static List<LiveBackdrop> Load(string movieDir)
    {
        var file = Path.Combine(movieDir, "backdrop.json");
        if (!File.Exists(file)) return [];
        try
        {
            if (JsonNode.Parse(File.ReadAllText(file)) is not JsonObject o) return [];
            if (o["backdrops"] is JsonArray all)
                return all.OfType<JsonObject>().Select(Parse).OfType<LiveBackdrop>().ToList();
            return Parse(o) is { } one ? [one] : [];
        }
        catch (Exception e) when (e is System.Text.Json.JsonException or IOException) { return []; }
    }

    /// <summary>One backdrop's settings (as backdrop.json has them; also the Help's pictures, which have no "image").</summary>
    public static LiveBackdrop? Parse(JsonObject o)
    {
        try
        {
            var b = new LiveBackdrop { Image = o["image"]?.GetValue<int>() ?? 0, Scene = o["scene"]!.GetValue<string>() };
            if (o["frame"] is { } f) b.Frame = f.GetValue<int>();
            if (o["layers"] is JsonObject layers)
                foreach (var (k, v) in layers) if (v != null) b.Layers[k] = v.GetValue<double>();
            if (o["sky"] is JsonArray { Count: 2 } sky) b.Loads.Add((sky[0]!.GetValue<string>(), sky[1]!.GetValue<string>()));
            if (o["loads"] is JsonArray loads)
                foreach (var l in loads) if (l is JsonArray { Count: 2 } pair) b.Loads.Add((pair[0]!.GetValue<string>(), pair[1]!.GetValue<string>()));
            if (o["hide"] is JsonArray hide) foreach (var h in hide) if (h != null) b.Hide.Add(h.GetValue<string>());
            b.Zoom = (float)(o["zoom"]?.GetValue<double>() ?? 1);
            b.X = (float)(o["x"]?.GetValue<double>() ?? 0);
            b.Y = (float)(o["y"]?.GetValue<double>() ?? 0);
            b.Blur = (float)(o["blur"]?.GetValue<double>() ?? 0);
            b.SharpBlur = (float)(o["sharpBlur"]?.GetValue<double>() ?? 0);
            if (o["sharpAt"] is JsonObject sa) foreach (var (k, v) in sa) if (v != null) b.SharpAt[k] = (float)v.GetValue<double>();
            if (o["softer"] is JsonObject so) foreach (var (k, v) in so) if (v != null) b.Softer[k] = (float)v.GetValue<double>();
            b.Wide = o["wide"]?.GetValue<bool>() == true;
            b.Vertical = o["vertical"]?.GetValue<bool>() == true;
            b.Mirror = o["mirror"]?.GetValue<bool>() == true;
            b.Frost = o["frost"]?.GetValue<bool>() == true;
            if (o["slow"] is JsonObject sl) foreach (var (k, v) in sl) if (v != null) b.Slow[k] = (float)v.GetValue<double>();
            if (o["hold"] is JsonObject hold) foreach (var (k, v) in hold) if (v != null) b.Hold[k] = v.GetValue<int>();
            if (o["hideParts"] is JsonObject hp)
                foreach (var (k, v) in hp) if (v is JsonArray a) b.HideParts[k] = a.Select(n => n!.GetValue<int>()).ToArray();
            if (o["move"] is JsonObject mv)
                foreach (var (k, v) in mv)
                    if (v is JsonArray { Count: 2 or 3 } d)
                        b.Move[k] = ((float)d[0]!.GetValue<double>(), (float)d[1]!.GetValue<double>(), d.Count > 2 ? (float)d[2]!.GetValue<double>() : 1);
            if (o["text"] is JsonObject tx)
                foreach (var (k, v) in tx)
                    if (v is JsonArray runs)
                        b.Text[k] = runs.OfType<JsonObject>().Select(r => new Mnog.Format.TextRunDoc
                        {
                            Font = r["font"]!.GetValue<int>(), Size = r["size"]!.GetValue<double>(), Color = r["color"]!.GetValue<string>(),
                            X = r["x"]!.GetValue<double>(), Y = r["y"]!.GetValue<double>(), Text = r["text"]?.GetValue<string>() ?? "",
                            Glyphs = r["glyphs"]!.AsArray().Select(n => n!.GetValue<int>()).ToList(),
                            Advances = r["advances"]!.AsArray().Select(n => n!.GetValue<double>()).ToList(),
                        }).ToList();
            if (o["depth"] is JsonObject dp)
                foreach (var (k, v) in dp) if (v != null) b.Depth[k] = v.GetValue<int>();
            if (o["alpha"] is JsonObject al)
                foreach (var (k, v) in al) if (v != null) b.Alpha[k] = (float)v.GetValue<double>();
            if (o["bloom"] is JsonObject bl)
                b.Bloom = ((float)(bl["threshold"]?.GetValue<double>() ?? 0), (float)(bl["radius"]?.GetValue<double>() ?? 6),
                    (float)(bl["strength"]?.GetValue<double>() ?? 1),
                    (bl["tint"] as JsonArray)?.Select(n => (float)n!.GetValue<double>()).ToArray() ?? [1f, 1f, 1f]);
            if (o["tone"] is JsonArray { Count: 3 } tone)
                b.Tone = tone.Select(ch => ch is JsonArray { Count: 2 } gg
                    ? Enumerable.Range(0, 256).Select(i => (byte)Math.Clamp(Math.Round(255 * gg[0]!.GetValue<double>() * Math.Pow(i / 255.0, gg[1]!.GetValue<double>())), 0, 255)).ToArray()
                    : Enumerable.Range(0, 256).Select(i => (byte)i).ToArray()).ToArray();
            if (o["light"] is JsonArray lights)
                foreach (var l in lights.OfType<JsonObject>())
                    b.Light.Add(((float)l["top"]!.GetValue<double>(), (float)l["bottom"]!.GetValue<double>(), Rgba(l["color"]!.GetValue<string>())));
            if (o["glows"] is JsonArray glows)
                foreach (var g in glows.OfType<JsonObject>())
                    b.Glows.Add(((float)g["x"]!.GetValue<double>(), (float)g["y"]!.GetValue<double>(), (float)g["r"]!.GetValue<double>(),
                        SKColors.White));
            if (o["shapes"] is JsonArray shapes)
                foreach (var sh in shapes.OfType<JsonObject>())
                {
                    static SKPoint Pt(JsonNode? n) => new((float)n![0]!.GetValue<double>(), (float)n[1]!.GetValue<double>());
                {
                    var color = sh["color"] is { } c0 ? Rgba(c0.GetValue<string>()) : SKColors.Transparent;
                    SKColor[] colors = sh["colors"] is JsonArray cs ? cs.Select(c => Rgba(c!.GetValue<string>())).ToArray()
                        : [color, sh["toColor"] is { } tc ? Rgba(tc.GetValue<string>()) : color.WithAlpha(0)];
                    b.Shapes.Add((sh["points"]!.AsArray().Select(Pt).ToArray(), Pt(sh["from"]), Pt(sh["to"]), colors,
                        sh["radial"]?.GetValue<bool>() == true, sh["under"]?.GetValue<bool>() == true));
                }
                }
            if (o["flares"] is JsonArray flares)
                foreach (var fl in flares.OfType<JsonObject>())
                    b.Flares.Add(((float)fl["x"]!.GetValue<double>(), (float)fl["y"]!.GetValue<double>(), (float)fl["r"]!.GetValue<double>(),
                        Rgba(fl["color"]!.GetValue<string>()), fl["ring"]?.GetValue<bool>() == true));
            if (o["copies"] is JsonArray copies)
                foreach (var c in copies.OfType<JsonObject>())
                    b.Copies.Add((c["of"]!.GetValue<string>(), (c["parts"] as JsonArray)?.Select(n => n!.GetValue<int>()).ToArray(),
                        c["depth"]!.GetValue<int>(), (float)(c["dx"]?.GetValue<double>() ?? 0), (float)(c["dy"]?.GetValue<double>() ?? 0),
                        (float)(c["scale"]?.GetValue<double>() ?? 1), c["flip"]?.GetValue<bool>() == true));
            if (o["sharp"] is JsonArray sharp) foreach (var sp in sharp) if (sp != null) b.Sharp.Add(sp.GetValue<string>());
            if (o["replaces"] is JsonArray rep) foreach (var r in rep) if (r != null) b.Replaces.Add(r.GetValue<int>());
            b.Still = o["still"]?.GetValue<bool>() == true;
            b.Overlay = o["overlay"]?.GetValue<bool>() == true;
            b.Played = o["played"]?.GetValue<bool>() == true;
            b.Unbar = o["unbar"]?.GetValue<bool>() == true;
            if (o["if"] is JsonObject cond) foreach (var (k, v) in cond) b.If[k] = v?.ToString() ?? "";
            if (o["clip"]?.GetValue<string>() is { Length: > 0 } clip) b.Clip = SKPath.ParseSvgPathData(clip);
            return b;
        }
        catch (Exception e) when (e is System.Text.Json.JsonException or IOException or InvalidOperationException or NullReferenceException) { return null; }
    }

    /// <summary>Hides the letterbox frames: shapes all solid black, wider and taller than the stage (in their own units,
    /// as the episodes place them at full size), near the top of the scene.</summary>
    public static void HideLetterbox(MovieClip clip, int depth)
    {
        foreach (var c in clip.Children.Values)
        {
            if (c is GraphicObject { Asset: ShapeAsset shape } g && shape.Bounds.Width >= 880 && shape.Bounds.Height >= 430 &&
                shape.Parts.Count > 0 && shape.Parts.All(p => p.Fill is { Type: "solid" } f && f.Color is { } col && col.StartsWith("#000000", StringComparison.OrdinalIgnoreCase)))
                g.Visible = false;
            else if (c is MovieClip mc && depth < 2) HideLetterbox(mc, depth + 1);
        }
    }

    /// <summary>"#RRGGBB" or "#RRGGBBAA" (the game's order, alpha last; SkiaSharp's own parser reads 8 digits as AARRGGBB).</summary>
    static SKColor Rgba(string c) =>
        c.Length == 9 && c[0] == '#' ? SKColor.Parse("#" + c[7..9] + c[1..7]) : SKColor.Parse(c);

    /// <summary>A path of names, or "#depth" for an unnamed object (e.g. "Middle-Ground/#9").</summary>
    public static DisplayObject? FindIn(MovieClip root, string path) => Find(root, path);

    static readonly System.Text.RegularExpressions.Regex TellTarget = new(
        @"tellTarget\s*\(\s*""([^""]*)""\s*\)\s*\{([^{}]*)\}");
    static readonly System.Text.RegularExpressions.Regex ClipCall = new(
        @"(gotoAndPlay|gotoAndStop|play|stop)\s*\(\s*(?:""([^""]*)""|(\d+))?\s*\)");

    static void Age(MovieClip clip, int tick)
    {
        clip.CreatedTick = Math.Min(clip.CreatedTick, tick);
        foreach (var c in clip.Children.Values)
            if (c is MovieClip mc) Age(mc, tick);
    }

    /// <summary>
    /// A played copy's cues at a frame of its timeline: the frame script's tellTarget blocks (both branches of an
    /// if alike), their gotoAndPlay, gotoAndStop, play and stop done on that clip.
    /// </summary>
    static void Cue(MovieClip root, int frame)
    {
        foreach (var chunk in root.Timeline.Frame(frame).Scripts)
        {
            int slash = chunk.Name.IndexOf('/');
            var file = slash > 0 ? Path.Combine(root.Movie.Dir, chunk.Name[(slash + 1)..]) : null;
            if (file == null || !File.Exists(file)) continue;
            foreach (System.Text.RegularExpressions.Match t in TellTarget.Matches(File.ReadAllText(file)))
            {
                if (Find(root, t.Groups[1].Value.TrimStart('/')) is not MovieClip clip) continue;
                foreach (System.Text.RegularExpressions.Match c in ClipCall.Matches(t.Groups[2].Value))
                {
                    int? to = c.Groups[2].Success ? clip.Timeline.Labels.TryGetValue(c.Groups[2].Value, out int l) ? l : null
                            : c.Groups[3].Success ? int.Parse(c.Groups[3].Value) : null;
                    switch (c.Groups[1].Value)
                    {
                        case "gotoAndPlay": if (to is int a) { clip.GotoFrame(a); clip.Playing = true; } break;
                        case "gotoAndStop": if (to is int b) { clip.GotoFrame(b); clip.Playing = false; } break;
                        case "play": clip.Playing = true; break;
                        case "stop": clip.Playing = false; break;
                    }
                }
            }
        }
    }

    static DisplayObject? Find(MovieClip root, string path)
    {
        DisplayObject? o = root;
        foreach (var part in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
            o = o is MovieClip mc
                ? part.StartsWith('#') && int.TryParse(part.AsSpan(1), out int depth) ? mc.Children.GetValueOrDefault(depth) : mc.ChildByName(part)
                : null;
        return o;
    }

    /// <summary>Makes the scene copy: its timeline at the backdrop's frame, layers placed, sky loaded, things hidden.</summary>
    /// <summary>Played: through its pauses, following its cues to its clips (see Build; the wallpapers').</summary>
    public bool Cues;

    public MovieClip? Build(Player player)
    {
        var movie = player.GetMovie(Scene);
        if (movie == null) return null;
        var root = new MovieClip(player, movie, movie.Timeline) { Inert = true, LoadedRoot = true, Name = "backdrop" };
        root.Construct();
        if (Played)
        {
            // (Inert: its scripts don't run, so it doesn't stop on the way. With Cues, a frame that just stops, a pause
            // for dialogue, is played through, and its cues to its clips, tellTarget's gotos, plays and stops, followed.)
            root.Playing = true;
            MovieClip.FollowLoops = Cues;
            for (int guard = 0; root.CurrentFrame < Frame && guard < 20000; guard++)
            {
                int was = root.CurrentFrame;
                root.RunFrame();
                if (!Cues) continue;
                // (All in one of the game's ticks: what was placed this step plays from the next, as on a new tick.)
                Age(root, player.TickCount - 1);
                root.Playing = true;
                if (root.CurrentFrame != was) Cue(root, root.CurrentFrame);
            }
            MovieClip.FollowLoops = false;
        }
        else if (Frame > 1) root.GotoFrame(Frame);
        root.Playing = false;
        if (Unbar) HideLetterbox(root, 0);
        foreach (var (name, x) in Layers)
            if (Find(root, name) is { } layer) { if (Vertical) layer.Y = x; else layer.X = x; }
        foreach (var (loadPath, loadMovie) in Loads)
            if (Find(root, loadPath) is MovieClip { Parent: MovieClip loadParent } holder && player.GetMovie(loadMovie) is { } loaded)
            {
                var clip = new MovieClip(player, loaded, loaded.Timeline) { Inert = true, LoadedRoot = true, Name = holder.Name, Matrix = holder.Matrix };
                loadParent.RemoveChild(holder);
                loadParent.AddChild(clip, holder.Depth);
                clip.Construct();
            }
        foreach (var path in Hide)
            if (Find(root, path) is { } o) o.Visible = false;
        // Copies first, taken from the art as the scene has it (before any of its parts are left out).
        // "flip": mirrored left to right across the parent's x = 0 (then moved by dx/dy).
        foreach (var (of, parts, depth, dx, dy, scale, flip) in Copies)
            if (Find(root, of) is GraphicObject { Asset: ShapeAsset shape, Parent: MovieClip parent } src && !parent.Children.ContainsKey(depth))
            {
                var copy = new GraphicObject(player, parts == null ? shape : new ShapeAsset(shape, parts))
                    { Matrix = src.Matrix.PreConcat(SKMatrix.CreateScale(scale, scale)).PostConcat(SKMatrix.CreateScale(flip ? -1 : 1, 1)).PostConcat(SKMatrix.CreateTranslation(dx, dy)), ColorTransform = src.ColorTransform, TransformedByScript = true };
                parent.AddChild(copy, depth);
            }
        foreach (var (path, parts) in HideParts)
            if (Find(root, path) is GraphicObject { Asset: ShapeAsset shape } g)
                g.Asset = new ShapeAsset(shape, Enumerable.Range(0, shape.Parts.Count).Except(parts));
        foreach (var (path, rate) in Slow)
            if (Find(root, path) is MovieClip slowed) slowed.Rate = rate;
        foreach (var (path, frame) in Hold)
            if (Find(root, path) is MovieClip held) { held.GotoFrame(frame); held.Playing = false; }
        foreach (var (path, runs) in Text)
            if (Find(root, path) is GraphicObject { Asset: TextAsset t } g)
                g.Asset = new TextAsset(t.Movie, new Mnog.Format.TextSymbol { Id = t.Id, Bounds = t.Doc.Bounds, Matrix = t.Doc.Matrix, Runs = runs });
        foreach (var (path, alpha) in Alpha)
            if (Find(root, path) is { } faded)
            {
                faded.ColorTransform = faded.ColorTransform with { AM = alpha, AA = 0 };
                faded.TransformedByScript = true;
            }
        foreach (var (path, (dx, dy, scale)) in Move)
            if (Find(root, path) is { } moved)
            {
                moved.Matrix = moved.Matrix.PreConcat(SKMatrix.CreateScale(scale, scale)).PostConcat(SKMatrix.CreateTranslation(dx, dy));
                // Kept there as its layer's timeline plays on (as when a script moves an object).
                moved.TransformedByScript = true;
            }
        // Re-layered last (paths name objects where the scene has them). Detached from the timeline (PlaceFrame 0), so
        // the layer's frames leave it where it now is.
        foreach (var (path, depth) in Depth.Select(kv => (kv.Key, kv.Value)).ToList())
            if (Find(root, path) is { Parent: MovieClip parent } obj && !parent.Children.ContainsKey(depth))
            {
                // Moved in the list directly: removing it (RemoveChild) would end it, stopping its animation.
                parent.Children.Remove(obj.Depth);
                obj.PlaceFrame = 0;
                obj.Depth = depth;
                parent.Children[depth] = obj;
            }
        SharpObjects = Sharp.Select(p => Find(root, p)).OfType<DisplayObject>().ToList();
        SharpAtObjects = SharpAt.Select(kv => (Find(root, kv.Key), kv.Value)).Where(t => t.Item1 != null).Select(t => (t.Item1!, t.Value)).ToList();
        SofterObjects = Softer.Select(kv => (Find(root, kv.Key), kv.Value)).Where(t => t.Item1 != null).Select(t => (t.Item1!, t.Value)).ToList();
        return root;
    }
}
