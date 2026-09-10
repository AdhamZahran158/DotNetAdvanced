using ECommerce.Application.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Utilities.Redis
{
    public class ProductViewCounter : IProductViewCounter
    {
        private const string KeyPrefix = "product:views:";

        private readonly IConnectionMultiplexer _redis;

        public ProductViewCounter(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task IncrementAsync(
            int productId,
            CancellationToken cancellationToken = default)
        {
            var database = _redis.GetDatabase();

            var key = $"{KeyPrefix}{productId}";

            await database.StringIncrementAsync(key);
        }

        public async Task<Dictionary<int, long>> GetAndResetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var database = _redis.GetDatabase();

            var result = new Dictionary<int, long>();

            foreach (var endpoint in _redis.GetEndPoints())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var server = _redis.GetServer(endpoint);

                var keys = server.Keys(
                    pattern: $"{KeyPrefix}*");

                foreach (var key in keys)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var keyString = key.ToString();

                    var productIdString =
                        keyString[KeyPrefix.Length..];

                    if (!int.TryParse(productIdString, out var productId))
                        continue;


                    var value = await database.ExecuteAsync(
                        "GETDEL",
                        key);

                    if (value.IsNull)
                        continue;

                    if (!long.TryParse(value.ToString(), out var count))
                        continue;

                    if (count <= 0)
                        continue;

                    result[productId] = count;
                }
            }

            return result;
        }
    }
}
