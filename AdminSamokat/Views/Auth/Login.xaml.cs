using AdminSamokat.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AdminSamokat.Views.Auth;

public partial class Login : ContentPage
{
    // Инициализация HTTP клиента
    private readonly HttpClient _httpClient = new HttpClient();
    public Login()
    {
        InitializeComponent();
    }
    // Аутентификация
    private async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        // Получение почты и пароля из формы
        string login = LoginEntry.Text;
        string password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Ошибка", "Введите логин и пароль", "ОК");
            return;
        }
        var loginResponse = await AuthenticateUserAsync(login, password);
        if (loginResponse != null)
        {
            // Сохраняем данные пользователя и токен
            Preferences.Set("UserToken", loginResponse.Token);
            Preferences.Set("UserSurname", loginResponse.User.Surname);
            Preferences.Set("UserName", loginResponse.User.Name);
            Preferences.Set("UserPatronymic", loginResponse.User.Patronymic);
            Preferences.Set("UserLogin", loginResponse.User.Login);
            Preferences.Set("UserPassword", loginResponse.User.Password);
            // Передаём данные пользователя и его токен на страницу Home
            await Navigation.PushAsync(new Home(loginResponse.User, loginResponse.Token));
        }
    }
    private async Task<AuthResponse> AuthenticateUserAsync(string login, string password)
    {
        // Формирование тела для отправки
        var loginData = new { login, password };
        // Преобразование данных в для отправки
        var jsonContent = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

        // Запрос серверу
        try
        {
            // Отправляем запрос и записываем ответ в response
            HttpResponseMessage response = await _httpClient.PostAsync("http://courseproject4/api/login", jsonContent);

            // Если ответ успех 200
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AuthResponse>(content);
                if (result?.Token != null)
                {
                    return result;
                }
            }
            // Если ответ 401
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await DisplayAlert("Ошибка входа", "Неправильный логин или пароль", "ОК");
            }
            else
            {
                await DisplayAlert("Ошибка", "Произошла ошибка на сервере", "ОК");
            }


        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка сети", ex.Message, "ОК");
        }
        return null;
    }

}