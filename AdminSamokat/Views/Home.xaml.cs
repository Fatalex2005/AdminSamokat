using AdminSamokat.Models;
using AdminSamokat.Views.Auth;
using AdminSamokat.Views.Fines;
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace AdminSamokat.Views;

public partial class Home : ContentPage
{
    // Инициализация HTTP клиента
    private readonly HttpClient _httpClient = new HttpClient();
    private User _user;
    private string _token;
    public Home(User user, string token)
    {
        InitializeComponent();
        _user = user;
        _token = token;
        int hour = DateTime.Now.Hour;
        if (hour >= 5 && hour < 12)
        {
            nameLabel.Text = "Доброе утро, " + user.Name + "!";
        }
        else if (hour >= 12 && hour < 18)
        {
            nameLabel.Text = "Добрый день, " + user.Name + "!";
        }
        else if (hour >= 18 && hour < 22)
        {
            nameLabel.Text = "Добрый вечер, " + user.Name + "!";
        }
        else
        {
            nameLabel.Text = "Доброй ночи, " + user.Name + "! Что вы здесь забыли в это время, ночью зрение портится, ай-ай-ай";
        }
    }
    private async void OnLogoutButtonClicked(object sender, EventArgs e)
    {
        bool isConfirmed = await DisplayAlert(
            "Подтверждение",
            "Вы уверены, что хотите выйти из аккаунта?",
            "Да",
            "Нет"
        );
        if (!isConfirmed)
        {
            return;
        }
        // Показываем индикатор загрузки
        Overlay.IsVisible = true;
        string userToken = Preferences.Get("UserToken", string.Empty);
        if (!string.IsNullOrEmpty(userToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync("http://courseproject4/api/logout", null);
                // Скрываем индикатор загрузки ПЕРЕД уведомлением
                Overlay.IsVisible = false;
                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Успех", "Вы успешно вышли из системы.", "ОК");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Ошибка", $"Не удалось завершить сессию. Ответ сервера: {errorContent}", "ОК");
                }
            }
            catch (Exception ex)
            {
                // Скрываем индикатор загрузки при исключении
                Overlay.IsVisible = false;

                await DisplayAlert("Ошибка", $"Не удалось выполнить запрос: {ex.Message}", "ОК");
            }
        }
        else
        {
            // Скрываем индикатор загрузки, если токен отсутствует
            Overlay.IsVisible = false;
        }
        Preferences.Clear();
        // Возвращаемся на страницу входа
        await Navigation.PushAsync(new Views.Auth.Login());
        Navigation.RemovePage(this);
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Скрыть системную кнопку "Назад"
        NavigationPage.SetHasBackButton(this, false);
    }
    private async void OnAccessesClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Accesses.AllAccesses(_user, _token));
    }
    private async void OnCouriersClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Couries.AllCouriers(_user, _token));
    }
    private async void OnRegisterCourierClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Auth.Register(_user, _token));
    }
    private async void OnBonusesClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Bonuses.AllBonuses(_user, _token));
    }
    private async void OnPenaltiesClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Fines.AllFines(_user, _token));
    }
    private async void OnProfileButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Profile(_user, _token));
    }
}