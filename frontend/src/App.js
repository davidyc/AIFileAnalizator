import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Chat from './pages/Chat';
import RagGenerate from './components/RagGenerate';
import OllamaGenerate from './components/OllamaGenerate';
import Header from './components/Header';

function App() {
  return (
    <Router>
      <Header />
      <Routes>
        <Route path="/" element={<Chat />} />
        <Route path="/generate" element={<OllamaGenerate />} />
        <Route path="Rag" element={<RagGenerate />} />
      </Routes>
    </Router>
  );
}

export default App;
