using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TradingApp.WebApi.Services;

namespace TradingApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WebSocketController : ControllerBase
{
    private const int ReceiveChunkSize = 64 * 1024; // 64KB chunking keeps memory usage reasonable
    private const int MaxMessageBytes = 2 * 1024 * 1024; // 2MB cap per message

    private readonly IWebSocketConnectionManager _connectionManager;
    private readonly ILogger<WebSocketController> _logger;

    public WebSocketController(IWebSocketConnectionManager connectionManager, ILogger<WebSocketController> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    [HttpGet("connect")]
    public async Task Connect(CancellationToken cancellationToken)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        var remoteEndpoint = HttpContext.Connection.RemoteIpAddress?.ToString();
        var connectionId = await _connectionManager.RegisterAsync(webSocket, remoteEndpoint, cancellationToken);

        _logger.LogInformation("WebSocket connection {ConnectionId} established from {Remote}", connectionId, remoteEndpoint);

        await PumpMessagesAsync(connectionId, webSocket, cancellationToken);
    }

    [HttpGet("connections")]
    public ActionResult<IEnumerable<WebSocketConnectionInfo>> GetConnections()
    {
        return Ok(_connectionManager.Connections);
    }

    [HttpDelete("{connectionId}")]
    public async Task<IActionResult> CloseConnection(string connectionId)
    {
        await _connectionManager.RemoveAsync(connectionId, WebSocketCloseStatus.NormalClosure, "Closed by server request");
        return NoContent();
    }

    private async Task PumpMessagesAsync(string connectionId, WebSocket socket, CancellationToken cancellationToken)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(ReceiveChunkSize);

        try
        {
            while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                using var messageStream = new MemoryStream(capacity: ReceiveChunkSize);
                ValueWebSocketReceiveResult result;

                do
                {
                    result = await socket.ReceiveAsync(buffer.AsMemory(0, ReceiveChunkSize), cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        return;
                    }

                    if (result.Count > 0)
                    {
                        messageStream.Write(buffer, 0, result.Count);

                        if (messageStream.Length > MaxMessageBytes)
                        {
                            _logger.LogWarning("WebSocket payload for {ConnectionId} exceeded {MaxBytes} bytes", connectionId, MaxMessageBytes);
                            await socket.CloseAsync(WebSocketCloseStatus.MessageTooBig, "Payload exceeds 2MB limit", CancellationToken.None);
                            return;
                        }
                    }
                } while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Text && messageStream.Length > 0)
                {
                    var message = Encoding.UTF8.GetString(messageStream.GetBuffer(), 0, (int)messageStream.Length);
                    await _connectionManager.BroadcastAsync($"{connectionId}: {message}", cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Request aborted, fall through to cleanup.
        }
        catch (WebSocketException exception)
        {
            _logger.LogWarning(exception, "WebSocket error for {ConnectionId}", connectionId);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
            await _connectionManager.RemoveAsync(connectionId, WebSocketCloseStatus.NormalClosure, "Client disconnected");
            _logger.LogInformation("WebSocket connection {ConnectionId} closed", connectionId);
        }
    }
}
