# QuizPlatform API

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp&logoColor=white)](https://docs.microsoft.com/dotnet/csharp/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Tests](https://img.shields.io/badge/Tests-25%20passed-brightgreen?logo=xunit)](https://xunit.net/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**REST API для создания и прохождения квизов** — полноценное веб-приложение для портфолио C# разработчика.

---

# Описание

QuizPlatform — это платформа, где пользователи могут:
- Регистрироваться и входить в систему (JWT аутентификация)
- Создавать собственные квизы с вопросами и вариантами ответов
- Проходить квизы других пользователей
- Получать статистику результатов
- Просматривать свои достижения и историю прохождений

Проект на .NET с соблюдением: Clean Architecture, Unit-тестирование, безопасность, документация.

---

# Технологический стек

| Категория               | Технологии                        |
|-------------------------|-----------------------------------|
| **Язык**                | C# 12.0                           |
| **Фреймворк**           | .NET 8, ASP.NET Core Web API      |
| **База данных**         | PostgreSQL 15                     |
| **ORM**                 | Entity Framework Core 8           |
| **Аутентификация**      | JWT Bearer Tokens, BCrypt         |
| **Тестирование**        | xUnit, Moq, FluentAssertions      |
| **Документация**        | Swagger/OpenAPI                   |
| **Валидация**           | FluentValidation, DataAnnotations |
| **Маппинг**             | AutoMapper                        |
| **Логирование**         | Serilog                           |

---

# Архитектура проекта

Client            │ (Swagger/Postman)               │
Controllers Layer │ (Auth, Quiz, QuizSubmission)    │
Services Layer    │ (AuthService, QuizService, JWT) │
Repository Layer  │ (AppDbContext + EF Core)        │
Database          │ PostgreSQL                      │

---

# Структура решения

QuizPlatform/
├── src/
│ ├── QuizPlatform.API/ # Контроллеры, Program.cs, настройки
│ ├── QuizPlatform.Domain/ # Сущности (Entities, Enums)
│ └── QuizPlatform.DTO/ # DTO (Requests, Responses)
├── tests/
│ └── QuizPlatform.Tests/ # Unit-тесты (xUnit + Moq)
├── README.md
└── QuizPlatform.sln

---

# Быстрый старт

# Требования

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 15+](https://www.postgresql.org/download/)
- [Git](https://git-scm.com/)

# Установка

``bash
# 1. Клонируй репозиторий
git clone https://github.com/Yaros15/QuizPlatform.git
cd QuizPlatform

# 2. Настрой подключение к БД
# Отредактируйте src/QuizPlatform.API/appsettings.json
# Укажите свои данные PostgreSQL:
#   "ConnectionStrings": {
#     "DefaultConnection": "Host=localhost;Port=5432;Database=quizdb;Username=postgres;Password=Ваш_Пароль"
#   }

# 3. Создайте базу данных
createdb quizdb

# 4. Примените миграции
dotnet ef database update --project src/QuizPlatform.API

# 5. Запустите приложение
dotnet run --project src/QuizPlatform.API

# 6. Проверка работы
# Открой в браузере: https://localhost:7055/swagger

---

# API Документация
Метод  | Эндпоинт                        | Описание                              | Auth
POST   | /api/auth/register              | Регистрация нового пользователя       | Нет
POST   | /api/auth/login                 | Вход и получение JWT токена           | Нет
GET    | /api/profile/me                 | Получить данные текущего пользователя | Да
GET    | /api/quizzes                    | Получить список всех квизов           | Нет
POST   | /api/quizzes                    | Создать новый квиз                    | Да
GET    | /api/quizzes/{id}               | Получить квиз по ID                   | Нет
PUT    | /api/quizzes/{id}               | Обновить квиз (только автор)          | Да
DELETE | /api/quizzes/{id}               | Удалить квиз (только автор)           | Да
POST   | /api/quizsubmission/{id}/submit | Пройти квиз                           | Да
GET    | /api/quizsubmission/me/results  | Мои результаты                        | Да
GET    | /api/quizsubmission/{id}/stats  | Статистика квиза (автор)              | Да

---

# Запуск тестов

# Запустить все тесты
dotnet test

# Запустить с подробным выводом
dotnet test --verbosity normal

# Запустить конкретный тест
dotnet test --filter "FullyQualifiedName~AuthServiceTests"

# Запустить с покрытием кода
dotnet test --collect:"XPlat Code Coverage"

---

# Покрытие тестами

Сервис                | Тестов
AuthService           | 7
QuizService           | 10
QuizSubmissionService | 8
Всего                 | 25

---

# Безопасность

JWT токены для аутентификации (срок действия 24 часа)
BCrypt для хеширования паролей
Ролевая модель (User, Admin)
Защита от несанкционированного доступа (только автор может редактировать/удалять свой квиз)
Валидация входных данных (FluentValidation + DataAnnotations)

---

# Ключевые особенности реализации

Функция              | Реализация
Clean Architecture   | Разделение на Controller → Service → Repository
Dependency Injection | Внедрение зависимостей через встроенный DI контейнер
Repository Pattern   | EF Core DbContext как репозиторий
DTO Pattern          | Разделение на Request/Response DTO
Unit Testing         | Изолированные тесты с Moq и In-Memory БД
Error Handling       | Глобальная обработка исключений
CORS                 | Настроено для кросс-доменных запросов

---

# Автор
Ярослав
C# / .NET Developer
