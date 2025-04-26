using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using CygnusOneMobile.Models;

namespace CygnusOneMobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://cygnusone-staging-572cc33c3856.herokuapp.com/api";
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService()
        {
            _httpClient = new HttpClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<ArticleResponse> GetArticlesAsync(int page = 1, int pageSize = 10, string? author = null, string? tag = null)
        {
            try
            {
                var requestUrl = $"{_baseUrl}/articles?page={page}&limit={pageSize}";
                
                if (!string.IsNullOrEmpty(author))
                {
                    requestUrl += $"&author={author}";
                }
                
                if (!string.IsNullOrEmpty(tag))
                {
                    requestUrl += $"&tag={tag}";
                }

                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ArticleResponse>(content, _jsonOptions);
                return result ?? new ArticleResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching articles: {ex.Message}");
                return new ArticleResponse { Data = new List<Article>() };
            }
        }

        public async Task<Article?> GetArticleByIdAsync(string id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/articles/{id}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Article>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching article: {ex.Message}");
                return null;
            }
        }
    }
}