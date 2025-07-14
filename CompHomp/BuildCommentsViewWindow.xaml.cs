using System.Linq;
using System.Windows;
using CompHomp.Data;
using Microsoft.EntityFrameworkCore;
using CompHomp.ViewModels;

namespace CompHomp
{
    public partial class BuildCommentsViewWindow : Window
    {
        public BuildCommentsViewWindow(int buildId, AppDbContext context)
        {
            InitializeComponent();

            var comments = context.BuildComments
                .Include(c => c.User)
                .Where(c => c.BuildId == buildId)
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => new BuildCommentViewModel
                {
                    Content = c.Content,
                    CreatedDate = c.CreatedDate,
                    AuthorName = c.User.Login
                })
                .ToList();

            CommentsItemsControl.ItemsSource = comments;
        }
    }
}
