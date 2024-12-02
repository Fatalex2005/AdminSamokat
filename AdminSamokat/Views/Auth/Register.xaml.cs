using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AdminSamokat.Models;

namespace AdminSamokat.Views.Auth;

public partial class Register : ContentPage
{
    // Инициализация HTTP клиента
    private readonly HttpClient _httpClient = new HttpClient();
    private User _user;
    private string _token;
    public Register(User user, string token)
    {
        InitializeComponent();
        _user = user;
        _token = token;
    }

    // Регистрация
    private async void OnRegisterButtonClicked(object sender, EventArgs e)
    {
        // Проверка на пустые поля
        if (string.IsNullOrWhiteSpace(SurnameEntry.Text) ||
            string.IsNullOrWhiteSpace(NameEntry.Text) ||
            string.IsNullOrWhiteSpace(LoginEntry.Text) ||
            string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            await DisplayAlert("Ошибка", "Все обязательные поля должны быть заполнены", "ОК");
            return;
        }

        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            await DisplayAlert("Ошибка", "Пароли не совпадают", "ОК");
            return;
        }

        // Сбор данных из формы
        string surname = SurnameEntry.Text;
        string name = NameEntry.Text;
        string? patronymic = PatronymicEntry.Text;
        string login = LoginEntry.Text;
        string password = PasswordEntry.Text;

        // Формирование тела запроса
        var registerData = new MultipartFormDataContent
        {
            {new StringContent(surname), "surname" },
            {new StringContent(name), "name" },
            {new StringContent(patronymic ?? string.Empty), "patronymic" },
            {new StringContent(login), "login" },
            {new StringContent(password), "password" },
        };

        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            // Отправляем запрос и записываем ответ в response
            HttpResponseMessage response = await _httpClient.PostAsync("http://courseproject4/api/register", registerData);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AuthResponse>(content);

                if (result?.Token != null)
                {
                    await DisplayAlert("Успешная регистрация", "Пользователь успешно добавлен", "ОК");
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось получить данные пользователя из ответа сервера", "ОК");
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Ошибка валидации", errorContent, "ОК");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Ошибка", $"Код ошибки: {(int)response.StatusCode}\n{errorContent}", "ОК");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка сети", ex.Message, "ОК");
        }




    }
}