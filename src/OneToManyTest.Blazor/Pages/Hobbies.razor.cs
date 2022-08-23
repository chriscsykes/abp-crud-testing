using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.DataGrid;
using Volo.Abp.BlazoriseUI.Components;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Components.Web.Theming.PageToolbars;
using OneToManyTest.Hobbies;
using OneToManyTest.Permissions;
using OneToManyTest.Shared;

namespace OneToManyTest.Blazor.Pages
{
    public partial class Hobbies
    {
        protected List<Volo.Abp.BlazoriseUI.BreadcrumbItem> BreadcrumbItems = new List<Volo.Abp.BlazoriseUI.BreadcrumbItem>();
        protected PageToolbar Toolbar {get;} = new PageToolbar();
        private IReadOnlyList<HobbyDto> HobbyList { get; set; }
        private int PageSize { get; } = LimitedResultRequestDto.DefaultMaxResultCount;
        private int CurrentPage { get; set; } = 1;
        private string CurrentSorting { get; set; }
        private int TotalCount { get; set; }
        private bool CanCreateHobby { get; set; }
        private bool CanEditHobby { get; set; }
        private bool CanDeleteHobby { get; set; }
        private HobbyCreateDto NewHobby { get; set; }
        private Validations NewHobbyValidations { get; set; }
        private HobbyUpdateDto EditingHobby { get; set; }
        private Validations EditingHobbyValidations { get; set; }
        private Guid EditingHobbyId { get; set; }
        private Modal CreateHobbyModal { get; set; }
        private Modal EditHobbyModal { get; set; }
        private GetHobbiesInput Filter { get; set; }
        private DataGridEntityActionsColumn<HobbyDto> EntityActionsColumn { get; set; }
        protected string SelectedCreateTab = "hobby-create-tab";
        protected string SelectedEditTab = "hobby-edit-tab";
        
        public Hobbies()
        {
            NewHobby = new HobbyCreateDto();
            EditingHobby = new HobbyUpdateDto();
            Filter = new GetHobbiesInput
            {
                MaxResultCount = PageSize,
                SkipCount = (CurrentPage - 1) * PageSize,
                Sorting = CurrentSorting
            };
        }

        protected override async Task OnInitializedAsync()
        {
            await SetToolbarItemsAsync();
            await SetBreadcrumbItemsAsync();
            await SetPermissionsAsync();
        }

        protected virtual ValueTask SetBreadcrumbItemsAsync()
        {
            BreadcrumbItems.Add(new Volo.Abp.BlazoriseUI.BreadcrumbItem(L["Menu:Hobbies"]));
            return ValueTask.CompletedTask;
        }

        protected virtual ValueTask SetToolbarItemsAsync()
        {
            Toolbar.AddButton(L["ExportToExcel"], async () =>{ await DownloadAsExcelAsync(); }, IconName.Download);
            
            Toolbar.AddButton(L["NewHobby"], async () =>
            {
                await OpenCreateHobbyModalAsync();
            }, IconName.Add, requiredPolicyName: OneToManyTestPermissions.Hobbies.Create);

            return ValueTask.CompletedTask;
        }

        private async Task SetPermissionsAsync()
        {
            CanCreateHobby = await AuthorizationService
                .IsGrantedAsync(OneToManyTestPermissions.Hobbies.Create);
            CanEditHobby = await AuthorizationService
                            .IsGrantedAsync(OneToManyTestPermissions.Hobbies.Edit);
            CanDeleteHobby = await AuthorizationService
                            .IsGrantedAsync(OneToManyTestPermissions.Hobbies.Delete);
        }

        private async Task GetHobbiesAsync()
        {
            Filter.MaxResultCount = PageSize;
            Filter.SkipCount = (CurrentPage - 1) * PageSize;
            Filter.Sorting = CurrentSorting;

            var result = await HobbiesAppService.GetListAsync(Filter);
            HobbyList = result.Items;
            TotalCount = (int)result.TotalCount;
        }

        protected virtual async Task SearchAsync()
        {
            CurrentPage = 1;
            await GetHobbiesAsync();
            await InvokeAsync(StateHasChanged);
        }

        private  async Task DownloadAsExcelAsync()
        {
            var token = (await HobbiesAppService.GetDownloadTokenAsync()).Token;
            NavigationManager.NavigateTo($"/api/app/hobbies/as-excel-file?DownloadToken={token}", forceLoad: true);
        }

        private async Task OnDataGridReadAsync(DataGridReadDataEventArgs<HobbyDto> e)
        {
            CurrentSorting = e.Columns
                .Where(c => c.SortDirection != SortDirection.Default)
                .Select(c => c.Field + (c.SortDirection == SortDirection.Descending ? " DESC" : ""))
                .JoinAsString(",");
            CurrentPage = e.Page;
            await GetHobbiesAsync();
            await InvokeAsync(StateHasChanged);
        }

        private async Task OpenCreateHobbyModalAsync()
        {
            NewHobby = new HobbyCreateDto{
                
                
            };
            await NewHobbyValidations.ClearAll();
            await CreateHobbyModal.Show();
        }

        private async Task CloseCreateHobbyModalAsync()
        {
            NewHobby = new HobbyCreateDto{
                
                
            };
            await CreateHobbyModal.Hide();
        }

        private async Task OpenEditHobbyModalAsync(HobbyDto input)
        {
            var hobby = await HobbiesAppService.GetAsync(input.Id);
            
            EditingHobbyId = hobby.Id;
            EditingHobby = ObjectMapper.Map<HobbyDto, HobbyUpdateDto>(hobby);
            await EditingHobbyValidations.ClearAll();
            await EditHobbyModal.Show();
        }

        private async Task DeleteHobbyAsync(HobbyDto input)
        {
            await HobbiesAppService.DeleteAsync(input.Id);
            await GetHobbiesAsync();
        }

        private async Task CreateHobbyAsync()
        {
            try
            {
                if (await NewHobbyValidations.ValidateAll() == false)
                {
                    return;
                }

                await HobbiesAppService.CreateAsync(NewHobby);
                await GetHobbiesAsync();
                await CloseCreateHobbyModalAsync();
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex);
            }
        }

        private async Task CloseEditHobbyModalAsync()
        {
            await EditHobbyModal.Hide();
        }

        private async Task UpdateHobbyAsync()
        {
            try
            {
                if (await EditingHobbyValidations.ValidateAll() == false)
                {
                    return;
                }

                await HobbiesAppService.UpdateAsync(EditingHobbyId, EditingHobby);
                await GetHobbiesAsync();
                await EditHobbyModal.Hide();                
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex);
            }
        }

        private void OnSelectedCreateTabChanged(string name)
        {
            SelectedCreateTab = name;
        }

        private void OnSelectedEditTabChanged(string name)
        {
            SelectedEditTab = name;
        }
        

    }
}
