using System.Collections.Immutable;
using H.Generators.Tests.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using VerifyMSTest;

namespace H.Generators.IntegrationTests;

[TestClass]
public partial class Tests : VerifyBase
{
    private async Task CheckSourceAsync(
        AdditionalText[] additionalTexts,
        CancellationToken cancellationToken = default)
    {
        var referenceAssemblies = ReferenceAssemblies.Net.Net60;
        var references = await referenceAssemblies.ResolveAsync(null, cancellationToken);
        var compilation = (Compilation)CSharpCompilation.Create(
            assemblyName: "Tests",
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var generator = new OpenAIGenerator();
        var driver = CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts(ImmutableArray.Create(additionalTexts))
            .WithUpdatedAnalyzerConfigOptions(new DictionaryAnalyzerConfigOptionsProvider(new Dictionary<string, string>
            {
                ["build_property.OpenAIGenerator_ApiKey"] =
                    Environment.GetEnvironmentVariable("OPENAI_API_KEY") ??
                    throw new AssertInconclusiveException("No OPENAI_API_KEY environment variable found."),
            }))
            .RunGeneratorsAndUpdateCompilation(compilation, out compilation, out _, cancellationToken);
        var diagnostics = compilation.GetDiagnostics(cancellationToken);

        await Task.WhenAll(
            this
                .Verify(diagnostics)
                .UseDirectory("Snapshots")
                .UseTextForParameters("Diagnostics"),
            this
                .Verify(driver)
                .UseDirectory("Snapshots"));
    }
}