using System;
using System.ComponentModel.DataAnnotations;

namespace CompHomp.Models
{
    /// <summary>
    /// Модель для хранения комментариев к сборкам
    /// </summary>
    public class BuildComment
    {
        /// <summary>
        /// Уникальный идентификатор комментария
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор сборки, к которой относится комментарий
        /// </summary>
        public int BuildId { get; set; }

        /// <summary>
        /// Идентификатор пользователя, оставившего комментарий
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Текст комментария
        /// </summary>
        [Required(ErrorMessage = "Текст комментария обязателен")]
        public string Content { get; set; }

        /// <summary>
        /// Дата создания комментария
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Дата последнего изменения комментария
        /// </summary>
        public DateTime? ModifiedDate { get; set; }

        // Навигационные свойства
        /// <summary>
        /// Связанная сборка
        /// </summary>
        public virtual Build Build { get; set; }

        /// <summary>
        /// Пользователь, оставивший комментарий
        /// </summary>
        public virtual User User { get; set; }

        public BuildComment()
        {
            CreatedDate = DateTime.Now;
        }
    }
}
