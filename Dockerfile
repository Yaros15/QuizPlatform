# ============================================================
# QuizPlatform API - Dockerfile
# ============================================================
# Многоэтапная сборка для минимального размера образа
# ============================================================

# --- Этап 1: Сборка приложения ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файлы проектов и восстанавливаем зависимости
# Это позволяет использовать кэш Docker и не скачивать пакеты при каждом изменении кода
COPY src/QuizPlatform.API/QuizPlatform.API.csproj ./src/QuizPlatform.API/
COPY src/QuizPlatform.Domain/QuizPlatform.Domain.csproj ./src/QuizPlatform.Domain/
COPY src/QuizPlatform.DTO/QuizPlatform.DTO.csproj ./src/QuizPlatform.DTO/
COPY tests/QuizPlatform.Tests/QuizPlatform.Tests.csproj ./tests/QuizPlatform.Tests/

RUN dotnet restore ./src/QuizPlatform.API/QuizPlatform.API.csproj

# Копируем весь исходный код и собираем проект
COPY . .

WORKDIR /src/src/QuizPlatform.API
RUN dotnet build "./QuizPlatform.API.csproj" -c Release -o /app/build

# --- Этап 2: Публикация ---
FROM build AS publish
RUN dotnet publish "./QuizPlatform.API.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# --- Этап 3: Рантайм (минимальный образ) ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Создаём пользователя для безопасности (не запускаем от root)
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Копируем опубликованное приложение из этапа сборки
COPY --from=publish /app/publish .

# Открываем порт приложения
EXPOSE 8080
EXPOSE 8081

# Настраиваем переменные окружения
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Запуск приложения
ENTRYPOINT ["dotnet", "QuizPlatform.API.dll"]