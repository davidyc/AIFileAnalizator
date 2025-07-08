import React, { useState, useRef } from 'react';
import { API_BASE } from './config';

const RagGenerate = () => {
  const [text, setText] = useState('');
  const [chunkId, setChunkId] = useState('');
  const [messages, setMessages] = useState([]);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [askMode, setAskMode] = useState(true);
  const fileInputRef = useRef(null);

  const handleSend = async () => {
    if (!text.trim()) return;
    setError('');
    setLoading(true);

    if (askMode) {
      const userMessage = { type: 'user', text };
      setMessages((prev) => [...prev, userMessage]);

      try {
        const res = await fetch(`${API_BASE}/rag/generate/ask`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ prompt: text }),
        });

        if (!res.ok) throw new Error('Ошибка при запросе');

        const data = await res.json();
        setMessages((prev) => [
          ...prev,
          { type: 'response', text: data.response },
        ]);
      } catch (err) {
        setError(err.message || 'Ошибка');
      }
    } else {
      try {
        const res = await fetch(`${API_BASE}/rag/generate/chunk`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ text, id: chunkId || null }),
        });

        if (!res.ok) throw new Error('Ошибка при загрузке чанка');
        setMessages((prev) => [
          ...prev,
          { type: 'response', text: '✅ Чанк успешно загружен' },
        ]);
        setChunkId('');
      } catch (err) {
        setError(err.message || 'Ошибка');
      }
    }

    setText('');
    setLoading(false);
  };

  const handleFileUpload = async () => {
    const file = fileInputRef.current?.files?.[0];
    if (!file) return;

    setMessages((prev) => [
      ...prev,
      { type: 'user', text: `📎 Загружается файл: ${file.name}` },
    ]);

    setLoading(true);
    try {
      const formData = new FormData();
      formData.append('file', file);

      const res = await fetch(`${API_BASE}/rag/generate/upload`, {
        method: 'POST',
        body: formData,
      });

      if (!res.ok) throw new Error('Ошибка при загрузке файла');

      const data = await res.json();
      setMessages((prev) => [
        ...prev,
        { type: 'response', text: data.message || '✅ Файл загружен' },
      ]);
    } catch (err) {
      setError(err.message || 'Ошибка');
    } finally {
      setLoading(false);
      if (fileInputRef.current) fileInputRef.current.value = '';
    }
  };

  return (
    <div className="chat-container">
      <h2>🤖 Ollama Generate RAG</h2>
      <div className="chat-window">
        {messages.map((msg, index) => (
          <div
            key={index}
            className={`chat-bubble ${
              msg.type === 'user' ? 'user' : 'response'
            }`}
          >
            {msg.text}
          </div>
        ))}
        {loading && (
          <div className="chat-bubble response">
            <em>AI печатает...</em>
          </div>
        )}
      </div>

      <div className="input-area">
        <textarea
          placeholder={
            askMode ? 'Введите вопрос...' : 'Введите текст для загрузки...'
          }
          value={text}
          onChange={(e) => setText(e.target.value)}
          disabled={loading}
        />
        <button onClick={handleSend} disabled={loading || !text.trim()}>
          {askMode ? 'Спросить' : 'Загрузить'}
        </button>

        {/* Кнопка 📎 только в режиме загрузки */}
        {!askMode && (
          <>
            <button
              onClick={() => fileInputRef.current?.click()}
              disabled={loading}
            >
              📎 Файл
            </button>
            <input
              type="file"
              ref={fileInputRef}
              style={{ display: 'none' }}
              onChange={handleFileUpload}
            />
          </>
        )}
      </div>

      {!askMode && (
        <input
          type="text"
          placeholder="Необязательный ID"
          value={chunkId}
          onChange={(e) => setChunkId(e.target.value)}
          style={{
            marginTop: '0.5rem',
            width: '100%',
            padding: '0.5rem',
            borderRadius: '8px',
            border: '1px solid #ccc',
          }}
        />
      )}

      <div style={{ marginTop: '1rem' }}>
        <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
          <input
            type="checkbox"
            checked={askMode}
            onChange={() => setAskMode(!askMode)}
          />
          {askMode ? 'Режим: Вопрос' : 'Режим: Загрузка'}
        </label>
      </div>

      {error && <div className="error-message">{error}</div>}
    </div>
  );
};

export default RagGenerate;
