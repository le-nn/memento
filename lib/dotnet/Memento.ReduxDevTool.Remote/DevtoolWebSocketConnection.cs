using Fleck;
using Memento.Core;
using Memento.ReduxDevTool.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Memento.ReduxDevTool.Remote;

public class DevtoolWebSocketConnection : IDisposable {
    const int _pingTimeout = 600000;

    readonly WebSocketServer _server;
    readonly object _locker = new();
    readonly List<IWebSocketConnection> _webSocketConnections = new();
    readonly HashSet<IWebSocketConnection> _initializedConnections = new();

    public event Action<string, string>? MessageHandled;

    public event Action<string>? SyncRequested;

    public event Action? SendStartRequested;

    public event Action<int>? HandshakeRequested;

    public bool IsDisposed { get; private set; }

    public DevtoolWebSocketConnection(
        string hostName = "0.0.0.0",
        ushort port = 8000,
        bool secure = false
    ) {
        _ = Ping();

        var protocol = secure ? "wss" : "ws";
        _server = new WebSocketServer($"{protocol}://{hostName}:{port}/socketcluster/");
        _server.Start(socket => {
            lock (_locker) {
                _webSocketConnections.Add(socket);
            }

            socket.OnPing = e => {
                Console.WriteLine("ping -------------");
                Console.WriteLine(e);
                socket.SendPong(""u8.ToArray());
            };

            socket.OnPong = e => {
                Console.WriteLine("pong -------------");
                e.ToList().ForEach(t => Console.Write(t));
                Console.WriteLine();
                Console.WriteLine(Encoding.UTF8.GetString(e));
                socket.SendPong(""u8.ToArray());
            };

            socket.OnError = e => {
                Console.WriteLine("error -------------");
                Console.WriteLine(e);
            };

            socket.OnOpen = () => {
                Console.WriteLine("Open devtool connection");
            };
            socket.OnClose = () => {
                lock (_locker) {
                    _webSocketConnections.Remove(socket);
                    _initializedConnections.Remove(socket);
                }
            };
            socket.OnMessage = message => {
                try {
                    if (message.StartsWith("#")) {
                        return;
                    }

                    var json = JsonSerializer.Deserialize<JsonDocument>(message);

                    var eventname = json?.RootElement.GetProperty("event").GetString();
                    var cid = json?.RootElement.TryGetProperty("cid", out var p) is true
                        ? p.GetInt32()
                        : 0;

                    if (eventname is "#handshake") {
                        HandshakeRequested?.Invoke(cid);
                    }
                    else if (eventname is "login") {
                        socket.Send($$"""
                        {
                            "rid": {{cid}},
                            "data" : "log"
                        }
                        """);
                    }
                    else if (eventname is "#subscribe") {
                        socket.Send($$"""
                            { "rid": {{cid}} }
                            """
                        );
                    }
                    else if (eventname?.StartsWith("sc-") is true) {
                        if (json?.RootElement.GetProperty("data").GetProperty("type").GetString() is "START") {
                            SyncRequested?.Invoke(eventname);
                        }
                        else {
                            var data = json?.RootElement.GetProperty("data");
                            if (data is { } d) {
                                var actualAction = new ActionItemFromDevtool(
                                    d.GetProperty("type").GetString() ?? "",
                                    d.GetProperty("action"),
                                    null
                                );
                                MessageHandled?.Invoke(eventname, JsonSerializer.Serialize(actualAction));
                            }
                        }
                    }
                    else if (eventname is "respond") {
                        SendStartRequested?.Invoke();
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.Message);

                }
            };
        });
    }

    public async Task ReplyHandshakeAsync(int cid, string id) {
        foreach (var connection in _webSocketConnections.ToArray()) {
            if (_initializedConnections.Contains(connection)) {
                continue;
            }

            await connection.Send($$"""
            {
                "rid": {{cid}},
                "data" : {
                    "id": "{{id}}",
                    "pingTimeout": {{_pingTimeout}},
                    "isAuthenticated": false
                }
            }
            """);
        }

    }

    public Task InitializeAsync(ImmutableDictionary<string, object> _) {
        return Task.CompletedTask;
    }

    public async Task SendStartAsync(string id) {
        var data = new {
            @event = "#publish",
            data = new {
                channel = "log",
                data = new {
                    type = "START",
                    id,
                }
            }
        };
        foreach (var connection in _webSocketConnections.ToArray()) {
            if (_initializedConnections.Contains(connection)) {
                continue;
            }

            _initializedConnections.Add(connection);
            await connection.Send(JsonSerializer.Serialize(data));
        }

        SyncRequested?.Invoke(id);
    }

    public async Task SendAsync(string id, HistoryStateContextJson context) {
        Console.WriteLine("send " + id);
        var json = new {
            @event = "#publish",
            data = new {
                channel = "log",
                data = new {
                    type = "STATE",
                    id,
                    payload = JsonSerializer.Serialize(context),
                }
            }
        };

        foreach (var connection in _webSocketConnections.ToArray()) {
            await connection.Send(JsonSerializer.Serialize(json));
        }
    }

    public async Task DisconnectAsync(string id) {
        var disconnectData = new {
            @event = "#publish",
            data = new {
                channel = "log",
                data = new {
                    id,
                    type = "DISCONNECTED"
                }
            }
        };
        foreach (var connection in _webSocketConnections.ToArray()) {
            await connection.Send(JsonSerializer.Serialize(disconnectData));
        }
    }

    async Task Ping() {
        while (true) {
            await Task.Delay(10000);

            foreach (var connection in _webSocketConnections.ToArray()) {
                await connection.SendPing(""u8.ToArray());
            }
        }
    }

    public void Dispose() {
        if (IsDisposed) {
            throw new InvalidOperationException("This instance has already been disposed.");
        }

        IsDisposed = true;
        _server.Dispose();
    }
}
