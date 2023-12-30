using config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a chat completion service to the list. It can be either an OpenAI or Azure OpenAI backend service.
    /// </summary>
    /// <param name="kernelBuilder"></param>
    /// <param name="kernelSettings"></param>
    /// <exception cref="ArgumentException"></exception>
    internal static IKernelBuilder AddChatCompletionService(this IKernelBuilder kernelBuilder, KernelSettings kernelSettings, HttpClientHandler handler)
    {
       
        switch (kernelSettings.ServiceType.ToUpperInvariant())
        {
            case ServiceTypes.AzureOpenAI:
                kernelBuilder = kernelBuilder.AddAzureOpenAIChatCompletion(kernelSettings.DeploymentId,  endpoint: kernelSettings.Endpoint, apiKey: kernelSettings.ApiKey, serviceId: kernelSettings.ServiceId, kernelSettings.ModelId);
                break;

            case ServiceTypes.OpenAI:
                kernelBuilder = kernelBuilder.AddOpenAIChatCompletion(modelId: kernelSettings.ModelId, apiKey: kernelSettings.ApiKey, orgId: kernelSettings.OrgId, serviceId: kernelSettings.ServiceId);
                break;

            case ServiceTypes.HunyuanAI:                
                kernelBuilder = kernelBuilder.AddOpenAIChatCompletion(modelId: kernelSettings.ModelId, apiKey: kernelSettings.ApiKey, httpClient: new HttpClient(handler));
                break;
            default:
                throw new ArgumentException($"Invalid service type value: {kernelSettings.ServiceType}");
        }

        return kernelBuilder;
    }
}
