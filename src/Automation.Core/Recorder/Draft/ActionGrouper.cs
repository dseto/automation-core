using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Json;
using Automation.Core.Recorder;

namespace Automation.Core.Recorder.Draft;

public sealed class ActionGrouper
{
    public IReadOnlyList<DraftAction> Group(RecorderSession session)
    {
        var actions = new List<DraftAction>();
        var groups = new List<GroupBuffer>();

        for (var i = 0; i < session.Events.Count; i++)
        {
            var ev = session.Events[i];
            var time = ParseTimestamp(ev.T);
            var rawScript = TryGetRawScript(ev.RawAction);

            if (groups.Count == 0 || !CanMerge(groups[^1], ev, time))
            {
                groups.Add(new GroupBuffer(ev, i, time, rawScript));
                continue;
            }

            groups[^1].Add(ev, i, time, rawScript);
        }

        foreach (var group in groups)
        {
            actions.Add(group.ToDraftAction());
        }

        return actions;
    }

    private static string? TryGetRawScript(object? rawAction)
    {
        if (rawAction is Dictionary<string, object?> dict && dict.TryGetValue("script", out var script))
            return script?.ToString();

        if (rawAction is JsonElement json && json.ValueKind == JsonValueKind.Object)
        {
            if (json.TryGetProperty("script", out var scriptProp))
                return scriptProp.GetString();
        }

        return null;
    }

    private static bool CanMerge(GroupBuffer group, RecorderEvent next, TimeSpan nextTime)
    {
        var last = group.LastEvent;

        var delta = nextTime - group.LastTime;
        if (delta > TimeSpan.FromMilliseconds(2000))
            return false;

        var lastIsNavigate = last.Type == "navigate";
        var nextIsNavigate = next.Type == "navigate";

        if (lastIsNavigate || nextIsNavigate)
            return lastIsNavigate && nextIsNavigate;

        if (!IsSemanticType(next.Type) || !IsSemanticType(last.Type))
            return false;

        var lastHint = NormalizeHint(GetHint(last));
        var nextHint = NormalizeHint(GetHint(next));

        if (!string.IsNullOrWhiteSpace(lastHint) && lastHint == nextHint)
            return true;

        if (last.Type == "click" && next.Type == "fill" && lastHint == nextHint)
            return true;

        if (last.Type == "click" && next.Type == "submit")
            return true;

        if (last.Type == "click" && IsGenericHint(lastHint) && !IsGenericHint(nextHint))
            return true;

        return false;
    }

    private static bool IsSemanticType(string type)
    {
        return type is "click" or "fill" or "submit" or "select" or "toggle";
    }

    private static string? GetHint(RecorderEvent ev)
    {
        if (ev.Target is Dictionary<string, object?> dict && dict.TryGetValue("hint", out var hint))
            return hint?.ToString();

        if (ev.Target is Dictionary<string, object> obj && obj.TryGetValue("hint", out var hintObj))
            return hintObj?.ToString();

        if (ev.Target is JsonElement json && json.ValueKind == JsonValueKind.Object)
        {
            if (json.TryGetProperty("hint", out var hintProp))
                return hintProp.GetString();
        }

        return null;
    }

    private static string NormalizeHint(string? hint) => HintHelpers.NormalizeHint(hint);


    private static bool IsGenericHint(string hint) => HintHelpers.IsGenericHint(hint);

    



    private static bool IsDataTestIdHint(string hint)
    {
        return hint.Contains("[data-testid='");
    }

    private static bool IsSpecificHint(string hint)
    {
        return hint.Contains("[data-testid=")
               || hint.Contains("#")
               || hint.Contains("[name=")
               || hint.Contains("[formcontrolname=")
               || hint.Contains("[aria-label=")
               || hint.Contains("[placeholder=")
               || hint.Contains("('");
    }

    private static int ScoreEvent(RecorderEvent ev)
    {
        var hint = NormalizeHint(GetHint(ev));

        return ev.Type switch
        {
            "fill" => 400,
            "click" when IsDataTestIdHint(hint) => 300,
            "click" when IsSpecificHint(hint) => 200,
            "submit" => 150,
            "navigate" => 100,
            "click" => 50,
            _ => 0
        };
    }

    private static TimeSpan ParseTimestamp(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return TimeSpan.Zero;

        if (TimeSpan.TryParseExact(value, "mm\\:ss\\.fff", CultureInfo.InvariantCulture, out var ts))
            return ts;

        if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var fallback))
            return fallback;

        return TimeSpan.Zero;
    }

    private sealed class GroupBuffer
    {
        private readonly List<GroupEntry> _entries = new();

        public GroupBuffer(RecorderEvent ev, int index, TimeSpan time, string? rawScript)
        {
            Add(ev, index, time, rawScript);
        }

        public RecorderEvent LastEvent => _entries[^1].Event;

        public TimeSpan LastTime => _entries[^1].Time;

        public void Add(RecorderEvent ev, int index, TimeSpan time, string? rawScript)
        {
            _entries.Add(new GroupEntry(ev, index, time, rawScript));
        }

        public DraftAction ToDraftAction()
        {
            var ordered = _entries
                .OrderByDescending(e => ScoreEvent(e.Event))
                .ThenBy(e => e.Index)
                .ToList();

            var events = ordered.Select(e => e.Event).ToList();
            // Ensure indexes align with the ordered events (PrimaryEvent should correspond to indexes[0])
            var indexes = ordered.Select(e => e.Index).ToList();
            var rawScript = ordered.Select(e => e.RawScript).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));

            return new DraftAction(events, indexes, rawScript);
        }
    }

    private readonly record struct GroupEntry(RecorderEvent Event, int Index, TimeSpan Time, string? RawScript);
}
