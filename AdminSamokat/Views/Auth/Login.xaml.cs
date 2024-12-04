using AdminSamokat.Models;
using System.Text;
using System.Text.Json;

namespace AdminSamokat.Views.Auth;

public partial class Login : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient();
    public Login()
    {
        InitializeComponent();
    }

    private async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        string login = LoginEntry.Text;
        string password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("������", "������� ����� � ������", "��");
            return;
        }

        try
        {
            // �������� ����� � ���������� ��������� ��������
            LoginForm.IsVisible = false;
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            var loginResponse = await AuthenticateUserAsync(login, password);

            if (loginResponse != null)
            {
                if (loginResponse.User.RoleId != 1)
                {
                    await DisplayAlert("������", "������ �������� ������ ���������������", "��");
                    return;
                }

                Preferences.Set("UserToken", loginResponse.Token);
                Preferences.Set("UserId", loginResponse.User.Id.ToString());
                Preferences.Set("UserSurname", loginResponse.User.Surname);
                Preferences.Set("UserName", loginResponse.User.Name);
                Preferences.Set("UserPatronymic", loginResponse.User.Patronymic);
                Preferences.Set("UserLogin", loginResponse.User.Login);
                Preferences.Set("UserPassword", loginResponse.User.Password);

                // ������� �� ������� ��������
                await Navigation.PushAsync(new Home(loginResponse.User, loginResponse.Token));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("������", $"��������� ������: {ex.Message}", "��");
        }
        finally
        {
            // ��������������� ���������, ���� ����������� �� �������
            LoginForm.IsVisible = true;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async Task<AuthResponse> AuthenticateUserAsync(string login, string password)
    {
        var loginData = new { login, password };
        var jsonContent = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync("http://courseproject4/api/login", jsonContent);

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AuthResponse>(content);
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await DisplayAlert("������ �����", "������������ ����� ��� ������", "��");
        }
        else
        {
            await DisplayAlert("������", "��������� ������ �� �������", "��");
        }

        return null;
    }
}
