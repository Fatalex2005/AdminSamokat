using AdminSamokat.Models;
using System.Text.Json;

namespace AdminSamokat.Views.Couries;

public partial class Courier : ContentPage
{
    private User _courier;
    private User _user;
    private string _token;
    private readonly HttpClient _httpClient = new HttpClient();

    public Courier(User courier, User user, string token)
    {
        InitializeComponent();
        _user = user;
        _token = token;
        _courier = courier;

        // ���������� ������ �������
        CourierFullNameLabel.Text = $"{_courier.Surname} {_courier.Name} {_courier.Patronymic}";
        CourierLoginLabel.Text = _courier.Login;

        // ��������� �����
        LoadFineDetails();

        // ��������� ������
        LoadStatusDetails();
    }
    private async void LoadFineDetails()
    {
        var token = Preferences.Get("UserToken", string.Empty);
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        try
        {
            // �������� ��������� �������� � ������
            LoadingFineIndicator.IsRunning = true;
            LoadingFineIndicator.IsVisible = true;

            // ��������� ���������� � �������
            var responseFine = await _httpClient.GetAsync($"http://courseproject4/api/fine/{_courier.FineId}");
            var contentFine = await responseFine.Content.ReadAsStringAsync();


            if (responseFine.IsSuccessStatusCode)
            {
                var fine = JsonSerializer.Deserialize<Fine>(contentFine);
                FineLabel.Text = fine.Description;
            }
            else
            {
                FineLabel.Text = "�� ������� ��������� �����";
                await DisplayAlert("������", $"���: {responseFine.StatusCode}, �����: {contentFine}", "OK");
            }
        }
        catch (Exception ex)
        {
            FineLabel.Text = "������ ��� �������� ������";
            await DisplayAlert("������", ex.Message, "OK");
        }
        finally
        {
            // ��������� ��������� ��������
            LoadingFineIndicator.IsRunning = false;
            LoadingFineIndicator.IsVisible = false;
        }
    }

    private async void LoadStatusDetails()
    {
        var token = Preferences.Get("UserToken", string.Empty);
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        try
        {
            // �������� ��������� �������� � �������
            LoadingStatusIndicator.IsRunning = true;
            LoadingStatusIndicator.IsVisible = true;

            // ��������� ���������� � �������
            var responseStatus = await _httpClient.GetAsync($"http://courseproject4/api/status/{_courier.StatusId}");
            var contentStatus = await responseStatus.Content.ReadAsStringAsync();

            if (responseStatus.IsSuccessStatusCode)
            {
                var status = JsonSerializer.Deserialize<Status>(contentStatus);
                StatusLabel.Text = status.Name;
            }
            else
            {
                FineLabel.Text = "�� ������� ��������� ������";
                await DisplayAlert("������", $"���: {responseStatus.StatusCode}, �����: {contentStatus}", "OK");
            }
        }
        catch (Exception ex)
        {
            FineLabel.Text = "������ ��� �������� �������";
            await DisplayAlert("������", ex.Message, "OK");
        }
        finally
        {
            // ��������� ��������� ��������
            LoadingStatusIndicator.IsRunning = false;
            LoadingStatusIndicator.IsVisible = false;
        }
    }

    private async void OnEditButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new EditProfileCourier(_courier, _user, _token));
    }

    private async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        // ������������� ����� ���������
        bool isConfirmed = await DisplayAlert(
            "�������������",
            "�� �������, ��� ������ ������� ����� �������?",
            "��",
            "���"
        );

        if (!isConfirmed)
        {
            // ���� ������������ ������ "���", �������� ����������
            return;
        }

        try
        {
            // �������� �������� ���������� � ���������� ���������
            MainContent.IsVisible = false;
            DeleteLoadingIndicator.IsRunning = true;
            DeleteLoadingIndicator.IsVisible = true;

            // ����������� ��������� � �����
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

            // ���������� DELETE-������ �� ������
            var response = await _httpClient.DeleteAsync($"http://courseproject4/api/profile/{_courier.Id}");

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("�����", "������ ������� �����.", "��");

                await Navigation.PushAsync(new Home(_user, _token));
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("������", $"�� ������� ������� �������: {response.StatusCode} - {responseContent}", "��");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("������", $"��������� ������: {ex.Message}", "��");
        }
        finally
        {
            // ���������� �������� ���������� � �������� ���������
            MainContent.IsVisible = true;
            DeleteLoadingIndicator.IsRunning = false;
            DeleteLoadingIndicator.IsVisible = false;
        }
    }
}