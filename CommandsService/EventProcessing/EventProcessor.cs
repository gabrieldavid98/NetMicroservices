using System;
using System.Text.Json;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CommandsService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMapper _mapper;
        private readonly ILogger<EventProcessor> _logger;

        public EventProcessor(IServiceScopeFactory scopeFactory, IMapper mapper, ILogger<EventProcessor> logger)
        {
            _scopeFactory = scopeFactory;
            _mapper = mapper;
            _logger = logger;
        }

        public void ProcessEvent(string message)
        {
            var eventType = DetermineEventType(message);

            switch (eventType)
            {
                case EventType.PlatformPublished:
                    AddPlatform(message);
                    break;
                default:
                    break;
            }
        }

        private EventType DetermineEventType(string message)
        {
            var genericEventDto = JsonSerializer.Deserialize<GenericEventDto>(message);
            switch (genericEventDto?.Event)
            {
                case "PlatformPublished":
                    _logger.LogInformation("--> Platform Published Event Detected");
                    return EventType.PlatformPublished;
                default:
                    _logger.LogInformation("--> Could not determine the event type");
                    return EventType.Undetermined;
            }
        }

        private void AddPlatform(string platformPublishedMessage)
        {
            using var scope = _scopeFactory.CreateScope();
            var commandRepository = scope.ServiceProvider.GetRequiredService<ICommandRepository>();
            var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMessage);

            try
            {
                var platform = _mapper.Map<Platform>(platformPublishedDto);

                if (commandRepository.ExternalPlatformExists(platform.ExternalId))
                {
                    _logger.LogWarning("--> Platform already exists");
                    return;
                }
                
                commandRepository.CreatePlatform(platform);
                commandRepository.SaveChanges();
                _logger.LogInformation("--> Platform added");
            }
            catch (Exception ex)
            {
                _logger.LogError("--> Could not add Platform to DB: {Message}", ex.Message);
            }
        }
    }

    enum EventType
    {
        PlatformPublished,
        Undetermined
    }
}