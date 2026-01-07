// <copyright file="ClientHandler.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace SimpleFTP;

using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

/// <summary>
/// Handles communication with a single client connection.
/// </summary>
internal class ClientHandler
{
    private readonly TcpClient client;
    private readonly string rootDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientHandler"/> class.
    /// </summary>
    /// <param name="client">The connected TCP client.</param>
    /// <param name="rootDirectory">The root directory from which relative paths are resolved.</param>
    public ClientHandler(TcpClient client, string rootDirectory)
    {
        this.client = client;
        this.rootDirectory = rootDirectory;
    }

    /// <summary>
    /// Processes incoming requests from the client.
    /// </summary>
    public void Handle()
    {
        try
        {
            using NetworkStream stream = this.client.GetStream();
            string? request = ReadLine(stream);
            if (request == null)
            {
                return;
            }

            string[] parts = request.Split(' ', 2);
            if (parts.Length < 2 || !int.TryParse(parts[0], out int command))
            {
                return;
            }

            string path = parts[1].Trim();

            switch (command)
            {
                case 1:
                    this.HandleList(stream, path);
                    break;
                case 2:
                    this.HandleGet(stream, path);
                    break;
                default:
                    // ignore
                    break;
            }
        }
        catch
        {
            // ignore
        }
        finally
        {
            this.client.Close();
        }
    }

    /// <summary>
    /// Sends a list response with the specified count.
    /// </summary>
    private static void SendListResponse(NetworkStream stream, int count)
    {
        string response = count + "\n";
        byte[] data = Encoding.UTF8.GetBytes(response);
        stream.Write(data, 0, data.Length);
    }

    /// <summary>
    /// Sends a file response with size and content.
    /// </summary>
    private static void SendFileResponse(NetworkStream stream, long size, byte[] content)
    {
        byte[] sizeBytes = BitConverter.GetBytes(size);
        stream.Write(sizeBytes, 0, sizeBytes.Length);

        if (size > 0)
        {
            stream.Write(content, 0, content.Length);
        }
    }

    /// <summary>
    /// Reads a line from the stream terminated by '\n'.
    /// </summary>
    private static string? ReadLine(NetworkStream stream)
    {
        var sb = new StringBuilder();
        int b;
        while ((b = stream.ReadByte()) != -1)
        {
            if (b == '\n')
            {
                break;
            }

            if (b != '\r')
            {
                sb.Append((char)b);
            }
        }

        return sb.Length == 0 ? null : sb.ToString();
    }

    /// <summary>
    /// Handles the List command (command code 1).
    /// </summary>
    private void HandleList(NetworkStream stream, string relativePath)
    {
        string fullPath = Path.GetFullPath(Path.Combine(this.rootDirectory, relativePath));

        if (!fullPath.StartsWith(this.rootDirectory, StringComparison.OrdinalIgnoreCase))
        {
            SendListResponse(stream, -1);
            return;
        }

        if (!Directory.Exists(fullPath))
        {
            SendListResponse(stream, -1);
            return;
        }

        try
        {
            var entries = Directory.GetFileSystemEntries(fullPath);
            var response = new StringBuilder();
            response.Append(entries.Length);

            foreach (string entry in entries)
            {
                string name = Path.GetFileName(entry);
                bool isDir = Directory.Exists(entry);
                response.Append($" {name} {isDir.ToString().ToLowerInvariant()}");
            }

            response.Append('\n');

            byte[] data = Encoding.UTF8.GetBytes(response.ToString());
            stream.Write(data, 0, data.Length);
        }
        catch
        {
            SendListResponse(stream, -1);
        }
    }

    /// <summary>
    /// Handles the Get command (command code 2).
    /// </summary>
    private void HandleGet(NetworkStream stream, string relativePath)
    {
        string fullPath = Path.GetFullPath(Path.Combine(this.rootDirectory, relativePath));

        if (!fullPath.StartsWith(this.rootDirectory, StringComparison.OrdinalIgnoreCase))
        {
            SendFileResponse(stream, -1, Array.Empty<byte>());
            return;
        }

        if (!File.Exists(fullPath))
        {
            SendFileResponse(stream, -1, Array.Empty<byte>());
            return;
        }

        try
        {
            byte[] content = File.ReadAllBytes(fullPath);
            SendFileResponse(stream, content.LongLength, content);
        }
        catch
        {
            SendFileResponse(stream, -1, Array.Empty<byte>());
        }
    }
}