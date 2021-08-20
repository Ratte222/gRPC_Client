using Google.Protobuf;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace gRPC_Client.Clients
{
    public class ProductPhotoClient
    {
        private readonly GrpcChannel _channel;
        public ProductPhotoClient(GrpcChannel channel)
        {
            _channel = channel;
        }

        public async Task AddPhoto()
        {
            var _client = new ProductPhotoProtoService.ProductPhotoProtoServiceClient(_channel);



            string _file_path = Directory.GetCurrentDirectory();
            for(int i = 0; i < 3; i++)
            {
                _file_path = Path.GetDirectoryName(_file_path);
            }
            _file_path = Path.Combine(_file_path, "Temp",
                "Florian Waltzer_chouette laponne_YEFrRGFR.jpg");

            if (File.Exists(_file_path))
            {
                var _file_info = new FileInfo(_file_path);
                var stream = _client.AddPhoto();//.RequestStream;
                var _chunk = new AddPhotoRequest();
                //{
                //    ChungMsg.FileName = Path.GetFileName(_file_path),
                //    ChungMsg.FileSize = _file_info.Length
                //};
                _chunk.ChungMsg.FileName = Path.GetFileName(_file_path);
                _chunk.ChungMsg.FileSize = _file_info.Length;
                _chunk.ProductId = 2;
                var _chunk_size = 64 * 1024;

                var _file_bytes = File.ReadAllBytes(_file_path);
                var _file_chunk = new byte[_chunk_size];

                var _offset = 0;

                while (_offset < _file_bytes.Length)
                {
                    //if (CancellationToken.IsCancellationRequested)
                        //break;

                    var _length = Math.Min(_chunk_size, _file_bytes.Length - _offset);
                    Buffer.BlockCopy(_file_bytes, _offset, _file_chunk, 0, _length);

                    _offset += _length;

                    _chunk.ChungMsg.ChunkSize = _length;
                    _chunk.ChungMsg.Chunk = ByteString.CopyFrom(_file_chunk);

                    await stream.RequestStream.WriteAsync(_chunk).ConfigureAwait(false);
                }
                var response = stream.ResponseAsync;
                Console.WriteLine(response.Result.Code);
            }
        }
    }
}
