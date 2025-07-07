# File loader

📥 Установка Ollama
Скачай и установи с официального сайта:
👉 https://ollama.com/download

📦 Загрузка моделей
Выполни следующие команды в терминале (после установки Ollama):

ollama pull nomic-embed-text
ollama pull llama3

Эти модели будут использоваться для:

nomic-embed-text — генерация эмбеддингов
llama3 — генерация ответов на основе контекста

так же перед запуском в папке frontend
npm install

🚀 Запуск проекта
В корне решения запусти:

dotnet run --project AIFileAnalizatorAspire
Это запустит Aspire AppHost, который поднимет:

✅ API-проект (AIFileAnalizator.Api)

✅ Qdrant (в Docker-контейнере)

✅ Frontend на React (http://localhost:3000)

🐳 Что поднимается в Docker
Проект использует Docker-контейнеры, поэтому обязательно убедись, что Docker Desktop запущен.
