using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using UnityEngine;
using URMT.Core.Managers;
using URMT.Core.Utils;

namespace URMT.Export.Networking {
    public class MessageServer : IDisposable {
        private TcpListener listener;
        private Thread serverThread;
        private CancellationTokenSource CancellationToken = new CancellationTokenSource();

        public MessageServer(string ip, int port) {
            StartServer(ip, port);
        }

        public void Dispose() {
            StopServer();
        }

        private void StartServer(string ip, int port) {
            // Start a new thread to listen for incoming messages
            Thread serverThread = new Thread(() => {
                try {
                    // Create a new TCP listener
                    listener = new TcpListener(System.Net.IPAddress.Parse(ip), port);
                    listener.Start();
                    MainThreadAction.Invoke(() => {
                        LoggerUtils.Log("Listening for messages on {0}:{1}", ip, port);
                    });
                    // Loop forever, accepting new connections and processing messages
                    while(!CancellationToken.Token.IsCancellationRequested) {
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
        }

        public void StopServer() {
            listener?.Stop();
            CancellationToken.Cancel();

            if(serverThread != null && serverThread.IsAlive) {
                serverThread.Join();
            }
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

            MethodInfo method = typeof(MessageBus).GetMethod(nameof(MessageBus.SendMessage));

            MainThreadAction.Invoke(() => {
                method.Invoke(null, new object[] { messageName, args });
            });
        }

        private string TrimQuotes(string input) {
            if(input.StartsWith("\"") && input.EndsWith("\"")) {
                return input.Substring(1, input.Length - 2);
            }
            return input;
        }
    }

}
