namespace ECommerce.Application.Interfaces;

public interface IProductViewCounter
{
    Task IncrementAsync(
        int productId,
        CancellationToken cancellationToken = default);

    Task<Dictionary<int, long>> GetAndResetAllAsync(
        CancellationToken cancellationToken = default);
}