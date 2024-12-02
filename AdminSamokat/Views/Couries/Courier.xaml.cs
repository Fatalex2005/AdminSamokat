using AdminSamokat.Models;

namespace AdminSamokat.Views.Couries;

public partial class Courier : ContentPage
{
    private User _courier;

    public Courier(User courier)
    {
        InitializeComponent();
        _courier = courier;

        // ���������� ������ �������
        CourierFullNameLabel.Text = $"{_courier.Name} {_courier.Surname} {_courier.Patronymic}";
        CourierLoginLabel.Text = _courier.Login;
        // �������� ������ ������ �� �������������
    }

    private void OnPenaltiesButtonClicked(object sender, EventArgs e)
    {

    }

    private void OnEditButtonClicked(object sender, EventArgs e)
    {

    }

    private void OnDeleteButtonClicked(object sender, EventArgs e)
    {

    }
}