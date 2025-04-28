using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using URLShortener.API.Data;
using URLShortener.API.Models;
using System.Text.RegularExpressions;

namespace URLShortener.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlController : ControllerBase
    {
        private readonly UrlContext _context;
        private readonly Random _random = new Random();

        public UrlController(UrlContext context)
        {
            _context = context;
        }

        [HttpPost("shorten")]
        public async Task<IActionResult> ShortenUrl([FromBody] ShortenUrlRequest request)
        {
            if (string.IsNullOrEmpty(request.OriginalUrl))
            {
                return BadRequest("URL cannot be empty");
            }

            if (!Uri.TryCreate(request.OriginalUrl, UriKind.Absolute, out _))
            {
                return BadRequest("Invalid URL format");
            }

            // Check if URL already exists
            var existingUrl = await _context.Urls.FirstOrDefaultAsync(u => u.OriginalUrl == request.OriginalUrl);
            if (existingUrl != null)
            {
                return Ok(new { shortUrl = existingUrl.ShortUrl });
            }

            // Generate a random short URL
            string shortUrl;
            if (!string.IsNullOrEmpty(request.CustomUrl))
            {
                shortUrl = request.CustomUrl;
                if (await _context.Urls.AnyAsync(u => u.ShortUrl == shortUrl))
                {
                    return BadRequest("Custom URL already exists");
                }
            }
            else
            {
                do
                {
                    shortUrl = GenerateRandomString(6);
                } while (await _context.Urls.AnyAsync(u => u.ShortUrl == shortUrl));
            }

            var url = new UrlModel
            {
                OriginalUrl = request.OriginalUrl,
                ShortUrl = shortUrl,
                CreatedAt = DateTime.UtcNow,
                ClickCount = 0,
                IsCustom = !string.IsNullOrEmpty(request.CustomUrl)
            };

            _context.Urls.Add(url);
            await _context.SaveChangesAsync();

            return Ok(new { shortUrl = url.ShortUrl });
        }

        [HttpGet("{shortUrl}")]
        public async Task<IActionResult> RedirectToOriginal(string shortUrl)
        {
            var url = await _context.Urls.FirstOrDefaultAsync(u => u.ShortUrl == shortUrl);
            if (url == null)
            {
                return NotFound();
            }

            url.ClickCount++;
            await _context.SaveChangesAsync();

            return Redirect(url.OriginalUrl);
        }

        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }

    public class ShortenUrlRequest
    {
        public required string OriginalUrl { get; set; }
        public string? CustomUrl { get; set; }
    }
} 