using AdminSamokat.Models;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;

namespace AdminSamokat.Views.Fines;

public partial class FinePage : ContentPage
{
    private User _user;
    private string _token;
    private Fine _fine;
    private readonly HttpClient _httpClient = new HttpClient();
    public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();
    public FinePage(Fine fine, User user, string token)
	{
		InitializeComponent();
        _fine = fine;
        _token = token;
        _user = user;
        UsersCollectionView.ItemsSource = Users;
        // ���������� ������ ������
        FineDescriptionLabel.Text = "���� �� ������ ��������� \"" + _fine.Description + "\"?";

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
            await Navigation.PushAsync(new Couries.Courier(selectedCourier, _user, _token));
        }

        // ���������� �����
        ((CollectionView)sender).SelectedItem = null;
    }

    private async void OnEditButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new EditFine(_fine, _user, _token));
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
            var response = await _httpClient.DeleteAsync($"http://courseproject4/api/fine/{_fine.Id}");

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

    private async void OnAssignButtonClicked(object sender, EventArgs e)
    {
        // �������� �������, ���������� � �������
        var selectedCourier = ((Button)sender).BindingContext as User;

        if (selectedCourier != null)
        {
            // ���������, �������� �� ��� ���� �����
            if (selectedCourier.FineId == _fine.Id)
            {
                await DisplayAlert("��������", "������������ ��� �������� ���� �����.", "��");
                return; // ���������� ����������, ���� ����� ��� ��������
            }

            // ������������� ����� ����������� ������
            bool isConfirmed = await DisplayAlert(
                "�������������",
                $"�� ����� ������ ��������� ����� \"{_fine.Description}\" ������� {selectedCourier.Name}?",
                "��",
                "���"
            );

            if (!isConfirmed)
            {
                return; // ���������� ����������, ���� ������������ ������� ��������
            }

            try
            {
                // ����������� ������
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                // ��������� ������ ��� ����������
                var updateData = new Dictionary<string, object>
            {
                { "fine_id", _fine.Id } // ������������� ID �������� ������
            };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(updateData),
                    Encoding.UTF8,
                    "application/json"
                );

                // ���������� ������ �� ������
                var response = await _httpClient.PutAsync($"http://courseproject4/api/profile/{selectedCourier.Id}", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    // ��������� �������� ������ �������
                    selectedCourier.FineId = _fine.Id;

                    await DisplayAlert("�����", "����� \"" + _fine.Description + $"\" ������� �������� ������� {selectedCourier.Name}!", "��");

                    var courierToUpdate = Users.FirstOrDefault(u => u.Id == selectedCourier.Id);
                    if (courierToUpdate != null)
                    {
                        courierToUpdate.FineId = _fine.Id;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("������", $"�� ������� ��������� �����: {response.StatusCode} - {errorContent}", "��");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("������", $"��������� ������: {ex.Message}", "��");
            }
        }
    }
}