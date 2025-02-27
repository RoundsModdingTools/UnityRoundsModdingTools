using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using URMT.Core.Utils;

namespace URMT.Networking {
    public class MessageServer : IDisposable {
        private TcpListener listener;
        private readonly Thread serverThread;
        private readonly CancellationTokenSource cancellationToken = new CancellationTokenSource();

        private static Dictionary<string, Action<object[]>> messageHandlers = new Dictionary<string, Action<object[]>>();

        public event Action<MessageServer> OnServerStarted;
        public string IP { get; private set; }
        public int Port { get; private set; }

        internal MessageServer(string ip, int port) {
            StartServer(ip, port);
        }

        public static void RegisterMessage(string message, Action<object[]> action) {
            if(messageHandlers.ContainsKey(message)) {
                messageHandlers[message] += action;
            } else {
                messageHandlers[message] = action;
            }
        }

        public void StopServer() {
            listener?.Stop();
            cancellationToken.Cancel();

            if(serverThread != null && serverThread.IsAlive) {
                serverThread.Join();
            }
        }

        public void RestartServer(string ip, int port) {
            StopServer();
            StartServer(ip, port);
        }

        private void StartServer(string ip, int port) {
            IP = ip;
            Port = port;

            Thread serverThread = new Thread(() => {
                try {
                    // Create a new TCP listener
                    listener = new TcpListener(System.Net.IPAddress.Parse(ip), port);
                    listener.Start();
                    MainThreadAction.Invoke(() => {
                        LoggerUtils.Log("Listening for messages on {0}:{1}", ip, port);
                    });
                    // Loop forever, accepting new connections and processing messages
                    while(!cancellationToken.Token.IsCancellationRequested) {
                        // Accept a new connection
                        TcpClient client = listener.AcceptTcpClient();
                        NetworkStream stream = client.GetStream();
                        StreamReader reader = new StreamReader(stream);

                        // Read the message from the client
                        string message = reader.ReadLine();
                        if(message != null) {
                            try {
                                ProcessMessage(message);
                            } catch(Exception ex) {
                                MainThreadAction.Invoke(() => {
                                    LoggerUtils.LogError("Error processing message: " + ex.Message);
                                });
                            }
                        }
                        // Close the connection
                        stream.Close();
                        client.Close();
                    }
                } catch(SocketException e) {
                    MainThreadAction.Invoke(() => {
                        LoggerUtils.Log("Server stopped: " + e.Message);
                    });
                } catch(Exception e) {
                    MainThreadAction.Invoke(() => {
                        LoggerUtils.LogError("Error in server thread: " + e.Message);
                    });
                } finally {
                    listener.Stop();
                }
            });

            serverThread.IsBackground = true;
            serverThread.Start();

            OnServerStarted?.Invoke(this);
        }

        private void ProcessMessage(string message) {
            // Split the message on commas
            string[] parts = message.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if(parts.Length < 2) {
                MainThreadAction.Invoke(() => {
                    LoggerUtils.LogError("Invalid message format: " + message);
                });
                return;
            }

            // Parse the message name and arguments by trimming quotes and whitespace
            string messageName = TrimQuotes(parts[0].Trim());
            string[] args = new string[parts.Length - 1];
            for(int i = 1; i < parts.Length; i++) {
                args[i - 1] = TrimQuotes(parts[i].Trim());
            }

            MainThreadAction.Invoke(() => {
                LoggerUtils.Log("Received message: {0}({1})", messageName, string.Join(", ", args));
            });


            MainThreadAction.Invoke(() => {
                SendMessage(messageName, args);
            });
        }

        private string TrimQuotes(string input) {
            if(input.StartsWith("\"") && input.EndsWith("\"")) {
                return input.Substring(1, input.Length - 2);
            }
            return input;
        }

        private void SendMessage(string message, object[] args) {
            if(messageHandlers.ContainsKey(message)) {
                messageHandlers[message]?.Invoke(args);
            }
        }

        public void Dispose() {
            StopServer();

            cancellationToken.Dispose();
        }
    }
}
