using System;
using System.Windows;
using CompHomp.Data;
using CompHomp.Models;

namespace CompHomp
{
    public partial class BuildCommentWindow : Window
    {
        private readonly int _buildId;
        private readonly AppDbContext _context;
        private readonly User _currentUser;

        public BuildCommentWindow(int buildId, AppDbContext context, User currentUser)
        {
            InitializeComponent();
            _buildId = buildId;
            _context = context;
            _currentUser = currentUser;
        }

        private void AddComment_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CommentTextBox.Text))
                return;

            var comment = new BuildComment
            {
                BuildId = _buildId,
                Content = CommentTextBox.Text,
                CreatedDate = DateTime.Now,
                UserId = _currentUser.Id
            };

            _context.BuildComments.Add(comment);
            _context.SaveChanges();

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
