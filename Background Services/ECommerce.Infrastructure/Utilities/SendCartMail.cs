using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Utilities
{
    public class SendCartMail : BackgroundService
    {
        IServiceScopeFactory serviceScopeFactory;

        public SendCartMail(IServiceScopeFactory serviceScopeFactory)
        {
            this.serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timer = new PeriodicTimer(TimeSpan.FromDays(1));
            while (stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                //get cart product times
                //if cart contains products about to expire
                using var scope = serviceScopeFactory.CreateScope();
                var mail = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                await mail.SendEmailAsync("","","<>");
            }
        }
    }
}
