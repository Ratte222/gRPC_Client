using Google.Protobuf;
using Grpc.Core;
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

        public async Task GetPhoto()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Temp", "temp.json");
            var _client = new ProductPhotoProtoService.ProductPhotoProtoServiceClient(_channel);

            var _request = new GetPhotoRequest { PhotoId = 4, FileName = "Florian Waltzer_chouette laponne_YEFrRGFR.jpg" };

            var _temp_file = $"temp_{DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")}.tmp";
            var _final_file = _temp_file;

            using (var _call = _client.GetPhoto(_request))
            {
                await using (var _fs = File.OpenWrite(_temp_file))
                {
                    await foreach (var _chunk in _call.ResponseStream.ReadAllAsync().ConfigureAwait(false))
                    {
                        var _total_size = _chunk.FileSize;

                        if (!String.IsNullOrEmpty(_chunk.FileName))
                        {
                            _final_file = Path.Combine(Directory.GetCurrentDirectory(),
                                "Temp", _chunk.FileName);
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

            try
            {
                if (_final_file != _temp_file)
                {
                    string targetDir = Path.GetDirectoryName(_final_file);
                    if (!Directory.Exists(targetDir))
                        Directory.CreateDirectory(targetDir);
                    File.Move(_temp_file, _final_file);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                File.Delete(_temp_file);
            }
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

                using AsyncClientStreamingCall<AddPhotoRequest, Status> stream = _client.AddPhoto();//.RequestStream;
                var _chunk = new AddPhotoRequest();
                //{
                //    ChungMsg.FileName = Path.GetFileName(_file_path),
                //    ChungMsg.FileSize = _file_info.Length
                //};
                _chunk.ChungMsg = new ChunkMsg();
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
                await stream.RequestStream.CompleteAsync();
                //stream.Dispose();
                while (stream.ResponseAsync.IsCompleted || stream.ResponseAsync.IsFaulted ||
                    stream.ResponseAsync.IsCanceled || stream.ResponseAsync.IsCompletedSuccessfully)
                    stream.ResponseAsync.Wait(1);
                Console.WriteLine($"Cod = {stream.ResponseAsync.Id} Res. " +
                    $"code = {stream.ResponseAsync.Result.Code}, mess = {stream.ResponseAsync.Result.Message}");
            }
        }
    }
}
