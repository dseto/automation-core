using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Automation.Core.Recorder;

public sealed class SessionRecorder
{
    private readonly object _sync = new();
    private readonly Stopwatch _clock = new();
    private RecorderSession? _session;

    // Configurable threshold (seconds). Default kept backwards-compatible.
    private readonly double _recordWaitLogThresholdSeconds;

    // Last event timestamp (ms elapsed from session start) — used to compute gaps deterministically.
    private long _lastEventMs = -1;

    public SessionRecorder(double recordWaitLogThresholdSeconds = 1.0)
    {
        _recordWaitLogThresholdSeconds = recordWaitLogThresholdSeconds;
    }

    public void Start()
    {
        lock (_sync)
        {
            if (_session != null) return;

            _session = new RecorderSession
            {
                SessionId = Guid.NewGuid().ToString("n"),
                StartedAt = DateTimeOffset.Now,
                Events = new List<RecorderEvent>()
            };

            _clock.Restart();
            _lastEventMs = -1;
        }
    }

    public void Stop()
    {
        lock (_sync)
        {
            if (_session == null) return;

            _session.EndedAt = DateTimeOffset.Now;
            _clock.Stop();
        }
    }

    public RecorderSession GetSession()
    {
        lock (_sync)
        {
            if (_session == null)
                throw new InvalidOperationException("Recorder session not started.");

            return _session;
        }
    }

    public void RecordNavigate(string route)
    {
        AddEvent(new RecorderEvent
        {
            T = Now(),
            Type = TypeValue(RecorderEventType.Navigate),
            Route = route
        });
    }

    public void RecordClick(string targetHint)
    {
        AddEvent(new RecorderEvent
        {
            T = Now(),
            Type = TypeValue(RecorderEventType.Click),
            Target = TargetHint(targetHint)
        });
    }

    public void RecordFill(string targetHint, string value)
    {
        AddEvent(new RecorderEvent
        {
            T = Now(),
            Type = TypeValue(RecorderEventType.Fill),
            Target = TargetHint(targetHint),
            Value = new Dictionary<string, object?> { ["literal"] = value }
        }, consolidateKey: targetHint);
    }

    public void RecordSelect(string targetHint, string value)
    {
        AddEvent(new RecorderEvent
        {
            T = Now(),
            Type = TypeValue(RecorderEventType.Select),
            Target = TargetHint(targetHint),
            Value = new Dictionary<string, object?> { ["literal"] = value }
        });
    }

    public void RecordToggle(string targetHint)
    {
        AddEvent(new RecorderEvent
        {
            T = Now(),
            Type = TypeValue(RecorderEventType.Toggle),
            Target = TargetHint(targetHint)
        });
    }

    public void RecordSubmit(string targetHint)
    {
        AddEvent(new RecorderEvent
        {
            T = Now(),
            Type = TypeValue(RecorderEventType.Submit),
            Target = TargetHint(targetHint)
        });
    }

    public void RecordModalOpen(string targetHint)
    {
        AddEvent(new RecorderEvent
        {
            T = Now(),
            Type = TypeValue(RecorderEventType.ModalOpen),
            Target = TargetHint(targetHint)
        });
    }

    public void RecordModalClose(string targetHint)
    {
        AddEvent(new RecorderEvent
        {
            T = Now(),
            Type = TypeValue(RecorderEventType.ModalClose),
            Target = TargetHint(targetHint)
        });
    }

    private void AddEvent(RecorderEvent ev, string? consolidateKey = null)
    {
        lock (_sync)
        {
            if (_session == null) return;

            var currentMs = _clock.ElapsedMilliseconds;

            // Consolidation: replace last fill for same target (preserve prior behavior),
            // but update the _lastEventMs to the replacement time.
            if (consolidateKey != null && _session.Events.Count > 0)
            {
                var last = _session.Events[^1];
                if (last.Type == TypeValue(RecorderEventType.Fill) && TargetHintKey(last) == consolidateKey)
                {
                    _session.Events[^1] = ev;
                    _lastEventMs = currentMs;
                    return;
                }
            }

            // If there is a previous event, compute the idle gap and emit waitMs when above threshold.
            if (_session.Events.Count > 0 && _lastEventMs >= 0)
            {
                var gapMs = (int)(currentMs - _lastEventMs);
                var thresholdMs = (int)(_recordWaitLogThresholdSeconds * 1000.0);
                if (gapMs > thresholdMs)
                {
                    // copy ev and include waitMs (RecorderEvent is init-only; create new instance)
                    var evWithWait = new RecorderEvent
                    {
                        T = ev.T,
                        Type = ev.Type,
                        Route = ev.Route,
                        Target = ev.Target,
                        Value = ev.Value,
                        RawAction = ev.RawAction,
                        WaitMs = gapMs
                    };

                    _session.Events.Add(evWithWait);
                    _lastEventMs = currentMs;
                    return;
                }
            }

            // Default: add event as-is and update last timestamp
            _session.Events.Add(ev);
            _lastEventMs = currentMs;
        }
    }

    private string Now() => FormatElapsed(_clock.Elapsed);

    private static string FormatElapsed(TimeSpan ts)
    {
        var minutes = (int)ts.TotalMinutes;
        return $"{minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
    }

    private static Dictionary<string, object?> TargetHint(string hint)
    {
        var dict = new Dictionary<string, object?> { ["hint"] = hint };
        // If hint contains a data-testid like [data-testid='xxx'] or [data-testid="xxx"], populate attributes.data-testid for downstream tools
        try
        {
            var m = System.Text.RegularExpressions.Regex.Match(hint ?? string.Empty, "data-testid\\s*=\\s*[\'\"](?<id>[^\'\"]+)[\'\"]", System.Text.RegularExpressions.RegexOptions.Compiled);
            if (m.Success)
            {
                var attrs = new Dictionary<string, object?> { ["data-testid"] = m.Groups["id"].Value };
                dict["attributes"] = attrs;
            }
        }
        catch
        {
            // swallow regex errors and fall back to hint-only behavior
        }

        return dict;
    }

    private static string TypeValue(RecorderEventType type) => type switch
    {
        RecorderEventType.Navigate => "navigate",
        RecorderEventType.Click => "click",
        RecorderEventType.Fill => "fill",
        RecorderEventType.Select => "select",
        RecorderEventType.Toggle => "toggle",
        RecorderEventType.Submit => "submit",
        RecorderEventType.ModalOpen => "modal_open",
        RecorderEventType.ModalClose => "modal_close",
        _ => "unknown"
    };

    private static string? TargetHintKey(RecorderEvent ev)
    {
        if (ev.Target is Dictionary<string, object?> dict && dict.TryGetValue("hint", out var value))
            return value?.ToString();

        return null;
    }
}
