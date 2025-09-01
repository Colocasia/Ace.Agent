using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AceAgent.Core.Interfaces;
using AceAgent.Core.Models;

namespace AceAgent.Tools
{
    /// <summary>
    /// 网络搜索工具
    /// 提供网络搜索功能，支持多种搜索引擎
    /// </summary>
    public class WebSearchTool : ITool
    {
        private readonly HttpClient _httpClient;
        private readonly string _searchApiKey;
        private readonly string _searchEngineId;

        /// <summary>
        /// 工具名称
        /// </summary>
        public string Name => "web_search";
        
        /// <summary>
        /// 工具描述
        /// </summary>
        public string Description => "搜索互联网获取信息";

        /// <summary>
        /// 初始化WebSearchTool实例
        /// </summary>
        public WebSearchTool()
        {
            _httpClient = new HttpClient();
            _searchApiKey = Environment.GetEnvironmentVariable("GOOGLE_SEARCH_API_KEY") ?? "";
            _searchEngineId = Environment.GetEnvironmentVariable("GOOGLE_SEARCH_ENGINE_ID") ?? "";
        }

        /// <summary>
        /// 执行网络搜索工具
        /// </summary>
        /// <param name="input">工具输入参数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>搜索结果</returns>
        public async Task<ToolResult> ExecuteAsync(ToolInput input, CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                var query = input.GetParameter<string>("query");
                var maxResults = input.GetParameter<int?>("max_results") ?? 10;
                var language = input.GetParameter<string>("language") ?? "zh-CN";
                var safeSearch = input.GetParameter<string>("safe_search") ?? "moderate";

                if (string.IsNullOrWhiteSpace(query))
                    return ToolResult.Failure("搜索查询不能为空");

                if (maxResults <= 0 || maxResults > 50)
                    return ToolResult.Failure("搜索结果数量必须在1-50之间");

                // 如果没有配置API密钥，返回模拟结果
                if (string.IsNullOrEmpty(_searchApiKey) || string.IsNullOrEmpty(_searchEngineId))
                {
                    return CreateMockSearchResult(query, maxResults);
                }

                var searchResults = await PerformGoogleSearchAsync(query, maxResults, language, safeSearch, cancellationToken);
                
                var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                
                var result = ToolResult.CreateSuccess(
                    $"搜索完成，找到 {searchResults.Count} 个结果",
                    new
                    {
                        Query = query,
                        ResultCount = searchResults.Count,
                        Results = searchResults
                    }
                );
                
                result.ExecutionTimeMs = (long)executionTime;
                result.Metadata["operation"] = "web_search";
                result.Metadata["query"] = query;
                result.Metadata["result_count"] = searchResults.Count;
                
                return result;
            }
            catch (Exception ex)
            {
                return ToolResult.FromException(ex);
            }
        }

        /// <summary>
        /// 验证输入参数
        /// </summary>
        /// <param name="input">工具输入参数</param>
        /// <returns>验证结果</returns>
        public async Task<bool> ValidateInputAsync(ToolInput input)
        {
            await Task.CompletedTask;
            
            var query = input.GetParameter<string>("query");
            var maxResults = input.GetParameter<int?>("max_results") ?? 10;

            return !string.IsNullOrWhiteSpace(query) && maxResults > 0 && maxResults <= 50;
        }

        private async Task<List<SearchResult>> PerformGoogleSearchAsync(
            string query, 
            int maxResults, 
            string language, 
            string safeSearch, 
            CancellationToken cancellationToken)
        {
            var url = $"https://www.googleapis.com/customsearch/v1?key={_searchApiKey}&cx={_searchEngineId}&q={Uri.EscapeDataString(query)}&num={Math.Min(maxResults, 10)}&lr=lang_{language}&safe={safeSearch}";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var searchResponse = JsonSerializer.Deserialize<GoogleSearchResponse>(content);
            
            var results = new List<SearchResult>();
            
            if (searchResponse?.Items != null)
            {
                foreach (var item in searchResponse.Items)
                {
                    results.Add(new SearchResult
                    {
                        Title = item.Title ?? "",
                        Url = item.Link ?? "",
                        Snippet = item.Snippet ?? "",
                        DisplayUrl = item.DisplayLink ?? ""
                    });
                }
            }
            
            return results;
        }

        private ToolResult CreateMockSearchResult(string query, int maxResults)
        {
            var mockResults = new List<SearchResult>
            {
                new SearchResult
                {
                    Title = $"关于 '{query}' 的搜索结果 1",
                    Url = "https://example.com/result1",
                    Snippet = $"这是关于 '{query}' 的第一个搜索结果的摘要信息。",
                    DisplayUrl = "example.com"
                },
                new SearchResult
                {
                    Title = $"关于 '{query}' 的搜索结果 2",
                    Url = "https://example.com/result2",
                    Snippet = $"这是关于 '{query}' 的第二个搜索结果的摘要信息。",
                    DisplayUrl = "example.com"
                }
            };

            // 限制结果数量
            var limitedResults = mockResults.Take(Math.Min(maxResults, mockResults.Count)).ToList();

            return ToolResult.CreateSuccess(
                $"模拟搜索完成，找到 {limitedResults.Count} 个结果（未配置搜索API）",
                new
                {
                    Query = query,
                    ResultCount = limitedResults.Count,
                    Results = limitedResults,
                    Note = "这是模拟结果，请配置 GOOGLE_SEARCH_API_KEY 和 GOOGLE_SEARCH_ENGINE_ID 环境变量以使用真实搜索"
                }
            );
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    /// <summary>
    /// 搜索结果
    /// </summary>
    public class SearchResult
    {
        /// <summary>
        /// 搜索结果标题
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// 搜索结果URL
        /// </summary>
        public string Url { get; set; } = string.Empty;
        
        /// <summary>
        /// 搜索结果摘要
        /// </summary>
        public string Snippet { get; set; } = string.Empty;
        
        /// <summary>
        /// 显示URL
        /// </summary>
        public string DisplayUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Google搜索API响应
    /// </summary>
    public class GoogleSearchResponse
    {
        /// <summary>
        /// 搜索结果项列表
        /// </summary>
        public GoogleSearchItem[]? Items { get; set; }
    }

    /// <summary>
    /// Google搜索项
    /// </summary>
    public class GoogleSearchItem
    {
        /// <summary>
        /// 搜索结果标题
        /// </summary>
        public string? Title { get; set; }
        
        /// <summary>
        /// 搜索结果链接
        /// </summary>
        public string? Link { get; set; }
        
        /// <summary>
        /// 搜索结果摘要
        /// </summary>
        public string? Snippet { get; set; }
        
        /// <summary>
        /// 显示链接
        /// </summary>
        public string? DisplayLink { get; set; }
    }
}