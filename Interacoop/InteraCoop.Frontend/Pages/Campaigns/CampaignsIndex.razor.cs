using CurrieTechnologies.Razor.SweetAlert2;
using InteraCoop.Frontend.Repositories;
using InteraCoop.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace InteraCoop.Frontend.Pages.Campaigns
{
    [Authorize(Roles = "Analist")]
    public partial class CampaignsIndex
    {
        private int currentPage = 1;
        private int totalPages;
        [Inject] private NavigationManager NavigationManager { get; set; } = null!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = null!;
        [Inject] private IRepository Repository { get; set; } = null!;
        [Parameter, SupplyParameterFromQuery] public string Page { get; set; } = string.Empty;
        [Parameter, SupplyParameterFromQuery]public string Filter { get; set; } = string.Empty;
        [Parameter, SupplyParameterFromQuery] public int RecordsNumber { get; set; } = 10;
        public List<Campaign>? Campaigns { get; set; }
        public bool FormPostedSuccessfully { get; set; } = false;
        private async Task FilterCallBack(string filter)
        {
            Filter = filter;
            await ApplyFilterAsync();
            StateHasChanged();
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadAsync();
        }

        private async Task SelectedRecordsNumberAsync(int recordsnumber)
        {
            RecordsNumber = recordsnumber;
            int page = 1;
            await LoadAsync(page);
            await SelectedPageAsync(page);
        }

        private async Task SelectedPageAsync(int page)
        {
            currentPage = page;
            await LoadAsync(page);
        }

        private async Task LoadAsync(int page = 1)
        {
            if (!string.IsNullOrWhiteSpace(Page))
            {
                page = Convert.ToInt32(Page);
            }

            var ok = await LoadListAsync(page);
            if (ok)
            {
                await LoadPagesAsync();
            }
        }

        private void ValidateRecordsNumber()
        {
            if (RecordsNumber == 0)
            {
                RecordsNumber = 10;
            }
        }

        private async Task<bool> LoadListAsync(int page)
        {
            ValidateRecordsNumber();
            var url = $"api/campaigns?page={page}&recordsnumber={RecordsNumber}";

            if (!string.IsNullOrEmpty(Filter))
            {
                url += $"&filter={Filter}";
            }

            var response = await Repository.GetAsync<List<Campaign>>(url);
            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return false;
            }

            Campaigns = response.Response;
            return true;
        }

        private async Task LoadPagesAsync()
        {
            var url = $"api/campaigns/totalPages?recordsnumber={RecordsNumber}";

            if (!string.IsNullOrEmpty(Filter))
            {
                url += $"&filter={Filter}";
            }

            var response = await Repository.GetAsync<int>(url);
            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            totalPages = response.Response;
        }

        private async Task Delete(int campaignId)
        {
            var campaign = Campaigns.FirstOrDefault(x => x.Id == campaignId);
            var result = await SweetAlertService.FireAsync(new SweetAlertOptions
            {
                Title = "Confirmaci�n",
                Text = $"�Esta seguro que quieres borrar la campa�a: {campaign.CampaignName}?",
                Icon = SweetAlertIcon.Question,
                ShowCancelButton = true
            });
            var confirm = string.IsNullOrEmpty(result.Value);

            if (confirm)
            {
                return;
            }

            var responseHttp = await Repository.DeleteAsync<Campaign>($"api/campaigns/{campaignId}");

            if (responseHttp.Error)
            {
                if (responseHttp.HttpResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    NavigationManager.NavigateTo("/");
                    return;
                }

                var mensajeError = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", mensajeError, SweetAlertIcon.Error);
                return;
            }

            await LoadAsync(1);
        }

        private async Task CleanFilterAsync()
        {
            Filter = string.Empty;
            await ApplyFilterAsync();
        }

        private async Task ApplyFilterAsync()
        {
            int page = 1;
            await LoadAsync(page);
            await SelectedPageAsync(page);
        }

        private string GetBadgeStyle(string propertyToColor)
        {
            return propertyToColor switch
            {
                "Sin asignar" => "background-color: #36B3E0",
                "Asignada" => "background-color: olivedrab",
                "Vencida" => "background-color: #E6443E",
                _ => "background-color:cornflowerblue"
            };
        }
    }
}