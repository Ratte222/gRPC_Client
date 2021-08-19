using System;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Net.Client;
using GrpcService;
using gRPC_Client.Clients;
using Grpc.Core;

namespace gRPC_Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string _token = await AccountRequests();
            if (String.IsNullOrEmpty(_token))
                throw new Exception();
            var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            {
                if (!string.IsNullOrEmpty(_token))
                {
                    metadata.Add("Authorization", $"Bearer {_token}");
                }
                return Task.CompletedTask;
            });

            // The port number(5001) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Create(new SslCredentials(), credentials)
            });
            var client = new Greeter.GreeterClient(channel);
            var reply = await client.SayHelloAsync(
                              new HelloRequest { Name = "GreeterClient" });
            Console.WriteLine("Greeting: " + reply.Message);
            
            await ProductRequests(channel);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static async Task<string> AccountRequests()
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Account.AccountClient(channel);
            var reply = await client.LoginAsync(
                              new LoginRequest { EmailOrUserName = "NBA",
                              Password = "aZ12345678*" });
            Console.WriteLine($"Login userName: {reply.UserName}");
            return reply.Token;
        }

        private static async Task ProductRequests(GrpcChannel channel)
        {
            ProductClient productClient = new ProductClient(channel);
            await productClient.GetProductsAsync();
            long productId = 3;
            Console.WriteLine("Testing create product? [y:n]");
            if (Console.ReadLine() == "y")
            {
                await productClient.CreateProductAsync();
            }
            Console.WriteLine("Testing update product? [y:n]");
            if (Console.ReadLine() == "y")
            {
                Console.Write("Product before edit: ");
                await productClient.GetProductAsync(productId);
                await productClient.EditProductAsync(productId);
                Console.Write("Product after edit: ");
                await productClient.GetProductAsync(productId);
            }
            Console.WriteLine("Testing delete product? [y:n]");
            if (Console.ReadLine() == "y")
            {
                await productClient.DeleteProductAsync(productId);
            }
        }
    }
}
