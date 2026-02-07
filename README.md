# Severstal Metal Rolls Warehouse API
Backend API для управления складом рулонов металла.  
Разработано в рамках задания.
## Быстрый старт
### Предварительные требования
- [.NET 7.0 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) или [VS Code](https://code.visualstudio.com/)
### Установка и запуск
1. Клонируйте репозиторий
   ```bash
   git clone https://github.com/Nizegorodcev/Nizhegorodcev_Task2_Severstal.git
   cd Nizhegorodcev_Task2_Severstal
   ```

2. Настройка базы данных

	В PostgreSQL создайте базу данных:
	```sql
	CREATE DATABASE metalrolls;
	```

3. Настройка подключения

	3.1 Через appsettings.json
	```json
		{
		  "ConnectionStrings": {
			"PostgresConnection": "Host=localhost;Port=5432;Database=metalrolls;Username=postgres;Password=ваш_пароль"
		  }
		}
	```

	3.2 Через переменную окружения
	```bash
		# Windows (PowerShell)	
		$env:DB_PASSWORD='ваш_пароль'
		# Windows (CMD)
		set DB_PASSWORD=ваш_пароль
		# Linux/macOS
		export DB_PASSWORD='ваш_пароль'
	```

    3.3 Через Visual Studio

	- Откройте проект в Visual Studio

    - Правой кнопкой по проекту → Свойства → Отладка

    - Добавьте переменную окружения: DB_PASSWORD = ваш_пароль

    - Сохраните изменения

4. Запустите приложение
```bash	
	dotnet restore
	dotnet run
```
5. Откройте Swagger UI

После запуска перейдите по адресу: https://localhost:7269/swagger