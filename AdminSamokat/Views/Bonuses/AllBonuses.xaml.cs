using AdminSamokat.Models;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace AdminSamokat.Views.Bonuses;

public partial class AllBonuses : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient();
    public ObservableCollection<Bonus> Bonuses { get; set; } = new ObservableCollection<Bonus>();
    public AllBonuses()
	{
		InitializeComponent();
        BonusesCollectionView.ItemsSource = Bonuses;
        LoadBonuses();
    }

    private async void LoadBonuses()
    {
        try
        {
            var token = Preferences.Get("UserToken", string.Empty);

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("http://courseproject4/api/bonus");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var bonuses = JsonSerializer.Deserialize<List<Bonus>>(content);

                Bonuses.Clear();
                foreach (var bonus in bonuses)
                {
                    Bonuses.Add(bonus);
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("������", $"���: {response.StatusCode}, �����: {errorContent}", "��");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("������", $"��������� ������: {ex.Message}", "��");
        }
    }

    private async void OnBonusSelected(object sender, SelectionChangedEventArgs e)
    {
        // �������� ���������� ������������
        if (e.CurrentSelection.FirstOrDefault() is Bonus selectedBonus)
        {
            // ������� �� �������� ���������� � �������
            await Navigation.PushAsync(new BonusPage(selectedBonus));
        }
    }
}