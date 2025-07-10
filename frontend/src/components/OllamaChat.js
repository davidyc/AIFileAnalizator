import React, { useEffect, useRef, useState } from 'react';
import './OllamaChat.css';
import { API_BASE } from './config';

function OllamaChat() {
  const [prompt, setPrompt] = useState('');
  const [messages, setMessages] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const chatEndRef = useRef(null);

  const scrollToBottom = () => {
    chatEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const sendPrompt = async () => {
    if (!prompt.trim()) return;

    const userMessage = { role: 'user', content: prompt };
    const updatedMessages = [...messages, userMessage];
    setMessages(updatedMessages);
    setLoading(true);
    setError('');

    try {
      const res = await fetch(`${API_BASE}/chat/ask`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          messages: updatedMessages,
        }),
      });

      if (!res.ok) {
        const msg = await res.text();
        throw new Error(msg || 'Ошибка от API');
      }

      const data = await res.json();
      const aiMessage = data.response;
      setMessages((prev) => [...prev, aiMessage]);
    } catch (err) {
      console.error(err);
      setError('Ошибка: ' + (err.response || 'Что-то пошло не так.'));
    } finally {
      setLoading(false);
      setPrompt('');
    }
  };

  const handleKeyPress = (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      sendPrompt();
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
            <div className="icon">{msg.role === 'user' ? '👤' : '🤖'}</div>
            <div className="content">
              <strong>{msg.role === 'user' ? 'Вы:' : 'AI:'}</strong>{' '}
              {msg.content}
            </div>
          </div>
        ))}

        {loading && (
          <div className="chat-bubble response">
            <div className="icon">🤖</div>
            <div className="content">
              <em>AI печатает...</em>
            </div>
          </div>
        )}

        <div ref={chatEndRef} />
      </div>

      <div className="input-area">
        <textarea
          value={prompt}
          onChange={(e) => setPrompt(e.target.value)}
          onKeyPress={handleKeyPress}
          placeholder="Введите сообщение... (Enter для отправки)"
          disabled={loading}
        />
        <button onClick={sendPrompt} disabled={loading || !prompt.trim()}>
          📤 Отправить
        </button>
      </div>

      {error && <div className="error-message">{error}</div>}
    </div>
  );
}

export default OllamaChat;
