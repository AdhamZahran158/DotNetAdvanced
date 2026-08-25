using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces.IRepositories;
using ECommerce.Application.Interfaces.IServices;
using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _productRepository.GetAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _productRepository.GetOneAsync(
                p => p.Id == id);
        }

        public async Task<Product> CreateAsync(CreateProductDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException(
                    "Product name is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.SKU))
            {
                throw new ArgumentException(
                    "Product SKU is required.");
            }

            if (dto.Price <= 0)
            {
                throw new ArgumentException(
                    "Product price must be greater than zero.");
            }

            if (dto.StockQuantity < 0)
            {
                throw new ArgumentException(
                    "Stock quantity cannot be negative.");
            }

            var normalizedSku = dto.SKU.Trim().ToUpper();

            var skuExists = await _productRepository.GetOneAsync(
                p => p.SKU.ToUpper() == normalizedSku);

            if (skuExists is not null)
            {
                throw new InvalidOperationException(
                    $"Product with SKU '{dto.SKU}' already exists.");
            }

            var product = new Product
            {
                Name = dto.Name.Trim(),
                SKU = normalizedSku,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity
            };

            await _productRepository.CreateAsync(product);

            await _productRepository.CommitAsync();

            return product;
        }

        public async Task<bool> UpdateAsync(
            int id,
            Product product)
        {
            var existing = await _productRepository.GetOneAsync(
                p => p.Id == id);

            if (existing is null)
            {
                return false;
            }

            if (product.Price <= 0)
            {
                throw new ArgumentException(
                    "Price must be positive.");
            }

            if (product.StockQuantity < 0)
            {
                throw new ArgumentException(
                    "Stock quantity cannot be negative.");
            }

            existing.Name = product.Name;
            existing.SKU = product.SKU.ToUpper();
            existing.Price = product.Price;
            existing.StockQuantity = product.StockQuantity;

            _productRepository.Update(existing);

            await _productRepository.CommitAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _productRepository.GetOneAsync(
                p => p.Id == id);

            if (product is null)
            {
                return false;
            }

            _productRepository.Delete(product);

            await _productRepository.CommitAsync();

            return true;
        }
    }
}
