using AdminSamokat.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AdminSamokat.Views.Fines;

public partial class EditFine : ContentPage
{
    // Инициализация HTTP клиента
    private readonly HttpClient _httpClient = new HttpClient();
    private Fine _fine;
    private User _user;
    private string _token;
    public EditFine(Fine fine, User user, string token)
	{
		InitializeComponent();
        _fine = fine;
        _user = user;
        _token = token;

        descriptionLabel.Text = fine.Description;
    }

    private async void OnSaveButtonClicked(object sender, EventArgs e)
    {
        // Проверка на пустые поля
        if (string.IsNullOrWhiteSpace(descriptionLabel.Text))
        {
            await DisplayAlert("Ошибка", "Все обязательные поля должны быть заполнены", "ОК");
            return;
        }

        // Формируем данные для обновления
        var updatedFine = new
        {
            Description = descriptionLabel.Text,
        };
        // Настраиваем сериализацию для преобразования ключей в нижний регистр
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Преобразует ключи в camelCase
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(updatedFine, options), Encoding.UTF8, "application/json");


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
            HttpResponseMessage response = await _httpClient.PutAsync($"http://courseproject4/api/fine/{_fine.Id}", jsonContent);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _fine.Description = updatedFine.Description;

                // Обновляем UI
                descriptionLabel.Text = updatedFine.Description;

                await DisplayAlert("Успех", "Штраф успешно обновлён!", "Вернуться на главную");

                await Navigation.PushAsync(new Views.Home(_user, token));
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await DisplayAlert("Ошибка", "Сессия истекла. Авторизуйтесь снова.", "OK");
                await Navigation.PushAsync(new Views.Auth.Login());
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Ошибка", $"Не удалось обновить штраф: {responseContent}", "OK");
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