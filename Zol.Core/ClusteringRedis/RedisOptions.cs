namespace Zol.Core.ClusteringRedis
{
    public class RedisOptions
    {
        public int Database { get; set; } = 0;
        public string ConnectionString { get; set; } = "localhost:6379";
    }
}