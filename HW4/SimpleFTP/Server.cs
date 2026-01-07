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
    private TcpListener? listener;

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
    /// Starts the server and begins accepting client connections.
    /// </summary>
    public void Start()
    {
        this.listener = new TcpListener(IPAddress.Any, this.port);
        this.listener.Start();
        Console.WriteLine($"Server started on port {this.port}");
        Console.WriteLine($"Root directory: {this.rootDirectory}");

        while (true)
        {
            try
            {
                TcpClient client = this.listener.AcceptTcpClient();
                Task.Run(() => this.ProcessClient(client));
            }
            catch (SocketException)
            {
                break; // Server stopped
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accepting client: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Stops the server.
    /// </summary>
    public void Stop()
    {
        this.listener?.Stop();
        Console.WriteLine("Server stopped.");
    }

    /// <summary>
    /// Processes a single client connection in a separate thread.
    /// </summary>
    private void ProcessClient(TcpClient client)
    {
        try
        {
            ClientHandler handler = new ClientHandler(client, this.rootDirectory);
            handler.Handle();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling client: {ex.Message}");
        }
    }
}