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
    }

    private async void OnSaveButtonClicked(object sender, EventArgs e)
    {
        // �������� �� ������ ����
        if (string.IsNullOrWhiteSpace(surnameLabel.Text) ||
        string.IsNullOrWhiteSpace(nameLabel.Text) ||
            string.IsNullOrWhiteSpace(loginLabel.Text) ||
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
        if (loginLabel.Text != _courier.Login)
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
            HttpResponseMessage response = await _httpClient.PutAsync($"http://courseproject4/api/profile/{_courier.Id}", jsonContent);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                // ��������� ��������� ����� ������ ������������
                _courier.Surname = surnameLabel.Text;
                _courier.Name = nameLabel.Text;
                _courier.Patronymic = string.IsNullOrEmpty(patronymicLabel.Text) ? null : patronymicLabel.Text;
                _courier.Password = passwordLabel.Text;

                if (loginLabel.Text != _courier.Login)
                {
                    _courier.Login = loginLabel.Text;
                }

                // ��������� UI
                surnameLabel.Text = _courier.Surname;
                nameLabel.Text = _courier.Name;
                patronymicLabel.Text = _courier.Patronymic ?? "";
                loginLabel.Text = _courier.Login;
                passwordLabel.Text = _courier.Password;

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
            // ��������������� ���������, ���� �������� �� �������
            MainContent.IsVisible = true;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}