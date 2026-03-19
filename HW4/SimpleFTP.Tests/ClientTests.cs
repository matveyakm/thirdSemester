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
    private Task? serverTask;
    private Client? client;

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
        this.serverTask = this.server.StartAsync();

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

        this.client = new Client(Host, Port);
    }

    /// <summary>
    /// Cleans up the test environment by stopping the server and deleting test files.
    /// </summary>
    /// <returns><>A <see cref="Task"/> representing the asynchronous unit test.</placeholder></returns>
    [TearDown]
    public async Task TearDown()
    {
        if (this.server != null)
        {
            await this.server.StopAsync();
            if (this.serverTask != null)
            {
                await this.serverTask;
            }
        }

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
    /// <returns><>A <see cref="Task"/> representing the asynchronous unit test.</placeholder></returns>
    [Test]
    public async Task List_RootDirectory_ReturnsCorrectEntries()
    {
        Assert.That(this.client, Is.Not.Null);

        var entries = await this.client.List(".");

        Assert.That(entries, Is.Not.Null);
        Assert.That(entries!.Length, Is.AtLeast(3));
        Assert.That(entries, Does.Contain(("Test1.txt", false)));
        Assert.That(entries, Does.Contain(("Test2.txt", false)));
        Assert.That(entries, Does.Contain(("TestF", true)));
    }

    /// <summary>
    /// Tests that the List method returns the correct entries for a subdirectory.
    /// </summary>
    /// <returns><>A <see cref="Task"/> representing the asynchronous unit test.</placeholder></returns>
    [Test]
    public async Task List_SubDirectory_ReturnsCorrectEntries()
    {
        Assert.That(this.client, Is.Not.Null);

        var entries = await this.client.List("./TestF");

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
        => Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await this.client!.List("./NonExistent"));

    /// <summary>
    /// Tests that the Get method returns the correct content for an existing file.
    /// </summary>
    /// <returns><>A <see cref="Task"/> representing the asynchronous unit test.</placeholder></returns>
    [Test]
    public async Task Get_ExistingFile_ReturnsCorrectContent()
    {
        var ms = new MemoryStream();
        await this.client!.Get("./Test1.txt", ms);

        byte[] content = ms.ToArray();
        Assert.That(content, Is.Not.Null);
        Assert.That(Encoding.UTF8.GetString(content), Is.EqualTo("Content of Test1"));
    }

    /// <summary>
    /// Tests that the Get method returns the correct content for a file in a subdirectory.
    /// </summary>
    /// <returns><>A <see cref="Task"/> representing the asynchronous unit test.</placeholder></returns>
    [Test]
    public async Task Get_FileInSubdirectory_ReturnsCorrectContent()
    {
        var ms = new MemoryStream();
        await this.client!.Get("./TestF/TestS.txt", ms);

        byte[] content = ms.ToArray();
        Assert.That(content, Is.Not.Null);
        Assert.That(Encoding.UTF8.GetString(content), Is.EqualTo("Nested file"));
    }

    /// <summary>
    /// Tests that the Get method returns null for a non-existent file.
    /// </summary>
    [Test]
    public void Get_NonExistentFile_ReturnsNull()
        => Assert.ThrowsAsync<FileNotFoundException>(async () => await this.client!.Get("./Missing.txt", new MemoryStream()));

    /// <summary>
    /// Tests that directory traversal is blocked for the List method.
    /// </summary>
    [Test]
    public void List_DirectoryTraversal_IsBlocked()
        => Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await this.client!.List("../"));

    /// <summary>
    /// Tests that directory traversal is blocked for the Get method.
    /// </summary>
    public void Get_DirectoryTraversal_IsBlocked()
        => Assert.ThrowsAsync<FileNotFoundException>(async () => await this.client!.Get("../secret.txt", new MemoryStream()));
}