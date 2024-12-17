using AdminSamokat.Models;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Net.Http;
using System.Text.Json;

namespace AdminSamokat.Views.Accesses;

public partial class AccessPage : ContentPage
{
    private User _user;
    private string _token;
    private Access _access;
    private readonly HttpClient _httpClient = new HttpClient();

    public AccessPage(Access access, User user, string token)
    {
        InitializeComponent();
        _access = access;
        _user = user;
        _token = token;

        // ������������� BindingContext
        BindingContext = _access;

        FillAccessDetails();
    }

    // ����� ��� ���������� ���������� � �����������
    private void FillAccessDetails()
    {
        // ���������� ��� ������������
        UserFullNameLabel.Text = _access.UserFullName;

        // ���������� ����� ������ � �����
        StartTimeLabel.Text = $"������: {_access.StartChange}";
        EndTimeLabel.Text = $"�����: {_access.EndChange}";

        // ���������� ������ �������������
        ConfirmStatusLabel.Text = _access.Confirm == 1 ? "������������" : "�� ������������";
    }

    // ���������� ������� �� ������ ��� ��������� ������������
    private async void OnViewUserButtonClicked(object sender, EventArgs e)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
        var userResponse = await _httpClient.GetAsync($"http://courseproject4/api/profile/{_access.UserId}");
        if (userResponse.IsSuccessStatusCode)
        {
            var userContent = await userResponse.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<User>(userContent);

            // ������� �� �������� �������
            await Navigation.PushAsync(new Couries.Courier(user, _user, _token));
        }
        else
        {
            await DisplayAlert("������", "�� ������� ��������� ���������� � ������������.", "��");
        }
    }

    // ���������� ������� �� ������ ��� ������������� �����������
    private async void OnConfirmButtonClicked(object sender, EventArgs e)
    {
        // ������������� ����� ��������������
        bool isConfirmed = await DisplayAlert(
            "�������������",
            "�� �������, ��� ������ ����������� �����������?",
            "��",
            "���"
        );

        if (!isConfirmed)
        {
            // ���� ������������ ������ "���", �������� ����������
            return;
        }
        // ���������� ��������� �������� � �������� ����� � ������
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;
        ConfirmButtonText.IsVisible = false;
        ConfirmButtonIcon.IsVisible = false;

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("confirm", "1")
        });

        try
        {
            var response = await _httpClient.PatchAsync(
                $"http://courseproject4/api/accesses-confirm/{_access.Id}", content);

            if (response.IsSuccessStatusCode)
            {
                // ������������� ��������� �������� ����� ������� DisplayAlert
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                ConfirmButtonText.IsVisible = true;
                ConfirmButtonIcon.IsVisible = true;
                // �������� ������ ������������� ����� ��������� �������������
                ConfirmButtonFrame.IsVisible = false;

                await DisplayAlert("�����", "����������� ������������.", "��");

                // ��������� �������� Confirm
                _access.Confirm = 1;

                // �������� �� ��������� ������� ��� ��������
                OnPropertyChanged(nameof(_access.Confirm));

                await Navigation.PushAsync(new Home(_user, _token));
            }
            else
            {
                // ������������� ��������� �������� ����� ������� DisplayAlert
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                ConfirmButtonText.IsVisible = true;
                ConfirmButtonIcon.IsVisible = true;

                var errorContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("������", $"�� ������� ����������� �����������. �����: {errorContent}", "��");
            }
        }
        catch (Exception ex)
        {
            // ������������� ��������� �������� ����� ������� DisplayAlert
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            ConfirmButtonText.IsVisible = true;
            ConfirmButtonIcon.IsVisible = true;

            await DisplayAlert("������", $"��������� ������: {ex.Message}", "��");
        }
    }
}