using AdminSamokat.Views;

namespace AdminSamokat
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
        protected override void OnStart()
        {
            string userToken = Preferences.Get("UserToken", null);
            string userName = Preferences.Get("UserName", null);
            string userLogin = Preferences.Get("UserLogin", null);

            if (!string.IsNullOrEmpty(userToken) && !string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(userLogin))
            {
                // Если токен и данные пользователя есть, создаём объект User
                var user = new Models.User
                {
                    Name = userName,
                    Login = userLogin
                };

                // Переход на главную страницу
                MainPage = new NavigationPage(new Home(user, userToken));
            }
            else
            {
                // Если данных нет, показываем страницу авторизации
                MainPage = new NavigationPage(new Views.Auth.Login());
            }
        }
    }
}
