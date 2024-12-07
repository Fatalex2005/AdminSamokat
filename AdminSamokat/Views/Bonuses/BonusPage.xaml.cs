using AdminSamokat.Models;

namespace AdminSamokat.Views.Bonuses;

public partial class BonusPage : ContentPage
{
    private User _user;
    private string _token;
    private Bonus _bonus;
    public BonusPage(Bonus bonus, User user, string token)
	{
		InitializeComponent();
		_bonus = bonus;
        _user = user;
        _token = token;

        // Отобразить данные бонуса
        BonusNameLabel.Text = _bonus.Title;
        BonusDescriptionLabel.Text = _bonus.Description;
        BonusPriceLabel.Text = _bonus.Price + " \u20BD";
    }

    private async void OnEditButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new EditBonus(_bonus, _user, _token));
    }

    private void OnDeleteButtonClicked(object sender, EventArgs e)
    {

    }
}