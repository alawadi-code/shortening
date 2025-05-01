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
                // Check if URL already exists
                var existingUrl = await _context.Urls.FirstOrDefaultAsync(u => u.OriginalUrl == request.OriginalUrl);
                
                if (existingUrl != null)
                {
                    // Always update the existing URL with new short URL
                    string newShortUrl;
                    if (!string.IsNullOrEmpty(request.CustomUrl))
                    {
                        // Check if the new custom URL is already taken by another URL
                        var customUrlExists = await _context.Urls
                            .AnyAsync(u => u.ShortUrl == request.CustomUrl && u.Id != existingUrl.Id);
                        
                        if (customUrlExists)
                        {
                            return BadRequest(new { message = "Custom URL already exists" });
                        }
                        newShortUrl = request.CustomUrl;
                    }
                    else
                    {
                        do
                        {
                            newShortUrl = GenerateRandomString(6);
                        } while (await _context.Urls.AnyAsync(u => u.ShortUrl == newShortUrl));
                    }

                    existingUrl.ShortUrl = newShortUrl;
                    existingUrl.IsCustom = !string.IsNullOrEmpty(request.CustomUrl);
                    await _context.SaveChangesAsync();
                    return Ok(new { shortUrl = existingUrl.ShortUrl, message = "URL updated successfully" });
                }

                // Generate a new URL entry
                string shortUrl;
                if (!string.IsNullOrEmpty(request.CustomUrl))
                {
                    shortUrl = request.CustomUrl;
                    if (await _context.Urls.AnyAsync(u => u.ShortUrl == shortUrl))
                    {
                        return BadRequest(new { message = "Custom URL already exists" });
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

                return Ok(new { shortUrl = url.ShortUrl, message = "URL created successfully" });
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