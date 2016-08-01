using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Protobuf;
using PokemonGo.RocketAPI.Exceptions;
using POGOProtos.Networking.Envelopes;

namespace PokemonGo.RocketAPI.Extensions
{
    public enum ApiOperation
    {
        Retry,
        Abort
    }

    public interface IApiFailureStrategy
    {
        Task<ApiOperation> HandleApiFailure();
        void HandleApiSuccess();
    }

    public static class HttpClientExtensions
    {
        public static async Task<TResponsePayload> PostProtoPayload<TRequest, TResponsePayload>(this System.Net.Http.HttpClient client,
            string url, RequestEnvelope requestEnvelope, IApiFailureStrategy strategy) where TRequest : IMessage<TRequest>
            where TResponsePayload : IMessage<TResponsePayload>, new()
        {
            Debug.WriteLine($"Requesting {typeof(TResponsePayload).Name}");
            var response = await PostProto<TRequest>(client, url, requestEnvelope);

            while (response.Returns.Count == 0)
            {
                var operation = await strategy.HandleApiFailure();
                if (operation == ApiOperation.Abort)
                {
                    break;
                }

                response = await PostProto<TRequest>(client, url, requestEnvelope);
            }

            if (response.Returns.Count == 0)
                throw new InvalidResponseException();

            strategy.HandleApiSuccess();

            //Decode payload
            //todo: multi-payload support
            var payload = response.Returns[0];
            var parsedPayload = new TResponsePayload();
            parsedPayload.MergeFrom(payload);

            return parsedPayload;
        }

        public static async Task<ResponseEnvelope> PostProto<TRequest>(this System.Net.Http.HttpClient client, string url,
            RequestEnvelope requestEnvelope) where TRequest : IMessage<TRequest>
        {
            //Encode payload and put in envelop, then send
            var data = requestEnvelope.ToByteString();
            var result = await client.PostAsync(url, new ByteArrayContent(data.ToByteArray()));

            //Decode message
            var responseData = await result.Content.ReadAsByteArrayAsync();
            var codedStream = new CodedInputStream(responseData);
            var decodedResponse = new ResponseEnvelope();
            decodedResponse.MergeFrom(codedStream);

            return decodedResponse;
        }
    }
}