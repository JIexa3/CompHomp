using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace CompHomp
{
    public partial class BaseComponentWindow : Window
    {
        protected Dictionary<string, TextBox> InputFields = new Dictionary<string, TextBox>();

        public BaseComponentWindow()
        {
            InitializeComponent();
        }

        protected void AddTextField(string label, string fieldName, string initialValue = "")
        {
            var textBlock = new TextBlock
            {
                Text = label,
                Margin = new Thickness(0, 0, 0, 5)
            };

            var textBox = new TextBox
            {
                Name = fieldName,
                Text = initialValue,
                Margin = new Thickness(0, 0, 0, 10)
            };

            InputPanel.Children.Add(textBlock);
            InputPanel.Children.Add(textBox);

            InputFields[fieldName] = textBox;
        }

        protected void AddNumericField(string label, string fieldName, string initialValue = "", bool isDecimal = false)
        {
            var textBlock = new TextBlock
            {
                Text = label,
                Margin = new Thickness(0, 0, 0, 5)
            };

            var textBox = new TextBox
            {
                Name = fieldName,
                Text = initialValue,
                Margin = new Thickness(0, 0, 0, 10)
            };

            textBox.PreviewTextInput += isDecimal 
                ? new TextCompositionEventHandler(DecimalValidationTextBox) 
                : new TextCompositionEventHandler(NumberValidationTextBox);

            InputPanel.Children.Add(textBlock);
            InputPanel.Children.Add(textBox);

            InputFields[fieldName] = textBox;
        }

        protected virtual void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        protected virtual void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        protected bool ValidateTextField(string fieldName, bool allowEmpty = false)
        {
            if (!InputFields.ContainsKey(fieldName))
                return false;

            var value = InputFields[fieldName].Text.Trim();
            
            if (!allowEmpty && string.IsNullOrWhiteSpace(value))
            {
                MessageBox.Show($"Поле '{fieldName}' не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        protected bool ValidateNumericField(string fieldName, bool isDecimal = false)
        {
            if (!InputFields.ContainsKey(fieldName))
                return false;

            var value = InputFields[fieldName].Text.Trim();

            if (isDecimal)
            {
                if (!decimal.TryParse(value, out _))
                {
                    MessageBox.Show($"Введите корректное число для поля '{fieldName}'.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            else
            {
                if (!int.TryParse(value, out _))
                {
                    MessageBox.Show($"Введите корректное целое число для поля '{fieldName}'.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            return true;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
        }

        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]*\.?[0-9]*$");
            e.Handled = !regex.IsMatch(e.Text);
        }
    }
}
