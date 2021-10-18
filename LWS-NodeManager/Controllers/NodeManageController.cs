using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Net.Client;
using LWS_NodeManager.DTO;
using LWS_NodeManager.Service;
using LWS_Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LWS_NodeManager.Controllers
{
    [ApiController]
    [Route("/api/manage/node")]
    public class NodeManageController: SuperControllerBase
    {
        private readonly NodeManageService _nodeManageService;

        public NodeManageController(NodeManageService service)
        {
            _nodeManageService = service;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterNewNode(NewNodeRequest newNodeRequest)
        {
            var result = await _nodeManageService.RegisterNewNodeInternal(newNodeRequest);
            var handledCase = new Dictionary<ResultCode, Func<IActionResult>>
            {
                [ResultCode.Unknown] = () => new ObjectResult(result) {StatusCode = StatusCodes.Status500InternalServerError},
                [ResultCode.Forbidden] = () => new ObjectResult(result) {StatusCode = StatusCodes.Status403Forbidden},
                [ResultCode.Success] = Ok
            };

            return HandleCase(handledCase, result.ResultCode);
        }
    }
}