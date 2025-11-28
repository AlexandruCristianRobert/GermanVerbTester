using GermanVerbTester.Data;
using GermanVerbTester.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GermanVerbTester.Services
{
    public class VerbCacheService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private List<Verb>? _cachedVerbs;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public VerbCacheService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<List<Verb>> GetAllVerbsAsync()
        {
            if (_cachedVerbs != null)
            {
                return _cachedVerbs;
            }

            await _lock.WaitAsync();
            try
            {
                if (_cachedVerbs == null)
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    _cachedVerbs = await context.Verbs.ToListAsync();
                }

                return _cachedVerbs;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task RefreshCacheAsync()
        {
            await _lock.WaitAsync();
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                _cachedVerbs = await context.Verbs.ToListAsync();
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}