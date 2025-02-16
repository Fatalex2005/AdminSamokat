using AdminSamokat.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AdminSamokat.Views.Accesses;

public partial class PartialAccessPage : ContentPage
{
    private Access _access;
    private User _user;
    private string _token;
    private readonly HttpClient _httpClient = new HttpClient();
    public PartialAccessPage(Access access, User user, string token)
    {
        InitializeComponent();
        _access = access;
        _user = user;
        _token = token;
        // Инициализация Picker с 24-часовым форматом и ограничением по доступности
        InitializeTimePickers();
        // Отображаем информацию о доступности
        StartTimeLabel.Text = $"Начало: {_access.StartChange.ToString(@"hh\:mm")}";
        EndTimeLabel.Text = $"Конец: {_access.EndChange.ToString(@"hh\:mm")}";
        // Отображаем дату доступности
        DateLabel.Text = $"Доступность на: {_access.Date.ToString("dd.MM.yyyy")}";
        // Отображаем имя пользователя
        UserFullNameLabel.Text = _access.UserFullName;
        // Устанавливаем текст для Label "Access" в зависимости от значения _access.Confirm
        if (_access.Confirm == 0)
        {
            Access.Text = "Подтверждение доступности";
            PartialConfirmButton.IsVisible = true; // Показываем кнопку "Подтвердить выделенное время"
            PartialCancelButton.IsVisible = false; // Скрываем кнопку "Отменить выделенное время"
        }
        else
        {
            Access.Text = "Отмена доступности";
            PartialConfirmButton.IsVisible = false; // Скрываем кнопку "Подтвердить выделенное время"
            PartialCancelButton.IsVisible = true;  // Показываем кнопку "Отменить выделенное время"
        }
    }
    // Инициализация Picker с 24-часовым форматом и ограничением по доступности
    private void InitializeTimePickers()
    {
        // Очищаем Picker
        StartTimePicker.Items.Clear();
        EndTimePicker.Items.Clear();
        // Заполняем Picker для начала времени (только доступные часы, исключая конец интервала)
        for (int hour = _access.StartChange.Hours; hour < _access.EndChange.Hours; hour++)
        {
            StartTimePicker.Items.Add(hour.ToString("00"));
        }
        // Заполняем Picker для окончания времени (только доступные часы, исключая начало интервала)
        for (int hour = _access.StartChange.Hours + 1; hour <= _access.EndChange.Hours; hour++)
        {
            EndTimePicker.Items.Add(hour.ToString("00"));
        }
        // Устанавливаем начальное значение для Picker
        StartTimePicker.SelectedIndex = 0; // Начало интервала
        EndTimePicker.SelectedIndex = EndTimePicker.Items.Count - 1; // Конец интервала
    }
    // Обработчик для кнопки частичного подтверждения
    private async void OnPartialConfirmButtonClicked(object sender, EventArgs e)
    {
        // Получаем выбранные значения времени
        var startTime = StartTimePicker.Items[StartTimePicker.SelectedIndex];
        var endTime = EndTimePicker.Items[EndTimePicker.SelectedIndex];
        // Проверка, что выбраны корректные временные интервалы
        if (int.Parse(startTime) >= int.Parse(endTime))
        {
            await DisplayAlert("Ошибка", "Время начала должно быть меньше времени окончания.", "ОК");
            return;
        }
        // Подтверждение действия
        bool isConfirmed = await DisplayAlert(
            "Подтверждение",
            "Вы уверены, что хотите подтвердить выбранную доступность?",
            "Да",
            "Нет"
        );
        if (!isConfirmed)
        {
            return;
        }
        // Показываем индикатор загрузки
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;
        PartialConfirmButton.IsEnabled = false;
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            // Отправляем запрос на сервер для частичного подтверждения
            var response = await _httpClient.PostAsync(
                $"http://courseproject4/api/accesses-partial-confirm/{_access.Id}",
                new StringContent(JsonSerializer.Serialize(new
                {
                    startChange = startTime, // Отправляем значение
                    endChange = endTime,     // Отправляем значение
                    Confirm = 1
                }), Encoding.UTF8, "application/json")
            );
            if (response.IsSuccessStatusCode)
            {
                // Останавливаем индикатор загрузки
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                PartialConfirmButton.IsEnabled = true;
                // Обновляем интерфейс
                await DisplayAlert("Успех", "Выбранная доступность подтверждена.", "ОК");
                // Возвращаемся на главную страницу
                await Navigation.PushAsync(new Home(_user, _token));
            }
            else
            {
                // Останавливаем индикатор загрузки
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                PartialConfirmButton.IsEnabled = true;
                var errorContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Ошибка", $"Не удалось подтвердить выбранную доступность. Ответ: {errorContent}", "ОК");
            }
        }
        catch (Exception ex)
        {
            // Останавливаем индикатор загрузки
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            PartialConfirmButton.IsEnabled = true;
            await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "ОК");
        }
    }

    // Обработчик для кнопки частичной отмены
    private async void OnPartialCancelButtonClicked(object sender, EventArgs e)
    {
        // Получаем выбранные значения времени
        var startTime = StartTimePicker.Items[StartTimePicker.SelectedIndex];
        var endTime = EndTimePicker.Items[EndTimePicker.SelectedIndex];
        // Проверка, что выбраны корректные временные интервалы
        if (int.Parse(startTime) >= int.Parse(endTime))
        {
            await DisplayAlert("Ошибка", "Время начала должно быть меньше времени окончания.", "ОК");
            return;
        }
        // Подтверждение действия
        bool isConfirmed = await DisplayAlert(
            "Подтверждение",
            "Вы уверены, что хотите отменить выбранную доступность?",
            "Да",
            "Нет"
        );
        if (!isConfirmed)
        {
            return;
        }
        // Показываем индикатор загрузки
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;
        PartialCancelButton.IsEnabled = false;
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            // Отправляем запрос на сервер для частичной отмены
            var response = await _httpClient.PostAsync(
                $"http://courseproject4/api/accesses-partial-cancel/{_access.Id}",
                new StringContent(JsonSerializer.Serialize(new
                {
                    startChange = startTime, // Отправляем значение
                    endChange = endTime,     // Отправляем значение
                    Confirm = 0
                }), Encoding.UTF8, "application/json")
            );
            if (response.IsSuccessStatusCode)
            {
                // Останавливаем индикатор загрузки
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                PartialCancelButton.IsEnabled = true;
                // Обновляем интерфейс
                await DisplayAlert("Успех", "Выбранная доступность отменена.", "ОК");
                // Возвращаемся на главную страницу
                await Navigation.PushAsync(new Home(_user, _token));
            }
            else
            {
                // Останавливаем индикатор загрузки
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                PartialCancelButton.IsEnabled = true;
                var errorContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Ошибка", $"Не удалось отменить выбранную доступность. Ответ: {errorContent}", "ОК");
            }
        }
        catch (Exception ex)
        {
            // Останавливаем индикатор загрузки
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            PartialCancelButton.IsEnabled = true;
            await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "ОК");
        }
    }
}