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

        // ���������� ������ �������
        CourierFullNameLabel.Text = $"{_courier.Name} {_courier.Surname} {_courier.Patronymic}";
        CourierLoginLabel.Text = _courier.Login;

        // ��������� ������
        LoadFineDetails();
    }
    private async void LoadFineDetails()
    {
        try
        {
            // ��������� ���������� � ������� �� FineId
            var response = await _httpClient.GetAsync($"http://courseproject4/api/fine/{_courier.FineId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var fine = JsonSerializer.Deserialize<Fine>(content);
                // ���������� �������� ������
                FineLabel.Text = fine.Description;
            }
        }
        catch (Exception ex)
        {
            FineLabel.Text = "������ ��� �������� ������";
            await DisplayAlert("������", ex.Message, "OK");
        }
    }

    private void OnEditButtonClicked(object sender, EventArgs e)
    {

    }

    private void OnDeleteButtonClicked(object sender, EventArgs e)
    {

    }
}