using AdminSamokat.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AdminSamokat.Views.Fines;

public partial class EditFine : ContentPage
{
    // ������������� HTTP �������
    private readonly HttpClient _httpClient = new HttpClient();
    private Fine _fine;
    private User _user;
    private string _token;
    public EditFine(Fine fine, User user, string token)
	{
		InitializeComponent();
        _fine = fine;
        _user = user;
        _token = token;
        descriptionLabel.Text = fine.Description;
    }
    private async void OnSaveButtonClicked(object sender, EventArgs e)
    {
        // �������� �� ������ ����
        if (string.IsNullOrWhiteSpace(descriptionLabel.Text))
        {
            await DisplayAlert("������", "��� ������������ ���� ������ ���� ���������", "��");
            return;
        }
        // ������������� ����� ����������� ���������
        bool isConfirmed = await DisplayAlert(
            "�������������",
            "�� �������, ��� ������ ��������� ���������?",
            "��",
            "���"
        );
        if (!isConfirmed)
        {
            // ���� ������������ ������ "���", ���������� ����������
            return;
        }
        // ��������� ������ ��� ����������
        var updatedFine = new
        {
            Description = descriptionLabel.Text,
        };
        // ����������� ������������ ��� �������������� ������ � ������ �������
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase // ����������� ����� � camelCase
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(updatedFine, options), Encoding.UTF8, "application/json");
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
            HttpResponseMessage response = await _httpClient.PutAsync($"http://courseproject4/api/fine/{_fine.Id}", jsonContent);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _fine.Description = updatedFine.Description;
                // ��������� UI
                descriptionLabel.Text = updatedFine.Description;
                await DisplayAlert("�����", "����� ������� �������!", "��������� �� �������");
                await Navigation.PushAsync(new Views.Home(_user, token));
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                try
                {
                    // ������ JSON ��� JsonDocument
                    using var document = JsonDocument.Parse(errorContent);
                    var root = document.RootElement;
                    var message = root.GetProperty("message").GetString();
                    if (root.TryGetProperty("errors", out var errors))
                    {
                        var errorMessages = new List<string>();
                        // �������� �� ���� �������
                        foreach (var error in errors.EnumerateObject())
                        {
                            foreach (var msg in error.Value.EnumerateArray())
                            {
                                errorMessages.Add(System.Text.RegularExpressions.Regex.Unescape(msg.GetString()));
                            }
                        }
                        var combinedErrors = string.Join("\n", errorMessages);
                        await DisplayAlert("������ ���������", combinedErrors, "��");
                    }
                    else
                    {
                        await DisplayAlert("������", message ?? "��������� ������ ���������.", "��");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("������", $"�� ������� ���������� �����: {ex.Message}\n\n�����: {errorContent}", "��");
                }
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("������", $"�� ������� �������� �����: {responseContent}", "OK");
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