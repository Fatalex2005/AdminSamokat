using AdminSamokat.Models;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace AdminSamokat.Views.Couries;

public partial class AllCouriers : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient();
    public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();

    public AllCouriers()
    {
        InitializeComponent();
        UsersCollectionView.ItemsSource = Users;
        LoadUsers();
    }

    private async void LoadUsers()
    {
        try
        {
            // Показываем индикатор загрузки
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            var token = Preferences.Get("UserToken", string.Empty);

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("http://courseproject4/api/profile");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<User>>(content);

                Users.Clear();

                // Фильтруем пользователей с ролью 2
                var couriers = users.Where(user => user.RoleId == 2); // Предполагается, что RoleId указывает роль пользователя

                foreach (var courier in couriers)
                {
                    Users.Add(courier);
                }
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
            // Скрываем индикатор загрузки
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }


    private async void OnCourierSelected(object sender, SelectionChangedEventArgs e)
    {
        // Получаем выбранного курьера
        if (e.CurrentSelection.FirstOrDefault() is User selectedCourier)
        {
            // Переход на страницу информации о курьере
            await Navigation.PushAsync(new Courier(selectedCourier));
        }

        // Сбрасываем выбор
        ((CollectionView)sender).SelectedItem = null;
    }
}
