using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using PlatformService.Data;

namespace PlatformService.SyncDataServices.Grpc
{
    public class GrpcPlatformService : GrpcPlatform.GrpcPlatformBase
    {
        private readonly IPlatformRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GrpcPlatformService> _logger;

        public GrpcPlatformService(IPlatformRepository repository, IMapper mapper, ILogger<GrpcPlatformService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public override Task<PlatformResponse> GetAllPlatforms(GetAllRequest request, ServerCallContext context)
        {
            var grpcPlatformModels = _repository.GetAll().Select(platform => _mapper.Map<GrpcPlatformModel>(platform));
            
            var platformResponse = new PlatformResponse();
            platformResponse.Platform.AddRange(grpcPlatformModels);

            return Task.FromResult(platformResponse);
        }
    }
}