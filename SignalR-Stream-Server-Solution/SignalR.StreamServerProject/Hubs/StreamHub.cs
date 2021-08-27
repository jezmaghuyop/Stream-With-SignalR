using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SignalR.StreamServerProject.Hubs
{
    public class StreamHub : Hub
    {
        #region Get Random Number
            public ChannelReader<int> GetRandomNumber(int numberOfIterations, CancellationToken cancellationToken)
            {
                var channel = Channel.CreateUnbounded<int>();

                WriteRandomNumber(channel.Writer, numberOfIterations, cancellationToken);

                return channel.Reader;
            }

            private async Task WriteRandomNumber(ChannelWriter<int> channelWriter, int numberOfIterations, CancellationToken cancellationToken)
            {
                Random rnd = new Random();

                for (int i = 0; i < numberOfIterations; i++)
                {
                    // Check if there is a cancellation token
                    cancellationToken.ThrowIfCancellationRequested();

                    Console.WriteLine($"Iternation Count: {i}");

                    // Adding cancellation token as well for safe guard
                    await channelWriter.WriteAsync(rnd.Next(), cancellationToken);
                }

                channelWriter.TryComplete();
            }

        #endregion

        #region Get Random Movies

        public ChannelReader<string> GetRandomMovies(int numberOfMovies, CancellationToken cancellationToken)
        {
            var channel = Channel.CreateUnbounded<string>();

            WriteMovies(channel.Writer, numberOfMovies, cancellationToken);

            return channel.Reader;
        }

        private async Task WriteMovies(ChannelWriter<string> channelWriter, int numberOfIterations, CancellationToken cancellationToken)
        {            
            for (int i = 0; i < numberOfIterations; i++)
            {
                // Check if there is a cancellation token
                cancellationToken.ThrowIfCancellationRequested();


                using (HttpClient client = new HttpClient())
                {
                    var json = await client.GetStringAsync("https://api.reelgood.com/v3.0/content/random?availability=onAnySource&content_kind=movie&nocache=true&region=us&sources=netflix");
                    var parsed = JObject.Parse(json);
                    
                    var bannerImage = $"https://img.reelgood.com/content/movie/{parsed["id"]}/poster-780.webp";

                    await channelWriter.WriteAsync(bannerImage, cancellationToken);
                }

                Console.WriteLine($"Iternation Count: {i}");
            }

            channelWriter.TryComplete();
        }

        #endregion


    }
}
