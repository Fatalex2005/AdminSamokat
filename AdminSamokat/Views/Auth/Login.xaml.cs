using AdminSamokat.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AdminSamokat.Views.Auth;

public partial class Login : ContentPage
{
    // ������������� HTTP �������
    private readonly HttpClient _httpClient = new HttpClient();
    public Login()
    {
        InitializeComponent();
    }
    // ��������������
    private async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        // ��������� ����� � ������ �� �����
        string login = LoginEntry.Text;
        string password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("������", "������� ����� � ������", "��");
            return;
        }
        var loginResponse = await AuthenticateUserAsync(login, password);
        if (loginResponse != null)
        {
            // ��������� ������ ������������ � �����
            Preferences.Set("UserToken", loginResponse.Token);
            Preferences.Set("UserSurname", loginResponse.User.Surname);
            Preferences.Set("UserName", loginResponse.User.Name);
            Preferences.Set("UserPatronymic", loginResponse.User.Patronymic);
            Preferences.Set("UserLogin", loginResponse.User.Login);
            Preferences.Set("UserPassword", loginResponse.User.Password);
            // ������� ������ ������������ � ��� ����� �� �������� Home
            await Navigation.PushAsync(new Home(loginResponse.User, loginResponse.Token));
        }
    }
    private async Task<AuthResponse> AuthenticateUserAsync(string login, string password)
    {
        // ������������ ���� ��� ��������
        var loginData = new { login, password };
        // �������������� ������ � ��� ��������
        var jsonContent = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

        // ������ �������
        try
        {
            // ���������� ������ � ���������� ����� � response
            HttpResponseMessage response = await _httpClient.PostAsync("http://courseproject4/api/login", jsonContent);

            // ���� ����� ����� 200
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AuthResponse>(content);
                if (result?.Token != null)
                {
                    return result;
                }
            }
            // ���� ����� 401
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await DisplayAlert("������ �����", "������������ ����� ��� ������", "��");
            }
            else
            {
                await DisplayAlert("������", "��������� ������ �� �������", "��");
            }


        }
        catch (Exception ex)
        {
            await DisplayAlert("������ ����", ex.Message, "��");
        }
        return null;
    }

}