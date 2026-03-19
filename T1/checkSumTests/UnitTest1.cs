using System.Security.Cryptography;
using System.Text;
using Xunit;
using checkSum;

public class ChecksumTests
{
    private string CreateTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(dir);
        return dir;
    }

    private void Cleanup(string path)
    {
        try { Directory.Delete(path, true); } catch { }
    }

    [Fact]
    public void EmptyDirectory_HashIsMD5OfName()
    {
        var dir = CreateTempDir();
        var dirName = Path.GetFileName(dir);
        var expected = MD5.HashData(Encoding.UTF8.GetBytes(dirName));

        var single = Program.ComputeChecksumSingleThread(dir);
        var multi = Program.ComputeChecksumMultiThread(dir);

        Assert.Equal(expected, single);
        Assert.Equal(expected, multi);
        Assert.Equal(single, multi);

        Cleanup(dir);
    }

    [Fact]
    public void SingleEmptyFile_HashCorrect()
    {
        var dir = CreateTempDir();
        var filePath = Path.Combine(dir, "test.txt");
        File.WriteAllText(filePath, "");

        var nameBytes = Encoding.UTF8.GetBytes("test.txt");
        using var ms = new MemoryStream();
        ms.Write(nameBytes, 0, nameBytes.Length);
        ms.Position = 0;
        var fileHash = MD5.HashData(ms.ToArray());

        // Директория: MD5(dirName + fileHash)
        var dirName = Path.GetFileName(dir);
        var dirNameBytes = Encoding.UTF8.GetBytes(dirName);
        using var msDir = new MemoryStream();
        msDir.Write(dirNameBytes, 0, dirNameBytes.Length);
        msDir.Write(fileHash, 0, fileHash.Length);
        msDir.Position = 0;
        var expected = MD5.HashData(msDir.ToArray());

        var actual = Program.ComputeChecksumSingleThread(dir);

        Assert.Equal(expected, actual);
        Cleanup(dir);
    }

    [Fact]
    public void NestedDirectories_Consistent()
    {
        var root = CreateTempDir();
        var sub = Directory.CreateDirectory(Path.Combine(root, "sub"));
        File.WriteAllText(Path.Combine(sub.FullName, "f1.txt"), "hello");
        File.WriteAllText(Path.Combine(root, "f2.txt"), "world");

        var single = Program.ComputeChecksumSingleThread(root);
        var multi = Program.ComputeChecksumMultiThread(root);

        Assert.Equal(single, multi);
        Cleanup(root);
    }

    [Fact]
    public void OrderIndependence_BySortingNames()
    {
        var dir1 = CreateTempDir();
        var dir2 = CreateTempDir();

        File.WriteAllText(Path.Combine(dir1, "b.txt"), "1");
        File.WriteAllText(Path.Combine(dir1, "a.txt"), "2");

        File.WriteAllText(Path.Combine(dir2, "a.txt"), "2");
        File.WriteAllText(Path.Combine(dir2, "b.txt"), "1");

        var hash1 = Program.ComputeChecksumSingleThread(dir1);
        var hash2 = Program.ComputeChecksumSingleThread(dir2);

        Assert.Equal(hash1, hash2); // Порядок не влияет благодаря сортировке по имени

        Cleanup(dir1);
        Cleanup(dir2);
    }
}