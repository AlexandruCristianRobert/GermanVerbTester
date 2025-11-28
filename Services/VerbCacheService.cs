using GermanVerbTester.Data;
using GermanVerbTester.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GermanVerbTester.Services
{
    public class VerbCacheService
    {
        private readonly AppDbContext _context;
        private List<Verb>? _cachedVerbs;
        private readonly object _lock = new object();

        public VerbCacheService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Verb>> GetAllVerbsAsync()
        {
            if (_cachedVerbs == null)
            {
                lock (_lock)
                {
                    if (_cachedVerbs == null)
                    {
                        _cachedVerbs = _context.Verbs.ToList();
                    }
                }
            }

            return await Task.FromResult(_cachedVerbs);
        }

        public async Task RefreshCacheAsync()
        {
            var verbs = await _context.Verbs.ToListAsync();
            lock (_lock)
            {
                _cachedVerbs = verbs;
            }
        }
    }
}