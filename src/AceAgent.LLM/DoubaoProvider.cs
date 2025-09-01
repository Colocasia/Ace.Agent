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
    /// 字节跳动豆包提供商实现
    /// </summary>
    public class DoubaoProvider : ILLMProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly Dictionary<string, ModelInfo> _supportedModels;

        public string ProviderName => "Doubao";

        public DoubaoProvider(string apiKey, string? baseUrl = null)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _baseUrl = baseUrl ?? "https://ark.cn-beijing.volces.com/api/v3";
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
                    throw new HttpRequestException($"Doubao API请求失败: {response.StatusCode}, {errorContent}");
                }

                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                var chatCompletion = JsonSerializer.Deserialize<DoubaoResponse>(responseJson, new JsonSerializerOptions
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
                    model = "doubao-lite-4k",
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
                ["doubao-pro-4k"] = new ModelInfo
                {
                    Name = "doubao-pro-4k",
                    DisplayName = "豆包 Pro 4K",
                    Description = "豆包专业版，4K上下文长度",
                    Provider = ProviderName,
                    MaxContextLength = 4096,
                    MaxOutputTokens = 4096,
                    SupportsTools = true,
                    SupportsStreaming = true,
                    SupportsVision = false,
                    InputPricePer1K = 0.0008m,
                    OutputPricePer1K = 0.002m,
                    SupportedLanguages = new List<string> { "zh", "en" },
                    Tags = new List<string> { "chat", "reasoning", "coding" }
                },
                ["doubao-pro-32k"] = new ModelInfo
                {
                    Name = "doubao-pro-32k",
                    DisplayName = "豆包 Pro 32K",
                    Description = "豆包专业版，32K上下文长度",
                    Provider = ProviderName,
                    MaxContextLength = 32768,
                    MaxOutputTokens = 4096,
                    SupportsTools = true,
                    SupportsStreaming = true,
                    SupportsVision = false,
                    InputPricePer1K = 0.0012m,
                    OutputPricePer1K = 0.0024m,
                    SupportedLanguages = new List<string> { "zh", "en" },
                    Tags = new List<string> { "chat", "reasoning", "coding" }
                },
                ["doubao-lite-4k"] = new ModelInfo
                {
                    Name = "doubao-lite-4k",
                    DisplayName = "豆包 Lite 4K",
                    Description = "豆包轻量版，经济实惠",
                    Provider = ProviderName,
                    MaxContextLength = 4096,
                    MaxOutputTokens = 4096,
                    SupportsTools = false,
                    SupportsStreaming = true,
                    SupportsVision = false,
                    InputPricePer1K = 0.0003m,
                    OutputPricePer1K = 0.0006m,
                    SupportedLanguages = new List<string> { "zh", "en" },
                    Tags = new List<string> { "chat" }
                },
                ["doubao-pro-128k"] = new ModelInfo
                {
                    Name = "doubao-pro-128k",
                    DisplayName = "豆包 Pro 128K",
                    Description = "豆包专业版，128K超长上下文",
                    Provider = ProviderName,
                    MaxContextLength = 131072,
                    MaxOutputTokens = 4096,
                    SupportsTools = true,
                    SupportsStreaming = true,
                    SupportsVision = false,
                    InputPricePer1K = 0.005m,
                    OutputPricePer1K = 0.009m,
                    SupportedLanguages = new List<string> { "zh", "en" },
                    Tags = new List<string> { "chat", "reasoning", "coding", "long-context" }
                }
            };
        }

        private object CreateChatCompletionRequest(IEnumerable<Message> messages, LLMOptions? options)
        {
            var doubaoMessages = messages.Select(m => new
            {
                role = m.Role.ToString().ToLowerInvariant(),
                content = m.Content
            }).Where(m => !string.IsNullOrEmpty(m.content)).ToArray();

            var request = new Dictionary<string, object>
            {
                ["model"] = options?.Model ?? "doubao-lite-4k",
                ["messages"] = doubaoMessages
            };

            if (options?.Temperature.HasValue == true)
                request["temperature"] = options.Temperature.Value;

            if (options?.MaxTokens.HasValue == true)
                request["max_tokens"] = options.MaxTokens.Value;

            if (options?.TopP.HasValue == true)
                request["top_p"] = options.TopP.Value;

            if (options?.Stop?.Any() == true)
                request["stop"] = options.Stop;

            if (options?.Stream == true)
                request["stream"] = true;

            // 豆包支持工具调用的模型
            if (options?.Tools?.Any() == true && IsToolSupportedModel(options.Model))
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

        private bool IsToolSupportedModel(string? modelName)
        {
            if (string.IsNullOrEmpty(modelName))
                return false;

            var modelInfo = GetModelInfo(modelName);
            return modelInfo?.SupportsTools == true;
        }

        private static ModelResponse ConvertToModelResponse(DoubaoResponse response)
        {
            var choice = response.Choices?.FirstOrDefault();
            var message = choice?.Message;

            var modelResponse = new ModelResponse
            {
                Content = message?.Content ?? string.Empty,
                Model = response.Model ?? string.Empty,
                FinishReason = choice?.FinishReason ?? string.Empty,
                Usage = response.Usage != null ? new TokenUsage
                {
                    PromptTokens = response.Usage.PromptTokens,
                    CompletionTokens = response.Usage.CompletionTokens,
                    TotalTokens = response.Usage.TotalTokens
                } : null
            };

            if (message?.ToolCalls?.Any() == true)
            {
                modelResponse.ToolCalls = message.ToolCalls.Select(tc => new ToolCall
                {
                    Id = tc.Id ?? string.Empty,
                    Name = tc.Function?.Name ?? string.Empty,
                    Arguments = tc.Function?.Arguments ?? string.Empty
                }).ToList();
            }

            return modelResponse;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    // Doubao API响应模型
    internal class DoubaoResponse
    {
        public string? Id { get; set; }
        public string? Object { get; set; }
        public long Created { get; set; }
        public string? Model { get; set; }
        public List<DoubaoChoice>? Choices { get; set; }
        public DoubaoUsage? Usage { get; set; }
    }

    internal class DoubaoChoice
    {
        public int Index { get; set; }
        public DoubaoMessage? Message { get; set; }
        public string? FinishReason { get; set; }
    }

    internal class DoubaoMessage
    {
        public string? Role { get; set; }
        public string? Content { get; set; }
        public List<DoubaoToolCall>? ToolCalls { get; set; }
    }

    internal class DoubaoToolCall
    {
        public string? Id { get; set; }
        public string? Type { get; set; }
        public DoubaoFunction? Function { get; set; }
    }

    internal class DoubaoFunction
    {
        public string? Name { get; set; }
        public string? Arguments { get; set; }
    }

    internal class DoubaoUsage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }
}