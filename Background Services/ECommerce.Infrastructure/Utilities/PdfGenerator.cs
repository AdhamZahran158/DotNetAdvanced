using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Utilities;

public class PdfGenerator : IPdfGenerator
{
    public async Task GenerateAsync(
        Order order,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        // PDF Generation to-be added (we need pdf parser tool)

        await Task.CompletedTask;
    }
}