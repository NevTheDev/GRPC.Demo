using Grpc.Core;
using GrpcDemo.Services;

namespace GRPC.Demo.Server
{
    public class MessageService : ReaderService.ReaderServiceBase
    {

        private readonly ILogger<MessageService> _logger;

        public MessageService(ILogger<MessageService> logger)
        {
            _logger = logger;
        }

        public override Task<ReaderResponse> SendMessage(ReaderMessage request, ServerCallContext context)
        {
            _logger.LogInformation(request.Message);

            return Task.FromResult(new ReaderResponse { Message = "Cool, cool. Message Received." });

        }

        public override async Task ServerStreamSendMessage(
            ReaderMessage request,
            IServerStreamWriter<ReaderResponse> responseStream, 
            ServerCallContext context)
        {
            _logger.LogInformation(request.Message);

            for (int i = 0; i < 10; i++)
            {
                var response = new ReaderResponse { Message = $"Cool, cool. {i} Message Received." };

                await responseStream.WriteAsync(response);
                await Task.Delay(500);
            }

            _logger.LogInformation("Done sending messages.");
        }

        public override async Task<ReaderResponse> ClientStreamSendMessage(
            IAsyncStreamReader<ReaderMessage> requestStream,
            ServerCallContext context)
        {
            var counter = 0;
            while (await requestStream.MoveNext())
            {
                var currnet = requestStream.Current;

                _logger.LogInformation(currnet.Message);

                counter++;
            }

            return new ReaderResponse { Message = $"Cool, cool. {counter} Messages Received." };
        }

        public override async Task BiStreamSendMessage(
            IAsyncStreamReader<ReaderMessage> requestStream,
            IServerStreamWriter<ReaderResponse> responseStream,
            ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var note = requestStream.Current;
                _logger.LogInformation(note.Message);

                for (int i = 0; i < 5; i++)
                {
                    var response = new ReaderResponse 
                    {
                        Message = $"Cool, cool. {i} Message Received.",
                    };

                    await responseStream.WriteAsync(response);

                }
            }
        }
    }
}
