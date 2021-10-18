using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using LWS_NodeManager.Model;
using LWS_NodeManager.Repository;
using LWS_Shared;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace LWS_NodeManager.Service
{
    public class NodeHealthCheckService: IHostedService
    {
        private readonly IMongoCollection<NodeInformation> _collection;
        private readonly ILogger _logger;   

        private Timer _timer;
        
        public NodeHealthCheckService(MongoContext mongoContext, ILogger<NodeHealthCheckService> logger)
        {
            _collection = mongoContext.MongoDatabase.GetCollection<NodeInformation>(nameof(NodeInformation));
            _logger = logger;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(Callback, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }

        private async void Callback(object? state)
        {
            // Get Node
            var list = await _collection.AsQueryable().ToListAsync();
            
            // Check Each node's health
            foreach (var eachNode in list)
            {
                _logger.LogInformation($"Heartbeat node {eachNode.NodeNickName}");
                var channel = GrpcChannel.ForAddress(eachNode.NodeUrl);
                var client = new NodeManagerService.NodeManagerServiceClient(channel);

                try
                {
                    // Heartbeat
                    var result = await client.NodeHeartbeatAsync(new HeartbeatRequest());

                    // Need to re-register.
                    if (result.ResultCode != ResultCode.Success)
                    {
                        _logger.LogCritical($"{eachNode.NodeNickName} is not responding! removing from node list.");

                        var filter = Builders<NodeInformation>.Filter.Eq(a => a.Id, eachNode.Id);
                        await _collection.DeleteOneAsync(filter);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogCritical($"Got exception while heart-beat: {e.Message}");

                    var filter = Builders<NodeInformation>.Filter.Eq(a => a.Id, eachNode.Id);
                    await _collection.DeleteOneAsync(filter);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
    }
}