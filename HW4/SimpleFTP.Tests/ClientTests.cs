// <copyright file="ClientTests.cs" company="matveyakm">
//     Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace SimpleFTP.Tests;

using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NUnit.Framework;
using SimpleFTP;

/// <summary>
/// Integration tests for the SimpleFTP client and server.
/// </summary>
[TestFixture]
public class ClientTests
{
    private const int Port = 8888;
    private const string Host = "127.0.0.1";

    private string testDir = string.Empty;
    private Server? server;
    private Thread? serverThread;

    /// <summary>
    /// Initializes the test environment by setting up the server and creating test files.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this.testDir = Directory.GetCurrentDirectory();

        File.WriteAllText(Path.Combine(this.testDir, "Test1.txt"), "Content of Test1");
        File.WriteAllText(Path.Combine(this.testDir, "Test2.txt"), "Another file content");

        string subDir = Path.Combine(this.testDir, "TestF");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "TestS.txt"), "Nested file");

        this.server = new Server(Port);

        this.serverThread = new Thread(() =>
        {
            try
            {
                this.server.Start();
            }
            catch
            {
            }
        });
        this.serverThread.Start();

        int attempts = 50;
        while (attempts-- > 0)
        {
            try
            {
                using var tc = new TcpClient(Host, Port);
                break;
            }
            catch
            {
                Thread.Sleep(100);
            }
        }

        if (attempts < 0)
        {
            Assert.Fail("Server did not start.");
        }
    }

    /// <summary>
    /// Cleans up the test environment by stopping the server and deleting test files.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        this.server?.Stop();
        this.serverThread?.Join(3000);

        try
        {
            File.Delete(Path.Combine(this.testDir, "Test1.txt"));
        }
        catch
        {
    }

        try
        {
            File.Delete(Path.Combine(this.testDir, "Test2.txt"));
        }
        catch
        {
        }

        try
        {
            File.Delete(Path.Combine(this.testDir, "TestF", "TestS.txt"));
        }
        catch
        {
        }

        try
        {
            Directory.Delete(Path.Combine(this.testDir, "TestF"));
        }
        catch
        {
        }
    }

    /// <summary>
    /// Tests that the List method returns the correct entries for the root directory.
    /// </summary>
    [Test]
    public void List_RootDirectory_ReturnsCorrectEntries()
    {
        var client = new Client(Host, Port);
        var entries = client.List(".");

        Assert.That(entries, Is.Not.Null);
        Assert.That(entries!.Length, Is.AtLeast(3));
        Assert.That(entries, Does.Contain(("Test1.txt", false)));
        Assert.That(entries, Does.Contain(("Test2.txt", false)));
        Assert.That(entries, Does.Contain(("TestF", true)));
    }

    /// <summary>
    /// Tests that the List method returns the correct entries for a subdirectory.
    /// </summary>
    [Test]
    public void List_SubDirectory_ReturnsCorrectEntries()
    {
        var client = new Client(Host, Port);
        var entries = client.List("./TestF");

        Assert.That(entries, Is.Not.Null);
        Assert.That(entries!.Length, Is.EqualTo(1));
        Assert.That(entries[0].Name, Is.EqualTo("TestS.txt"));
        Assert.That(entries[0].IsDirectory, Is.False);
    }

    /// <summary>
    /// Tests that the List method returns null for a non-existent directory.
    /// </summary>
    [Test]
    public void List_NonExistentDirectory_ReturnsNull()
    {
        var client = new Client(Host, Port);
        var entries = client.List("./NonExistent");

        Assert.That(entries, Is.Null);
    }

    /// <summary>
    /// Tests that the Get method returns the correct content for an existing file.
    /// </summary>
    [Test]
    public void Get_ExistingFile_ReturnsCorrectContent()
    {
        var client = new Client(Host, Port);
        byte[]? content = client.Get("./Test1.txt");

        Assert.That(content, Is.Not.Null);
        Assert.That(Encoding.UTF8.GetString(content!), Is.EqualTo("Content of Test1"));
    }

    /// <summary>
    /// Tests that the Get method returns the correct content for a file in a subdirectory.
    /// </summary>
    [Test]
    public void Get_FileInSubdirectory_ReturnsCorrectContent()
    {
        var client = new Client(Host, Port);
        byte[]? content = client.Get("./TestF/TestS.txt");

        Assert.That(content, Is.Not.Null);
        Assert.That(Encoding.UTF8.GetString(content!), Is.EqualTo("Nested file"));
    }

    /// <summary>
    /// Tests that the Get method returns null for a non-existent file.
    /// </summary>
    [Test]
    public void Get_NonExistentFile_ReturnsNull()
    {
        var client = new Client(Host, Port);
        byte[]? content = client.Get("./Missing.txt");

        Assert.That(content, Is.Null);
    }

    /// <summary>
    /// Tests that directory traversal is blocked for the List method.
    /// </summary>
    [Test]
    public void List_DirectoryTraversal_IsBlocked()
    {
        var client = new Client(Host, Port);
        var entries = client.List("../");

        Assert.That(entries, Is.Null);
    }

    /// <summary>
    /// Tests that directory traversal is blocked for the Get method.
    /// </summary>
    [Test]
    public void Get_DirectoryTraversal_IsBlocked()
    {
        var client = new Client(Host, Port);
        byte[]? content = client.Get("../secret.txt");

        Assert.That(content, Is.Null);
    }
}