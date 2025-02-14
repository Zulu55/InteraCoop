using Blazored.Modal.Services;
using Blazored.Modal;
using CurrieTechnologies.Razor.SweetAlert2;
using InteraCoop.Frontend.Repositories;
using InteraCoop.Frontend.Services;
using InteraCoop.Shared.Dtos;
using InteraCoop.Shared.Entities;
using InteraCoop.Shared.Enums;
using Microsoft.AspNetCore.Components;

namespace InteraCoop.Frontend.Pages.Auth
{
    public partial class Register
    {
        private UserDto userDto= new();
        private List<Country>? countries;
        private List<State>? states;
        private List<City>? cities;
        private bool loading;
        private string? imageUrl;
        private bool wasClose;

        [Inject] private IRepository Repository { get; set; } = null!;
        [Inject] private NavigationManager NavigationManager { get; set; } = null!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = null!;
        //[Inject] private ILoginService LoginService { get; set; } = null!;
        [Parameter, SupplyParameterFromQuery] public bool IsAdmin { get; set; }
        [CascadingParameter] BlazoredModalInstance BlazoredModal { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await LoadCountriesAsync();
        }

        private void ImageSelected(string imagenBase64)
        {
            userDto.Photo = imagenBase64;
            imageUrl = null;
        }
        private async Task LoadCountriesAsync()
        {
            var responseHttp = await Repository.GetAsync<List<Country>>("/api/countries/combo");
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            countries = responseHttp.Response;
        }

        private async Task CountryChangedAsync(ChangeEventArgs e)
        {
            var selectedCountry = Convert.ToInt32(e.Value!);
            states = null;
            cities = null;
            userDto.CityId = 0;
            await LoadStatesAsyn(selectedCountry);
        }

        private async Task LoadStatesAsyn(int countryId)
        {
            var responseHttp = await Repository.GetAsync<List<State>>($"/api/states/combo/{countryId}");
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            states = responseHttp.Response;
        }

        private async Task StateChangedAsync(ChangeEventArgs e)
        {
            var selectedState = Convert.ToInt32(e.Value!);
            cities = null;
            userDto.CityId = 0;
            await LoadCitiesAsyn(selectedState);
        }

        private async Task LoadCitiesAsyn(int stateId)
        {
            var responseHttp = await Repository.GetAsync<List<City>>($"/api/cities/combo/{stateId}");
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            cities = responseHttp.Response;
        }
        private async Task CreateUserAsync()
        {        
            userDto.UserName = userDto.Email;
            userDto.UserType = UserType.Employee;

            if (IsAdmin)
            {
                userDto.UserType = UserType.Admin;
            }

            loading = true;
            var responseHttp = await Repository.PostAsync<UserDto>("/api/accounts/CreateUser", userDto);
            loading = false;

            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            await SweetAlertService.FireAsync("Confirmaci�n", "Su cuenta ha sido creada con �xito. Se te ha enviado un correo electr�nico con las instrucciones para activar tu usuario.", SweetAlertIcon.Info);
            NavigationManager.NavigateTo("/");
        }

        private async Task CloseModalAsync()
        {
            wasClose = true;
            await BlazoredModal.CloseAsync(ModalResult.Ok());
        }
    }
}