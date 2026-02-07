namespace Nizhegorodcev_Task2_Severstal.Models
{
    /// <summary>
    /// Рулон металла на складе.
    /// Инварианты:
    /// 1. Длина и вес должны быть положительными
    /// 2. Нельзя удалить уже удаленный рулон
    /// 3. Нельзя изменить удаленный рулон
    /// </summary>
    public class MetalRoll : Entity
    {
        private decimal _length;
        private decimal _weight;

        public int Id { get; private set; }

        /// <summary>
        /// Длина рулона (метры)
        /// </summary>
        /// <exception cref="DomainException">При попытке установить неположительное значение</exception>
        public decimal Length
        {
            get => _length;
            private set
            {
                if (value <= 0)
                    throw new DomainException("Длина рулона должна быть положительной", nameof(Length));
                _length = Math.Round(value, 2, MidpointRounding.AwayFromZero);
            }
        }

        /// <summary>
        /// Вес рулона (тонны)
        /// </summary>
        /// <exception cref="DomainException">При попытке установить неположительное значение</exception>
        public decimal Weight
        {
            get => _weight;
            private set
            {
                if (value <= 0)
                    throw new DomainException("Вес рулона должен быть положительным", nameof(Weight));
                _weight = Math.Round(value, 2, MidpointRounding.AwayFromZero);
            }
        }

        public DateTime AddedDate { get; private set; }
        public DateTime? DeletedDate { get; private set; }

        public bool IsDeleted => DeletedDate.HasValue;

        // Для EF Core
        protected MetalRoll() { }

        /// <summary>
        /// Создание нового рулона
        /// </summary>
        /// <param name="length">Длина (м), > 0</param>
        /// <param name="weight">Вес (т), > 0</param>
        public MetalRoll(decimal length, decimal weight)
        {
            Length = length;
            Weight = weight;
            AddedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Обновление параметров рулона
        /// </summary>
        /// <exception cref="DomainException">При попытке изменить удаленный рулон</exception>
        public void Update(decimal length, decimal weight)
        {
            if (IsDeleted)
                throw new DomainException("Нельзя изменить удаленный рулон", nameof(Update));

            Length = length;
            Weight = weight;
        }

        /// <summary>
        /// Мягкое удаление рулона
        /// </summary>
        /// <exception cref="DomainException">При попытке удалить уже удаленный рулон</exception>
        public void MarkAsDeleted()
        {
            if (IsDeleted)
                throw new DomainException("Рулон уже удален", nameof(MarkAsDeleted));

            DeletedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Восстановление удаленного рулона
        /// </summary>
        /// <exception cref="DomainException">При попытке восстановить неудаленный рулон</exception>
        public void Restore()
        {
            if (!IsDeleted)
                throw new DomainException("Рулон не был удален", nameof(Restore));

            DeletedDate = null;
        }
    }

    /// <summary>
    /// Базовый класс для всех сущностей
    /// </summary>
    public abstract class Entity
    {
        protected static void Validate<T>(T value, Func<T, bool> validator, string message, string paramName)
        {
            if (!validator(value))
                throw new DomainException(message, paramName);
        }
    }

    /// <summary>
    /// Исключение предметной области
    /// </summary>
    public class DomainException : Exception
    {
        public string ParameterName { get; }

        public DomainException(string message, string parameterName) : base(message)
        {
            ParameterName = parameterName;
        }

        public DomainException(string message, string parameterName, Exception innerException)
            : base(message, innerException)
        {
            ParameterName = parameterName;
        }
    }
}
