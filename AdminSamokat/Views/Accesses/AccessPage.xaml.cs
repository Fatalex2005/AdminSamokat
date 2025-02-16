using AdminSamokat.Models;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Net.Http;
using System.Text.Json;

namespace AdminSamokat.Views.Accesses;

public partial class AccessPage : ContentPage
{
    private User _user;
    private string _token;
    private Access _access;
    private readonly HttpClient _httpClient = new HttpClient();
    public AccessPage(Access access, User user, string token)
    {
        InitializeComponent();
        _access = access;
        _user = user;
        _token = token;
        // Устанавливаем BindingContext
        BindingContext = _access;
        FillAccessDetails();
    }
    // Метод для заполнения информации о доступности
    private void FillAccessDetails()
    {
        // Отображаем имя пользователя
        UserFullNameLabel.Text = _access.UserFullName;
        // Отображаем время начала и конца
        StartTimeLabel.Text = $"Начало: {_access.StartChange.ToString(@"hh\:mm")}";
        EndTimeLabel.Text = $"Конец: {_access.EndChange.ToString(@"hh\:mm")}";
        // Отображаем дату доступности
        DateLabel.Text = $"Доступность на: {_access.Date.ToString("dd.MM.yyyy")}";
        // Отображаем статус подтверждения
        ConfirmStatusLabel.Text = _access.Confirm == 1 ? "Статус: Подтверждено" : "Статус: Не подтверждено";
    }
    // Обработчик нажатия на кнопку для просмотра пользователя
    private async void OnViewUserButtonClicked(object sender, EventArgs e)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
        var userResponse = await _httpClient.GetAsync($"http://courseproject4/api/profile/{_access.UserId}");
        if (userResponse.IsSuccessStatusCode)
        {
            var userContent = await userResponse.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<User>(userContent);

            // Переход на страницу курьера
            await Navigation.PushAsync(new Couries.Courier(user, _user, _token));
        }
        else
        {
            await DisplayAlert("Ошибка", "Не удалось загрузить информацию о пользователе.", "ОК");
        }
    }
    // Обработчик нажатия на кнопку для подтверждения доступности
    private async void OnConfirmButtonClicked(object sender, EventArgs e)
    {
        // Подтверждение перед подтверждением
        bool isConfirmed = await DisplayAlert(
            "Подтверждение",
            "Вы уверены, что хотите подтвердить доступность?",
            "Да",
            "Нет"
        );
        if (!isConfirmed)
        {
            // Если пользователь выбрал "Нет", создание отменяется
            return;
        }
        // Показываем индикатор загрузки и скрываем текст и иконку
        LoadingConfirmIndicator.IsRunning = true;
        LoadingConfirmIndicator.IsVisible = true;
        ConfirmButtonText.IsVisible = false;
        ConfirmButtonIcon.IsVisible = false;
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("confirm", "1")
        });
        try
        {
            var response = await _httpClient.PatchAsync(
                $"http://courseproject4/api/accesses-confirm/{_access.Id}", content);
            if (response.IsSuccessStatusCode)
            {
                // Останавливаем индикатор загрузки перед вызовом DisplayAlert
                LoadingConfirmIndicator.IsRunning = false;
                LoadingConfirmIndicator.IsVisible = false;
                ConfirmButtonText.IsVisible = true;
                ConfirmButtonIcon.IsVisible = true;
                // Не показываем кнопку подтверждения
                ConfirmButtonFrame.IsVisible = false;
                // Показываем кнопку отмены
                CancelButtonFrame.IsVisible = true;
                PartialConfirmButtonFrame.IsVisible = false;
                PartialCancelButtonFrame.IsVisible = true;
                ConfirmStatusLabel.Text = "Статус: Подтверждено";
                await DisplayAlert("Успех", "Доступность подтверждена.", "ОК");
                // Обновляем свойство Confirm
                _access.Confirm = 1;
                // Сообщаем об изменении свойств для привязки
                OnPropertyChanged(nameof(_access.Confirm));
                await Navigation.PushAsync(new Home(_user, _token));
            }
            else
            {
                // Останавливаем индикатор загрузки перед вызовом DisplayAlert
                LoadingConfirmIndicator.IsRunning = false;
                LoadingConfirmIndicator.IsVisible = false;
                ConfirmButtonText.IsVisible = true;
                ConfirmButtonIcon.IsVisible = true;

                var errorContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Ошибка", $"Не удалось подтвердить доступность. Ответ: {errorContent}", "ОК");
            }
        }
        catch (Exception ex)
        {
            // Останавливаем индикатор загрузки перед вызовом DisplayAlert
            LoadingConfirmIndicator.IsRunning = false;
            LoadingConfirmIndicator.IsVisible = false;
            ConfirmButtonText.IsVisible = true;
            ConfirmButtonIcon.IsVisible = true;
            await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "ОК");
        }
    }
    private async void OnCancelButtonClicked(object sender, EventArgs e)
    {
        // Подтверждение перед отменой
        bool isConfirmed = await DisplayAlert(
            "Подтверждение",
            "Вы уверены, что хотите отменить доступность?",
            "Да",
            "Нет"
        );
        if (!isConfirmed)
        {
            return;
        }
        // Показываем индикатор загрузки и скрываем текст и иконку
        LoadingCancelIndicator.IsRunning = true;
        LoadingCancelIndicator.IsVisible = true;
        CancelButtonText.IsVisible = false;
        CancelButtonIcon.IsVisible = false;
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
        var content = new FormUrlEncodedContent(new[]
        {
        new KeyValuePair<string, string>("confirm", "0")
    });
        try
        {
            var response = await _httpClient.PatchAsync(
                $"http://courseproject4/api/accesses-cancel/{_access.Id}", content);
            if (response.IsSuccessStatusCode)
            {
                // Останавливаем индикатор загрузки
                LoadingCancelIndicator.IsRunning = false;
                LoadingCancelIndicator.IsVisible = false;
                CancelButtonText.IsVisible = true;
                CancelButtonIcon.IsVisible = true;
                // Показываем кнопку подтверждения
                ConfirmButtonFrame.IsVisible = true;
                // Не показываем кнопку отмены
                CancelButtonFrame.IsVisible = false;
                PartialConfirmButtonFrame.IsVisible = true;
                PartialCancelButtonFrame.IsVisible = false;
                // Обновляем статус
                ConfirmStatusLabel.Text = "Статус: Не подтверждено";
                await DisplayAlert("Успех", "Доступность отменена.", "ОК");
                // Обновляем свойство Confirm
                _access.Confirm = 0;
                // Сообщаем об изменении свойств для привязки
                OnPropertyChanged(nameof(_access.Confirm));
                await Navigation.PushAsync(new Home(_user, _token));
            }
            else
            {
                // Останавливаем индикатор загрузки
                LoadingCancelIndicator.IsRunning = false;
                LoadingCancelIndicator.IsVisible = false;
                CancelButtonText.IsVisible = true;
                CancelButtonIcon.IsVisible = true;

                var errorContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Ошибка", $"Не удалось отменить доступность. Ответ: {errorContent}", "ОК");
            }
        }
        catch (Exception ex)
        {
            // Останавливаем индикатор загрузки
            LoadingCancelIndicator.IsRunning = false;
            LoadingCancelIndicator.IsVisible = false;
            CancelButtonText.IsVisible = true;
            CancelButtonIcon.IsVisible = true;

            await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "ОК");
        }
    }
    private async void OnPartialConfirmButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PartialAccessPage(_access, _user, _token));
    }
    private async void OnPartialCancelButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PartialAccessPage(_access, _user, _token));
    }
}