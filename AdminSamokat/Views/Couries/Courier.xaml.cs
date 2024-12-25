using AdminSamokat.Models;
using System.Text.Json;

namespace AdminSamokat.Views.Couries;

public partial class Courier : ContentPage
{
    private User _courier;
    private User _user;
    private string _token;
    private readonly HttpClient _httpClient = new HttpClient();

    public Courier(User courier, User user, string token)
    {
        InitializeComponent();
        _user = user;
        _token = token;
        _courier = courier;

        // Отобразить данные курьера
        CourierFullNameLabel.Text = $"{_courier.Surname} {_courier.Name} {_courier.Patronymic}";
        CourierLoginLabel.Text = _courier.Login;

        // Загружаем штраф
        LoadFineDetails();

        // Загружаем статус
        LoadStatusDetails();
    }
    private async void LoadFineDetails()
    {
        var token = Preferences.Get("UserToken", string.Empty);
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        try
        {
            // Включаем индикатор загрузки у штрафа
            LoadingFineIndicator.IsRunning = true;
            LoadingFineIndicator.IsVisible = true;

            // Загружаем информацию о штрафах
            var responseFine = await _httpClient.GetAsync($"http://courseproject4/api/fine/{_courier.FineId}");
            var contentFine = await responseFine.Content.ReadAsStringAsync();


            if (responseFine.IsSuccessStatusCode)
            {
                var fine = JsonSerializer.Deserialize<Fine>(contentFine);
                FineLabel.Text = fine.Description;
            }
            else
            {
                FineLabel.Text = "Не удалось загрузить штраф";
                await DisplayAlert("Ошибка", $"Код: {responseFine.StatusCode}, Ответ: {contentFine}", "OK");
            }
        }
        catch (Exception ex)
        {
            FineLabel.Text = "Ошибка при загрузке штрафа";
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
        finally
        {
            // Выключаем индикатор загрузки
            LoadingFineIndicator.IsRunning = false;
            LoadingFineIndicator.IsVisible = false;
        }
    }

    private async void LoadStatusDetails()
    {
        var token = Preferences.Get("UserToken", string.Empty);
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        try
        {
            // Включаем индикатор загрузки у статуса
            LoadingStatusIndicator.IsRunning = true;
            LoadingStatusIndicator.IsVisible = true;

            // Загружаем информацию о статусе
            var responseStatus = await _httpClient.GetAsync($"http://courseproject4/api/status/{_courier.StatusId}");
            var contentStatus = await responseStatus.Content.ReadAsStringAsync();

            if (responseStatus.IsSuccessStatusCode)
            {
                var status = JsonSerializer.Deserialize<Status>(contentStatus);
                StatusLabel.Text = status.Name;
            }
            else
            {
                FineLabel.Text = "Не удалось загрузить статус";
                await DisplayAlert("Ошибка", $"Код: {responseStatus.StatusCode}, Ответ: {contentStatus}", "OK");
            }
        }
        catch (Exception ex)
        {
            FineLabel.Text = "Ошибка при загрузке статуса";
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
        finally
        {
            // Выключаем индикатор загрузки
            LoadingStatusIndicator.IsRunning = false;
            LoadingStatusIndicator.IsVisible = false;
        }
    }

    private async void OnEditButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new EditProfileCourier(_courier, _user, _token));
    }

    private async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        // Подтверждение перед удалением
        bool isConfirmed = await DisplayAlert(
            "Подтверждение",
            "Вы уверены, что хотите удалить этого курьера?",
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
            DeleteLoadingIndicator.IsRunning = true;
            DeleteLoadingIndicator.IsVisible = true;

            // Настраиваем заголовки и токен
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

            // Отправляем DELETE-запрос на сервер
            var response = await _httpClient.DeleteAsync($"http://courseproject4/api/profile/{_courier.Id}");

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Успех", "Курьер успешно удалён.", "ОК");

                await Navigation.PushAsync(new Home(_user, _token));
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Ошибка", $"Не удалось удалить курьера: {response.StatusCode} - {responseContent}", "ОК");
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
            DeleteLoadingIndicator.IsRunning = false;
            DeleteLoadingIndicator.IsVisible = false;
        }
    }
}