using System;
using System.Collections.Generic;
using System.Linq;
using PlatformService.Models;

namespace PlatformService.Data
{
    public class PlatformRepository : IPlatformRepository
    {
        private readonly AppDbContext _context;

        public PlatformRepository(AppDbContext context)
        {
            _context = context;
        }

        public bool SaveChanges() => _context.SaveChanges() >= 0;

        public IEnumerable<Platform> GetAll() => _context.Platforms.ToList();

        public Platform GetById(int id) => _context.Platforms.Find(id);

        public void Create(Platform platform)
        {
            if (platform is null)
            {
                throw new ArgumentNullException(nameof(platform));
            }
            
            _context.Platforms.Add(platform);
        }
    }
}