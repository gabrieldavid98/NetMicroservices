using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlatformService.Dtos;

namespace PlatformService.SyncDataServices.Http
{
    public class HttpCommandDataClient : ICommandDataClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpCommandDataClient> _logger;
        private readonly IConfiguration _configuration;

        public HttpCommandDataClient(
            HttpClient httpClient,
            ILogger<HttpCommandDataClient> logger,
            IConfiguration configuration
        )
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendPlatformToCommand(PlatformReadDto platformReadDto)
        {
            var stringContent = new StringContent(
                content: JsonSerializer.Serialize(platformReadDto),
                encoding: Encoding.UTF8,
                mediaType: MediaTypeNames.Application.Json
            );

            var response = await _httpClient.PostAsync(
                $"{_configuration["CommandService"]}/api/c/platforms",
                stringContent
            );

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("->> Sync POST to CommandService was OK!");
                return;
            }
            
            _logger.LogInformation("->> Sync POST to CommandService was NOT OK!");
        }
    }
}