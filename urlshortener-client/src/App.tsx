import React, { useState } from 'react';
import './App.css';

interface ShortenUrlRequest {
  OriginalUrl: string;
  CustomUrl?: string;
}

function App() {
  const [url, setUrl] = useState('');
  const [customUrl, setCustomUrl] = useState('');
  const [shortenedUrl, setShortenedUrl] = useState('');
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setShortenedUrl('');

    try {
      const request: ShortenUrlRequest = {
        OriginalUrl: url,
        CustomUrl: customUrl || undefined
      };

      const response = await fetch('http://localhost:5169/api/url/shorten', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Failed to shorten URL');
      }

      const data = await response.json();
      setShortenedUrl(`http://localhost:5169/api/url/${data.shortUrl}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred');
    }
  };

  return (
    <div className="app-container">
      <div className="url-form">
        <h1>URL Shortener</h1>
        <form onSubmit={handleSubmit}>
          <div className="input-group">
            <input
              type="text"
              value={url}
              onChange={(e) => setUrl(e.target.value)}
              placeholder="Enter URL to shorten"
              required
            />
            <input
              type="text"
              value={customUrl}
              onChange={(e) => setCustomUrl(e.target.value)}
              placeholder="Custom URL (optional)"
              className="custom-url-input"
            />
          </div>
          <button type="submit">Shorten URL</button>
        </form>
        {error && <div className="error-message">{error}</div>}
        {shortenedUrl && (
          <div className="shortened-url">
            <p>Shortened URL:</p>
            <a href={shortenedUrl} target="_blank" rel="noopener noreferrer">
              {shortenedUrl}
            </a>
          </div>
        )}
      </div>
    </div>
  );
}

export default App;
