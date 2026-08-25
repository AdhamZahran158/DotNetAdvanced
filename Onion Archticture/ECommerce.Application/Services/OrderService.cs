using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.IRepositories;
using ECommerce.Application.Interfaces.IServices;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IRepository<Coupon> _couponRepository;
    private readonly IRepository<Payment> _paymentRepository;

    public OrderService(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IRepository<Coupon> couponRepository,
        IRepository<Payment> paymentRepository)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _couponRepository = couponRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _orderRepository.GetWithDetailsAsync(id);
    }

    public async Task<List<Order>> GetCustomerOrdersAsync(
        int customerId)
    {
        return await _orderRepository
            .GetByCustomerIdAsync(customerId);
    }

    public async Task CancelAsync(int id)
    {
        var order = await _orderRepository
            .GetWithDetailsAsync(id);

        if (order is null)
        {
            throw new KeyNotFoundException(
                "Order not found.");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Order is already cancelled.");
        }

        /*
         * If the order was paid, restore the inventory.
         */
        if (order.Status == OrderStatus.Paid)
        {
            foreach (var item in order.Items)
            {
                if (item.Product is not null)
                {
                    item.Product.StockQuantity += item.Quantity;
                }
            }
        }

        order.Status = OrderStatus.Cancelled;

        _orderRepository.Update(order);

        await _orderRepository.CommitAsync();
    }

    public async Task<CheckoutResultDto> CheckoutAsync(
        CreateOrderDto request)
    {

        if (request.Items is null ||
            !request.Items.Any())
        {
            throw new ArgumentException(
                "Cannot checkout an empty order.");
        }


        var customer = await _customerRepository.GetOneAsync(
            c => c.Id == request.CustomerId);

        if (customer is null)
        {
            throw new KeyNotFoundException(
                $"Customer with ID {request.CustomerId} not found.");
        }

 
        var productIds = request.Items
            .Select(i => i.ProductId)
            .Distinct()
            .ToList();

        var products = await _productRepository
            .GetByIdsAsync(productIds);

        var productsById = products
            .ToDictionary(p => p.Id);


        decimal subtotal = 0m;

        var orderItems = new List<OrderItem>();

        foreach (var itemDto in request.Items)
        {
            if (itemDto.Quantity <= 0)
            {
                throw new ArgumentException(
                    "Product quantity must be at least 1.");
            }

            if (!productsById.TryGetValue(
                    itemDto.ProductId,
                    out var product))
            {
                throw new KeyNotFoundException(
                    $"Product with ID {itemDto.ProductId} not found.");
            }

            if (product.StockQuantity < itemDto.Quantity)
            {
                throw new InvalidOperationException(
                    $"Insufficient stock for product " +
                    $"'{product.Name}'. " +
                    $"Available: {product.StockQuantity}, " +
                    $"Requested: {itemDto.Quantity}");
            }

            subtotal +=
                product.Price * itemDto.Quantity;

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = itemDto.Quantity,
                UnitPrice = product.Price
            });

            product.StockQuantity -= itemDto.Quantity;
        }


        decimal discount = 0m;

        if (customer.IsVip)
        {
            discount += Math.Round(
                subtotal * 0.15m,
                2);
        }


        if (!string.IsNullOrWhiteSpace(
                request.CouponCode))
        {
            var couponCode =
                request.CouponCode.Trim().ToUpper();

            var coupon = await _couponRepository.GetOneAsync(
                c => c.Code.ToUpper() == couponCode &&
                     c.IsActive);

            if (coupon is null)
            {
                throw new InvalidOperationException(
                    $"Invalid or inactive coupon code " +
                    $"'{request.CouponCode}'.");
            }

            discount += Math.Round(
                subtotal *
                (coupon.DiscountPercentage / 100m),
                2);
        }


        if (discount > subtotal)
        {
            discount = subtotal;
        }


        var netAmount = subtotal - discount;

        var tax = Math.Round(
            netAmount * 0.14m,
            2);

        var shipping =
            netAmount >= 1000m
                ? 0m
                : 75m;

        var finalTotal =
            netAmount +
            tax +
            shipping;

 
        if (finalTotal > 50000m)
        {
            throw new InvalidOperationException(
                "Payment processing failed. " +
                "Amount exceeds limit.");
        }

 
        var now = DateTime.UtcNow;

        var transactionReference =
            $"TX-{Guid.NewGuid():N}"
            .Substring(0, 11)
            .ToUpper();

        var order = new Order
        {
            CustomerId = customer.Id,
            CreatedAt = now,
            Status = OrderStatus.Paid,
            Subtotal = subtotal,
            DiscountAmount = discount,
            TaxAmount = tax,
            ShippingFee = shipping,
            TotalAmount = finalTotal,
            Items = orderItems
        };

 
        var payment = new Payment
        {
            Order = order,
            Amount = finalTotal,
            PaymentDate = now,
            TransactionReference = transactionReference,
            IsSuccess = true
        };

 
        await _orderRepository.CreateAsync(order);

        await _paymentRepository.CreateAsync(payment);

        
        await _orderRepository.CommitAsync();


        return new CheckoutResultDto
        {
            OrderId = order.Id,
            Status = order.Status.ToString(),
            Subtotal = order.Subtotal,
            Discount = order.DiscountAmount,
            Tax = order.TaxAmount,
            Shipping = order.ShippingFee,
            Total = order.TotalAmount,
            TransactionReference =
                transactionReference
        };
    }
}