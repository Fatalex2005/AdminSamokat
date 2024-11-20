using AdminSamokat.Models;

namespace AdminSamokat.Views;

public partial class Profile : ContentPage
{
    public Profile()
	{
		InitializeComponent();
	}

    private void OnEditProfileButtonClicked(object sender, EventArgs e)
    {
		Navigation.PushAsync(new EditProfile());
    }
}