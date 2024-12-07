using AdminSamokat.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AdminSamokat.Views.Bonuses;

public partial class EditBonus : ContentPage
{
    // ������������� HTTP �������
    private readonly HttpClient _httpClient = new HttpClient();
    private Bonus _bonus;
    private User _user;
    private string _token;
    public EditBonus(Bonus bonus, User user, string token)
    {
        InitializeComponent();
        _user = user;
        _token = token;
        _bonus = bonus;
        titleLabel.Text = bonus.Title;
        descriptionLabel.Text = bonus.Description;
        priceLabel.Text = bonus.Price;
    }

    private async void OnSaveButtonClicked(object sender, EventArgs e)
    {
        // �������� �� ������ ����
        if (string.IsNullOrWhiteSpace(titleLabel.Text) ||
            string.IsNullOrWhiteSpace(descriptionLabel.Text) ||
            string.IsNullOrWhiteSpace(priceLabel.Text))
        {
            await DisplayAlert("������", "��� ������������ ���� ������ ���� ���������", "��");
            return;
        }

        // ��������� ������ ��� ����������
        var updatedBonus = new
        {
            Title = titleLabel.Text,
            Description = descriptionLabel.Text,
            Price = priceLabel.Text
        };
        // ����������� ������������ ��� �������������� ������ � ������ �������
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase // ����������� ����� � camelCase
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(updatedBonus, options), Encoding.UTF8, "application/json");


        // ������ �������
        try
        {
            // �������� ����� � ���������� ��������� ��������
            MainContent.IsVisible = false;
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            // ���������� ������ � ���������� ����� � response
            var token = Preferences.Get("UserToken", string.Empty);
            _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await _httpClient.PutAsync($"http://courseproject4/api/bonus/{_bonus.Id}", jsonContent);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _bonus.Title = updatedBonus.Title;
                _bonus.Description = updatedBonus.Description;
                _bonus.Price = updatedBonus.Price;

                // ��������� UI
                titleLabel.Text = updatedBonus.Title;
                descriptionLabel.Text = updatedBonus.Description;
                priceLabel.Text = updatedBonus.Price;

                await DisplayAlert("�����", "����� ������� �������!", "��������� �� �������");

                await Navigation.PushAsync(new Views.Home(_user, token));
                Navigation.RemovePage(this); // ������� ������� �������� �� �����
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await DisplayAlert("������", "������ �������. ������������� �����.", "OK");
                await Navigation.PushAsync(new Views.Auth.Login());
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("������", $"�� ������� �������� �������: {responseContent}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("������", $"��������� ������: {ex.Message}", "OK");
        }
        finally
        {
            // ��������������� ���������, ���� ����������� �� �������
            MainContent.IsVisible = true;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}