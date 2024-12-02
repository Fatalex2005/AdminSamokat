using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AdminSamokat.Models;

namespace AdminSamokat.Views.Auth;

public partial class Register : ContentPage
{
    // ������������� HTTP �������
    private readonly HttpClient _httpClient = new HttpClient();
    private User _user;
    private string _token;
    public Register(User user, string token)
    {
        InitializeComponent();
        _user = user;
        _token = token;
    }

    // �����������
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

        // ���� ������ �� �����
        string surname = SurnameEntry.Text;
        string name = NameEntry.Text;
        string? patronymic = PatronymicEntry.Text;
        string login = LoginEntry.Text;
        string password = PasswordEntry.Text;

        // ������������ ���� �������
        var registerData = new MultipartFormDataContent
        {
            {new StringContent(surname), "surname" },
            {new StringContent(name), "name" },
            {new StringContent(patronymic ?? string.Empty), "patronymic" },
            {new StringContent(login), "login" },
            {new StringContent(password), "password" },
        };

        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            // ���������� ������ � ���������� ����� � response
            HttpResponseMessage response = await _httpClient.PostAsync("http://courseproject4/api/register", registerData);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AuthResponse>(content);

                if (result?.Token != null)
                {
                    await DisplayAlert("�������� �����������", "������������ ������� ��������", "��");
                }
                else
                {
                    await DisplayAlert("������", "�� ������� �������� ������ ������������ �� ������ �������", "��");
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("������ ���������", errorContent, "��");
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




    }
}