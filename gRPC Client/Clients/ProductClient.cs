using Grpc.Net.Client;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace gRPC_Client.Clients
{
    public class ProductClient
    {
        private readonly GrpcChannel _channel;
        public ProductClient(GrpcChannel channel)
        {
            _channel = channel;
        }

        public async Task<ProductProto> GetProductAsync(long id)
        {
            var clientProduct = new Product.ProductClient(_channel);
            var replyProduct = await clientProduct.GetProductAsync(new ProductRequest { ProductId = id });
            Console.WriteLine($"Product: {replyProduct.Id}, {replyProduct.Name}, " +
                $"{replyProduct.Description}, {replyProduct.Cost}");
            return replyProduct;
        }

        public async Task<ProductsReply> GetProductsAsync()
        {
            var clientProduct = new Product.ProductClient(_channel);
            var replyProducts = await clientProduct.GetProductsAsync(new ProductsRequest { });
            Console.WriteLine($"PageResponse: {replyProducts.PageResponse.PageLength}, " +
                $"{replyProducts.PageResponse.PageNumber}, {replyProducts.PageResponse.ItemCount}, " +
                $"{replyProducts.PageResponse.TotalItems}, {replyProducts.PageResponse.TotalPages}");
            Console.WriteLine("Products:");
            foreach (var res in replyProducts.Products)
            {
                Console.WriteLine($"Product: {res.Id}, {res.Name}, " +
                $"{res.Description}, {res.Cost}");
                foreach(var value in res.ProductPhotos)
                {
                    Console.WriteLine($"\tPhoto: id = {value.Id}, name: {value.Name}");
                }
            }
            return replyProducts;
        }

        public async Task<string> CreateProductAsync()
        {
            var client = new Product.ProductClient(_channel);
            var reply = await client.CreateProductAsync(
                              new NewProductProto
                              {
                                  Name = "Tea",
                                  Description = "GB Tea",
                                  Cost = 46
                              });
            Console.WriteLine(reply.Message);
            return reply.Message;
        }

        public async Task<string> EditProductAsync(long productId)
        {
            var client = new Product.ProductClient(_channel);
            var reply = await client.EditProductAsync(
                              new ProductProto
                              {
                                  Id = productId,
                                  Name = "Tea",
                                  Description = "Great Britain Tea",
                                  Cost = 46
                              });
            Console.WriteLine(reply.Message);
            return reply.Message;
        }

        public async Task<string> DeleteProductAsync(long productId)
        {
            var client = new Product.ProductClient(_channel);
            var reply = await client.DeleteProductAsync(new ProductRequest { ProductId = productId });
            Console.WriteLine(reply.Message);
            return reply.Message;
        }

    }
}
