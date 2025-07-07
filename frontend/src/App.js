import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Chat from './pages/Chat';
import RagChat from './components/RagChat';
import Header from './components/Header';

function App() {
  return (
    <Router>
      <Header />
      <Routes>
        <Route path="/" element={<Chat />} />
        <Route path="Rag" element={<RagChat />} />
      </Routes>
    </Router>
  );
}

export default App;
