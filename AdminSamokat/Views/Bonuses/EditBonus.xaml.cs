using AdminSamokat.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AdminSamokat.Views.Bonuses;

public partial class EditBonus : ContentPage
{
    // Инициализация HTTP клиента
    private readonly HttpClient _httpClient = new HttpClient();
    private Bonus _bonus;
    private User _user;
    private string _token;
    public EditBonus(Bonus bonus, User user, string token)
    {
        InitializeComponent();
        _user = user;
        _token = token;
        _bonus = bonus;
        titleLabel.Text = bonus.Title;
        descriptionLabel.Text = bonus.Description;
        priceLabel.Text = bonus.Price;
        // Устанавливаем список ролей в Picker
        rolePicker.ItemsSource = Roles;
        // Устанавливаем выбранную роль
        if (_bonus.RoleId > 0)
        {
            var selectedRole = Roles.FirstOrDefault(r => r.Id == _bonus.RoleId);
            if (selectedRole != null)
            {
                rolePicker.SelectedItem = selectedRole;
            }
        }
    }
    private static readonly List<Role> Roles = new List<Role>
{
    new Role { Id = 1, Name = "Администратор", Code = "admin" },
    new Role { Id = 2, Name = "Курьер", Code = "courier" }
};

    private async void OnSaveButtonClicked(object sender, EventArgs e)
    {
        // Проверка на пустые поля
        if (string.IsNullOrWhiteSpace(titleLabel.Text) ||
            string.IsNullOrWhiteSpace(descriptionLabel.Text) ||
            string.IsNullOrWhiteSpace(priceLabel.Text))
        {
            await DisplayAlert("Ошибка", "Все обязательные поля должны быть заполнены", "ОК");
            return;
        }
        // Проверка, что роль выбрана
        if (rolePicker.SelectedItem == null)
        {
            await DisplayAlert("Ошибка", "Выберите роль", "ОК");
            return;
        }
        // Подтверждение перед сохранением изменений
        bool isConfirmed = await DisplayAlert(
            "Подтверждение",
            "Вы уверены, что хотите сохранить изменения?",
            "Да",
            "Нет"
        );
        if (!isConfirmed)
        {
            // Если пользователь выбрал "Нет", сохранение отменяется
            return;
        }
        // Получаем выбранную роль
        var selectedRole = (Role)rolePicker.SelectedItem;
        var updatedBonus = new Bonus
        {
            Title = titleLabel.Text,
            Description = descriptionLabel.Text,
            Price = priceLabel.Text,
            RoleId = selectedRole.Id
        };
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Преобразует ключи в camelCase
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(updatedBonus, options), Encoding.UTF8, "application/json");
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
            HttpResponseMessage response = await _httpClient.PutAsync($"http://courseproject4/api/bonus/{_bonus.Id}", jsonContent);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _bonus.Title = updatedBonus.Title;
                _bonus.Description = updatedBonus.Description;
                _bonus.Price = updatedBonus.Price;
                _bonus.RoleId = updatedBonus.RoleId; // Обновляем RoleId
                // Обновляем UI
                titleLabel.Text = updatedBonus.Title;
                descriptionLabel.Text = updatedBonus.Description;
                priceLabel.Text = updatedBonus.Price;
                await DisplayAlert("Успех", "Бонус успешно обновлён!", "Вернуться на главную");
                await Navigation.PushAsync(new Views.Home(_user, token));
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                try
                {
                    // Парсим JSON как JsonDocument
                    using var document = JsonDocument.Parse(errorContent);
                    var root = document.RootElement;
                    var message = root.GetProperty("message").GetString();
                    if (root.TryGetProperty("errors", out var errors))
                    {
                        var errorMessages = new List<string>();
                        // Проходим по всем ошибкам
                        foreach (var error in errors.EnumerateObject())
                        {
                            foreach (var msg in error.Value.EnumerateArray())
                            {
                                errorMessages.Add(System.Text.RegularExpressions.Regex.Unescape(msg.GetString()));
                            }
                        }
                        var combinedErrors = string.Join("\n", errorMessages);
                        await DisplayAlert("Ошибка валидации", combinedErrors, "ОК");
                    }
                    else
                    {
                        await DisplayAlert("Ошибка", message ?? "Произошла ошибка валидации.", "ОК");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Ошибка", $"Не удалось обработать ответ: {ex.Message}\n\nОтвет: {errorContent}", "ОК");
                }
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Ошибка", $"Не удалось обновить бонус: {responseContent}", "OK");
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