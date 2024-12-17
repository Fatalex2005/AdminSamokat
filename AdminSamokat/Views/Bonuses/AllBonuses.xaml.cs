using AdminSamokat.Models;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace AdminSamokat.Views.Bonuses;

public partial class AllBonuses : ContentPage
{
    private User _user;
    private string _token;
    private readonly HttpClient _httpClient = new HttpClient();
    public ObservableCollection<Bonus> Bonuses { get; set; } = new ObservableCollection<Bonus>();
    public AllBonuses(User user, string token)
	{
		InitializeComponent();
        _user = user;
        _token = token;
        BonusesCollectionView.ItemsSource = Bonuses;
        LoadBonuses();
    }

    private async void LoadBonuses()
    {
        try
        {
            // Показываем индикатор загрузки
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var token = Preferences.Get("UserToken", string.Empty);

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("http://courseproject4/api/bonus");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var bonuses = JsonSerializer.Deserialize<List<Bonus>>(content);

                Bonuses.Clear();
                foreach (var bonus in bonuses)
                {
                    Bonuses.Add(bonus);
                }

                // Управляем видимостью элементов в зависимости от количества бонусов
                BonusesCollectionView.IsVisible = Bonuses.Count > 0;
                EmptyMessageLabel.IsVisible = Bonuses.Count == 0;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Ошибка авторизации
                await DisplayAlert("Ошибка", "Ваш токен не актуален. Авторизуйтесь заново!", "ОК");
                await Navigation.PushAsync(new Views.Auth.Login());
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Ошибка", $"Код: {response.StatusCode}, Ответ: {errorContent}", "ОК");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "ОК");
        }
        finally
        {
            // Скрываем индикатор загрузки после завершения загрузки данных
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }



    private async void OnBonusSelected(object sender, SelectionChangedEventArgs e)
    {
        // Получаем выбранного пользователя
        if (e.CurrentSelection.FirstOrDefault() is Bonus selectedBonus)
        {
            // Переход на страницу информации о бонусе
            await Navigation.PushAsync(new BonusPage(selectedBonus, _user, _token));
        }

        // Сбрасываем выбор
        ((CollectionView)sender).SelectedItem = null;
    }

    private async void OnCreateBonusClicked(object sender, EventArgs e)
    {
        // Переход на страницу создания бонуса
        await Navigation.PushAsync(new CreateBonusPage(_user, _token));
    }
}