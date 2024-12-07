using AdminSamokat.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http.Headers;
using System.Net.Http;

namespace AdminSamokat.Views.Bonuses;

public partial class CreateBonusPage : ContentPage
{
    private User _user;
    private string _token;
    private readonly HttpClient _httpClient = new HttpClient();
    public CreateBonusPage(User user, string token)
	{
		InitializeComponent();
        _user = user;
        _token = token;
    }

    private async void OnCreateButtonClicked(object sender, EventArgs e)
    {
        // Проверка на пустые поля
        if (string.IsNullOrWhiteSpace(titleEntry.Text) ||
            string.IsNullOrWhiteSpace(descriptionEntry.Text) ||
            string.IsNullOrWhiteSpace(priceEntry.Text))
        {
            await DisplayAlert("Ошибка", "Все обязательные поля должны быть заполнены", "ОК");
            return;
        }

        // Сбор данных из формы
        string title = titleEntry.Text;
        string description = descriptionEntry.Text;
        string price = priceEntry.Text;

        var createData = new MultipartFormDataContent
        {
            { new StringContent(title), "title" },
            { new StringContent(description), "description" },
            { new StringContent(price), "price" },
        };

        try
        {
            // Отображаем индикатор загрузки и скрываем форму
            CreateForm.IsVisible = false;
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            HttpResponseMessage response = await _httpClient.PostAsync("http://courseproject4/api/bonus", createData);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Успех", "Бонус успешно добавлен", "ОК");
                await Navigation.PushAsync(new Views.Home(_user, _token));
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Ошибка валидации", errorContent, "ОК");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Ошибка", $"Код ошибки: {(int)response.StatusCode}\n{errorContent}", "ОК");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка сети", ex.Message, "ОК");
        }
        finally
        {
            // Восстанавливаем форму и скрываем индикатор загрузки
            CreateForm.IsVisible = true;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}