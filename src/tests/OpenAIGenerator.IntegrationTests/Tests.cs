namespace H.Ipc.Generator.IntegrationTests;

[TestClass]
public class NSwagGeneratorTests
{
    [TestMethod]
    public void GeneratesClientCorrectly()
    {
        var calculator = new Calculator();
        
        calculator.Add(1, 1).Should().Be(2);
        calculator.Multiply(1, 1).Should().Be(1);
    }
}