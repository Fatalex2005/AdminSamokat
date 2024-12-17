using AdminSamokat.Models;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace AdminSamokat.Views.Couries;

public partial class AllCouriers : ContentPage
{
    private User _user;
    private string _token;
    private readonly HttpClient _httpClient = new HttpClient();
    public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();

    public AllCouriers(User user, string token)
    {
        InitializeComponent();
        _user = user;
        _token = token;
        UsersCollectionView.ItemsSource = Users;
        LoadUsers();
    }

    private async void LoadUsers()
    {
        try
        {
            // ���������� ��������� ��������
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            var token = Preferences.Get("UserToken", string.Empty);

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("http://courseproject4/api/profile");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<User>>(content);

                Users.Clear();

                // ��������� ������������� � ����� 2
                var couriers =
                    users.Where(user => user.RoleId == 2); // ��������������, ��� RoleId ��������� ���� ������������

                foreach (var courier in couriers)
                {
                    Users.Add(courier);
                }

                // ��������� ���������� ���������
                UsersCollectionView.IsVisible = Users.Count > 0;
                EmptyMessageLabel.IsVisible = Users.Count == 0;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // ������ �����������
                await DisplayAlert("������", "��� ����� �� ��������. ������������� ������!", "��");
                await Navigation.PushAsync(new Views.Auth.Login());
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
        finally
        {
            // �������� ��������� ��������
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }



    private async void OnCourierSelected(object sender, SelectionChangedEventArgs e)
    {
        // �������� ���������� �������
        if (e.CurrentSelection.FirstOrDefault() is User selectedCourier)
        {
            // ������� �� �������� ���������� � �������
            await Navigation.PushAsync(new Courier(selectedCourier, _user, _token));
        }

        // ���������� �����
        ((CollectionView)sender).SelectedItem = null;
    }
}
