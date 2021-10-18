using System.Collections.Generic;
using System.Threading.Tasks;
using LWS_NodeManager.Model;
using MongoDB.Driver;

namespace LWS_NodeManager.Repository
{
    public class NodeRepository
    {
        private readonly IMongoCollection<NodeInformation> _nodeCollection;

        public NodeRepository(MongoContext mongoContext)
        {
            _nodeCollection = mongoContext.MongoDatabase.GetCollection<NodeInformation>(nameof(NodeInformation));
        }

        public async Task AddNodeInfoAsync(NodeInformation nodeInformation)
        {
            await _nodeCollection.InsertOneAsync(nodeInformation);
        }
    }
}