// <copyright file="Client.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace SimpleFTP;

using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Represents a simple FTP client that can list directory contents and download files from a server.
/// </summary>
public class Client(string host, int port)
{
    /// <summary>
    /// Sends a request to list the contents of a directory on the server.
    /// </summary>
    /// <param name="path">The relative path to the directory on the server.</param>
    /// <returns>
    /// An array of tuples containing file/directory name and a boolean indicating whether it is a directory.
    /// Returns null if the directory does not exist.
    /// </returns>
    public async Task<(string Name, bool IsDirectory)[]> List(string path)
    {
        using TcpClient client = new();
        await client.ConnectAsync(host, port);
        using NetworkStream stream = client.GetStream();

        string request = $"1 {path}\n";
        byte[] requestBytes = Encoding.UTF8.GetBytes(request);
        stream.Write(requestBytes, 0, requestBytes.Length);

        string response = ReadLine(stream);

        string[] parts = response.Split(' ');
        if (!int.TryParse(parts[0], out int count) || count == -1)
        {
            throw new DirectoryNotFoundException($"Directory '{path}' does not exist on the server.");
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
    /// <param name="destination">The stream to write the file content to (must be writable).</param>
    /// <returns>
    /// The file content as a byte array, or null if the file does not exist.
    /// </returns>
    public async Task Get(string path, Stream destination)
    {
        using TcpClient client = new();
        await client.ConnectAsync(host, port);
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
            throw new FileNotFoundException($"The file '{path}' does not exist on the server.");
        }

        byte[] buffer = new byte[8192];
        long remaining = size;
        while (remaining > 0)
        {
            int bytesToRead = (int)Math.Min(8192L, remaining);
            int bytes = stream.Read(buffer, 0, bytesToRead);
            if (bytes == 0)
            {
                throw new IOException("Connection closed prematurely while reading file content.");
            }

            destination.Write(buffer, 0, bytes);
            remaining -= bytes;
        }
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