// checkSum/Program.cs
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace checkSum;

class Program
{
    static async Task Main()
    {
        Console.Write("Enter directory path: ");
        string? path = Console.ReadLine()?.Trim('"', ' ', '\t');

        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            Console.WriteLine("Error: Directory not found or path is empty.");
            return;
        }

        Console.WriteLine($"Calculating checksum for: {path}\n");

        var sw1 = Stopwatch.StartNew();
        var single = ComputeChecksumSingleThread(path);
        sw1.Stop();

        var sw2 = Stopwatch.StartNew();
        var multi = await ComputeChecksumMultiThreadAsync(path);
        sw2.Stop();

        Console.WriteLine($"Single-threaded: {ToHex(single)} ({sw1.ElapsedMilliseconds} ms)");
        Console.WriteLine($"Multi-threaded : {ToHex(multi)} ({sw2.ElapsedMilliseconds} ms)");

        bool match = single.SequenceEqual(multi);
        Console.WriteLine($"\nMatch: {match}");

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    #region SINGLE
    static byte[] ComputeChecksumSingleThread(string path)
    {
        var di = new DirectoryInfo(path);
        var nameBytes = Encoding.UTF8.GetBytes(di.Name);

        var entries = di.GetFileSystemInfos()
                        .OrderBy(e => e.Name, StringComparer.Ordinal)
                        .ToArray();

        using var md5 = MD5.Create();
        using var ms = new MemoryStream();
        ms.Write(nameBytes, 0, nameBytes.Length);

        foreach (var entry in entries)
        {
            byte[] hash = entry is DirectoryInfo d
                ? ComputeChecksumSingleThread(d.FullName)
                : ComputeFileHash(entry.FullName);
            ms.Write(hash, 0, hash.Length);
        }

        ms.Position = 0;
        return md5.ComputeHash(ms);
    }
    #endregion

    #region MULTI — BFS + задача ДО TryDequeue
    static async Task<byte[]> ComputeChecksumMultiThreadAsync(string rootPath)
    {
        var results = new ConcurrentDictionary<string, byte[]>();
        var tasks = new ConcurrentBag<Task>();
        var visited = new ConcurrentDictionary<string, bool>();

        var queue = new ConcurrentQueue<string>();
        queue.Enqueue(rootPath);

        while (queue.TryDequeue(out var currentPath))
        {
            // Блокируем путь: если уже обрабатывается — пропускаем
            if (!visited.TryAdd(currentPath, true)) continue;

            var di = new DirectoryInfo(currentPath);

            var task = Task.Run(() =>
            {
                var entries = di.GetFileSystemInfos()
                                .OrderBy(e => e.Name, StringComparer.Ordinal)
                                .ToArray();

                using var md5 = MD5.Create();
                using var ms = new MemoryStream();
                ms.Write(Encoding.UTF8.GetBytes(di.Name), 0, di.Name.Length);

                foreach (var entry in entries)
                {
                    if (entry is DirectoryInfo dir)
                    {
                        // Добавляем в очередь — задача будет создана позже
                        queue.Enqueue(dir.FullName);
                        continue;
                    }

                    var fileHash = ComputeFileHash(entry.FullName);
                    ms.Write(fileHash, 0, fileHash.Length);
                }

                ms.Position = 0;
                var dirHash = md5.ComputeHash(ms);
                results[currentPath] = dirHash;
            });

            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        return FinalHash(rootPath, results);
    }

    static byte[] FinalHash(string path, ConcurrentDictionary<string, byte[]> results)
    {
        var di = new DirectoryInfo(path);
        var entries = di.GetFileSystemInfos()
                        .OrderBy(e => e.Name, StringComparer.Ordinal)
                        .ToArray();

        using var md5 = MD5.Create();
        using var ms = new MemoryStream();
        ms.Write(Encoding.UTF8.GetBytes(di.Name), 0, di.Name.Length);

        foreach (var e in entries)
        {
            byte[] childHash = e is DirectoryInfo d
                ? results[d.FullName]
                : ComputeFileHash(e.FullName);

            ms.Write(childHash, 0, childHash.Length);
        }

        ms.Position = 0;
        return md5.ComputeHash(ms);
    }
    #endregion

    #region FILE HASH
    static byte[] ComputeFileHash(string filePath)
    {
        var fi = new FileInfo(filePath);
        var nameBytes = Encoding.UTF8.GetBytes(fi.Name);

        using var md5 = MD5.Create();
        using var ms = new MemoryStream();
        ms.Write(nameBytes, 0, nameBytes.Length);

        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920);
        fs.CopyTo(ms);

        ms.Position = 0;
        return md5.ComputeHash(ms);
    }
    #endregion

    #region UTILS
    static string ToHex(byte[] data) =>
        BitConverter.ToString(data).Replace("-", "").ToLowerInvariant();
    #endregion
}