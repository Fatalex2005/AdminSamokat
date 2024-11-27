using AdminSamokat.Models;
using AdminSamokat.Views.Auth;
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace AdminSamokat.Views;

public partial class Home : ContentPage
{
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
            nameLabel.Text = "������ ����, " + user.Name;
        }
        else if (hour >= 12 && hour < 18)
        {
            nameLabel.Text = "������ ����, " + user.Name;
        }
        else if (hour >= 18 && hour < 22)
        {
            nameLabel.Text = "������ �����, " + user.Name;
        }
        else
        {
            nameLabel.Text = "������ ����, " + user.Name + ", ��� �� ����� ������ � ��� �����, ����� ������ ��������, ��-��-��";
        }
    }

    private async void OnLogoutButtonClicked(object sender, EventArgs e)
    {
        // ������� ���������� ������
        Preferences.Remove("UserToken");
        Preferences.Remove("UserId");
        Preferences.Remove("UserSurname");
        Preferences.Remove("UserName");
        Preferences.Remove("UserPatronymic");
        Preferences.Remove("UserLogin");
        Preferences.Remove("UserPassword");


        // ������������ �� �������� �����
        await Navigation.PushAsync(new Views.Auth.Login());
        Navigation.RemovePage(this); // ������� ������� �������� �� �����
    }

    private void OnOrdersClicked(object sender, EventArgs e)
    {

    }

    private void OnCouriersClicked(object sender, EventArgs e)
    {

    }

    private void OnRegisterCourierClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new Auth.Register());
    }

    private void OnBonusesClicked(object sender, EventArgs e)
    {

    }

    private void OnPenaltiesClicked(object sender, EventArgs e)
    {

    }

    private async void OnProfileButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Profile(_user, _token));
    }
}