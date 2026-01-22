using System.Threading;
using Automation.Core.Recorder;
using Xunit;

namespace Automation.Core.Tests;

public class SessionRecorderTests
{
    [Fact]
    public void Recorder_EmitsWaitMs_OnlyForGapsAboveThreshold()
    {
        var recorder = new SessionRecorder(recordWaitLogThresholdSeconds: 1.0);
        recorder.Start();

        recorder.RecordNavigate("/login");
        Thread.Sleep(900); // 900ms gap - below threshold
        recorder.RecordFill("username", "admin");

        Thread.Sleep(2100); // 2100ms gap - above threshold
        recorder.RecordFill("password", "secret");

        var session = recorder.GetSession();
        Assert.Equal(3, session.Events.Count);

        // Second event (first fill) should not have waitMs
        Assert.Null(session.Events[1].WaitMs);

        // Third event should have waitMs >= 2100
        Assert.NotNull(session.Events[2].WaitMs);
        Assert.True(session.Events[2].WaitMs >= 2100);
    }

    [Fact]
    public void Recorder_DoesNotEmitWaitMs_WhenFillIsConsolidated()
    {
        var recorder = new SessionRecorder(recordWaitLogThresholdSeconds: 1.0);
        recorder.Start();

        recorder.RecordFill("username", "one");
        Thread.Sleep(1100); // > threshold
        // second fill with same target should consolidate previous fill instead of emitting waitMs
        recorder.RecordFill("username", "two");

        var session = recorder.GetSession();
        // Only one fill event (consolidated) and no waitMs should be present
        Assert.Single(session.Events);
        Assert.Null(session.Events[0].WaitMs);
    }
}