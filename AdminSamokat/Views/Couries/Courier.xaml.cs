using AdminSamokat.Models;
using System.Text.Json;

namespace AdminSamokat.Views.Couries;

public partial class Courier : ContentPage
{
    private User _courier;
    private readonly HttpClient _httpClient = new HttpClient();

    public Courier(User courier)
    {
        InitializeComponent();
        _courier = courier;

        // Отобразить данные курьера
        CourierFullNameLabel.Text = $"{_courier.Name} {_courier.Surname} {_courier.Patronymic}";
        CourierLoginLabel.Text = _courier.Login;

        // Загружаем штрафы
        LoadFineDetails();
    }
    private async void LoadFineDetails()
    {
        try
        {
            // Загружаем информацию о штрафах по FineId
            var response = await _httpClient.GetAsync($"http://courseproject4/api/fine/{_courier.FineId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var fine = JsonSerializer.Deserialize<Fine>(content);
                // Отображаем описание штрафа
                FineLabel.Text = fine.Description;
            }
        }
        catch (Exception ex)
        {
            FineLabel.Text = "Ошибка при загрузке штрафа";
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private void OnEditButtonClicked(object sender, EventArgs e)
    {

    }

    private void OnDeleteButtonClicked(object sender, EventArgs e)
    {

    }
}