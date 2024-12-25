using AdminSamokat.Models;
using System.Net.Http;

namespace AdminSamokat.Views.Bonuses;

public partial class BonusPage : ContentPage
{
    private User _user;
    private string _token;
    private Bonus _bonus;
    private readonly HttpClient _httpClient = new HttpClient();
    public BonusPage(Bonus bonus, User user, string token)
    {
        InitializeComponent();
        _bonus = bonus;
        _user = user;
        _token = token;

        // ���������� ������ ������
        BonusNameLabel.Text = _bonus.Title;
        BonusDescriptionLabel.Text = _bonus.Description;
        BonusPriceLabel.Text = _bonus.Price + " \u20BD";

        // ���������� ����
        if (_bonus.Role != null)
        {
            BonusRoleLabel.Text = $"��� ����: {_bonus.Role.Name}";
        }
        else
        {
            BonusRoleLabel.Text = "���� �� �������";
        }
    }

    private async void OnEditButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new EditBonus(_bonus, _user, _token));
    }

    private async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        // ������������� ����� ���������
        bool isConfirmed = await DisplayAlert(
            "�������������",
            "�� �������, ��� ������ ������� ���� �����?",
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
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            // ����������� ��������� � �����
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

            // ���������� DELETE-������ �� ������
            var response = await _httpClient.DeleteAsync($"http://courseproject4/api/bonus/{_bonus.Id}");

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("�����", "����� ������� �����.", "��");

                await Navigation.PushAsync(new Home(_user, _token));
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("������", $"�� ������� ������� �����: {response.StatusCode} - {responseContent}", "��");
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
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}