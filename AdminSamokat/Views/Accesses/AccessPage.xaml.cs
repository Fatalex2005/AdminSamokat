using AdminSamokat.Models;

namespace AdminSamokat.Views.Accesses;

public partial class AccessPage : ContentPage
{
    private User _user;
    private string _token;
    private Access _access;
    private readonly HttpClient _httpClient = new HttpClient();
    public AccessPage(Access access, User user, string token)
	{
		InitializeComponent();
        _access = access;
        _user = user;
        _token = token;
    }
}