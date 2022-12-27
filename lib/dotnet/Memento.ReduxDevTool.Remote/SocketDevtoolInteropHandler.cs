using Fleck;
using Memento.Core;
using Memento.ReduxDevTool.Internal;
using ScClient;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Memento.ReduxDevTool.Remote;

internal class SocketDevtoolInteropHandler : IDevtoolInteropHandler, IDisposable {
    readonly WebSocketServer _server;
    readonly string _instanceId = Guid.NewGuid().ToString();
    IWebSocketConnection? _webSocketConnection;

    readonly TaskCompletionSource _initializeCompletionSource = new();

    IWebSocketConnection Conneciton => _webSocketConnection
        ?? throw new Exception();

    public Action<string>? MessageHandled { get; set; }
    public Action? SyncRequested { get; set; }

    public SocketDevtoolInteropHandler() {
        Console.WriteLine("--------------------------------------" + _instanceId);

        _server = new WebSocketServer("ws://127.0.0.1:8000/socketcluster/");

        _server.Start(socket => {
            _webSocketConnection = socket;
            _initializeCompletionSource.TrySetResult();

            socket.OnOpen = () => {
                _ = Ping();
                Console.WriteLine("Open!");
            };
            socket.OnClose = () => Console.WriteLine("Close!");
            socket.OnMessage = message => {
               // Console.WriteLine(message);

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
                        socket.Send($$"""
                        {
                            "rid": {{cid}},
                            "data" : {
                                "id": "{{Guid.NewGuid()}}",
                                "pingTimeout": 20000,
                                "isAuthenticated": false
                            }
                        }
                        """);
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
                            {
                                "rid": {{cid}}
                            }
                            """
                        );
                    }
                    else if (eventname == $"sc-{_instanceId}") {
                        if (json?.RootElement.GetProperty("data").GetProperty("type").GetString() is "START") {
                            SyncRequested?.Invoke();
                        }
                        else {
                            var action = json?.RootElement.GetProperty("data").Deserialize<ActionItemFromDevtoolRemote>();
                            if (action is not null) {
                                var actualAction = new ActionItemFromDevtool(
                                    action.Type,
                                    action?.Action,
                                    null
                                );
                                MessageHandled?.Invoke(JsonSerializer.Serialize(actualAction));
                            }
                        }
                    }
                    else if (eventname is "respond") {
                        var data = new {
                            @event = "#publish",
                            data = new {
                                channel = "log",
                                data = new {
                                    type = "START",
                                    id = _instanceId
                                }
                            }
                        };

                        socket.Send(JsonSerializer.Serialize(data));
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.Message);

                }
            };
        });
    }

    public void HandleMessage(string json) {

    }

    public Task InitializeAsync(ImmutableDictionary<string, object> state) {
        return _initializeCompletionSource.Task;
    }

    public async Task SendAsync(Command? command, HistoryStateContextJson context) {
        Console.WriteLine("send" + _instanceId);
        var json = new {
            @event = "#publish",
            data = new {
                channel = "log",
                data = new {
                    type = "STATE",
                    id = _instanceId,
                    payload = JsonSerializer.Serialize(context),
                }
            }
        };
        if (_webSocketConnection is { }) {
            await _webSocketConnection.Send(JsonSerializer.Serialize(json));
        }
    }


    async Task Ping() {
        while (true) {
            await Task.Delay(10000);
            if (_webSocketConnection is null) {
                continue;
            }


            await _webSocketConnection.SendPong(new[] { (byte)1 });
        }
    }

    void Disconnect() {
        var disconnectData = new {
            @event = "#publish",
            data = new {
                channel = "log",
                data = new {
                    id = "2oZhrpqqHJZMqb3RAAAD",
                    type = "DISCONNECTED"
                }
            }
        };
        _webSocketConnection?.Send(JsonSerializer.Serialize(disconnectData)).Wait();
    }

    public void Dispose() {
        Console.WriteLine("disconnect !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        Disconnect();

        _server.Dispose();
        _webSocketConnection?.Close();
    }
}

public record ActionItemFromDevtoolRemote(
    string Type,
    Dictionary<string, JsonElement>? Action
);