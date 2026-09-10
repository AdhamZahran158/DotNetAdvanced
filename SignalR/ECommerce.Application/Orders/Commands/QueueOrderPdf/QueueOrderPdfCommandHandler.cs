using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using MediatR;

namespace ECommerce.Application.Orders.Commands.QueueOrderPdf;

public class QueueOrderPdfCommandHandler
    : IRequestHandler<QueueOrderPdfCommand>
{
    private readonly IPdfJobQueue _pdfJobQueue;

    public QueueOrderPdfCommandHandler(
        IPdfJobQueue pdfJobQueue)
    {
        _pdfJobQueue = pdfJobQueue;
    }

    public async Task Handle(
        QueueOrderPdfCommand request,
        CancellationToken cancellationToken)
    {
        await _pdfJobQueue.EnqueueAsync(
            new GeneratePdfJob(
                request.OrderId,
                request.OutputPath),
            cancellationToken);
    }
}