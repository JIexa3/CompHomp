using System;
using System.ComponentModel.DataAnnotations;

namespace CompHomp.Models
{
    /// <summary>
    /// Модель для хранения оценок сборок
    /// </summary>
    public class BuildRating
    {
        /// <summary>
        /// Уникальный идентификатор оценки
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор оцениваемой сборки
        /// </summary>
        public int BuildId { get; set; }

        /// <summary>
        /// Идентификатор пользователя, оставившего оценку
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Тип оценки (лайк/дизлайк)
        /// </summary>
        public bool IsLike { get; set; }

        /// <summary>
        /// Дата создания оценки
        /// </summary>
        public DateTime CreatedDate { get; set; }

        // Навигационные свойства
        /// <summary>
        /// Связанная сборка
        /// </summary>
        public virtual Build Build { get; set; }

        /// <summary>
        /// Пользователь, оставивший оценку
        /// </summary>
        public virtual User User { get; set; }

        public BuildRating()
        {
            CreatedDate = DateTime.Now;
        }
    }
}
