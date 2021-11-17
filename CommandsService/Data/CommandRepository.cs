using System;
using System.Collections.Generic;
using System.Linq;
using CommandsService.Models;

namespace CommandsService.Data
{
    public class CommandRepository : ICommandRepository
    {
        private readonly AppDbContext _context;

        public CommandRepository(AppDbContext context)
        {
            _context = context;
        }

        public bool SaveChanges() =>
            _context.SaveChanges() >= 0;

        #region Platforms

        public IEnumerable<Platform> GetAllPlatforms() =>
            _context.Platforms.ToList();

        public void CreatePlatform(Platform platform)
        {
            if (platform == null)
            {
                throw new ArgumentNullException(nameof(platform));
            }

            _context.Platforms.Add(platform);
        }

        public bool PlatformExists(int platformId) =>
            _context.Platforms.Any(p => p.Id == platformId);

        public bool ExternalPlatformExists(int externalPlatformId) =>
            _context.Platforms.Any(p => p.ExternalId == externalPlatformId);

        #endregion

        #region Commands

        public IEnumerable<Command> GetCommandsForPlatform(int platformId) =>
            _context.Commands
                .Where(c => c.PlatformId == platformId)
                .OrderBy(c => c.Platform.Name)
                .ToList();

        public Command GetCommand(int platformId, int commandId) =>
            _context.Commands.FirstOrDefault(c => c.PlatformId == platformId && c.Id == commandId);

        public void CreateCommand(int platformId, Command command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            command.PlatformId = platformId;
            _context.Commands.Add(command);
        }
        
        #endregion
    }
}