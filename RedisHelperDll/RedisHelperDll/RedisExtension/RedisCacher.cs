using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using RedisHelperDll.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisHelperDll.RedisExtension
{
  //This class is used for simple redis caching , it saves the data as key value pairs
  //Just by running redis you can use this class
  public static class RedisCacher
  {
    /// <summary>
    /// Save a record in redis db
    /// </summary>
    /// <param name="cache">IDistributedCache</param>
    /// <param name="key">A key for our data</param>
    /// <param name="data">Our data to cache</param>
    /// <param name="expirationDuration">Cache would automaticly be expired after this time</param>
    /// <param name="unusedExpirationDuration">Cache would be expired if it is not used during this time</param>
 
    public async static Task SetRecordAsync(this IDistributedCache cache,
      string key , object data,
      TimeSpan? expirationDuration = null,
      TimeSpan? unusedExpirationDuration = null)
    {
      DistributedCacheEntryOptions options = GenerateCacheOptions(expirationDuration,unusedExpirationDuration).Result;
      cache.SetString(key, data.Serialize() ,options);
    }

    /// <summary>
    /// Save a List of records in redis db
    /// Save each records of dictionary as a key,value pairs in redis
    /// </summary>
    /// <param name="cache">IDistributedCache</param>
    /// <param name="records">A Dictionary type which contains keies and data to cache</param>
    /// <param name="expirationDuration">Cache would automaticly be expired after this time</param>
    /// <param name="unusedExpirationDuration">Cache would be expired if it is not used during this time</param>
    public async static Task SetRecordsListAsync(this IDistributedCache cache,
      Dictionary<string, object> records,
      TimeSpan? expirationDuration = null,
      TimeSpan? unusedExpirationDuration = null)
    {
      foreach(var record in records)
      {
         await cache.SetRecordAsync(record.Key, record.Value, expirationDuration, unusedExpirationDuration);
      }
    }

    /// <summary>
    /// Get the value of the key parameter
    /// </summary>
    /// <typeparam name="T">Type of data we want to retrive from db </typeparam>
    /// <param name="cache">IDistributedCache</param>
    /// <param name="key">The key of record in db</param>
    /// <returns></returns>
    public async static Task<T> GetRecordAsync<T>(this IDistributedCache cache,
      string key)
    => cache.GetStringAsync(key).Result.Deserialize<T>();

    /// <summary>
    /// Get a List of Records in db which match the keis 
    /// </summary>
    /// <typeparam name="T">Type of data we want retrive from db</typeparam>
    /// <param name="cache">IDistributedCache</param>
    /// <param name="keies">List of kies we want their data</param>
    /// <returns>A Dictionary Type of kies and their value</returns>
    public async static Task<Dictionary<string, T>> GetRecordsListAsync<T>(this IDistributedCache cache,List<string> keies)
    {
      Dictionary<string, T> result = new ();
      keies.ForEach(async key => result.Add(key,await cache.GetRecordAsync<T>(key)));
      return result;
    }

    public async static Task Update(this IDistributedCache cache,
      string key, object data,
      TimeSpan? expirationDuration = null,
      TimeSpan? unusedExpirationDuration = null)
    {
      var record = cache.GetAsync(key);
      if(record != null)
      {
        await cache.RemoveAsync(key);
      }
      await cache.SetRecordAsync(key, data, expirationDuration, unusedExpirationDuration);
    }

    private static async Task<DistributedCacheEntryOptions> GenerateCacheOptions(TimeSpan? expirationDuration , TimeSpan? unusedExpirationDuration)
    {
      DistributedCacheEntryOptions options = new();
      options.AbsoluteExpirationRelativeToNow = expirationDuration == null ? TimeSpan.FromSeconds(60) : expirationDuration;
      options.SlidingExpiration = unusedExpirationDuration;
      return options;
    }
  }
}
