using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using System.Threading.Channels;

namespace ECommerce.Infrastructure.Utilities;

public class PdfJobQueue : IPdfJobQueue
{
    private readonly Channel<GeneratePdfJob> _queue;

    public PdfJobQueue()
    {
        _queue = Channel.CreateBounded<GeneratePdfJob>(
            new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            });
    }

    public ValueTask EnqueueAsync(
        GeneratePdfJob job,
        CancellationToken cancellationToken = default)
    {
        return _queue.Writer.WriteAsync(job, cancellationToken);
    }

    public ValueTask<GeneratePdfJob> DequeueAsync(
        CancellationToken cancellationToken = default)
    {
        return _queue.Reader.ReadAsync(cancellationToken);
    }
}