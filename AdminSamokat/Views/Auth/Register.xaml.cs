using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AdminSamokat.Models;

namespace AdminSamokat.Views.Auth;

public partial class Register : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient();
    private User _user;
    private string _token;

    public Register(User user, string token)
    {
        InitializeComponent();
        _user = user;
        _token = token;
    }

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

        // Подтверждение перед регистрацией
        bool isConfirmed = await DisplayAlert(
            "Подтверждение",
            "Вы уверены, что хотите добавить нового пользователя?",
            "Да",
            "Нет"
        );

        if (!isConfirmed)
        {
            // Если пользователь выбрал "Нет", регистрация отменяется
            return;
        }

        // Сбор данных из формы
        string surname = SurnameEntry.Text;
        string name = NameEntry.Text;
        string? patronymic = PatronymicEntry.Text;
        string login = LoginEntry.Text;
        string password = PasswordEntry.Text;

        var registerData = new MultipartFormDataContent
        {
            { new StringContent(surname), "surname" },
            { new StringContent(name), "name" },
            { new StringContent(patronymic ?? string.Empty), "patronymic" },
            { new StringContent(login), "login" },
            { new StringContent(password), "password" },
        };

        try
        {
            // Отображаем индикатор загрузки и скрываем форму
            RegistrationForm.IsVisible = false;
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            HttpResponseMessage response = await _httpClient.PostAsync("http://courseproject4/api/register", registerData);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Успешная регистрация", "Пользователь успешно добавлен", "ОК");
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
        finally
        {
            // Восстанавливаем форму и скрываем индикатор загрузки
            RegistrationForm.IsVisible = true;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}
