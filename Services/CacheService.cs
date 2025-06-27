using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;

namespace HospitalAssetTracker.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheService> _logger;
        private readonly HashSet<string> _cacheKeys;

        public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _cacheKeys = new HashSet<string>();
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                if (_memoryCache.TryGetValue(key, out var value) && value is T result)
                {
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                    return result;
                }

                _logger.LogDebug("Cache miss for key: {Key}", key);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                if (value == null)
                {
                    await RemoveAsync(key);
                    return;
                }

                var options = new MemoryCacheEntryOptions();
                
                if (expiration.HasValue)
                {
                    options.SetAbsoluteExpiration(expiration.Value);
                }
                else
                {
                    // Default expiration of 30 minutes
                    options.SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
                }

                // Set sliding expiration to keep frequently accessed items longer
                options.SetSlidingExpiration(TimeSpan.FromMinutes(5));

                // Register removal callback to track cache keys
                options.RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                    lock (_cacheKeys)
                    {
                        _cacheKeys.Remove(key.ToString() ?? string.Empty);
                    }
                });

                _memoryCache.Set(key, value, options);
                
                lock (_cacheKeys)
                {
                    _cacheKeys.Add(key);
                }

                _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", key, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                _memoryCache.Remove(key);
                
                lock (_cacheKeys)
                {
                    _cacheKeys.Remove(key);
                }

                _logger.LogDebug("Removed cache entry for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing value from cache for key: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                var regex = new Regex(pattern.Replace("*", ".*"), RegexOptions.IgnoreCase | RegexOptions.Compiled);
                var keysToRemove = new List<string>();

                lock (_cacheKeys)
                {
                    foreach (var key in _cacheKeys)
                    {
                        if (regex.IsMatch(key))
                        {
                            keysToRemove.Add(key);
                        }
                    }
                }

                foreach (var key in keysToRemove)
                {
                    await RemoveAsync(key);
                }

                _logger.LogDebug("Removed {Count} cache entries matching pattern: {Pattern}", keysToRemove.Count, pattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache entries by pattern: {Pattern}", pattern);
            }
        }

        public async Task ClearAsync()
        {
            try
            {
                var keysToRemove = new List<string>();

                lock (_cacheKeys)
                {
                    keysToRemove.AddRange(_cacheKeys);
                }

                foreach (var key in keysToRemove)
                {
                    _memoryCache.Remove(key);
                }

                lock (_cacheKeys)
                {
                    _cacheKeys.Clear();
                }

                _logger.LogInformation("Cleared all cache entries");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache");
            }
        }
    }
}
