using System.Reflection;
using config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Plugins;

var kernelSettings = KernelSettings.LoadSettings();
var handler = new OpenAIHttpclientHandler(kernelSettings);
IKernelBuilder builder = Kernel.CreateBuilder();
builder.Services.AddLogging(c => c.SetMinimumLevel(LogLevel.Information).AddDebug());
builder.AddChatCompletionService(kernelSettings,handler);
builder.Plugins.AddFromType<LightPlugin>();

Kernel kernel = builder.Build();

// Load prompt from resource
using StreamReader reader = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("prompts.Chat.yaml")!);
KernelFunction prompt = kernel.CreateFunctionFromPromptYaml(
    reader.ReadToEnd(),
    promptTemplateFactory: new HandlebarsPromptTemplateFactory()
);

// Create the chat history
ChatHistory chatMessages = [];

// Loop till we are cancelled
while (true)
{
    // Get user input
    System.Console.Write("User > ");
    chatMessages.AddUserMessage(Console.ReadLine()!);

    // Get the chat completions
    OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
    {
    };

    var result = kernel.InvokeStreamingAsync<StreamingChatMessageContent>(
        prompt,
        arguments: new KernelArguments(openAIPromptExecutionSettings) {
            { "messages", chatMessages }
        });

    // Print the chat completions
    ChatMessageContent? chatMessageContent = null;
    await foreach (var content in result)
    {
        System.Console.Write(content);
        if (chatMessageContent == null)
        {
            System.Console.Write("Assistant > ");
            chatMessageContent = new ChatMessageContent(
                content.Role ?? AuthorRole.Assistant,
                content.ModelId!,
                content.Content!,
                content.InnerContent,
                content.Encoding,
                content.Metadata);
        }
        else
        {
            chatMessageContent.Content += content;
        }
    }
    System.Console.WriteLine();
    chatMessages.Add(chatMessageContent!);
}
