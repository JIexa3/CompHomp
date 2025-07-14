using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using Microsoft.EntityFrameworkCore;
using CompHomp.Models;
using CompHomp.Services;
using CompHomp.Data;
using CompHomp.ViewModels;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CompHomp
{
    public partial class MainUserWindow : Window
    {
        private readonly AppDbContext _context;
        private readonly User _currentUser;
        private readonly BuildRatingService _buildRatingService;
        private bool _showMyBuilds;

        public MainUserWindow(User user)
        {
            InitializeComponent();
            
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));
            
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(
                "Data Source=HOME-PC;Initial Catalog=CompHompDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False"
            );

            _context = new AppDbContext(optionsBuilder.Options);
            _buildRatingService = new BuildRatingService(_context);
            
            Loaded += MainUserWindow_Loaded;
            InitializeEventHandlers();
        }

        private void MainUserWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBuilds(false);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private async void LoadBuilds(bool myBuildsOnly)
        {
            try
            {
                if (_context == null)
                {
                    Debug.WriteLine("_context is null");
                    return;
                }

                var buildsQuery = _context.Builds.AsQueryable();

                if (buildsQuery == null)
                {
                    Debug.WriteLine("buildsQuery is null");
                    return;
                }

                buildsQuery = buildsQuery
                    .Include(b => b.BuildRatings)
                    .Include(b => b.BuildComments)
                    .Include(b => b.User)
                    .Include(b => b.Cpu)
                    .Include(b => b.Gpu)
                    .Include(b => b.Motherboard)
                    .Include(b => b.Ram)
                    .Include(b => b.Storage)
                    .Include(b => b.Psu)
                    .Include(b => b.Case)
                    .Where(b => b.Status == BuildStatus.Approved);

                if (myBuildsOnly && _currentUser != null)
                {
                    buildsQuery = buildsQuery.Where(b => b.UserId == _currentUser.Id);
                }

                var builds = await buildsQuery
                    .Select(b => new BuildViewModel
                    {
                        Id = b.Id,
                        Name = b.Name,
                        Description = b.Description,
                        ImagePath = b.Image,
                        LikesCount = b.BuildRatings.Count(r => r.IsLike),
                        DislikesCount = b.BuildRatings.Count(r => !r.IsLike),
                        AuthorName = b.User != null ? b.User.Login : "Автор не указан",
                        Comments = b.BuildComments
                            .OrderByDescending(c => c.CreatedDate)
                            .Select(c => new BuildCommentViewModel
                            {
                                Content = c.Content,
                                CreatedDate = c.CreatedDate,
                                AuthorName = c.User != null ? c.User.Login : "Автор не указан"
                            }).ToList(),
                        IsLiked = b.BuildRatings.Any(r => r.UserId == _currentUser.Id && r.IsLike),
                        IsDisliked = b.BuildRatings.Any(r => r.UserId == _currentUser.Id && !r.IsLike)
                    })
                    .ToListAsync();

                // Пересчитываем цены для каждой сборки
                foreach (var buildViewModel in builds)
                {
                    var build = await _context.Builds
                        .Include(b => b.Cpu)
                        .Include(b => b.Gpu)
                        .Include(b => b.Motherboard)
                        .Include(b => b.Ram)
                        .Include(b => b.Storage)
                        .Include(b => b.Psu)
                        .Include(b => b.Case)
                        .FirstOrDefaultAsync(b => b.Id == buildViewModel.Id);

                    if (build != null)
                    {
                        build.CalculateTotalPrice();
                        buildViewModel.BasePrice = build.BasePrice;
                        buildViewModel.TotalPrice = build.TotalPrice;
                    }
                }

                // Сортируем сборки по рейтингу (лайки минус дизлайки)
                builds = builds
                    .OrderByDescending(b => b.LikesCount - b.DislikesCount) // Сначала по разнице лайков и дизлайков
                    .ThenByDescending(b => b.LikesCount) // Затем по количеству лайков
                    .ToList();

                BuildsPanel.Children.Clear();

                foreach (var build in builds)
                {
                    var buildControl = new Border
                    {
                        Margin = new Thickness(10),
                        Padding = new Thickness(10),
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D2D2D")),
                        CornerRadius = new CornerRadius(5),
                        Width = 300
                    };

                    var stackPanel = new StackPanel();

                    // Изображение сборки
                    var image = new Image
                    {
                        Height = 200,
                        Width = 280,
                        Stretch = Stretch.Uniform,
                        Margin = new Thickness(0, 0, 0, 10)
                    };

                    try
                    {
                        if (!string.IsNullOrEmpty(build.ImagePath) && System.IO.File.Exists(build.ImagePath))
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(build.ImagePath, UriKind.Absolute);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            image.Source = bitmap;
                        }
                        else
                        {
                            // Загружаем изображение из ресурсов
                            image.Source = new BitmapImage(new Uri("/CompHomp;component/res/computer.png", UriKind.Relative));
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Ошибка при загрузке изображения: {ex.Message}");
                        try
                        {
                            // Загружаем изображение из ресурсов при ошибке
                            image.Source = new BitmapImage(new Uri("/CompHomp;component/res/computer.png", UriKind.Relative));
                        }
                        catch (Exception resourceEx)
                        {
                            Debug.WriteLine($"Ошибка при загрузке ресурса: {resourceEx.Message}");
                        }
                    }

                    stackPanel.Children.Add(image);

                    // Название сборки
                    var nameTextBlock = new TextBlock
                    {
                        Text = build.Name,
                        Foreground = Brushes.White,
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 0, 0, 5)
                    };
                    stackPanel.Children.Add(nameTextBlock);

                    // Автор
                    var authorTextBlock = new TextBlock
                    {
                        Text = $"Автор: {build.AuthorName}",
                        Foreground = Brushes.LightGray,
                        Margin = new Thickness(0, 0, 0, 5)
                    };
                    stackPanel.Children.Add(authorTextBlock);

                    // Цена
                    var priceTextBlock = new TextBlock
                    {
                        Text = $"Цена: {build.TotalPrice:C}",
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FF00")),
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 5, 0, 10)
                    };
                    stackPanel.Children.Add(priceTextBlock);

                    // Лайки и дизлайки
                    var ratingPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(0, 0, 0, 10)
                    };

                    var likesButton = new Button
                    {
                        Style = (Style)FindResource("LikeButton"),
                        Tag = build,
                        Margin = new Thickness(0, 0, 10, 0),
                        DataContext = build
                    };

                    var likesStackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    var likeIcon = new TextBlock { Text = "👍", FontSize = 24 };
                    var likesCount = new TextBlock 
                    { 
                        FontSize = 18, 
                        Margin = new Thickness(5, 0, 0, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    likesCount.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("LikesCount") 
                    { 
                        UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged 
                    });
                    likesStackPanel.Children.Add(likeIcon);
                    likesStackPanel.Children.Add(likesCount);
                    likesButton.Content = likesStackPanel;
                    likesButton.Click += LikeButton_Click;

                    var dislikesButton = new Button
                    {
                        Style = (Style)FindResource("DislikeButton"),
                        Tag = build,
                        DataContext = build
                    };

                    var dislikesStackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    var dislikeIcon = new TextBlock { Text = "👎", FontSize = 24 };
                    var dislikesCount = new TextBlock 
                    { 
                        FontSize = 18, 
                        Margin = new Thickness(5, 0, 0, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    dislikesCount.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("DislikesCount") 
                    { 
                        UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged 
                    });
                    dislikesStackPanel.Children.Add(dislikeIcon);
                    dislikesStackPanel.Children.Add(dislikesCount);
                    dislikesButton.Content = dislikesStackPanel;
                    dislikesButton.Click += DislikeButton_Click;

                    ratingPanel.Children.Add(likesButton);
                    ratingPanel.Children.Add(dislikesButton);

                    var writeCommentButton = new Button
                    {
                        Style = (Style)FindResource("CommentButton"),
                        Tag = build,
                        DataContext = build,
                        Margin = new Thickness(8, 0, 0, 0)
                    };

                    var commentStackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    var commentIcon = new TextBlock { Text = "💬", FontSize = 24 };
                    var commentCount = new TextBlock 
                    { 
                        FontSize = 18, 
                        Margin = new Thickness(5, 0, 0, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    commentCount.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Comments.Count") 
                    { 
                        UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged 
                    });
                    commentStackPanel.Children.Add(commentIcon);
                    commentStackPanel.Children.Add(commentCount);
                    writeCommentButton.Content = commentStackPanel;
                    writeCommentButton.Click += WriteCommentButton_Click;

                    ratingPanel.Children.Add(writeCommentButton);
                    stackPanel.Children.Add(ratingPanel);

                    // Кнопки действий
                    var actionPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal
                    };

                    var detailsButton = new Button
                    {
                        Content = "Подробнее",
                        Style = (Style)FindResource("ActionButton"),
                        Margin = new Thickness(0, 0, 10, 0)
                    };
                    detailsButton.Click += (s, e) => ShowBuildDetails(build.Id);

                    var commentsButton = new Button
                    {
                        Content = "Комментарии",
                        Style = (Style)FindResource("ActionButton"),
                        Tag = build.Id
                    };
                    commentsButton.Click += CommentButton_Click;

                    var addToCartButton = new Button
                    {
                        Content = "Купить",
                        Style = (Style)FindResource("ActionButton"),
                        Tag = build
                    };
                    addToCartButton.Click += AddToCart_Click;

                    actionPanel.Children.Add(detailsButton);
                    actionPanel.Children.Add(commentsButton);
                    actionPanel.Children.Add(addToCartButton);
                    stackPanel.Children.Add(actionPanel);

                    buildControl.Child = stackPanel;
                    BuildsPanel.Children.Add(buildControl);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке сборок: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RateBuild(int buildId, bool isLike)
        {
            try
            {
                await _buildRatingService.RateBuildAsync(buildId, _currentUser.Id, isLike);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оценке сборки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowBuildDetails(int buildId)
        {
            try
            {
                var build = _context.Builds
                    .Include(b => b.User)
                    .Include(b => b.Cpu)
                    .Include(b => b.Gpu)
                    .Include(b => b.Motherboard)
                    .Include(b => b.Ram)
                    .Include(b => b.Storage)
                    .Include(b => b.Psu)
                    .Include(b => b.Case)
                    .FirstOrDefault(b => b.Id == buildId);

                if (build == null)
                {
                    MessageBox.Show("Сборка не найдена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var detailsWindow = new BuildDetailsWindow(build);
                detailsWindow.Owner = this;
                detailsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке деталей сборки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CommentButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is int buildId)
                {
                    var commentsWindow = new BuildCommentsViewWindow(buildId, _context);
                    commentsWindow.Owner = this;
                    commentsWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии комментариев: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BuildViewModel build)
            {
                try
                {
                    await _buildRatingService.RateBuildAsync(build.Id, _currentUser.Id, true);
                    
                    // Обновляем состояние и счетчики
                    if (!build.IsLiked)
                    {
                        build.LikesCount++;
                        if (build.IsDisliked)
                        {
                            build.DislikesCount--;
                            build.IsDisliked = false;
                        }
                        build.IsLiked = true;
                    }
                    else
                    {
                        build.LikesCount--;
                        build.IsLiked = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при оценке сборки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void DislikeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BuildViewModel build)
            {
                try
                {
                    await _buildRatingService.RateBuildAsync(build.Id, _currentUser.Id, false);
                    
                    // Обновляем состояние и счетчики
                    if (!build.IsDisliked)
                    {
                        build.DislikesCount++;
                        if (build.IsLiked)
                        {
                            build.LikesCount--;
                            build.IsLiked = false;
                        }
                        build.IsDisliked = true;
                    }
                    else
                    {
                        build.DislikesCount--;
                        build.IsDisliked = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при оценке сборки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ShowComments(int buildId)
        {
            var commentsWindow = new BuildCommentsViewWindow(buildId, _context);
            commentsWindow.Owner = this;
            commentsWindow.ShowDialog();
        }

        private void ShowDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int buildId)
            {
                ShowBuildDetails(buildId);
            }
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BuildViewModel build)
            {
                var cartService = new CartService(_context);
                cartService.AddToCart(_currentUser.Id, build.Id);
                MessageBox.Show("Сборка добавлена в корзину!", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                var loginWindow = new MainWindow();
                loginWindow.Owner = this;
                Close();
                if (loginWindow.ShowDialog() == true)
                {
                    LoadBuilds(false);
                }
            }
            else
            {
                var buildWindow = new BuildWindow(_currentUser);
                buildWindow.Owner = this;
                buildWindow.Closed += (s, args) => LoadBuilds(_showMyBuilds);
                buildWindow.ShowDialog();
            }
        }

        private void CreateBuild_Click(object sender, RoutedEventArgs e)
        {
            var buildWindow = new BuildWindow(_currentUser);
            buildWindow.Owner = this;
            buildWindow.Closed += (s, args) => LoadBuilds(_showMyBuilds);
            buildWindow.ShowDialog();
        }

        private void MyBuilds_Checked(object sender, RoutedEventArgs e)
        {
            _showMyBuilds = true;
            LoadBuilds(true);
        }

        private void AllBuilds_Checked(object sender, RoutedEventArgs e)
        {
            _showMyBuilds = false;
            LoadBuilds(false);
        }

        private void OpenCart_Click(object sender, RoutedEventArgs e)
        {
            CartWindow cartWindow = new CartWindow(_currentUser);
            cartWindow.Show();
        }

        private void OpenProfile_Click(object sender, RoutedEventArgs e)
        {
            ProfileWindow profileWindow = new ProfileWindow(_currentUser);
            profileWindow.Show();
        }

        private void ViewCommentsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BuildViewModel build)
            {
                var commentsWindow = new BuildCommentsViewWindow(build.Id, _context);
                commentsWindow.ShowDialog();
            }
        }

        private void AllBuilds_Click(object sender, RoutedEventArgs e)
        {
            LoadBuilds(false);
        }

        private void MyBuilds_Click(object sender, RoutedEventArgs e)
        {
            LoadBuilds(true);
        }

        private void WriteCommentButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is BuildViewModel build)
                {
                    var commentWindow = new BuildCommentWindow(build.Id, _context, _currentUser);
                    if (commentWindow.ShowDialog() == true)
                    {
                        // Обновляем список комментариев после добавления нового
                        var updatedComments = _context.BuildComments
                            .Where(c => c.BuildId == build.Id)
                            .OrderByDescending(c => c.CreatedDate)
                            .Select(c => new BuildCommentViewModel
                            {
                                Content = c.Content,
                                CreatedDate = c.CreatedDate,
                                AuthorName = c.User != null ? c.User.Login : "Автор не указан"
                            })
                            .ToList();

                        build.Comments = updatedComments;
                        // Свойство Comments уже вызовет PropertyChanged через сеттер
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании комментария: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewComments_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BuildViewModel build)
            {
                var commentsWindow = new BuildCommentsViewWindow(build.Id, _context);
                commentsWindow.ShowDialog();
            }
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BuildViewModel build)
            {
                var buildEntity = _context.Builds
                    .Find(build.Id);
                if (buildEntity != null)
                {
                    var detailsWindow = new BuildDetailsWindow(buildEntity);
                    detailsWindow.ShowDialog();
                }
            }
        }

        private void InitializeEventHandlers()
        {
            CartButton.Click += (s, e) =>
            {
                var cartWindow = new CartWindow(_currentUser);
                cartWindow.ShowDialog();
            };

            ProfileButton.Click += (s, e) =>
            {
                var profileWindow = new ProfileWindow(_currentUser);
                profileWindow.ShowDialog();
            };

            ExitButton.Click += (s, e) =>
            {
                var loginWindow = new MainWindow();
                loginWindow.Show();
                Close();
            };
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
        }
    }

    public class BuildCommentViewModel
    {
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public string AuthorName { get; set; }
    }

    public class BuildViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _likesCount;
        private int _dislikesCount;
        private bool _isLiked;
        private bool _isDisliked;
        private List<BuildCommentViewModel> _comments;

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string ImagePath { get; set; }
        public string AuthorName { get; set; }
        
        public List<BuildCommentViewModel> Comments
        {
            get => _comments;
            set
            {
                if (_comments != value)
                {
                    _comments = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public int LikesCount 
        { 
            get => _likesCount;
            set
            {
                if (_likesCount != value)
                {
                    _likesCount = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public int DislikesCount 
        { 
            get => _dislikesCount;
            set
            {
                if (_dislikesCount != value)
                {
                    _dislikesCount = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public bool IsLiked 
        { 
            get => _isLiked;
            set
            {
                if (_isLiked != value)
                {
                    _isLiked = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public bool IsDisliked 
        { 
            get => _isDisliked;
            set
            {
                if (_isDisliked != value)
                {
                    _isDisliked = value;
                    OnPropertyChanged();
                }
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }
}
