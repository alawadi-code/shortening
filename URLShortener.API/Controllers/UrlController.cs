using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using URLShortener.API.Data;
using URLShortener.API.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace URLShortener.API.Controllers
{
    [ApiController]
    [Route("")]
    public class UrlController : ControllerBase
    {
        private readonly UrlContext _context;
        private readonly Random _random = new Random();
        
        public UrlController(UrlContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ShortenUrl([FromBody] ShortenUrlRequest request)
        {
            if (string.IsNullOrEmpty(request.OriginalUrl))
            {
                return BadRequest(new { message = "URL cannot be empty" });
            }

            if (!Uri.TryCreate(request.OriginalUrl, UriKind.Absolute, out _))
            {
                return BadRequest(new { message = "Invalid URL format" });
            }

            try
            {
                // First, find any existing URL with the same original URL
                var existingUrl = await _context.Urls.FirstOrDefaultAsync(u => u.OriginalUrl == request.OriginalUrl);

                // If a custom URL is requested
                if (!string.IsNullOrEmpty(request.CustomUrl))
                {
                    // Find if the requested custom URL is used by another URL
                    var customUrlEntry = await _context.Urls.FirstOrDefaultAsync(u => u.ShortUrl == request.CustomUrl);
                    
                    // If custom URL exists and is used by a different original URL
                    if (customUrlEntry != null && customUrlEntry.OriginalUrl != request.OriginalUrl)
                    {
                        return BadRequest(new { message = "Custom URL already exists and is used by another URL" });
                    }

                    // If custom URL exists, delete it (whether it's for this URL or another)
                    if (customUrlEntry != null)
                    {
                        _context.Urls.Remove(customUrlEntry);
                        await _context.SaveChangesAsync();
                    }

                    // If there was an existing URL with a different short URL, delete it
                    if (existingUrl != null && existingUrl.ShortUrl != request.CustomUrl)
                    {
                        _context.Urls.Remove(existingUrl);
                        await _context.SaveChangesAsync();
                    }

                    // Create new entry with custom URL
                    var newUrl = new UrlModel
                    {
                        OriginalUrl = request.OriginalUrl,
                        ShortUrl = request.CustomUrl,
                        CreatedAt = DateTime.UtcNow,
                        ClickCount = 0,
                        IsCustom = true
                    };

                    _context.Urls.Add(newUrl);
                    await _context.SaveChangesAsync();
                    return Ok(new { shortUrl = newUrl.ShortUrl, message = "Custom URL created successfully" });
                }
                else
                {
                    // If no custom URL requested but URL exists, generate new random URL
                    if (existingUrl != null)
                    {
                        _context.Urls.Remove(existingUrl);
                        await _context.SaveChangesAsync();
                    }

                    // Generate new random short URL
                    string shortUrl;
                    do
                    {
                        shortUrl = GenerateRandomString(6);
                    } while (await _context.Urls.AnyAsync(u => u.ShortUrl == shortUrl));

                    var url = new UrlModel
                    {
                        OriginalUrl = request.OriginalUrl,
                        ShortUrl = shortUrl,
                        CreatedAt = DateTime.UtcNow,
                        ClickCount = 0,
                        IsCustom = false
                    };

                    _context.Urls.Add(url);
                    await _context.SaveChangesAsync();
                    return Ok(new { shortUrl = url.ShortUrl, message = "URL created successfully with random short URL" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request" });
            }
        }

        [HttpGet("{shortUrl}")]
        [ProducesResponseType(302)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RedirectToOriginal(string shortUrl)
        {
            var url = await _context.Urls.FirstOrDefaultAsync(u => u.ShortUrl == shortUrl);
            if (url == null)
            {
                return NotFound(new { message = "URL not found" });
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