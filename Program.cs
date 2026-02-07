using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Nizhegorodcev_Task2_Severstal.Data;
using Nizhegorodcev_Task2_Severstal.Models;
using Nizhegorodcev_Task2_Severstal.Repositories;
using Npgsql;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ========== НАСТРОЙКА ПОДКЛЮЧЕНИЯ К БАЗЕ ДАННЫХ ==========
Console.WriteLine("🔧 Настройка подключения к базе данных...");

var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");

if (string.IsNullOrEmpty(connectionString))
{
    // Не используем хардкод пароля!
    throw new InvalidOperationException(
        "Строка подключения 'PostgresConnection' не найдена в конфигурации.\n" +
        "Добавьте её в appsettings.json или установите переменную окружения."
    );
}

// Проверяем переменную окружения DB_PASSWORD
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
if (!string.IsNullOrEmpty(dbPassword))
{
    try
    {
        // Безопасно заменяем пароль через NpgsqlConnectionStringBuilder
        var csBuilder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Password = dbPassword
        };
        connectionString = csBuilder.ConnectionString;

        // Маскируем пароль для вывода в лог
        var safeString = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Password = "*****"
        }.ConnectionString;

        Console.WriteLine($"✅ Используется пароль из переменной окружения DB_PASSWORD");
        Console.WriteLine($"   Строка подключения: {safeString}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Ошибка обработки строки подключения: {ex.Message}");
        Console.WriteLine($"   Используется оригинальная строка из конфигурации");
    }
}
else
{
    Console.WriteLine("ℹ️  Переменная DB_PASSWORD не найдена");
    Console.WriteLine("   Используется пароль из конфигурации (appsettings.json)");

    // Маскируем пароль для вывода
    try
    {
        var csBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        var safeString = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Password = "*****"
        }.ConnectionString;
        Console.WriteLine($"   Строка подключения: {safeString}");
    }
    catch
    {
        Console.WriteLine($"   Строка подключения: {connectionString?.Replace("Password=", "Password=****")}");
    }
}

// Регистрируем DbContext
try
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));

    Console.WriteLine("✅ DbContext успешно зарегистрирован");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Ошибка регистрации DbContext: {ex.Message}");
    throw;
}

// ========== НАСТРОЙКА СЕРВИСОВ ==========
Console.WriteLine("\n🔧 Настройка сервисов приложения...");

// Настройка контроллеров с обработкой JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Severstal Metal Rolls API",
        Version = "v1",
        Description = "API для управления складом рулонов металла компании Северсталь",
        Contact = new OpenApiContact
        {
            Name = "Северсталь",
            Url = new Uri("https://www.severstal.com")
        }
    });
});

// Регистрация репозитория
builder.Services.AddScoped<IRollRepository, RollRepository>();

// Добавляем логирование
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// ========== НАСТРОЙКА КОНВЕЙЕРА ЗАПРОСОВ ==========
Console.WriteLine("\n🔧 Настройка конвейера запросов...");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Severstal API v1");
        c.DisplayRequestDuration();
        c.EnableFilter();
        c.DefaultModelExpandDepth(2);
    });

    Console.WriteLine("✅ Swagger включен для среды разработки");
}
else
{
    Console.WriteLine("ℹ️  Swagger отключен (не в режиме разработки)");
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ========== ИНИЦИАЛИЗАЦИЯ БАЗЫ ДАННЫХ ==========
Console.WriteLine("\n🔧 Инициализация базы данных...");

try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Проверяем подключение к базе данных
    Console.WriteLine("Проверка подключения к PostgreSQL...");
    var canConnect = await dbContext.Database.CanConnectAsync();

    if (canConnect)
    {
        Console.WriteLine("✅ Подключение к PostgreSQL успешно установлено");

        // Создаем базу и таблицы (если их нет)
        await dbContext.Database.EnsureCreatedAsync();
        Console.WriteLine("✅ База данных готова к работе");

        // Добавляем тестовые данные, если таблица пуста
        if (!dbContext.Rolls.Any())
        {
            Console.WriteLine("Добавление тестовых данных...");
            var testRolls = new List<MetalRoll>
            {
                new MetalRoll(10.5m, 2.3m),
                new MetalRoll(8.2m, 1.8m),
                new MetalRoll(12.7m, 3.1m)
            };

            await dbContext.Rolls.AddRangeAsync(testRolls);
            await dbContext.SaveChangesAsync();
            Console.WriteLine($"✅ Добавлено {testRolls.Count} тестовых рулонов");
        }
        else
        {
            var rollCount = await dbContext.Rolls.CountAsync();
            Console.WriteLine($"ℹ️  В базе данных уже есть {rollCount} рулонов");
        }
    }
    else
    {
        Console.WriteLine("❌ Не удалось подключиться к PostgreSQL");
        Console.WriteLine("   Проверьте:");
        Console.WriteLine("   1. Запущен ли сервер PostgreSQL");
        Console.WriteLine("   2. Правильность строки подключения");
        Console.WriteLine("   3. Существует ли база данных 'metalrolls'");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️  Ошибка инициализации БД: {ex.Message}");

    if (ex.InnerException != null)
    {
        Console.WriteLine($"   Внутренняя ошибка: {ex.InnerException.Message}");
    }

    Console.WriteLine("\n📋 Для настройки подключения:");
    Console.WriteLine("   1. Установите PostgreSQL и создайте базу 'metalrolls'");
    Console.WriteLine("   2. Настройте строку подключения в appsettings.json");
    Console.WriteLine("   3. Или установите переменную окружения DB_PASSWORD");
    Console.WriteLine("      Пример: export DB_PASSWORD='ваш_пароль'");

    // Продолжаем работу в демо-режиме
    Console.WriteLine("\n⚠️  Продолжаем без базы данных (режим демонстрации)");
}

// ========== ЗАПУСК ПРИЛОЖЕНИЯ ==========
Console.WriteLine("\n" + new string('=', 50));
Console.WriteLine("🚀 Severstal Metal Rolls API запущен!");
Console.WriteLine($"📅 {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
Console.WriteLine($"🌍 Среда: {app.Environment.EnvironmentName}");
Console.WriteLine($"🔗 Swagger: {app.Urls.FirstOrDefault()}/swagger");
Console.WriteLine(new string('=', 50));

app.Run();