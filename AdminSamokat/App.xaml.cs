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
            string userSurname = Preferences.Get("UserSurname", null);
            string userName = Preferences.Get("UserName", null);
            string userPatronymic = Preferences.Get("UserPatronymic", null);
            string userLogin = Preferences.Get("UserLogin", null);
            string userPassword = Preferences.Get("UserPassword", null);

            if (!string.IsNullOrEmpty(userToken) && !string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(userLogin))
            {
                // Если токен и данные пользователя есть, создаём объект User
                var user = new Models.User
                {
                    Surname = userSurname,
                    Name = userName,
                    Patronymic = userPatronymic,
                    Login = userLogin,
                    Password = userPassword
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
