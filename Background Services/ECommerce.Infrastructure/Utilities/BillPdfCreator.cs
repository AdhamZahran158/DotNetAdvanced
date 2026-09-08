using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Orders.Commands.GenerateOrderPdf;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Utilities;

public class BillPdfCreator : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IPdfJobQueue _queue;
    private readonly ILogger<BillPdfCreator> _logger;

    public BillPdfCreator(
        IServiceScopeFactory scopeFactory,
        IPdfJobQueue queue,
        ILogger<BillPdfCreator> logger)
    {
        _scopeFactory = scopeFactory;
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            GeneratePdfJob job;

            try
            {
                job = await _queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();

                var mediator =
                    scope.ServiceProvider
                        .GetRequiredService<IMediator>();

                await mediator.Send(
                    new GenerateOrderPdfCommand(
                        job.OrderId,
                        job.OutputPath),
                    stoppingToken);

                _logger.LogInformation(
                    "PDF generated successfully for Order {OrderId}",
                    job.OrderId);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to generate PDF for Order {OrderId}",
                    job.OrderId);
            }
        }
    }
}