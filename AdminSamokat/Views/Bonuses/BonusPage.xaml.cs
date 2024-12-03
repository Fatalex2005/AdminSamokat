using AdminSamokat.Models;

namespace AdminSamokat.Views.Bonuses;

public partial class BonusPage : ContentPage
{
    private Bonus _bonus;
    public BonusPage(Bonus bonus)
	{
		InitializeComponent();
		_bonus = bonus;
	}
}