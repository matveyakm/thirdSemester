// <copyright file="Client.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace SimpleFTP;

using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

/// <summary>
/// Represents a simple FTP client that can list directory contents and download files from a server.
/// </summary>
public class Client
{
    private readonly string host;
    private readonly int port;

    /// <summary>
    /// Initializes a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="host">The hostname or IP address of the server.</param>
    /// <param name="port">The port number on which the server is listening.</param>
    public Client(string host, int port)
    {
        this.host = host;
        this.port = port;
    }

    /// <summary>
    /// Sends a request to list the contents of a directory on the server.
    /// </summary>
    /// <param name="path">The relative path to the directory on the server.</param>
    /// <returns>
    /// An array of tuples containing file/directory name and a boolean indicating whether it is a directory.
    /// Returns null if the directory does not exist.
    /// </returns>
    public (string Name, bool IsDirectory)[]? List(string path)
    {
        using TcpClient client = new TcpClient(this.host, this.port);
        using NetworkStream stream = client.GetStream();

        string request = $"1 {path}\n";
        byte[] requestBytes = Encoding.UTF8.GetBytes(request);
        stream.Write(requestBytes, 0, requestBytes.Length);

        string response = ReadLine(stream);
        if (string.IsNullOrEmpty(response))
        {
            return null;
        }

        string[] parts = response.Split(' ');
        if (!int.TryParse(parts[0], out int count) || count == -1)
        {
            return null;
        }

        var result = new (string, bool)[count];
        for (int i = 0; i < count; i++)
        {
            string name = parts[1 + (i * 2)];
            bool isDir = bool.Parse(parts[2 + (i * 2)]);
            result[i] = (name, isDir);
        }

        return result;
    }

    /// <summary>
    /// Downloads a file from the server.
    /// </summary>
    /// <param name="path">The relative path to the file on the server.</param>
    /// <returns>
    /// The file content as a byte array, or null if the file does not exist.
    /// </returns>
    public byte[]? Get(string path)
    {
        using TcpClient client = new TcpClient(this.host, this.port);
        using NetworkStream stream = client.GetStream();

        string request = $"2 {path}\n";
        byte[] requestBytes = Encoding.UTF8.GetBytes(request);
        stream.Write(requestBytes, 0, requestBytes.Length);

        // Read size (8 bytes, long)
        byte[] sizeBytes = new byte[8];
        int read = 0;
        while (read < 8)
        {
            int bytes = stream.Read(sizeBytes, read, 8 - read);
            if (bytes == 0)
            {
                throw new IOException("Connection closed prematurely.");
            }

            read += bytes;
        }

        long size = BitConverter.ToInt64(sizeBytes, 0);
        if (size == -1)
        {
            return null;
        }

        byte[] content = new byte[size];
        read = 0;
        while (read < size)
        {
            int bytes = stream.Read(content, read, (int)(size - read));
            if (bytes == 0)
            {
                throw new IOException("Connection closed prematurely while reading file content.");
            }

            read += bytes;
        }

        return content;
    }

    /// <summary>
    /// Reads a line from the stream terminated by '\n'.
    /// </summary>
    private static string ReadLine(NetworkStream stream)
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

        return sb.ToString();
    }
}