using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Automation.Core.Recorder;

public sealed class SessionRecorder
{
    private readonly object _sync = new();
    private readonly Stopwatch _clock = new();
    private RecorderSession? _session;

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

            if (consolidateKey != null && _session.Events.Count > 0)
            {
                var last = _session.Events[^1];
                if (last.Type == TypeValue(RecorderEventType.Fill) && TargetHintKey(last) == consolidateKey)
                {
                    _session.Events[^1] = ev;
                    return;
                }
            }

            _session.Events.Add(ev);
        }
    }

    private string Now() => FormatElapsed(_clock.Elapsed);

    private static string FormatElapsed(TimeSpan ts)
    {
        var minutes = (int)ts.TotalMinutes;
        return $"{minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
    }

    private static Dictionary<string, object?> TargetHint(string hint) => new() { ["hint"] = hint };

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
