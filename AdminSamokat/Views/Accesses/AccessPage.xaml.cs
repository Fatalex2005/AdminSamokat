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
        StartTimeLabel.Text = $"������: {_access.StartChange.ToString(@"hh\:mm")}";
        EndTimeLabel.Text = $"�����: {_access.EndChange.ToString(@"hh\:mm")}";
        // ���������� ���� �����������
        DateLabel.Text = $"����������� ��: {_access.Date.ToString("dd.MM.yyyy")}";
        // ���������� ������ �������������
        ConfirmStatusLabel.Text = _access.Confirm == 1 ? "������: ������������" : "������: �� ������������";
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
        LoadingConfirmIndicator.IsRunning = true;
        LoadingConfirmIndicator.IsVisible = true;
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
                LoadingConfirmIndicator.IsRunning = false;
                LoadingConfirmIndicator.IsVisible = false;
                ConfirmButtonText.IsVisible = true;
                ConfirmButtonIcon.IsVisible = true;
                // �� ���������� ������ �������������
                ConfirmButtonFrame.IsVisible = false;
                // ���������� ������ ������
                CancelButtonFrame.IsVisible = true;
                PartialConfirmButtonFrame.IsVisible = false;
                PartialCancelButtonFrame.IsVisible = true;
                ConfirmStatusLabel.Text = "������: ������������";
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
                LoadingConfirmIndicator.IsRunning = false;
                LoadingConfirmIndicator.IsVisible = false;
                ConfirmButtonText.IsVisible = true;
                ConfirmButtonIcon.IsVisible = true;

                var errorContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("������", $"�� ������� ����������� �����������. �����: {errorContent}", "��");
            }
        }
        catch (Exception ex)
        {
            // ������������� ��������� �������� ����� ������� DisplayAlert
            LoadingConfirmIndicator.IsRunning = false;
            LoadingConfirmIndicator.IsVisible = false;
            ConfirmButtonText.IsVisible = true;
            ConfirmButtonIcon.IsVisible = true;
            await DisplayAlert("������", $"��������� ������: {ex.Message}", "��");
        }
    }
    private async void OnCancelButtonClicked(object sender, EventArgs e)
    {
        // ������������� ����� �������
        bool isConfirmed = await DisplayAlert(
            "�������������",
            "�� �������, ��� ������ �������� �����������?",
            "��",
            "���"
        );
        if (!isConfirmed)
        {
            return;
        }
        // ���������� ��������� �������� � �������� ����� � ������
        LoadingCancelIndicator.IsRunning = true;
        LoadingCancelIndicator.IsVisible = true;
        CancelButtonText.IsVisible = false;
        CancelButtonIcon.IsVisible = false;
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
        var content = new FormUrlEncodedContent(new[]
        {
        new KeyValuePair<string, string>("confirm", "0")
    });
        try
        {
            var response = await _httpClient.PatchAsync(
                $"http://courseproject4/api/accesses-cancel/{_access.Id}", content);
            if (response.IsSuccessStatusCode)
            {
                // ������������� ��������� ��������
                LoadingCancelIndicator.IsRunning = false;
                LoadingCancelIndicator.IsVisible = false;
                CancelButtonText.IsVisible = true;
                CancelButtonIcon.IsVisible = true;
                // ���������� ������ �������������
                ConfirmButtonFrame.IsVisible = true;
                // �� ���������� ������ ������
                CancelButtonFrame.IsVisible = false;
                PartialConfirmButtonFrame.IsVisible = true;
                PartialCancelButtonFrame.IsVisible = false;
                // ��������� ������
                ConfirmStatusLabel.Text = "������: �� ������������";
                await DisplayAlert("�����", "����������� ��������.", "��");
                // ��������� �������� Confirm
                _access.Confirm = 0;
                // �������� �� ��������� ������� ��� ��������
                OnPropertyChanged(nameof(_access.Confirm));
                await Navigation.PushAsync(new Home(_user, _token));
            }
            else
            {
                // ������������� ��������� ��������
                LoadingCancelIndicator.IsRunning = false;
                LoadingCancelIndicator.IsVisible = false;
                CancelButtonText.IsVisible = true;
                CancelButtonIcon.IsVisible = true;

                var errorContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("������", $"�� ������� �������� �����������. �����: {errorContent}", "��");
            }
        }
        catch (Exception ex)
        {
            // ������������� ��������� ��������
            LoadingCancelIndicator.IsRunning = false;
            LoadingCancelIndicator.IsVisible = false;
            CancelButtonText.IsVisible = true;
            CancelButtonIcon.IsVisible = true;

            await DisplayAlert("������", $"��������� ������: {ex.Message}", "��");
        }
    }
    private async void OnPartialConfirmButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PartialAccessPage(_access, _user, _token));
    }
    private async void OnPartialCancelButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PartialAccessPage(_access, _user, _token));
    }
}