using AdminSamokat.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;

namespace AdminSamokat.Views.Accesses;

public partial class AllAccesses : ContentPage
{
    private User _user;
    private string _token;
    private readonly HttpClient _httpClient = new HttpClient();
    public ObservableCollection<Access> Accesses { get; set; } = new ObservableCollection<Access>();

    public AllAccesses(User user, string token)
    {
        InitializeComponent();
        _user = user;
        _token = token;
        AccessesCollectionView.ItemsSource = Accesses;
        LoadAccesses();

    }

    private async void LoadAccesses()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var token = Preferences.Get("UserToken", string.Empty);
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("http://courseproject4/api/accesses");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var accesses = JsonSerializer.Deserialize<List<Access>>(content);

                // Загружаем пользователей для каждой доступности
                foreach (var access in accesses)
                {
                    var userResponse = await _httpClient.GetAsync($"http://courseproject4/api/profile/{access.UserId}");
                    if (userResponse.IsSuccessStatusCode)
                    {
                        var userContent = await userResponse.Content.ReadAsStringAsync();
                        var user = JsonSerializer.Deserialize<User>(userContent);
                        access.UserFullName = $"{user.Surname} {user.Name} {user.Patronymic}";
                    }
                }

                var groupedAccesses = accesses
                    .GroupBy(a => a.Date.ToShortDateString())
                    .Select(group => new Grouping<string, Access>(group.Key, group))
                    .ToList();

                AccessesCollectionView.ItemsSource = groupedAccesses;

                AccessesCollectionView.IsVisible = groupedAccesses.Any();
                EmptyMessageLabel.IsVisible = !groupedAccesses.Any();
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
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }


    public class Grouping<K, T> : ObservableCollection<T>
    {
        public K Key { get; }

        public Grouping(K key, IEnumerable<T> items) : base(items)
        {
            Key = key;
        }
    }
    private async void OnAccessSelected(object sender, SelectionChangedEventArgs e)
    {
        // Логика, которая будет срабатывать при выборе другого элемента доступности
        if (e.CurrentSelection.FirstOrDefault() is Access selectedAccess)
        {
            // Здесь можно добавить логику для перехода на страницу доступа (или оставить как есть)
            await Navigation.PushAsync(new AccessPage(selectedAccess, _user, _token));
        }

    // Сбрасываем выбор
    ((CollectionView)sender).SelectedItem = null;
    }
}