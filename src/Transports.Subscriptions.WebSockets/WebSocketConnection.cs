using System.Threading.Tasks;
using GraphQL.Server.Transports.Subscriptions.Abstractions;

namespace GraphQL.Server.Transports.WebSockets
{
    public class WebSocketConnection
    {
        private readonly WebSocketTransport _transport;
        private readonly SubscriptionServer _server;

        public WebSocketConnection(
            WebSocketTransport transport,
            SubscriptionServer subscriptionServer)
        {
            _transport = transport;
            _server = subscriptionServer;
        }

        public virtual async Task Connect()
        {
            try
            {
                await _server.OnConnect();
                await _server.OnDisconnect();
                await _transport.CloseAsync();
            }
            finally
            {
                _transport.Dispose();
            }
        }
    }
}
