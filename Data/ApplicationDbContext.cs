
using global::Nizhegorodcev_Task2_Severstal.Models;
using Microsoft.EntityFrameworkCore;
using Nizhegorodcev_Task2_Severstal.Models;

namespace Nizhegorodcev_Task2_Severstal.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<MetalRoll> Rolls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MetalRoll>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Id).IsUnique();

                // Вариант 1: Простая конфигурация (если не работает с приватными полями)
                entity.Property(e => e.Length)
                    .IsRequired()
                    .HasColumnType("numeric(10,2)")
                    .HasPrecision(10, 2);

                entity.Property(e => e.Weight)
                    .IsRequired()
                    .HasColumnType("numeric(10,2)")
                    .HasPrecision(10, 2);

                entity.Property(e => e.AddedDate)
                    .IsRequired()
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.DeletedDate)
                    .HasColumnType("timestamp with time zone");

                // Индексы
                entity.HasIndex(e => e.AddedDate);
                entity.HasIndex(e => e.DeletedDate);

                // Комментарий: EF Core должен использовать приватный конструктор автоматически
                // если есть конструктор без параметров (у нас protected MetalRoll() { })
            });
        }

        // Метод для упрощенного создания БД
        public async Task InitializeDatabaseAsync()
        {
            // Создаем БД, если её нет
            await Database.EnsureCreatedAsync();

            // Добавляем тестовые данные, если таблица пуста
            if (!Rolls.Any())
            {
                var testRolls = new List<MetalRoll>
            {
                new MetalRoll(10.5m, 2.3m),
                new MetalRoll(8.2m, 1.8m),
                new MetalRoll(12.7m, 3.1m)
            };

                // Для обхода приватных сеттеров используем конструктор
                await Rolls.AddRangeAsync(testRolls);
                await SaveChangesAsync();
            }
        }
    }
}
