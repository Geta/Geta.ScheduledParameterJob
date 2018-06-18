using System.Collections.Generic;

namespace Geta.ScheduledParameterJob.Parameters
{
    [EPiServerDataStore(
        StoreName = "ScheduledJobParameters",
        AutomaticallyCreateStore = true,
        AutomaticallyRemapStore = true
    )]
    public class ScheduledJobParameters
    {
        public string PluginId { get; set; }
        public Dictionary<string, object> PersistedValues { get; set; }
    }
}
