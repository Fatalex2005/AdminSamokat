using AdminSamokat.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AdminSamokat.Views;

public partial class EditProfile : ContentPage
{
    // Инициализация HTTP клиента
    private readonly HttpClient _httpClient = new HttpClient();
    private User _user;
    private string _token;
    public EditProfile(User user, string token)
	{
		InitializeComponent();
        _user = user;
        _token = token;
        surnameLabel.Text = user.Surname;
        nameLabel.Text = user.Name;
        if (string.IsNullOrEmpty(user.Patronymic))
        {
            patronymicLabel.Text = "";
        }
        else
        {
            patronymicLabel.Text = user.Patronymic;
        }
        loginLabel.Text = user.Login;
    }

    private async void OnSaveButtonClicked(object sender, EventArgs e)
    {
        // Проверка на пустые поля
        if (string.IsNullOrWhiteSpace(surnameLabel.Text) ||
        string.IsNullOrWhiteSpace(nameLabel.Text) ||
            string.IsNullOrWhiteSpace(loginLabel.Text) ||
            string.IsNullOrWhiteSpace(passwordLabel.Text))
        {
            await DisplayAlert("Ошибка", "Все обязательные поля должны быть заполнены", "ОК");
            return;
        }

        if (passwordLabel.Text != confirmPasswordLabel.Text)
        {
            await DisplayAlert("Ошибка", "Пароли не совпадают", "ОК");
            return;
        }

        // Формируем данные для обновления
        var updatedUser = new
        {
            Surname = surnameLabel.Text,
            Name = nameLabel.Text,
            Patronymic = string.IsNullOrEmpty(patronymicLabel.Text) ? null : patronymicLabel.Text,
            Login = loginLabel.Text,
            Password = passwordLabel.Text
        };
        // Настраиваем сериализацию для преобразования ключей в нижний регистр
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Преобразует ключи в camelCase
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(updatedUser, options), Encoding.UTF8, "application/json");


        // Запрос серверу
        try
        {
            // Отправляем запрос и записываем ответ в response
            _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            HttpResponseMessage response = await _httpClient.PutAsync($"http://courseproject4/api/profile/{_user.Id}", jsonContent);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _user.Surname = updatedUser.Surname;
                _user.Name = updatedUser.Name;
                _user.Patronymic = updatedUser.Patronymic;
                _user.Login = updatedUser.Login;
                _user.Password = updatedUser.Password;

                Preferences.Set("UserSurname", updatedUser.Surname);
                Preferences.Set("UserName", updatedUser.Name);
                Preferences.Set("UserPatronymic", updatedUser.Patronymic ?? "");
                Preferences.Set("UserLogin", updatedUser.Login);
                Preferences.Set("UserPassword", updatedUser.Password);

                // Обновляем UI
                surnameLabel.Text = updatedUser.Surname;
                nameLabel.Text = updatedUser.Name;
                patronymicLabel.Text = updatedUser.Patronymic ?? "";
                loginLabel.Text = updatedUser.Login;
                passwordLabel.Text = updatedUser.Password;

                await DisplayAlert("Успех", "Профиль успешно обновлён!", "OK");
                await Navigation.PushAsync(new Profile(_user, _token));
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await DisplayAlert("Ошибка", "Сессия истекла. Авторизуйтесь снова.", "OK");
                await Navigation.PushAsync(new Views.Auth.Login());
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Ошибка", $"Не удалось обновить профиль: {responseContent}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
        }
    }

}