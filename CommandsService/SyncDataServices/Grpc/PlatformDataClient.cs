using System;
using System.Collections.Generic;
using AutoMapper;
using CommandsService.Models;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlatformService;

namespace CommandsService.SyncDataServices.Grpc
{
    public class PlatformDataClient : IPlatformDataClient
    {
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger<PlatformDataClient> _logger;

        public PlatformDataClient(IConfiguration configuration, IMapper mapper, ILogger<PlatformDataClient> logger)
        {
            _configuration = configuration;
            _mapper = mapper;
            _logger = logger;
        }

        public IEnumerable<Platform> ReturnAllPlatforms()
        {
            _logger.LogInformation("--> Calling GRPC service: {Service}", _configuration["GrpcPlatform"]);
            var channel = GrpcChannel.ForAddress(_configuration["GrpcPlatform"]);
            var client = new GrpcPlatform.GrpcPlatformClient(channel);
            var request = new GetAllRequest();

            try
            {
                var platformResponse = client.GetAllPlatforms(request);
                return _mapper.Map<IEnumerable<Platform>>(platformResponse.Platform);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "--> Could not call GRPC Server");
                return null;
            }
        }
    }
}