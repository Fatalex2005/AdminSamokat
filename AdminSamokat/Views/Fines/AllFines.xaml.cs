using AdminSamokat.Models;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace AdminSamokat.Views.Fines;

public partial class AllFines : ContentPage
{
    private User _user;
    private string _token;
    private readonly HttpClient _httpClient = new HttpClient();
    public ObservableCollection<Fine> Fines { get; set; } = new ObservableCollection<Fine>();
    public AllFines(User user, string token)
	{
		InitializeComponent();
        _user = user;
        _token = token;
        FinesCollectionView.ItemsSource = Fines;
        LoadFines();
    }
    private async void LoadFines()
    {
        try
        {
            // Показываем индикатор загрузки
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var token = Preferences.Get("UserToken", string.Empty);

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("http://courseproject4/api/fine");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var fines = JsonSerializer.Deserialize<List<Fine>>(content);

                Fines.Clear();
                foreach (var fine in fines)
                {
                    Fines.Add(fine);
                }

                // Управляем видимостью элементов в зависимости от количества бонусов
                FinesCollectionView.IsVisible = Fines.Count > 0;
                EmptyMessageLabel.IsVisible = Fines.Count == 0;
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
    
    private async void OnFineSelected(object sender, SelectionChangedEventArgs e)
    {
        // Получаем выбранного пользователя
        if (e.CurrentSelection.FirstOrDefault() is Fine selectedFine)
        {
            // Переход на страницу информации о бонусе
            await Navigation.PushAsync(new FinePage(selectedFine, _user, _token));
        }

        // Сбрасываем выбор
        ((CollectionView)sender).SelectedItem = null;
    }
    

    private async void OnCreateFineClicked(object sender, EventArgs e)
    {
        // Переход на страницу создания штрафа
        await Navigation.PushAsync(new CreateFinePage(_user, _token));
    }
}