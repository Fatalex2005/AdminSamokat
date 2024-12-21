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
    private List<Grouping<string, Access>> _groupedAccesses;

    public AllAccesses(User user, string token)
    {
        InitializeComponent();
        _user = user;
        _token = token;
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
                        access.UserFullName = $"{user.Surname} {user.Name}";
                        access.StartTime = $"Начало: {access.StartChange.ToString(@"hh\:mm")}";
                        access.EndTime = $"Конец: {access.EndChange.ToString(@"hh\:mm")}";
                    }
                }

                _groupedAccesses = accesses
                    .GroupBy(a => a.Date.ToShortDateString())
                    .OrderBy(group => DateTime.Parse(group.Key)) // Сортируем группы по дате
                    .Select(group => new Grouping<string, Access>(group.Key, group.OrderBy(a => a.StartChange))) // Сортируем элементы внутри группы
                    .ToList();

                // Устанавливаем начальные значения для иконок
                foreach (var group in _groupedAccesses)
                {
                    group.IsExpanded = false; // По умолчанию группы скрыты
                    foreach (var access in group)
                    {
                        access.IsVisible = false; // Скрываем доступности по умолчанию
                    }
                }

                AccessesCollectionView.ItemsSource = _groupedAccesses;

                EmptyMessageLabel.IsVisible = !_groupedAccesses.Any();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Ошибка авторизации
                await DisplayAlert("Ошибка", "Ваш токен не актуален. Авторизуйтесь заново!", "ОК");
                await Navigation.PushAsync(new Views.Auth.Login());
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                EmptyMessageLabel.IsVisible = true;
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
        public bool IsExpanded { get; set; } // Состояние раскрытия группы
        public string IconSource => IsExpanded ? "up_icon.png" : "down_icon.png"; // Источник иконки

        public Grouping(K key, IEnumerable<T> items) : base(items)
        {
            Key = key;
        }

        public void Toggle()
        {
            IsExpanded = !IsExpanded;
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(IconSource))); // Уведомление об изменении
        }
    }

    private void OnDateFrameTapped(object sender, EventArgs e)
    {
        var frame = ((sender as Element)?.Parent as Frame) ?? (sender as Frame);

        if (frame != null)
        {
            var selectedDate = frame.BindingContext as Grouping<string, Access>;

            if (selectedDate != null)
            {
                // Переключаем состояние группы и уведомляем об изменении
                selectedDate.Toggle();

                // Переключаем видимость доступностей
                foreach (var access in selectedDate)
                {
                    access.IsVisible = selectedDate.IsExpanded;
                }

                // Обновляем CollectionView
                AccessesCollectionView.ItemsSource = null;
                AccessesCollectionView.ItemsSource = _groupedAccesses;
            }
        }
    }

}