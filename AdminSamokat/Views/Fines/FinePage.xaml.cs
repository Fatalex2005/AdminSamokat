using AdminSamokat.Models;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;

namespace AdminSamokat.Views.Fines;

public partial class FinePage : ContentPage
{
    private User _user;
    private string _token;
    private Fine _fine;
    private readonly HttpClient _httpClient = new HttpClient();
    public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();
    public FinePage(Fine fine, User user, string token)
	{
		InitializeComponent();
        _fine = fine;
        _token = token;
        _user = user;
        UsersCollectionView.ItemsSource = Users;
        // Отобразить данные штрафа
        FineDescriptionLabel.Text = "Кому вы хотите назначить \"" + _fine.Description + "\"?";

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
                var couriers =
                    users.Where(user => user.RoleId == 2); // Предполагается, что RoleId указывает роль пользователя

                foreach (var courier in couriers)
                {
                    Users.Add(courier);
                }

                // Управляем видимостью элементов
                UsersCollectionView.IsVisible = Users.Count > 0;
                EmptyMessageLabel.IsVisible = Users.Count == 0;
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
            await Navigation.PushAsync(new Couries.Courier(selectedCourier, _user, _token));
        }

        // Сбрасываем выбор
        ((CollectionView)sender).SelectedItem = null;
    }

    private async void OnEditButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new EditFine(_fine, _user, _token));
    }

    private async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        // Подтверждение перед удалением
        bool isConfirmed = await DisplayAlert(
            "Подтверждение",
            "Вы уверены, что хотите удалить этот штраф?",
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
            var response = await _httpClient.DeleteAsync($"http://courseproject4/api/fine/{_fine.Id}");

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Успех", "Штраф успешно удалён.", "ОК");

                await Navigation.PushAsync(new Home(_user, _token));
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Ошибка", $"Не удалось удалить штраф: {response.StatusCode} - {responseContent}", "ОК");
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

    private async void OnAssignButtonClicked(object sender, EventArgs e)
    {
        // Получаем курьера, связанного с кнопкой
        var selectedCourier = ((Button)sender).BindingContext as User;

        if (selectedCourier != null)
        {
            // Проверяем, назначен ли уже этот штраф
            if (selectedCourier.FineId == _fine.Id)
            {
                await DisplayAlert("Внимание", "Пользователю уже назначен этот штраф.", "ОК");
                return; // Прекращаем выполнение, если штраф уже назначен
            }

            // Подтверждение перед назначением штрафа
            bool isConfirmed = await DisplayAlert(
                "Подтверждение",
                $"Вы точно хотите назначить штраф \"{_fine.Description}\" курьеру {selectedCourier.Name}?",
                "Да",
                "Нет"
            );

            if (!isConfirmed)
            {
                return; // Прекращаем выполнение, если пользователь отменил действие
            }

            try
            {
                // Настраиваем запрос
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                // Формируем данные для обновления
                var updateData = new Dictionary<string, object>
            {
                { "fine_id", _fine.Id } // Устанавливаем ID текущего штрафа
            };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(updateData),
                    Encoding.UTF8,
                    "application/json"
                );

                // Отправляем запрос на сервер
                var response = await _httpClient.PutAsync($"http://courseproject4/api/profile/{selectedCourier.Id}", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    // Обновляем локально данные курьера
                    selectedCourier.FineId = _fine.Id;

                    await DisplayAlert("Успех", "Штраф \"" + _fine.Description + $"\" успешно назначен курьеру {selectedCourier.Name}!", "ОК");

                    var courierToUpdate = Users.FirstOrDefault(u => u.Id == selectedCourier.Id);
                    if (courierToUpdate != null)
                    {
                        courierToUpdate.FineId = _fine.Id;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Ошибка", $"Не удалось назначить штраф: {response.StatusCode} - {errorContent}", "ОК");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "ОК");
            }
        }
    }
}