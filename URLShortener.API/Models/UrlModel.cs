using System;

namespace URLShortener.API.Models
{
    public class UrlModel
    {
        public int Id { get; set; }
        public required string OriginalUrl { get; set; }
        public required string ShortUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ClickCount { get; set; }
        public bool IsCustom { get; set; }
    }
} 