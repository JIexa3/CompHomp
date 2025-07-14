using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace CompHomp.Services
{
    public class ImageService
    {
        private readonly string _imageStoragePath;

        public ImageService()
        {
            // Создаем папку для хранения изображений в папке приложения
            _imageStoragePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            if (!Directory.Exists(_imageStoragePath))
            {
                Directory.CreateDirectory(_imageStoragePath);
            }
        }

        public string SaveImage(string sourcePath)
        {
            if (string.IsNullOrEmpty(sourcePath))
                return null;

            try
            {
                // Генерируем уникальное имя файла
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(sourcePath)}";
                string destinationPath = Path.Combine(_imageStoragePath, fileName);

                // Копируем файл в папку хранения
                File.Copy(sourcePath, destinationPath, true);

                return destinationPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при сохранении изображения: {ex.Message}");
            }
        }

        public BitmapImage LoadImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                return null;

            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(imagePath);
                image.EndInit();
                return image;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при загрузке изображения: {ex.Message}");
            }
        }
    }
}
