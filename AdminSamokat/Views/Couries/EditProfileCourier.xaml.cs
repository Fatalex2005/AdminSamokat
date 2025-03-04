using AdminSamokat.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AdminSamokat.Views.Couries;

public partial class EditProfileCourier : ContentPage
{
    // ������������� HTTP �������
    private readonly HttpClient _httpClient = new HttpClient();
    private User _courier;
    private User _user;
    private string _token;
    public EditProfileCourier(User courier, User user, string token)
    {
        InitializeComponent();
        _user = user;
        _token = token;
        _courier = courier;
        surnameLabel.Text = courier.Surname;
        nameLabel.Text = courier.Name;
        if (string.IsNullOrEmpty(courier.Patronymic))
        {
            patronymicLabel.Text = "";
        }
        else
        {
            patronymicLabel.Text = courier.Patronymic;
        }
        loginLabel.Text = courier.Login;
        LoadStatuses();
    }
    private async void OnSaveButtonClicked(object sender, EventArgs e)
    {
        // �������� �� ������ ����
        if (string.IsNullOrWhiteSpace(surnameLabel.Text) ||
            string.IsNullOrWhiteSpace(nameLabel.Text) ||
            string.IsNullOrWhiteSpace(loginLabel.Text))
        {
            await DisplayAlert("������", "��� ������������ ���� ������ ���� ���������", "��");
            return;
        }
        if (!checkPassword.IsChecked && passwordLabel.Text != confirmPasswordLabel.Text)
        {
            await DisplayAlert("������", "������ �� ���������", "��");
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
        var selectedStatus = StatusPicker.SelectedItem as Status;
        // ��������� ������ ��� ����������
        var updatedUser = new Dictionary<string, object>
        {
            { "surname", surnameLabel.Text },
            { "name", nameLabel.Text },
            { "patronymic", string.IsNullOrEmpty(patronymicLabel.Text) ? null : patronymicLabel.Text },
            // ��������� ��������� ID �������
            { "status_id", selectedStatus.Id }
        };
        // ��������� ���� "login", ������ ���� ��� ����������
        if (loginLabel.Text != _courier.Login)
        {
            updatedUser.Add("login", loginLabel.Text);
        }
        // ��������� ���� "password", ������ ���� ������� �����
        if (!checkPassword.IsChecked)
        {
            updatedUser.Add("password", passwordLabel.Text);
        }
        // ����������� ������������ ��� �������������� ������ � ������ �������
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase // ����������� ����� � camelCase
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(updatedUser, options), Encoding.UTF8, "application/json");
        // ������ �������
        try
        {
            // �������� ����� � ���������� ��������� ��������
            MainContent.IsVisible = false;
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            // ���������� ������ � ���������� ����� � response
            _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            HttpResponseMessage response = await _httpClient.PutAsync($"http://courseproject4/api/profile/{_courier.Id}", jsonContent);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                // ��������� ��������� ����� ������ ������������
                _courier.Surname = surnameLabel.Text;
                _courier.Name = nameLabel.Text;
                _courier.Patronymic = string.IsNullOrEmpty(patronymicLabel.Text) ? null : patronymicLabel.Text;
                _courier.StatusId = selectedStatus.Id; // ��������� ��������� ������
                if (loginLabel.Text != _courier.Login)
                {
                    _courier.Login = loginLabel.Text;
                }
                // ��������� UI
                surnameLabel.Text = _courier.Surname;
                nameLabel.Text = _courier.Name;
                patronymicLabel.Text = _courier.Patronymic ?? "";
                loginLabel.Text = _courier.Login;
                await DisplayAlert("�����", "������� ������� �������!", "��������� �� �������");
                await Navigation.PushAsync(new Views.Home(_user, _token));
                Navigation.RemovePage(this); // ������� ������� �������� �� �����
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
                await DisplayAlert("������", $"�� ������� �������� �������: {responseContent}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("������", $"��������� ������: {ex.Message}", "OK");
        }
        finally
        {
            // ��������������� ���������, ���� �������� �� �������
            MainContent.IsVisible = true;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
    private async void LoadStatuses()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            // ��������� ������ ��������
            var statusesResponse = await _httpClient.GetAsync("http://courseproject4/api/status");
            if (statusesResponse.IsSuccessStatusCode)
            {
                var statusesContent = await statusesResponse.Content.ReadAsStringAsync();
                var statuses = JsonSerializer.Deserialize<List<Status>>(statusesContent);
                // ����������� ������ �������� � Picker
                StatusPicker.ItemsSource = statuses;
                // ������������� ��������� ��������, ��������������� �������� ������� ������������
                StatusPicker.SelectedItem = statuses.FirstOrDefault(s => s.Id == _courier.StatusId);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("������", $"��������� ������ ��� �������� ������: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }
    private void OnCheckPasswordChanged(object sender, CheckedChangedEventArgs e)
    {
        bool isChecked = e.Value;
        // ���� ������� �����, ���������� ���� ��� ����� ������
        password.IsVisible = !isChecked;
        confirmPassword.IsVisible = !isChecked;
    }
}