using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.IRepositories;
using MediatR;

namespace ECommerce.Application.Orders.Commands.GenerateOrderPdf;

public class GenerateOrderPdfCommandHandler
    : IRequestHandler<GenerateOrderPdfCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPdfGenerator _pdfGenerator;

    public GenerateOrderPdfCommandHandler(
        IOrderRepository orderRepository,
        IPdfGenerator pdfGenerator)
    {
        _orderRepository = orderRepository;
        _pdfGenerator = pdfGenerator;
    }

    public async Task Handle(
        GenerateOrderPdfCommand request,
        CancellationToken cancellationToken)
    {
        var order =
            await _orderRepository
                .GetWithDetailsAsync(request.OrderId);

        if (order is null)
        {
            throw new KeyNotFoundException(
                $"Order {request.OrderId} was not found.");
        }

        await _pdfGenerator.GenerateAsync(
            order,
            request.OutputPath,
            cancellationToken);
    }
}