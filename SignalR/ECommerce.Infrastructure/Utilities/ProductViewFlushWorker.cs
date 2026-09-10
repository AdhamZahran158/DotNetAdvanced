using ECommerce.Application.Products.Commands.FlushProductViews;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Utilities
{
    public class ProductViewFlushWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ProductViewFlushWorker> _logger;

        public ProductViewFlushWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<ProductViewFlushWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            using var timer =
                new PeriodicTimer(TimeSpan.FromMinutes(10));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope =
                        _scopeFactory.CreateScope();

                    var mediator =
                        scope.ServiceProvider.GetRequiredService<IMediator>();

                    await mediator.Send(
                        new FlushProductViewsCommand(),
                        stoppingToken);
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
                        "Error while flushing product view counters.");
                }
            }
        }
    }
}
