using H.Generators.Tests.Extensions;
using Microsoft.CodeAnalysis;

namespace H.Generators.IntegrationTests;

public partial class Tests
{
    [TestMethod]
    public Task GeneratesWithoutFilesCorrectly()
    {
        return this.CheckSourceAsync(
            Array.Empty<AdditionalText>());
    }

    [TestMethod]
    public Task GeneratesCalculatorCorrectly()
    {
        return this.CheckSourceAsync(
            new AdditionalText[]
            {
                new MemoryAdditionalText(
                    path: Resources.calculator_prompt.FileName,
                    text: Resources.calculator_prompt.AsString()),
            });
    }
}