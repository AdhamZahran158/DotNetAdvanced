using MediatR;

namespace ECommerce.Application.Orders.Commands.QueueOrderPdf;

public record QueueOrderPdfCommand(int OrderId, string OutputPath) : IRequest;