using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;

namespace gRPC_Client.Clients
{
    public class FileTransferClient
    {
        private readonly GrpcChannel _channel;
        public FileTransferClient(GrpcChannel channel)
        {
            _channel = channel;
        }

        public async Task GetFile()
        {
            //var channel = Grpc.Net.Client.GrpcChannel.ForAddress("https://localhost:5001/", new GrpcChannelOptions
            //{
            //    MaxReceiveMessageSize = 5 * 1024 * 1024, // 5 MB
            //    MaxSendMessageSize = 5 * 1024 * 1024, // 5 MB
            //});

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Temp", "temp.json");
            var _client = new FileTransferService.FileTransferServiceClient(_channel);

            var _request = new FileRequest { FilePath = filePath };

            var _temp_file = $"temp_{DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")}.tmp";
            var _final_file = _temp_file;

            using (var _call = _client.DownloadFile(_request))
            {
                await using (var _fs = File.OpenWrite(_temp_file))
                {
                    await foreach (var _chunk in _call.ResponseStream.ReadAllAsync().ConfigureAwait(false))
                    {
                        var _total_size = _chunk.FileSize;

                        if (!String.IsNullOrEmpty(_chunk.FileName))
                        {
                            _final_file = _chunk.FileName;
                        }

                        if (_chunk.Chunk.Length == _chunk.ChunkSize)
                            _fs.Write(_chunk.Chunk.ToByteArray());
                        else
                        {
                            _fs.Write(_chunk.Chunk.ToByteArray(), 0, _chunk.ChunkSize);
                            Console.WriteLine($"final chunk size: {_chunk.ChunkSize}");
                        }
                    }
                }
            }

            if (_final_file != _temp_file)
                File.Move(_temp_file, _final_file);
        }
    }
}
