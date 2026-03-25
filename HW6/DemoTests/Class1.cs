using MyNUnit.Attributes;

namespace DemoTests;

public class DemoTestClass
{
    [BeforeClass]
    public static void BeforeAll()
    {
        Console.WriteLine("=== BeforeClass выполнился ===");
    }

    [AfterClass]
    public static void AfterAll()
    {
        Console.WriteLine("=== AfterClass выполнился ===");
    }

    [Test]
    public void Test_Passed()
    {
        Console.WriteLine("Тест прошёл успешно");
    }

    [Test]
    public void Test_Passed2()
    {
        Console.WriteLine("Тест прошёл успешно2");
    }

    [Test]
    public async Task Test_Passed_But_Used_Much_Time()
    {
        await Task.Delay(5000);
        Console.WriteLine("Тест прошёл успешно2");
    }


    [Test(Ignore = "Этот тест пока не готов")]
    public void Test_Ignored() { }

    [Test]
    public void Test_Failed()
    {
        throw new Exception("Этот тест специально падает");
    }

    [Test(Expected = typeof(InvalidOperationException))]
    public void Test_WithExpectedException()
    {
        throw new InvalidOperationException("Ожидаемое исключение");
    }
}