using AdminSamokat.Models;

namespace AdminSamokat.Views;

public partial class Profile : ContentPage
{
    private User _user;
    private string _token;
    public Profile(User user, string token)
	{
		InitializeComponent();
        _user = user;
        _token = token;
        surnameLabel.Text = user.Surname;
        nameLabel.Text = user.Name;
        if (string.IsNullOrEmpty(user.Patronymic))
        {
            patronymicLabel.Text = "Отчество отсутствует";
        }
        else
        {
            patronymicLabel.Text = user.Patronymic;
        }
        loginLabel.Text = user.Login;
    }
    private async void OnEditProfileButtonClicked(object sender, EventArgs e)
    {
		await Navigation.PushAsync(new EditProfile(_user, _token));
    }
}