using Newtonsoft.Json;
using Orleans;

namespace Zol.Core.ClusteringRedis
{
    internal class VersionedEntry
    {
        public MembershipEntry Entry { get; set; }
        public TableVersion TableVersion { get; set; }
        public string ResourceVersion { get; set; }

        public VersionedEntry(MembershipEntry entry, TableVersion tableVersion)
        {
            Entry = entry;
            TableVersion = tableVersion;
            ResourceVersion = tableVersion.VersionEtag;
        }

        public VersionedEntry()
        {

        }
    }
}