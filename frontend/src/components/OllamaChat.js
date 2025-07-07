import React, { useEffect, useRef, useState } from 'react';
import './OllamaChat.css';
import { API_BASE } from './config';

function OllamaChat() {
  const [prompt, setPrompt] = useState('');
  const [messages, setMessages] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const chatEndRef = useRef(null);
  const fileInputRef = useRef(null);

  const scrollToBottom = () => {
    chatEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const sendPrompt = async () => {
    if (!prompt.trim()) return;

    const userMessage = { role: 'user', content: prompt };
    setMessages((prev) => [...prev, userMessage]);
    setLoading(true);
    setError('');

    try {
      const res = await fetch(`${API_BASE}/ask`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ prompt }),
      });

      if (!res.ok) {
        const msg = await res.text();
        throw new Error(msg || 'Ошибка от API');
      }

      const data = await res.json();
      setMessages((prev) => [...prev, { role: 'ai', content: data.response }]);
    } catch (err) {
      console.error(err);
      setError('Ошибка: ' + (err.message || 'Что-то пошло не так.'));
    } finally {
      setLoading(false);
      setPrompt('');
    }
  };

  const uploadFile = async () => {
    const file = fileInputRef.current?.files[0];
    if (!file) return;

    const formData = new FormData();
    formData.append('file', file);
    formData.append('prompt', prompt.trim());

    setMessages((prev) => [
      ...prev,
      {
        role: 'user',
        content: `📎 Загружен файл: ${file.name}\n📝 Задание: ${prompt}`,
      },
    ]);
    setLoading(true);
    setError('');

    try {
      const res = await fetch(`${API_BASE}/upload`, {
        method: 'POST',
        body: formData,
      });

      if (!res.ok) {
        const msg = await res.text();
        throw new Error(msg || 'Ошибка от API');
      }

      const data = await res.json();
      setMessages((prev) => [...prev, { role: 'ai', content: data.response }]);
    } catch (err) {
      console.error(err);
      setError('Ошибка: ' + (err.message || 'Ошибка при анализе файла.'));
    } finally {
      setLoading(false);
      setPrompt('');
      if (fileInputRef.current) fileInputRef.current.value = '';
    }
  };

  return (
    <div className="chat-container">
      <h2>🤖 Ollama Chat</h2>

      <div className="chat-window">
        {messages.map((msg, idx) => (
          <div
            key={idx}
            className={`chat-bubble ${
              msg.role === 'user' ? 'user' : 'response'
            }`}
          >
            <strong>{msg.role === 'user' ? 'Вы:' : 'AI:'}</strong> {msg.content}
          </div>
        ))}

        {loading && (
          <div className="chat-bubble response">
            <em>AI печатает...</em>
          </div>
        )}

        <div ref={chatEndRef} />
      </div>

      <div className="input-area">
        <textarea
          value={prompt}
          onChange={(e) => setPrompt(e.target.value)}
          placeholder="Введите запрос или описание файла..."
        />
        <button onClick={sendPrompt} disabled={loading}>
          Отправить
        </button>
        <input
          type="file"
          ref={fileInputRef}
          style={{ display: 'none' }}
          onChange={uploadFile}
        />
        <button
          onClick={() => fileInputRef.current?.click()}
          disabled={loading}
        >
          📎
        </button>
      </div>

      {error && <div className="error-message">{error}</div>}
    </div>
  );
}

export default OllamaChat;
