using AdminSamokat.Models;

namespace AdminSamokat.Views.Bonuses;

public partial class BonusPage : ContentPage
{
    private Bonus _bonus;
    public BonusPage(Bonus bonus)
	{
		InitializeComponent();
		_bonus = bonus;

        // Отобразить данные бонуса
        BonusNameLabel.Text = _bonus.Title;
        BonusDescriptionLabel.Text = _bonus.Description;
        BonusPriceLabel.Text = _bonus.Price + " \u20BD";
    }

    private void OnEditButtonClicked(object sender, EventArgs e)
    {

    }

    private void OnDeleteButtonClicked(object sender, EventArgs e)
    {

    }
}