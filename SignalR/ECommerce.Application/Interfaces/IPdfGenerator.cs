using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface IPdfGenerator
{
    Task GenerateAsync(
        Order order,
        string outputPath,
        CancellationToken cancellationToken = default);
}