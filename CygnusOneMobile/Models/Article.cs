using System;
using System.Collections.Generic;

namespace CygnusOneMobile.Models
{
    public class Article
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public Author Author { get; set; }
        public List<string> Tags { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class Author
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class ArticleResponse
    {
        public List<Article> Data { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool HasMore { get; set; }
    }
}