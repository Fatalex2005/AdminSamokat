using AdminSamokat.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;

namespace AdminSamokat.Views.Bonuses;

public partial class CreateBonusPage : ContentPage
{
    private User _user;
    private string _token;
    private readonly HttpClient _httpClient = new HttpClient();
    public CreateBonusPage(User user, string token)
    {
        InitializeComponent();
        _user = user;
        _token = token;
        // ������������� ������ ����� � Picker
        rolePicker.ItemsSource = Roles;
        // ������������� ��������� ���� �� ���������
        if (Roles.Count > 0)
        {
            rolePicker.SelectedItem = Roles[0];
        }
    }
    private static readonly List<Role> Roles = new List<Role>
{
    new Role { Id = 1, Name = "�������������", Code = "admin" },
    new Role { Id = 2, Name = "������", Code = "courier" }
};
    private async void OnCreateButtonClicked(object sender, EventArgs e)
    {
        // �������� �� ������ ����
        if (string.IsNullOrWhiteSpace(titleEntry.Text) ||
            string.IsNullOrWhiteSpace(descriptionEntry.Text) ||
            string.IsNullOrWhiteSpace(priceEntry.Text) ||
            rolePicker.SelectedItem == null)
        {
            await DisplayAlert("������", "��� ������������ ���� ������ ���� ���������", "��");
            return;
        }
        // ������������� ����� ���������
        bool isConfirmed = await DisplayAlert(
            "�������������",
            "�� �������, ��� ������ �������� ����� �����?",
            "��",
            "���"
        );
        if (!isConfirmed)
        {
            // ���� ������������ ������ "���", �������� ����������
            return;
        }
        // ���� ������ �� �����
        string title = titleEntry.Text;
        string description = descriptionEntry.Text;
        string price = priceEntry.Text;
        var selectedRole = (Role)rolePicker.SelectedItem;
        var createData = new MultipartFormDataContent
    {
        { new StringContent(title), "title" },
        { new StringContent(description), "description" },
        { new StringContent(price), "price" },
        { new StringContent(selectedRole.Id.ToString()), "role_id" } // ��������� role_id
    };
        try
        {
            // ���������� ��������� �������� � �������� �����
            CreateForm.IsVisible = false;
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            HttpResponseMessage response = await _httpClient.PostAsync("http://courseproject4/api/bonus", createData);
            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("�����", "����� ������� ��������", "��");
                await Navigation.PushAsync(new Views.Home(_user, _token));
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
                var errorContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("������", $"��� ������: {(int)response.StatusCode}\n{errorContent}", "��");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("������ ����", ex.Message, "��");
        }
        finally
        {
            // ��������������� ����� � �������� ��������� ��������
            CreateForm.IsVisible = true;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}