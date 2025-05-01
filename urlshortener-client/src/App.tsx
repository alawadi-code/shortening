import React, { useState } from 'react';
import './App.css';

interface ShortenUrlRequest {
  OriginalUrl: string;
  CustomUrl?: string;
}

interface ApiResponse {
  shortUrl: string;
  message: string;
}

function App() {
  const [url, setUrl] = useState('');
  const [customUrl, setCustomUrl] = useState('');
  const [shortenedUrl, setShortenedUrl] = useState('');
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setMessage('');
    setShortenedUrl('');

    try {
      const request: ShortenUrlRequest = {
        OriginalUrl: url,
        CustomUrl: customUrl || undefined
      };

      const response = await fetch('http://localhost:5169', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(request),
        mode: 'cors',
        credentials: 'include'
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Failed to shorten URL');
      }

      const data: ApiResponse = await response.json();
      setShortenedUrl(`http://localhost:5169/${data.shortUrl}`);
      setMessage(data.message);
    } catch (err) {
      console.error('Error:', err);
      setError(err instanceof Error ? err.message : 'Failed to shorten URL. Please try again.');
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
          <p className="help-text">
            Note: When you enter a URL that already exists, a new short URL will be generated 
            or your custom URL will be applied if provided.
          </p>
          <button type="submit">Shorten URL</button>
        </form>
        {error && <div className="error-message">{error}</div>}
        {message && <div className="success-message">{message}</div>}
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
