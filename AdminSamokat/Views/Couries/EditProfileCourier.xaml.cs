using AdminSamokat.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AdminSamokat.Views.Couries;

public partial class EditProfileCourier : ContentPage
{
    // Инициализация HTTP клиента
    private readonly HttpClient _httpClient = new HttpClient();
    private User _courier;
    private User _user;
    private string _token;
    public EditProfileCourier(User courier, User user, string token)
    {
        InitializeComponent();
        _user = user;
        _token = token;
        _courier = courier;
        surnameLabel.Text = courier.Surname;
        nameLabel.Text = courier.Name;
        if (string.IsNullOrEmpty(courier.Patronymic))
        {
            patronymicLabel.Text = "";
        }
        else
        {
            patronymicLabel.Text = courier.Patronymic;
        }
        loginLabel.Text = courier.Login;
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
            // Скрываем форму и показываем индикатор загрузки
            MainContent.IsVisible = false;
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            // Отправляем запрос и записываем ответ в response
            var token = Preferences.Get("UserToken", string.Empty);
            _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await _httpClient.PutAsync($"http://courseproject4/api/profile/{_courier.Id}", jsonContent);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _courier.Surname = updatedUser.Surname;
                _courier.Name = updatedUser.Name;
                _courier.Patronymic = updatedUser.Patronymic;
                _courier.Login = updatedUser.Login;
                _courier.Password = updatedUser.Password;

                // Обновляем UI
                surnameLabel.Text = updatedUser.Surname;
                nameLabel.Text = updatedUser.Name;
                patronymicLabel.Text = updatedUser.Patronymic ?? "";
                loginLabel.Text = updatedUser.Login;
                passwordLabel.Text = updatedUser.Password;

                await DisplayAlert("Успех", "Профиль успешно обновлён!", "Вернуться на главную");

                await Navigation.PushAsync(new Views.Home(_user, token));
                Navigation.RemovePage(this); // Убираем текущую страницу из стека
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
        finally
        {
            // Восстанавливаем интерфейс, если авторизация не удалась
            MainContent.IsVisible = true;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}