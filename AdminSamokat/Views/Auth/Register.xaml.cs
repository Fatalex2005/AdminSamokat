using System.Net.Http.Json;
using System.Text.Json;
using AdminSamokat.Models;

namespace AdminSamokat.Views.Auth;

public partial class Register : ContentPage
{
    // ������������� HTTP �������
    private readonly HttpClient _httpClient = new HttpClient();
    public Register()
    {
        InitializeComponent();
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
            // ���������� ������ � ���������� ����� � response
            HttpResponseMessage response = await _httpClient.PostAsync("http://courseproject4/api/register", registerData);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AuthResponse>(content);

                if (result?.Token != null)
                {
                    await DisplayAlert("�������� ����������� ������ ������������", "� ������� �������� ����� ������������", "��");
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableContent)
            {
                await DisplayAlert("������ ��������� ������", "�� ���-�� ����� �������", "��");
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




    }
}