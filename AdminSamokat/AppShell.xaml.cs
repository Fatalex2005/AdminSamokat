namespace AdminSamokat
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Отключаем кнопку "Назад" на всех страницах
            Navigated += OnNavigated;
        }

        private void OnNavigated(object sender, ShellNavigatedEventArgs e)
        {
            // Устанавливаем поведение для текущей страницы
            if (Shell.Current?.CurrentPage != null)
            {
                Shell.SetBackButtonBehavior(Shell.Current.CurrentPage, new BackButtonBehavior
                {
                    IsEnabled = false, // Отключаем функциональность
                    IsVisible = false  // Полностью скрываем кнопку
                });
            }
        }
    }
}
