using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;

namespace AuctionServer
{
    class Program
    {
        // Week 8: List to manage multiple client connections (PDC Principle)
        private static List<TcpClient> connectedClients = new List<TcpClient>();
        private static TcpListener serverListener;
        private const int Port = 5000;

        static async Task Main(string[] args)
        {
            // Week 7: Initialize the Server
            serverListener = new TcpListener(IPAddress.Any, Port);
            serverListener.Start();
            Console.WriteLine($"[SERVER] Turbo-Auction Master Node started on port {Port}...");
            Console.WriteLine("[SERVER] Waiting for teammates to connect...");

            // Week 8: Non-blocking loop to accept multiple clients
            _ = Task.Run(() => AcceptClientsAsync());

            Console.WriteLine("Press Enter to broadcast a 'Start' message to all clients, or type 'exit' to stop.");

            while (true)
            {
                string input = Console.ReadLine();
                if (input?.ToLower() == "exit") break;

                // Week 8: Broadcasting logic (The Leader's Tool)
                BroadcastMessage($"[SERVER ALERT]: {input}");
            }

            serverListener.Stop();
        }

        private static async Task AcceptClientsAsync()
        {
            while (true)
            {
                try
                {
                    TcpClient client = await serverListener.AcceptTcpClientAsync();

                    lock (connectedClients)
                    {
                        connectedClients.Add(client);
                    }

                    Console.WriteLine($"[CONNECTION] A new teammate has joined! Total Bidders: {connectedClients.Count}");

                    // Start a background task to handle this specific client
                    _ = Task.Run(() => HandleClientCommunications(client));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Connection error: {ex.Message}");
                    break;
                }
            }
        }

        private static async Task HandleClientCommunications(TcpClient client)
        {
            var reader = new System.IO.StreamReader(client.GetStream());
            while (true)
            {
                try
                {
                    string jsonReceived = await reader.ReadLineAsync();
                    if (jsonReceived != null)
                    {
                        // Turn the JSON string back into a Car object
                        var bidData = System.Text.Json.JsonSerializer.Deserialize<Car>(jsonReceived);

                        Console.WriteLine($"[BID RECEIVED] New bid for: {bidData.Brand} {bidData.Model} at ${bidData.StartingPrice}");

                        // Week 10: Broadcast this bid to EVERYONE else so they see the update
                        BroadcastMessage($"A teammate just placed a bid on the {bidData.Brand} {bidData.Model}!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DISCONNECT] A teammate has left the auction.");
                    break;
                }
            }
        }

        private static void BroadcastMessage(string message)
        {
            lock (connectedClients)
            {
                foreach (var client in connectedClients)
                {
                    try
                    {
                        var writer = new System.IO.StreamWriter(client.GetStream()) { AutoFlush = true };
                        writer.WriteLine(message);
                    }
                    catch { /* Handle disconnected clients here */ }
                }
            }
            Console.WriteLine($"[BROADCAST] Sent: {message}");
        }
    }
}
