using H.Generators.Extensions;
using Microsoft.CodeAnalysis;
using OpenAI_API;
using OpenAI_API.Models;

namespace H.Generators;

[Generator]
// ReSharper disable once InconsistentNaming
public class OpenAIGenerator : IIncrementalGenerator
{
    #region Constants

    private const string Name = nameof(OpenAIGenerator);
    private const string Id = "OAIG";

    #endregion

    #region Properties

    private static Dictionary<string, string> Cache { get; } = new();

    private static readonly char[] TrimChars = { ' ', '\r', '\n' };

    #endregion

    #region Methods

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.AdditionalTextsProvider
            .Where(static text => text.Path.EndsWith(".prompt", StringComparison.InvariantCultureIgnoreCase))
            .Combine(context.AnalyzerConfigOptionsProvider
                .Select(static (x, _) => (
                    UseCache: bool.Parse(x.GetGlobalOption("UseCache", prefix: Name) ?? bool.FalseString),
                    ApiKey: x.GetRequiredGlobalOption("ApiKey", prefix: Name),
                    SystemMessage: x.GetGlobalOption("SystemMessage", prefix: Name) ?? string.Empty,
                    Temperature: double.TryParse(x.GetGlobalOption("Temperature", prefix: Name), out var result) ? result : 0.0,
                    Model: x.GetGlobalOption("Model", prefix: Name) ?? "gpt-3.5-turbo")))
            .SelectAndReportExceptions(GetSourceCode, context, Id)
            .AddSource(context);
    }

    private static FileWithName GetSourceCode(
        (AdditionalText text, (bool UseCache, string ApiKey, string SystemMessage, double Temperature, string Model)) tuple,
        CancellationToken cancellationToken = default)
    {
        var (text, (useCache, apiKey, systemMessage, temperature, model)) = tuple;
        
        string source;
        if (useCache &&
            Cache.TryGetValue(text.Path, out var value))
        {
            source = value;
        }
        else
        {
            var prompt = text.GetText(cancellationToken)?.ToString() ?? string.Empty;
            source = Task.Run(() => GetSourceCodeByPromptAsync(
                prompt: prompt,
                apiKey: apiKey,
                systemMessage: systemMessage,
                temperature: temperature,
                model: model,
                cancellationToken), cancellationToken).Result;
            
            Cache[text.Path] = source;
        }

        return new FileWithName(
            Name: $"{Path.GetFileName(text.Path)}.cs",
            Text: source);
    }

    private static async Task<string> GetSourceCodeByPromptAsync(
        string prompt,
        string apiKey,
        string systemMessage,
        double temperature,
        string model,
        // ReSharper disable once UnusedParameter.Local
        CancellationToken cancellationToken = default)
    {
        var api = new OpenAIAPI(apiKeys: apiKey);
        var chat = api.Chat.CreateConversation();
        chat.RequestParameters.Temperature = temperature;
        chat.Model = new Model(model) { OwnedBy = "openai" };
        if (!string.IsNullOrWhiteSpace(systemMessage))
        {
            chat.AppendSystemMessage(content: systemMessage);
        }
        chat.AppendUserInput(content: prompt);
        
        var response = await chat.GetResponseFromChatbotAsync().ConfigureAwait(false);
        
        return ExtractCode(response);
    }

    private static string ExtractCode(string response)
    {
        if (!response.Contains(value: "```"))
        {
            return response;
        }

        response = response.Replace("```csharp", "```");
        
        var startIndex = response.IndexOf(value: "```", comparisonType: StringComparison.Ordinal) + "```".Length;
        var endIndex = response.IndexOf(value: "```", startIndex: startIndex, comparisonType: StringComparison.Ordinal);
        response = response[startIndex..endIndex].TrimStart(trimChars: TrimChars);

        return response;
    }

    #endregion
}
