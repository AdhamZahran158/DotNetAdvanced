using ECommerce.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces
{
    public interface IPdfJobQueue
    {
        ValueTask EnqueueAsync(
            GeneratePdfJob job,
            CancellationToken cancellationToken = default);

        ValueTask<GeneratePdfJob> DequeueAsync(
            CancellationToken cancellationToken = default);
    }
}
