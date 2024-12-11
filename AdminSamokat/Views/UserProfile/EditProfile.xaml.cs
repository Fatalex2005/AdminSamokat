using AdminSamokat.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AdminSamokat.Views;

public partial class EditProfile : ContentPage
{
    // Инициализация HTTP клиента
    private readonly HttpClient _httpClient = new HttpClient();
    private User _user;
    private string _token;
    public EditProfile(User user, string token)
	{
		InitializeComponent();
        _user = user;
        _token = token;
        surnameLabel.Text = user.Surname;
        nameLabel.Text = user.Name;
        if (string.IsNullOrEmpty(user.Patronymic))
        {
            patronymicLabel.Text = "";
        }
        else
        {
            patronymicLabel.Text = user.Patronymic;
        }
        loginLabel.Text = user.Login;
    }

    private async void OnSaveButtonClicked(object sender, EventArgs e)
{
    // Проверка на пустые поля
    if (string.IsNullOrWhiteSpace(surnameLabel.Text) ||
        string.IsNullOrWhiteSpace(nameLabel.Text))
    {
        await DisplayAlert("Ошибка", "Все обязательные поля должны быть заполнены", "ОК");
        return;
    }

    if (!checkPassword.IsChecked && passwordLabel.Text != confirmPasswordLabel.Text)
        {
            await DisplayAlert("Ошибка", "Пароли не совпадают", "ОК");
            return;
        }

    // Формируем данные для обновления
    var updatedUser = new Dictionary<string, object>
    {
        { "surname", surnameLabel.Text },
        { "name", nameLabel.Text },
        { "patronymic", string.IsNullOrEmpty(patronymicLabel.Text) ? null : patronymicLabel.Text },
    };

    // Добавляем поле "login", только если оно изменилось
    if (loginLabel.Text != _user.Login)
    {
        updatedUser.Add("login", loginLabel.Text);
    }

    // Добавляем поле "password", только если галочка снята
        if (!checkPassword.IsChecked)
        {
            updatedUser.Add("password", passwordLabel.Text);
        }

    // Настраиваем сериализацию для преобразования ключей в нижний регистр
    var options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Преобразует ключи в camelCase
    };
    var jsonContent = new StringContent(JsonSerializer.Serialize(updatedUser, options), Encoding.UTF8, "application/json");

    // Запрос серверу
    try
    {
        // Скрываем форму и показываем индикатор загрузки
        MainContent.IsVisible = false;
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;

        // Отправляем запрос и записываем ответ в response
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
        HttpResponseMessage response = await _httpClient.PutAsync($"http://courseproject4/api/profile/{_user.Id}", jsonContent);

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            // Обновляем локальную копию данных пользователя
            _user.Surname = surnameLabel.Text;
            _user.Name = nameLabel.Text;
            _user.Patronymic = string.IsNullOrEmpty(patronymicLabel.Text) ? null : patronymicLabel.Text;

            if (loginLabel.Text != _user.Login)
            {
                _user.Login = loginLabel.Text;
            }

            Preferences.Set("UserSurname", _user.Surname);
            Preferences.Set("UserName", _user.Name);
            Preferences.Set("UserPatronymic", _user.Patronymic ?? "");
            Preferences.Set("UserLogin", _user.Login);

            // Обновляем UI
            surnameLabel.Text = _user.Surname;
            nameLabel.Text = _user.Name;
            patronymicLabel.Text = _user.Patronymic ?? "";
            loginLabel.Text = _user.Login;

            await DisplayAlert("Успех", "Профиль успешно обновлён!", "Вернуться на главную");

            await Navigation.PushAsync(new Views.Home(_user, _token));
            Navigation.RemovePage(this); // Убираем текущую страницу из стека
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await DisplayAlert("Ошибка", "Сессия истекла. Авторизуйтесь снова.", "OK");
            await Navigation.PushAsync(new Views.Auth.Login());
        }
        else
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            await DisplayAlert("Ошибка", $"Не удалось обновить профиль: {responseContent}", "OK");
        }
    }
    catch (Exception ex)
    {
        await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
    }
    finally
    {
        // Восстанавливаем интерфейс, если авторизация не удалась
        MainContent.IsVisible = true;
        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;
    }
}
    private void OnCheckPasswordChanged(object sender, CheckedChangedEventArgs e)
    {
        bool isChecked = e.Value;

        // Если галочка снята, показываем поля для ввода пароля
        password.IsVisible = !isChecked;
        confirmPassword.IsVisible = !isChecked;
    }

}