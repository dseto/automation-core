using Xunit;
using Automation.Core.Recorder;
using Automation.Core.Recorder.Draft;

namespace Automation.Core.Tests
{
    public class ActionGrouperTests
    {
        [Fact]
        public void Group_DoesNotMerge_DistinctNavigates()
        {
            var session = new RecorderSession();
            // Two distinct navigate events within short time window
            session.Events.Add(new RecorderEvent { T = "00:00.000", Type = "navigate", Route = "/" });
            session.Events.Add(new RecorderEvent { T = "00:00.500", Type = "navigate", Route = "/login.html" });

            var grouper = new ActionGrouper();

            // diagnostic: show event count and details
            System.Console.WriteLine($"DEBUG: events.count={session.Events.Count}");
            for (int i=0;i<session.Events.Count;i++) System.Console.WriteLine($"DEBUG: ev[{i}] type={session.Events[i].Type} route={session.Events[i].Route} t={session.Events[i].T}");

            var actions = grouper.Group(session);

            Assert.True(session.Events.Count == 2, $"events.count={session.Events.Count}; evList={string.Join(";", session.Events.Select(e => (e.Type, e.Route, e.T)))}");
            var summary = string.Join("|", System.Linq.Enumerable.Select(actions, a => string.Join(",", a.EventIndexes)));
            if (actions.Count != 2)
            {
                throw new System.Exception($"Unexpected actions count {actions.Count}. groups: {summary}");
            }
        }

        [Fact]
        public void Group_Merges_IdenticalNavigates()
        {
            var session = new RecorderSession();
            // Two identical navigate events within short time window
            session.Events.Add(new RecorderEvent { T = "00:00.000", Type = "navigate", Route = "/" });
            session.Events.Add(new RecorderEvent { T = "00:00.500", Type = "navigate", Route = "/" });

            var grouper = new ActionGrouper();
            var actions = grouper.Group(session);

            Assert.Single(actions);
            // merged action should include two events
            Assert.Equal(2, actions[0].EventIndexes.Count);
        }

        [Fact]
        public void Group_RealSession_SeguroSim_Preserves_Login_Navigate()
        {
            var path = "ui-tests/artifacts/seguro-sim/session.json";
            if (!System.IO.File.Exists(path)) { System.Console.WriteLine($"Skipping test: sample session not found: {path}"); return; }
            var content = System.IO.File.ReadAllText(path);
            var session = System.Text.Json.JsonSerializer.Deserialize<RecorderSession>(content) ?? new RecorderSession();

            var grouper = new ActionGrouper();
            var actions = grouper.Group(session);

            var navs = System.Linq.Enumerable.Where(actions, a => a.PrimaryEvent?.Type == "navigate");
            var navList = System.Linq.Enumerable.ToList(navs);
            var summary = string.Join("|", System.Linq.Enumerable.Select(navList, a => (a.PrimaryEvent?.Route ?? a.PrimaryEvent?.Url ?? "(null)", string.Join(",", a.EventIndexes))));
            Assert.True(navList.Count >= 2, $"Expected >=2 navigate actions; found {navList.Count}. summary: {summary}");
        }
    }
}
