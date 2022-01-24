using Grpc.Net.Client;
using GrpcDemo.Services;

namespace GRPC.Demo.Client
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ReaderService.ReaderServiceClient _readerClient;

        public Worker(
            ILogger<Worker> logger,
            ReaderService.ReaderServiceClient readerClient)
        {
            _logger = logger;
            _readerClient = readerClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                // await SendMessage();
                // await StreamServerMessage(stoppingToken);
                // await StreamClientMessages(stoppingToken);
                await BiSendMessage(stoppingToken);

                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task StreamClientMessages(CancellationToken stoppingToken)
        {
            using (var stream = _readerClient.ClientStreamSendMessage())
            {
                for (int i = 0; i < 20; i++)
                {
                    var message = new ReaderMessage { Message = $"Hello from the outside. {i}." };

                    await stream.RequestStream.WriteAsync(message);
                    await Task.Delay(1000, stoppingToken);
                }
                await stream.RequestStream.CompleteAsync();

                var response = await stream.ResponseAsync;

                _logger.LogInformation(response.Message);
            }
        }

        private async Task StreamServerMessage(CancellationToken cancellationToken)
        {
            var message = new ReaderMessage { Message = "Hello from the outside." };

            var response = _readerClient.ServerStreamSendMessage(message);

            while (await response.ResponseStream.MoveNext(cancellationToken))
            {
                _logger.LogInformation(response.ResponseStream.Current.Message);
            }
        }

        private async Task SendMessage()
        {
            var message = new ReaderMessage { Message = "Hello from the outside." };

            var response = await _readerClient.SendMessageAsync(message);

            _logger.LogInformation(response.Message);
        }

        private async Task BiSendMessage(CancellationToken cancellationToken)
        {
            using (var call = _readerClient.BiStreamSendMessage())
            {
                var responseReaderTask = Task.Run(async () =>
                {
                    while (await call.ResponseStream.MoveNext(cancellationToken))
                    {
                        var note = call.ResponseStream.Current;

                        _logger.LogInformation(note.Message);
                    }
                });

                for (int i = 0; i < 20; i++)
                {
                    var message = new ReaderMessage 
                    { 
                        Message = $"Hello from the outside. {i}.",
                    };

                    await call.RequestStream.WriteAsync(message);
                }

                await call.RequestStream.CompleteAsync();
                await responseReaderTask;
            }
        }
    }
}