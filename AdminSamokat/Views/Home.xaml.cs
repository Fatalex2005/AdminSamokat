using AdminSamokat.Models;
using AdminSamokat.Views.Auth;
using AdminSamokat.Views.Fines;
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace AdminSamokat.Views;

public partial class Home : ContentPage
{
    // ������������� HTTP �������
    private readonly HttpClient _httpClient = new HttpClient();
    private User _user;
    private string _token;
    public Home(User user, string token)
    {
        InitializeComponent();
        _user = user;
        _token = token;
        int hour = DateTime.Now.Hour;
        if (hour >= 5 && hour < 12)
        {
            nameLabel.Text = "������ ����, " + user.Name + "!";
        }
        else if (hour >= 12 && hour < 18)
        {
            nameLabel.Text = "������ ����, " + user.Name + "!";
        }
        else if (hour >= 18 && hour < 22)
        {
            nameLabel.Text = "������ �����, " + user.Name + "!";
        }
        else
        {
            nameLabel.Text = "������ ����, " + user.Name + "! ��� �� ����� ������ � ��� �����, ����� ������ ��������, ��-��-��";
        }
    }
    private async void OnLogoutButtonClicked(object sender, EventArgs e)
    {
        bool isConfirmed = await DisplayAlert(
            "�������������",
            "�� �������, ��� ������ ����� �� ��������?",
            "��",
            "���"
        );
        if (!isConfirmed)
        {
            return;
        }
        // ���������� ��������� ��������
        Overlay.IsVisible = true;
        string userToken = Preferences.Get("UserToken", string.Empty);
        if (!string.IsNullOrEmpty(userToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync("http://courseproject4/api/logout", null);
                // �������� ��������� �������� ����� ������������
                Overlay.IsVisible = false;
                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("�����", "�� ������� ����� �� �������.", "��");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("������", $"�� ������� ��������� ������. ����� �������: {errorContent}", "��");
                }
            }
            catch (Exception ex)
            {
                // �������� ��������� �������� ��� ����������
                Overlay.IsVisible = false;

                await DisplayAlert("������", $"�� ������� ��������� ������: {ex.Message}", "��");
            }
        }
        else
        {
            // �������� ��������� ��������, ���� ����� �����������
            Overlay.IsVisible = false;
        }
        Preferences.Clear();
        // ������������ �� �������� �����
        await Navigation.PushAsync(new Views.Auth.Login());
        Navigation.RemovePage(this);
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        // ������ ��������� ������ "�����"
        NavigationPage.SetHasBackButton(this, false);
    }
    private async void OnAccessesClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Accesses.AllAccesses(_user, _token));
    }
    private async void OnCouriersClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Couries.AllCouriers(_user, _token));
    }
    private async void OnRegisterCourierClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Auth.Register(_user, _token));
    }
    private async void OnBonusesClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Bonuses.AllBonuses(_user, _token));
    }
    private async void OnPenaltiesClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Fines.AllFines(_user, _token));
    }
    private async void OnProfileButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Profile(_user, _token));
    }
}