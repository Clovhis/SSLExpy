using System.Windows;
using System.Windows.Controls;

namespace AzureKvSslExpirationChecker.Helpers
{
    /// <summary>
    /// Allows binding the <see cref="PasswordBox.Password"/> property.
    /// </summary>
    public static class PasswordBoxHelper
    {
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BoundPassword",
                typeof(string),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

        public static readonly DependencyProperty BindPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BindPassword",
                typeof(bool),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(false, OnBindPasswordChanged));

        private static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached(
                "IsUpdating",
                typeof(bool),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(false));

        public static string? GetBoundPassword(DependencyObject d)
            => (string?)d.GetValue(BoundPasswordProperty);

        public static void SetBoundPassword(DependencyObject d, string? value)
            => d.SetValue(BoundPasswordProperty, value);

        public static bool GetBindPassword(DependencyObject d)
            => (bool)d.GetValue(BindPasswordProperty);

        public static void SetBindPassword(DependencyObject d, bool value)
            => d.SetValue(BindPasswordProperty, value);

        private static bool GetIsUpdating(DependencyObject d)
            => (bool)d.GetValue(IsUpdatingProperty);

        private static void SetIsUpdating(DependencyObject d, bool value)
            => d.SetValue(IsUpdatingProperty, value);

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox box && !GetIsUpdating(box))
            {
                box.Password = e.NewValue as string ?? string.Empty;
            }
        }

        private static void OnBindPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox box)
            {
                bool wasBound = (bool)e.OldValue;
                bool needBound = (bool)e.NewValue;

                if (wasBound)
                {
                    box.PasswordChanged -= HandlePasswordChanged;
                }

                if (needBound)
                {
                    box.PasswordChanged += HandlePasswordChanged;
                }
            }
        }

        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox box)
            {
                SetIsUpdating(box, true);
                SetBoundPassword(box, box.Password);
                SetIsUpdating(box, false);
            }
        }
    }
}
