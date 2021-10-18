using System.Threading.Tasks;
using Grpc.Net.Client;
using LWS_NodeManager.DTO;
using LWS_NodeManager.Model;
using LWS_NodeManager.Repository;
using LWS_Shared;
using Newtonsoft.Json;

namespace LWS_NodeManager.Service
{
    public class NodeConfiguration
    {
        public string NodeKey { get; set; }
        public string NodeNickName { get; set; }
        public int NodeMaximumCpu { get; set; }
        public int NodeMaximumRam { get; set; }
    }
    
    public class NodeManageService
    {
        private readonly NodeRepository _nodeRepository;

        public NodeManageService(NodeRepository nodeRepository)
        {
            _nodeRepository = nodeRepository;
        }
        
        public async Task<Result> RegisterNewNodeInternal(NewNodeRequest nodeRequest)
        {
            // Get Node Information
            // Register Node to DB(with identifier/Nickname)
            var channel = GrpcChannel.ForAddress(nodeRequest.NodeServerUrl);
            var client = new NodeManagerService.NodeManagerServiceClient(channel);
            
            // Get
            var newNodeRequest = new RegisterNodeRequest
            {
                Code = nodeRequest.SecretKey
            };
            
            // Request information
            var result = await client.GetNodeInformationAsync(newNodeRequest);
            
            // If not succeed, return result
            if (result.ResultCode != ResultCode.Success) return result;

            var nodeMetadata = JsonConvert.DeserializeObject<NodeConfiguration>(result.Content);
            
            // When metadata convert fails
            if (nodeMetadata == null)
            {
                return new Result
                {
                    ResultCode = ResultCode.Unknown,
                    Message = $"Cannot deserialize object configuration! {result.Content}"
                };
            }
            
            // Add to database
            await _nodeRepository.AddNodeInfoAsync(new NodeInformation
            {
                NodeMaximumCpu = nodeMetadata.NodeMaximumCpu,
                NodeUrl = nodeRequest.NodeServerUrl,
                NodeMaximumRam = nodeMetadata.NodeMaximumRam,
                NodeNickName = nodeMetadata.NodeNickName,
                NodeAllocatedCpu = 0,
                NodeAllocatedRam = 0,
                NodeCpuUsage = 0.0,
                NodeRamUsage = 0.0
            });

            return new Result
            {
                ResultCode = ResultCode.Success
            };
        }
    }
}