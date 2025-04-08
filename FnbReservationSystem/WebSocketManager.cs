using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class WebSocketManager
{
    private static List<WebSocket> _sockets = new List<WebSocket>();

    // Add a WebSocket to the list
    public void AddSocket(WebSocket webSocket)
    {
        _sockets.Add(webSocket);
    }

    // Remove a WebSocket from the list
    public void RemoveSocket(WebSocket webSocket)
    {
        _sockets.Remove(webSocket);
    }

    // Handle incoming WebSocket connections and broadcast messages
    public async Task HandleWebSocketAsync(WebSocket webSocket)
    {
        AddSocket(webSocket); // Store the socket connection

        var buffer = new byte[1024 * 4];
        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Received: {message}");

                // Optionally, send a response back
                var responseMessage = Encoding.UTF8.GetBytes("Message received");
                await webSocket.SendAsync(new ArraySegment<byte>(responseMessage), WebSocketMessageType.Text, true, CancellationToken.None);

                // Broadcast to all connected clients
                await BroadcastToClients("Queue update: " + message);
            }
        }

        RemoveSocket(webSocket); // Remove the socket when disconnected
    }

    // Broadcast message to all connected clients
    public async Task BroadcastToClients(string message)
    {
        var byteMessage = Encoding.UTF8.GetBytes(message);

        foreach (var socket in _sockets)
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(new ArraySegment<byte>(byteMessage), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
