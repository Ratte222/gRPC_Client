using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace gRPC_Client.Clients
{
    public class AccountClient
    {
        //private readonly GrpcChannel _channel;
        //public AccountClient()

        public async Task<string> Registration()
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Account.AccountClient(channel);
            var reply = await client.RegistrationAsync(
                              new RegistrationRequest
                              {
                                  FirstName = "Dima",
                                  LastName = "Gold",
                                  UserName = "GD1955",
                                  Email = "GD1955@gmail.com",
                                  Password = "aZ12345678*"
                              });
            Console.WriteLine($"message: {reply.Message}");
            return reply.Message;
        }

        public async Task<string> AccountRequests()
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Account.AccountClient(channel);
            var reply = await client.LoginAsync(
                              new LoginRequest
                              {
                                  EmailOrUserName = "NBA",
                                  Password = "aZ12345678*"
                              });
            Console.WriteLine($"Login userName: {reply.UserName}");
            return reply.Token;
        }
    }
}
