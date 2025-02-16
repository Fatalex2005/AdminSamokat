using AdminSamokat.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AdminSamokat.Views.Accesses;

public partial class PartialAccessPage : ContentPage
{
    private Access _access;
    private User _user;
    private string _token;
    private readonly HttpClient _httpClient = new HttpClient();
    public PartialAccessPage(Access access, User user, string token)
    {
        InitializeComponent();
        _access = access;
        _user = user;
        _token = token;
        // ������������� Picker � 24-������� �������� � ������������ �� �����������
        InitializeTimePickers();
        // ���������� ���������� � �����������
        StartTimeLabel.Text = $"������: {_access.StartChange.ToString(@"hh\:mm")}";
        EndTimeLabel.Text = $"�����: {_access.EndChange.ToString(@"hh\:mm")}";
        // ���������� ���� �����������
        DateLabel.Text = $"����������� ��: {_access.Date.ToString("dd.MM.yyyy")}";
        // ���������� ��� ������������
        UserFullNameLabel.Text = _access.UserFullName;
        // ������������� ����� ��� Label "Access" � ����������� �� �������� _access.Confirm
        if (_access.Confirm == 0)
        {
            Access.Text = "������������� �����������";
            PartialConfirmButton.IsVisible = true; // ���������� ������ "����������� ���������� �����"
            PartialCancelButton.IsVisible = false; // �������� ������ "�������� ���������� �����"
        }
        else
        {
            Access.Text = "������ �����������";
            PartialConfirmButton.IsVisible = false; // �������� ������ "����������� ���������� �����"
            PartialCancelButton.IsVisible = true;  // ���������� ������ "�������� ���������� �����"
        }
    }
    // ������������� Picker � 24-������� �������� � ������������ �� �����������
    private void InitializeTimePickers()
    {
        // ������� Picker
        StartTimePicker.Items.Clear();
        EndTimePicker.Items.Clear();
        // ��������� Picker ��� ������ ������� (������ ��������� ����, �������� ����� ���������)
        for (int hour = _access.StartChange.Hours; hour < _access.EndChange.Hours; hour++)
        {
            StartTimePicker.Items.Add(hour.ToString("00"));
        }
        // ��������� Picker ��� ��������� ������� (������ ��������� ����, �������� ������ ���������)
        for (int hour = _access.StartChange.Hours + 1; hour <= _access.EndChange.Hours; hour++)
        {
            EndTimePicker.Items.Add(hour.ToString("00"));
        }
        // ������������� ��������� �������� ��� Picker
        StartTimePicker.SelectedIndex = 0; // ������ ���������
        EndTimePicker.SelectedIndex = EndTimePicker.Items.Count - 1; // ����� ���������
    }
    // ���������� ��� ������ ���������� �������������
    private async void OnPartialConfirmButtonClicked(object sender, EventArgs e)
    {
        // �������� ��������� �������� �������
        var startTime = StartTimePicker.Items[StartTimePicker.SelectedIndex];
        var endTime = EndTimePicker.Items[EndTimePicker.SelectedIndex];
        // ��������, ��� ������� ���������� ��������� ���������
        if (int.Parse(startTime) >= int.Parse(endTime))
        {
            await DisplayAlert("������", "����� ������ ������ ���� ������ ������� ���������.", "��");
            return;
        }
        // ������������� ��������
        bool isConfirmed = await DisplayAlert(
            "�������������",
            "�� �������, ��� ������ ����������� ��������� �����������?",
            "��",
            "���"
        );
        if (!isConfirmed)
        {
            return;
        }
        // ���������� ��������� ��������
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;
        PartialConfirmButton.IsEnabled = false;
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            // ���������� ������ �� ������ ��� ���������� �������������
            var response = await _httpClient.PostAsync(
                $"http://courseproject4/api/accesses-partial-confirm/{_access.Id}",
                new StringContent(JsonSerializer.Serialize(new
                {
                    startChange = startTime, // ���������� ��������
                    endChange = endTime,     // ���������� ��������
                    Confirm = 1
                }), Encoding.UTF8, "application/json")
            );
            if (response.IsSuccessStatusCode)
            {
                // ������������� ��������� ��������
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                PartialConfirmButton.IsEnabled = true;
                // ��������� ���������
                await DisplayAlert("�����", "��������� ����������� ������������.", "��");
                // ������������ �� ������� ��������
                await Navigation.PushAsync(new Home(_user, _token));
            }
            else
            {
                // ������������� ��������� ��������
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                PartialConfirmButton.IsEnabled = true;
                var errorContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("������", $"�� ������� ����������� ��������� �����������. �����: {errorContent}", "��");
            }
        }
        catch (Exception ex)
        {
            // ������������� ��������� ��������
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            PartialConfirmButton.IsEnabled = true;
            await DisplayAlert("������", $"��������� ������: {ex.Message}", "��");
        }
    }

    // ���������� ��� ������ ��������� ������
    private async void OnPartialCancelButtonClicked(object sender, EventArgs e)
    {
        // �������� ��������� �������� �������
        var startTime = StartTimePicker.Items[StartTimePicker.SelectedIndex];
        var endTime = EndTimePicker.Items[EndTimePicker.SelectedIndex];
        // ��������, ��� ������� ���������� ��������� ���������
        if (int.Parse(startTime) >= int.Parse(endTime))
        {
            await DisplayAlert("������", "����� ������ ������ ���� ������ ������� ���������.", "��");
            return;
        }
        // ������������� ��������
        bool isConfirmed = await DisplayAlert(
            "�������������",
            "�� �������, ��� ������ �������� ��������� �����������?",
            "��",
            "���"
        );
        if (!isConfirmed)
        {
            return;
        }
        // ���������� ��������� ��������
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;
        PartialCancelButton.IsEnabled = false;
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            // ���������� ������ �� ������ ��� ��������� ������
            var response = await _httpClient.PostAsync(
                $"http://courseproject4/api/accesses-partial-cancel/{_access.Id}",
                new StringContent(JsonSerializer.Serialize(new
                {
                    startChange = startTime, // ���������� ��������
                    endChange = endTime,     // ���������� ��������
                    Confirm = 0
                }), Encoding.UTF8, "application/json")
            );
            if (response.IsSuccessStatusCode)
            {
                // ������������� ��������� ��������
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                PartialCancelButton.IsEnabled = true;
                // ��������� ���������
                await DisplayAlert("�����", "��������� ����������� ��������.", "��");
                // ������������ �� ������� ��������
                await Navigation.PushAsync(new Home(_user, _token));
            }
            else
            {
                // ������������� ��������� ��������
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                PartialCancelButton.IsEnabled = true;
                var errorContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("������", $"�� ������� �������� ��������� �����������. �����: {errorContent}", "��");
            }
        }
        catch (Exception ex)
        {
            // ������������� ��������� ��������
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            PartialCancelButton.IsEnabled = true;
            await DisplayAlert("������", $"��������� ������: {ex.Message}", "��");
        }
    }
}