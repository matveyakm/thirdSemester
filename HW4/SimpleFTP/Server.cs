// <copyright file="Server.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace SimpleFTP;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

/// <summary>
/// Represents a simple FTP-like server that handles List and Get commands.
/// </summary>
public class Server
{
    private readonly int port;
    private readonly string rootDirectory;
    private readonly List<Task> clientTasks = new();
    private TcpListener? listener;
    private CancellationTokenSource? cancellationTokenSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="Server"/> class.
    /// </summary>
    /// <param name="port">The port to listen on.</param>
    /// <param name="rootDirectory">
    /// The root directory from which all relative paths will be resolved.
    /// Defaults to the current directory.
    /// </param>
    public Server(int port, string? rootDirectory = null)
    {
        this.port = port;
        this.rootDirectory = string.IsNullOrEmpty(rootDirectory)
            ? Environment.CurrentDirectory
            : Path.GetFullPath(rootDirectory);
    }

    /// <summary>
    /// Starts the server asynchronously and begins accepting client connections.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests (allows graceful shutdown).</param>
    /// <returns><>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        this.cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        this.listener = new TcpListener(IPAddress.Any, this.port);
        this.listener.Start();
        Console.WriteLine($"Server started on port {this.port}");
        Console.WriteLine($"Root directory: {this.rootDirectory}");

        while (true)
        {
            try
            {
                TcpClient client = await this.listener.AcceptTcpClientAsync();
                var clientTask = Task.Run(() => this.ProcessClient(client));
                this.clientTasks.Add(clientTask);
            }
            catch (SocketException)
            {
                break; // Server stopped
            }
        }
    }

    /// <summary>
    /// Stops the server.
    /// </summary>
    /// <returns><>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public async Task StopAsync()
    {
        this.cancellationTokenSource?.Cancel();
        this.listener?.Stop();

        if (this.clientTasks.Count > 0)
        {
            await Task.WhenAll(this.clientTasks);
        }

        this.clientTasks.Clear();
        Console.WriteLine("Server stopped.");
    }

    /// <summary>
    /// Processes a single client connection in a separate thread.
    /// </summary>
    private void ProcessClient(TcpClient client)
    {
        try
        {
            ClientHandler handler = new(client, this.rootDirectory);
            handler.Handle();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling client: {ex.Message}");
        }
    }
}