using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using CompHomp.Models;
using CompHomp.Data;

namespace CompHomp.Services
{
    /// <summary>
    /// Сервис для работы с лайками и комментариями сборок
    /// </summary>
    public class BuildRatingService
    {
        private readonly AppDbContext _context;

        public BuildRatingService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Поставить или обновить лайк/дизлайк сборки
        /// </summary>
        public async Task RateBuildAsync(int buildId, int userId, bool isLike)
        {
            var existingRating = await _context.BuildRatings
                .FirstOrDefaultAsync(r => r.BuildId == buildId && r.UserId == userId);

            if (existingRating != null)
            {
                if (existingRating.IsLike == isLike)
                {
                    _context.BuildRatings.Remove(existingRating);
                }
                else
                {
                    existingRating.IsLike = isLike;
                }
            }
            else
            {
                _context.BuildRatings.Add(new BuildRating
                {
                    BuildId = buildId,
                    UserId = userId,
                    IsLike = isLike,
                    CreatedDate = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Получить количество лайков сборки
        /// </summary>
        public async Task<int> GetBuildLikesCountAsync(int buildId)
        {
            return await _context.BuildRatings
                .CountAsync(r => r.BuildId == buildId && r.IsLike);
        }

        /// <summary>
        /// Получить количество дизлайков сборки
        /// </summary>
        public async Task<int> GetBuildDislikesCountAsync(int buildId)
        {
            return await _context.BuildRatings
                .CountAsync(r => r.BuildId == buildId && !r.IsLike);
        }

        /// <summary>
        /// Проверить, поставил ли пользователь лайк сборке
        /// </summary>
        public async Task<bool?> GetUserRatingAsync(int buildId, int userId)
        {
            var rating = await _context.BuildRatings
                .FirstOrDefaultAsync(r => r.BuildId == buildId && r.UserId == userId);
            return rating?.IsLike;
        }

        /// <summary>
        /// Получить топ сборок по лайкам
        /// </summary>
        public async Task<List<Build>> GetTopRatedBuildsAsync(int limit = 10)
        {
            var topBuilds = await _context.Builds
                .Include(b => b.BuildRatings)
                .Where(b => b.BuildRatings.Any())
                .Select(b => new
                {
                    Build = b,
                    LikesCount = b.BuildRatings.Count(r => r.IsLike),
                    DislikesCount = b.BuildRatings.Count(r => !r.IsLike)
                })
                .OrderByDescending(x => x.LikesCount - x.DislikesCount)
                .Take(limit)
                .Select(x => x.Build)
                .ToListAsync();

            return topBuilds;
        }

        /// <summary>
        /// Добавить комментарий к сборке
        /// </summary>
        public async Task AddCommentAsync(int buildId, int userId, string content)
        {
            var comment = new BuildComment
            {
                BuildId = buildId,
                UserId = userId,
                Content = content,
                CreatedDate = DateTime.Now
            };

            _context.BuildComments.Add(comment);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Обновить комментарий
        /// </summary>
        public async Task<BuildComment> UpdateCommentAsync(int commentId, string content)
        {
            var comment = await _context.BuildComments.FindAsync(commentId);
            if (comment == null)
                throw new KeyNotFoundException("Комментарий не найден");

            comment.Content = content;
            comment.ModifiedDate = DateTime.Now;
            
            _context.BuildComments.Update(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        /// <summary>
        /// Удалить комментарий
        /// </summary>
        public async Task DeleteCommentAsync(int commentId)
        {
            var comment = await _context.BuildComments.FindAsync(commentId);
            if (comment != null)
            {
                _context.BuildComments.Remove(comment);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Получить все комментарии к сборке
        /// </summary>
        public async Task<List<BuildComment>> GetBuildCommentsAsync(int buildId)
        {
            return await _context.BuildComments
                .Include(c => c.User)
                .Where(c => c.BuildId == buildId)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }
    }
}
