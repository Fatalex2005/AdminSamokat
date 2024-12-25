using AdminSamokat.Models;
using System.Net.Http;

namespace AdminSamokat.Views.Bonuses;

public partial class BonusPage : ContentPage
{
    private User _user;
    private string _token;
    private Bonus _bonus;
    private readonly HttpClient _httpClient = new HttpClient();
    public BonusPage(Bonus bonus, User user, string token)
    {
        InitializeComponent();
        _bonus = bonus;
        _user = user;
        _token = token;

        // Отобразить данные бонуса
        BonusNameLabel.Text = _bonus.Title;
        BonusDescriptionLabel.Text = _bonus.Description;
        BonusPriceLabel.Text = _bonus.Price + " \u20BD";

        // Отобразить роль
        if (_bonus.Role != null)
        {
            BonusRoleLabel.Text = $"Для роли: {_bonus.Role.Name}";
        }
        else
        {
            BonusRoleLabel.Text = "Роль не указана";
        }
    }

    private async void OnEditButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new EditBonus(_bonus, _user, _token));
    }

    private async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        // Подтверждение перед удалением
        bool isConfirmed = await DisplayAlert(
            "Подтверждение",
            "Вы уверены, что хотите удалить этот бонус?",
            "Да",
            "Нет"
        );

        if (!isConfirmed)
        {
            // Если пользователь выбрал "Нет", удаление отменяется
            return;
        }

        try
        {
            // Скрываем основное содержимое и показываем индикатор
            MainContent.IsVisible = false;
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            // Настраиваем заголовки и токен
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

            // Отправляем DELETE-запрос на сервер
            var response = await _httpClient.DeleteAsync($"http://courseproject4/api/bonus/{_bonus.Id}");

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Успех", "Бонус успешно удалён.", "ОК");

                await Navigation.PushAsync(new Home(_user, _token));
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Ошибка", $"Не удалось удалить бонус: {response.StatusCode} - {responseContent}", "ОК");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "ОК");
        }
        finally
        {
            // Возвращаем основное содержимое и скрываем индикатор
            MainContent.IsVisible = true;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}