using GermanVerbTester.Data;
using GermanVerbTester.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GermanVerbTester.Services
{
    public class PrepositionCacheService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private List<Preposition>? _cachedPrepositions;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public PrepositionCacheService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<List<Preposition>> GetAllPrepositionsAsync()
        {
            if (_cachedPrepositions != null)
            {
                return _cachedPrepositions;
            }

            await _lock.WaitAsync();
            try
            {
                if (_cachedPrepositions == null)
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    _cachedPrepositions = await context.Prepositions.ToListAsync();
                }

                return _cachedPrepositions;
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
                _cachedPrepositions = await context.Prepositions.ToListAsync();
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}