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
    /// Anthropic Claude提供商实现
    /// </summary>
    public class AnthropicProvider : ILLMProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly Dictionary<string, ModelInfo> _supportedModels;

        public string ProviderName => "Anthropic";

        public AnthropicProvider(string apiKey, string? baseUrl = null)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _baseUrl = baseUrl ?? "https://api.anthropic.com";
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
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
                var request = CreateMessageRequest(messages, options);
                var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/v1/messages", content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException($"Anthropic API请求失败: {response.StatusCode}, {errorContent}");
                }

                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                var anthropicResponse = JsonSerializer.Deserialize<AnthropicMessageResponse>(responseJson, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                return ConvertToModelResponse(anthropicResponse!);
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
                    model = "claude-3-haiku-20240307",
                    max_tokens = 1,
                    messages = new[]
                    {
                        new { role = "user", content = "Hello" }
                    }
                };

                var requestJson = JsonSerializer.Serialize(request);
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/v1/messages", content);

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
                ["claude-3-opus-20240229"] = new ModelInfo
                {
                    Name = "claude-3-opus-20240229",
                    DisplayName = "Claude 3 Opus",
                    Description = "Anthropic最强大的模型，适合复杂推理任务",
                    Provider = ProviderName,
                    MaxContextLength = 200000,
                    MaxOutputTokens = 4096,
                    SupportsTools = true,
                    SupportsStreaming = true,
                    SupportsVision = true,
                    InputPricePer1K = 0.015m,
                    OutputPricePer1K = 0.075m,
                    SupportedLanguages = new List<string> { "zh", "en", "ja", "ko", "fr", "de", "es" },
                    Tags = new List<string> { "chat", "reasoning", "coding", "vision" }
                },
                ["claude-3-sonnet-20240229"] = new ModelInfo
                {
                    Name = "claude-3-sonnet-20240229",
                    DisplayName = "Claude 3 Sonnet",
                    Description = "平衡性能和成本的模型",
                    Provider = ProviderName,
                    MaxContextLength = 200000,
                    MaxOutputTokens = 4096,
                    SupportsTools = true,
                    SupportsStreaming = true,
                    SupportsVision = true,
                    InputPricePer1K = 0.003m,
                    OutputPricePer1K = 0.015m,
                    SupportedLanguages = new List<string> { "zh", "en", "ja", "ko", "fr", "de", "es" },
                    Tags = new List<string> { "chat", "reasoning", "coding", "vision" }
                },
                ["claude-3-haiku-20240307"] = new ModelInfo
                {
                    Name = "claude-3-haiku-20240307",
                    DisplayName = "Claude 3 Haiku",
                    Description = "快速且经济的模型",
                    Provider = ProviderName,
                    MaxContextLength = 200000,
                    MaxOutputTokens = 4096,
                    SupportsTools = true,
                    SupportsStreaming = true,
                    SupportsVision = true,
                    InputPricePer1K = 0.00025m,
                    OutputPricePer1K = 0.00125m,
                    SupportedLanguages = new List<string> { "zh", "en", "ja", "ko", "fr", "de", "es" },
                    Tags = new List<string> { "chat", "coding" }
                }
            };
        }

        private object CreateMessageRequest(IEnumerable<Message> messages, LLMOptions? options)
        {
            var anthropicMessages = messages
                .Where(m => m.Role != MessageRole.System)
                .Select(m => new
                {
                    role = m.Role == MessageRole.Assistant ? "assistant" : "user",
                    content = m.Content
                }).ToArray();

            var systemMessage = messages.FirstOrDefault(m => m.Role == MessageRole.System)?.Content;

            var request = new Dictionary<string, object>
            {
                ["model"] = options?.Model ?? "claude-3-haiku-20240307",
                ["max_tokens"] = options?.MaxTokens ?? 4096,
                ["messages"] = anthropicMessages
            };

            if (!string.IsNullOrEmpty(systemMessage))
                request["system"] = systemMessage;

            if (options?.Temperature.HasValue == true)
                request["temperature"] = options.Temperature.Value;

            if (options?.TopP.HasValue == true)
                request["top_p"] = options.TopP.Value;

            if (options?.Stop?.Any() == true)
                request["stop_sequences"] = options.Stop;

            if (options?.Stream == true)
                request["stream"] = true;

            return request;
        }

        private static ModelResponse ConvertToModelResponse(AnthropicMessageResponse response)
        {
            var content = response.Content?.FirstOrDefault()?.Text ?? string.Empty;

            return new ModelResponse
            {
                Content = content,
                Model = response.Model ?? string.Empty,
                FinishReason = response.StopReason ?? string.Empty,
                Usage = response.Usage != null ? new TokenUsage
                {
                    PromptTokens = response.Usage.InputTokens,
                    CompletionTokens = response.Usage.OutputTokens,
                    TotalTokens = response.Usage.InputTokens + response.Usage.OutputTokens
                } : null
            };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    // Anthropic API响应模型
    internal class AnthropicMessageResponse
    {
        public string? Id { get; set; }
        public string? Type { get; set; }
        public string? Role { get; set; }
        public List<AnthropicContent>? Content { get; set; }
        public string? Model { get; set; }
        public string? StopReason { get; set; }
        public string? StopSequence { get; set; }
        public AnthropicUsage? Usage { get; set; }
    }

    internal class AnthropicContent
    {
        public string? Type { get; set; }
        public string? Text { get; set; }
    }

    internal class AnthropicUsage
    {
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
    }
}