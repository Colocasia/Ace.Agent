using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AceAgent.Core.Interfaces;
using AceAgent.Core.Models;

namespace AceAgent.LLM
{
    /// <summary>
    /// OpenAI提供商实现
    /// </summary>
    public class OpenAIProvider : ILLMProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly Dictionary<string, ModelInfo> _supportedModels;

        public string ProviderName => "OpenAI";

        public OpenAIProvider(string apiKey, string? baseUrl = null)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _baseUrl = baseUrl ?? "https://api.openai.com/v1";
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "AceAgent/1.0");
            
            _supportedModels = InitializeSupportedModels();
        }

        public async Task<ModelResponse> GenerateResponseAsync(
            IEnumerable<Message> messages, 
            LLMOptions? options = null, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var request = CreateChatCompletionRequest(messages, options);
                var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/chat/completions", content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException($"OpenAI API请求失败: {response.StatusCode}, {errorContent}");
                }

                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                var chatCompletion = JsonSerializer.Deserialize<OpenAIChatCompletion>(responseJson, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                return ConvertToModelResponse(chatCompletion!);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"生成响应时发生错误: {ex.Message}", ex);
            }
        }

        public IEnumerable<string> GetSupportedModels()
        {
            return _supportedModels.Keys;
        }

        public async Task<bool> ValidateConfigurationAsync()
        {
            try
            {
                var request = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new { role = "user", content = "Hello" }
                    },
                    max_tokens = 1
                };

                var requestJson = JsonSerializer.Serialize(request);
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/chat/completions", content);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public ModelInfo? GetModelInfo(string modelName)
        {
            return _supportedModels.TryGetValue(modelName, out var modelInfo) ? modelInfo : null;
        }

        private Dictionary<string, ModelInfo> InitializeSupportedModels()
        {
            return new Dictionary<string, ModelInfo>
            {
                ["gpt-4"] = new ModelInfo
                {
                    Name = "gpt-4",
                    DisplayName = "GPT-4",
                    Description = "OpenAI最先进的大型语言模型",
                    Provider = ProviderName,
                    MaxContextLength = 8192,
                    MaxOutputTokens = 4096,
                    SupportsTools = true,
                    SupportsStreaming = true,
                    SupportsVision = false,
                    InputPricePer1K = 0.03m,
                    OutputPricePer1K = 0.06m,
                    SupportedLanguages = new List<string> { "zh", "en", "ja", "ko", "fr", "de", "es" },
                    Tags = new List<string> { "chat", "reasoning", "coding" }
                },
                ["gpt-4-turbo"] = new ModelInfo
                {
                    Name = "gpt-4-turbo",
                    DisplayName = "GPT-4 Turbo",
                    Description = "更快、更便宜的GPT-4版本",
                    Provider = ProviderName,
                    MaxContextLength = 128000,
                    MaxOutputTokens = 4096,
                    SupportsTools = true,
                    SupportsStreaming = true,
                    SupportsVision = true,
                    InputPricePer1K = 0.01m,
                    OutputPricePer1K = 0.03m,
                    SupportedLanguages = new List<string> { "zh", "en", "ja", "ko", "fr", "de", "es" },
                    Tags = new List<string> { "chat", "reasoning", "coding", "vision" }
                },
                ["gpt-3.5-turbo"] = new ModelInfo
                {
                    Name = "gpt-3.5-turbo",
                    DisplayName = "GPT-3.5 Turbo",
                    Description = "快速且经济的聊天模型",
                    Provider = ProviderName,
                    MaxContextLength = 16385,
                    MaxOutputTokens = 4096,
                    SupportsTools = true,
                    SupportsStreaming = true,
                    SupportsVision = false,
                    InputPricePer1K = 0.0015m,
                    OutputPricePer1K = 0.002m,
                    SupportedLanguages = new List<string> { "zh", "en", "ja", "ko", "fr", "de", "es" },
                    Tags = new List<string> { "chat", "coding" }
                }
            };
        }

        private object CreateChatCompletionRequest(IEnumerable<Message> messages, LLMOptions? options)
        {
            var openAIMessages = messages.Select(m => new
            {
                role = m.Role.ToString().ToLowerInvariant(),
                content = m.Content,
                tool_call_id = m.ToolCallId,
                name = m.ToolName
            }).Where(m => !string.IsNullOrEmpty(m.content)).ToArray();

            var request = new Dictionary<string, object>
            {
                ["model"] = options?.Model ?? "gpt-3.5-turbo",
                ["messages"] = openAIMessages
            };

            if (options?.Temperature.HasValue == true)
                request["temperature"] = options.Temperature.Value;

            if (options?.MaxTokens.HasValue == true)
                request["max_tokens"] = options.MaxTokens.Value;

            if (options?.TopP.HasValue == true)
                request["top_p"] = options.TopP.Value;

            if (options?.FrequencyPenalty.HasValue == true)
                request["frequency_penalty"] = options.FrequencyPenalty.Value;

            if (options?.PresencePenalty.HasValue == true)
                request["presence_penalty"] = options.PresencePenalty.Value;

            if (options?.Stop?.Any() == true)
                request["stop"] = options.Stop;

            if (options?.Stream == true)
                request["stream"] = true;

            if (options?.Tools?.Any() == true)
            {
                request["tools"] = options.Tools.Select(t => new
                {
                    type = t.Type,
                    function = new
                    {
                        name = t.Function?.Name,
                        description = t.Function?.Description,
                        parameters = t.Function?.Parameters
                    }
                }).ToArray();

                if (!string.IsNullOrEmpty(options.ToolChoice))
                    request["tool_choice"] = options.ToolChoice;
            }

            return request;
        }

        private static ModelResponse ConvertToModelResponse(OpenAIChatCompletion completion)
        {
            var choice = completion.Choices?.FirstOrDefault();
            var message = choice?.Message;

            var response = new ModelResponse
            {
                Content = message?.Content ?? string.Empty,
                Model = completion.Model ?? string.Empty,
                FinishReason = choice?.FinishReason ?? string.Empty,
                Usage = completion.Usage != null ? new TokenUsage
                {
                    PromptTokens = completion.Usage.PromptTokens,
                    CompletionTokens = completion.Usage.CompletionTokens,
                    TotalTokens = completion.Usage.TotalTokens
                } : null
            };

            if (message?.ToolCalls?.Any() == true)
            {
                response.ToolCalls = message.ToolCalls.Select(tc => new ToolCall
                {
                    Id = tc.Id ?? string.Empty,
                    Name = tc.Function?.Name ?? string.Empty,
                    Arguments = tc.Function?.Arguments ?? string.Empty
                }).ToList();
            }

            return response;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    // OpenAI API响应模型
    internal class OpenAIChatCompletion
    {
        public string? Id { get; set; }
        public string? Object { get; set; }
        public long Created { get; set; }
        public string? Model { get; set; }
        public List<OpenAIChoice>? Choices { get; set; }
        public OpenAIUsage? Usage { get; set; }
    }

    internal class OpenAIChoice
    {
        public int Index { get; set; }
        public OpenAIMessage? Message { get; set; }
        public string? FinishReason { get; set; }
    }

    internal class OpenAIMessage
    {
        public string? Role { get; set; }
        public string? Content { get; set; }
        public List<OpenAIToolCall>? ToolCalls { get; set; }
    }

    internal class OpenAIToolCall
    {
        public string? Id { get; set; }
        public string? Type { get; set; }
        public OpenAIFunction? Function { get; set; }
    }

    internal class OpenAIFunction
    {
        public string? Name { get; set; }
        public string? Arguments { get; set; }
    }

    internal class OpenAIUsage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }
}