using AdminSamokat.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;

namespace AdminSamokat.Views.Bonuses;

public partial class CreateBonusPage : ContentPage
{
    private User _user;
    private string _token;
    private readonly HttpClient _httpClient = new HttpClient();
    public CreateBonusPage(User user, string token)
    {
        InitializeComponent();
        _user = user;
        _token = token;
        // Устанавливаем список ролей в Picker
        rolePicker.ItemsSource = Roles;
        // Устанавливаем выбранную роль по умолчанию
        if (Roles.Count > 0)
        {
            rolePicker.SelectedItem = Roles[0];
        }
    }
    private static readonly List<Role> Roles = new List<Role>
{
    new Role { Id = 1, Name = "Администратор", Code = "admin" },
    new Role { Id = 2, Name = "Курьер", Code = "courier" }
};
    private async void OnCreateButtonClicked(object sender, EventArgs e)
    {
        // Проверка на пустые поля
        if (string.IsNullOrWhiteSpace(titleEntry.Text) ||
            string.IsNullOrWhiteSpace(descriptionEntry.Text) ||
            string.IsNullOrWhiteSpace(priceEntry.Text) ||
            rolePicker.SelectedItem == null)
        {
            await DisplayAlert("Ошибка", "Все обязательные поля должны быть заполнены", "ОК");
            return;
        }
        // Подтверждение перед созданием
        bool isConfirmed = await DisplayAlert(
            "Подтверждение",
            "Вы уверены, что хотите добавить новый бонус?",
            "Да",
            "Нет"
        );
        if (!isConfirmed)
        {
            // Если пользователь выбрал "Нет", создание отменяется
            return;
        }
        // Сбор данных из формы
        string title = titleEntry.Text;
        string description = descriptionEntry.Text;
        string price = priceEntry.Text;
        var selectedRole = (Role)rolePicker.SelectedItem;
        var createData = new MultipartFormDataContent
    {
        { new StringContent(title), "title" },
        { new StringContent(description), "description" },
        { new StringContent(price), "price" },
        { new StringContent(selectedRole.Id.ToString()), "role_id" } // Добавляем role_id
    };
        try
        {
            // Отображаем индикатор загрузки и скрываем форму
            CreateForm.IsVisible = false;
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            HttpResponseMessage response = await _httpClient.PostAsync("http://courseproject4/api/bonus", createData);
            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Успех", "Бонус успешно добавлен", "ОК");
                await Navigation.PushAsync(new Views.Home(_user, _token));
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
            CreateForm.IsVisible = true;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}