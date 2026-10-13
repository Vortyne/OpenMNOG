using Mnog.Engine.Display;
using SkiaSharp;

namespace Mnog.Engine;

/// <summary>
/// Diagnostic panel drawn over the game's menu bar at the bottom of the stage (only there, not over the scene), so screenshots of
/// problems carry the context needed to investigate them.
/// </summary>
public sealed class DebugOverlay
{
    readonly Player _player;
    readonly Queue<double> _frameTimes = new();
    readonly Queue<double> _tickTimes = new();
    readonly SKFont _font = new(SKTypeface.FromFamilyName("Consolas") ?? SKTypeface.Default, 7f);
    readonly SKPaint _text = new() { Color = new SKColor(0xFF, 0xE0, 0x80), IsAntialias = true };
    readonly SKPaint _back = new() { Color = new SKColor(0, 0, 0, 0xD8) };

    public DebugOverlay(Player player) => _player = player;

    public void FrameDrawn() => Sample(_frameTimes);
    public void TickRan() => Sample(_tickTimes);

    void Sample(Queue<double> q)
    {
        double now = _player.ElapsedMs;
        q.Enqueue(now);
        while (q.Count > 0 && now - q.Peek() > 1000) q.Dequeue();
    }

    /// <summary>Draws the panel in stage coordinates (the canvas is already mapped to the stage).</summary>
    public void Draw(SKCanvas canvas, float left, float right)
    {
        var lines = Lines();
        // (Within the bottom bar, from where its brown edge starts (stage y 430, under the scene's last row and
        // the vector bar's open top): never over the scene. Its lines packed to fit.)
        const float barTop = 430;
        float lineHeight = Math.Min(9.2f, (_player.StageHeight - barTop - 2) / lines.Count);
        float top = barTop;
        canvas.DrawRect(new SKRect(left, top, right, _player.StageHeight), _back);
        float y = top + 1 + 6.5f;
        foreach (var line in lines)
        {
            canvas.DrawText(line, left + 4, y, _font, _text);
            y += lineHeight;
        }
    }

    List<string> Lines()
    {
        var p = _player;
        var level1 = p.GetLevel(1);
        var holder = level1?.ChildByName("holder") as MovieClip;
        string Movie(string clip) => (level1?.ChildByName(clip) as MovieClip)?.Movie.Name ?? "-";

        string scene = holder != null ? $"{holder.Movie.Name}  frame {holder.CurrentFrame}/{holder.FrameCount}" : "-";
        string player = level1 != null ? $"{level1.Movie.Name} frame {level1.CurrentFrame}" : "-";
        var mouse = p.MouseStage;
        string over = p.HoverDescription;

        var lines = new List<string>
        {
            $"Scene {scene}   |   {player}   |   tick {p.TickCount}   |   game {_tickTimes.Count} fps (target {p.FrameRate:0}), drawn {_frameTimes.Count} fps",
            $"Mouse {mouse.X:0},{mouse.Y:0}   |   {over}",
            $"Music {Movie("Music-holder")}   |   Background {Movie("Sound-Background")}   |   SFX {Movie("Sound-SFX")}   |   " +
            $"widescreen {(p.Widescreen ? "on" : "off")}, smoothing {(!p.Smooth ? "off" : p.SmoothAll ? "full" : "partial")}, window {p.WindowSize.W}x{p.WindowSize.H}",
        };
        var sounds = p.Audio.Describe();
        lines.Add($"Sounds {sounds.Count}: " + (sounds.Count > 0 ? string.Join(", ", sounds.Take(6)) + (sounds.Count > 6 ? ", ..." : "") : "none"));
        var recent = p.RecentLog;
        lines.Add(recent.Count > 0 ? "Last: " + recent[^1] : "Last: (no warnings)");
        return lines;
    }
}
