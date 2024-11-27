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
            string userId = Preferences.Get("UserId", null);
            string userSurname = Preferences.Get("UserSurname", null);
            string userName = Preferences.Get("UserName", null);
            string userPatronymic = Preferences.Get("UserPatronymic", null);
            string userLogin = Preferences.Get("UserLogin", null);
            string userPassword = Preferences.Get("UserPassword", null);

            // Проверяем и преобразуем userId в ulong
            if (ulong.TryParse(userId, out ulong parsedUserId))
            {
                // Создаём объект User, если id успешно преобразован
                var user = new Models.User
                {
                    Id = parsedUserId, // Преобразованный ID
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
                // Если userId некорректный, отправляем пользователя на страницу авторизации
                MainPage = new NavigationPage(new Views.Auth.Login());
            }
        }
    }
}
