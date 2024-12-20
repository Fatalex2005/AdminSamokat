using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AdminSamokat.Models;

namespace AdminSamokat.Views.Auth;

public partial class Register : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient();
    private User _user;
    private string _token;

    public Register(User user, string token)
    {
        InitializeComponent();
        _user = user;
        _token = token;
    }

    private async void OnRegisterButtonClicked(object sender, EventArgs e)
    {
        // �������� �� ������ ����
        if (string.IsNullOrWhiteSpace(SurnameEntry.Text) ||
            string.IsNullOrWhiteSpace(NameEntry.Text) ||
            string.IsNullOrWhiteSpace(LoginEntry.Text) ||
            string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            await DisplayAlert("������", "��� ������������ ���� ������ ���� ���������", "��");
            return;
        }

        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            await DisplayAlert("������", "������ �� ���������", "��");
            return;
        }

        // ������������� ����� ������������
        bool isConfirmed = await DisplayAlert(
            "�������������",
            "�� �������, ��� ������ �������� ������ ������������?",
            "��",
            "���"
        );

        if (!isConfirmed)
        {
            // ���� ������������ ������ "���", ����������� ����������
            return;
        }

        // ���� ������ �� �����
        string surname = SurnameEntry.Text;
        string name = NameEntry.Text;
        string? patronymic = PatronymicEntry.Text;
        string login = LoginEntry.Text;
        string password = PasswordEntry.Text;

        var registerData = new MultipartFormDataContent
        {
            { new StringContent(surname), "surname" },
            { new StringContent(name), "name" },
            { new StringContent(patronymic ?? string.Empty), "patronymic" },
            { new StringContent(login), "login" },
            { new StringContent(password), "password" },
        };

        try
        {
            // ���������� ��������� �������� � �������� �����
            RegistrationForm.IsVisible = false;
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            HttpResponseMessage response = await _httpClient.PostAsync("http://courseproject4/api/register", registerData);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("�������� �����������", "������������ ������� ��������", "��");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // ������ �����������
                await DisplayAlert("������", "��� ����� �� ��������. ������������� ������!", "��");
                await Navigation.PushAsync(new Views.Auth.Login());
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
            RegistrationForm.IsVisible = true;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}
