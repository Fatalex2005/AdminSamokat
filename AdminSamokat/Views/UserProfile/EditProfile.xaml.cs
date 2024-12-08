using AdminSamokat.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AdminSamokat.Views;

public partial class EditProfile : ContentPage
{
    // ������������� HTTP �������
    private readonly HttpClient _httpClient = new HttpClient();
    private User _user;
    private string _token;
    public EditProfile(User user, string token)
	{
		InitializeComponent();
        _user = user;
        _token = token;
        surnameLabel.Text = user.Surname;
        nameLabel.Text = user.Name;
        if (string.IsNullOrEmpty(user.Patronymic))
        {
            patronymicLabel.Text = "";
        }
        else
        {
            patronymicLabel.Text = user.Patronymic;
        }
        loginLabel.Text = user.Login;
        passwordLabel.Text = user.Password;
        confirmPasswordLabel.Text = user.Password;
    }

    private async void OnSaveButtonClicked(object sender, EventArgs e)
{
    // �������� �� ������ ����
    if (string.IsNullOrWhiteSpace(surnameLabel.Text) ||
        string.IsNullOrWhiteSpace(nameLabel.Text) ||
        string.IsNullOrWhiteSpace(passwordLabel.Text))
    {
        await DisplayAlert("������", "��� ������������ ���� ������ ���� ���������", "��");
        return;
    }

    if (passwordLabel.Text != confirmPasswordLabel.Text)
    {
        await DisplayAlert("������", "������ �� ���������", "��");
        return;
    }

    // ��������� ������ ��� ����������
    var updatedUser = new Dictionary<string, object>
    {
        { "surname", surnameLabel.Text },
        { "name", nameLabel.Text },
        { "patronymic", string.IsNullOrEmpty(patronymicLabel.Text) ? null : patronymicLabel.Text },
        { "password", passwordLabel.Text }
    };

    // ��������� ���� "login", ������ ���� ��� ����������
    if (loginLabel.Text != _user.Login)
    {
        updatedUser.Add("login", loginLabel.Text);
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
        HttpResponseMessage response = await _httpClient.PutAsync($"http://courseproject4/api/profile/{_user.Id}", jsonContent);

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            // ��������� ��������� ����� ������ ������������
            _user.Surname = surnameLabel.Text;
            _user.Name = nameLabel.Text;
            _user.Patronymic = string.IsNullOrEmpty(patronymicLabel.Text) ? null : patronymicLabel.Text;
            _user.Password = passwordLabel.Text;

            if (loginLabel.Text != _user.Login)
            {
                _user.Login = loginLabel.Text;
            }

            Preferences.Set("UserSurname", _user.Surname);
            Preferences.Set("UserName", _user.Name);
            Preferences.Set("UserPatronymic", _user.Patronymic ?? "");
            Preferences.Set("UserLogin", _user.Login);
            Preferences.Set("UserPassword", _user.Password);

            // ��������� UI
            surnameLabel.Text = _user.Surname;
            nameLabel.Text = _user.Name;
            patronymicLabel.Text = _user.Patronymic ?? "";
            loginLabel.Text = _user.Login;
            passwordLabel.Text = _user.Password;

            await DisplayAlert("�����", "������� ������� �������!", "��������� �� �������");

            await Navigation.PushAsync(new Views.Home(_user, _token));
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