using EntityLayer.DTOs;
using BusinessLayer.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ERPClient.GenelModule;
using EntityLayer.Entities;

namespace ERPClient.ViewModel
{
    public class GeneralStockViewModel : BaseViewModel
    {
        private readonly IStokService _service;
        private readonly IDepoService _depo;
        private readonly IParcalarService _parca;
        private readonly ITedarikciService _tedarikci;

        public ObservableCollection<GeneralStockDto> AllStocks { get; set; } = new();
        public ObservableCollection<GeneralStockDto> FilteredList { get; set; } = new();
        public ObservableCollection<GeneralStockDto> PagedList { get; set; } = new();


        public DateTime? FilterStartDate { get; set; }
        public DateTime? FilterEndDate { get; set; }

        public ObservableCollection<F_GeneralPart> PartList { get; set; } = new();
        public ObservableCollection<F_Supplier> SupplierList { get; set; } = new();
        public ObservableCollection<F_Warehouse> WarehouseList { get; set; } = new();

        public F_GeneralPart SelectedPart { get; set; }
        public F_Supplier SelectedSupplier { get; set; }
        public int PageSize { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages => (int)Math.Ceiling((double)FilteredList.Count / PageSize);

        public ICommand LoadCommand { get; }
        public ICommand OpenAddPopupCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand ApplyFiltersCommand { get; }
        public ICommand ClearFiltersCommand { get; }

        public GeneralStockViewModel(IStokService service, IDepoService depo, IParcalarService parca, ITedarikciService tedarikci)
        {
            _service = service;
            _depo = depo;
            _parca = parca;
            _tedarikci = tedarikci;

            LoadCommand = new RelayCommand<object>(async _ => await LoadListAsync());
            OpenAddPopupCommand = new RelayCommand<object>(_ => OpenAddPopup());
            PreviousPageCommand = new RelayCommand<object>(_ => ChangePage(-1), _ => CurrentPage > 1);
            NextPageCommand = new RelayCommand<object>(_ => ChangePage(1), _ => CurrentPage < TotalPages);

            ApplyFiltersCommand = new RelayCommand<object>(_ => ApplyFilters());
            ClearFiltersCommand = new RelayCommand<object>(_ => ClearFilters());

            _ = LoadListAsync();
        }

        private async Task LoadListAsync()
        {
            try
            {
                PartList = new ObservableCollection<F_GeneralPart>(await _parca.GetAllParcalar());
                SupplierList = new ObservableCollection<F_Supplier>(await _tedarikci.GetAllTedarikcis());
                WarehouseList = new ObservableCollection<F_Warehouse>(await _depo.GetWarehouses());

                OnPropertyChanged(nameof(PartList));
                OnPropertyChanged(nameof(SupplierList));
                OnPropertyChanged(nameof(WarehouseList));

                var result = await _service.GetAllStoks();

                AllStocks.Clear();
                foreach (var item in result)
                {
                    AllStocks.Add(new GeneralStockDto
                    {
                        Id = item.Id,
                        GeneralPartId = item.GeneralPartId,
                        SupplierId = item.SupplierId,
                        WarehouseId = item.WarehouseId,
                        OrderDate = item.OrderDate,
                        DeliveryDate = item.DeliveryDate,
                        OrderQuantity = item.OrderQuantity,
                        ReceivedQuantity = item.ReceivedQuantity,

                        GeneralPartName = item.GeneralPart?.F_MalzemeAdi,
                        SupplierName = item.Supplier?.F_TedarikciAdi,
                        WarehouseName = item.Warehose?.DepoAdi
                    });
                }


                ApplyFilters(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veri yüklenirken hata oluştu:\n" + ex.Message);
            }
        }


        public void ApplyFilters()
        {
            FilteredList.Clear();

            var filtered = AllStocks.AsEnumerable();

            if (FilterStartDate != null)
                filtered = filtered.Where(x => x.DeliveryDate >= FilterStartDate.Value);

            if (FilterEndDate != null)
                filtered = filtered.Where(x => x.DeliveryDate <= FilterEndDate.Value);

            if (SelectedPart != null)
                filtered = filtered.Where(x => x.GeneralPartId == SelectedPart.Id);

            if (SelectedSupplier != null)
                filtered = filtered.Where(x => x.SupplierId == SelectedSupplier.Id);

            foreach (var item in filtered)
                FilteredList.Add(item);

            CurrentPage = 1;
            UpdatePagedList();
            OnPropertyChanged(nameof(TotalPages));
        }


        private void UpdatePagedList()
        {
            PagedList.Clear();

            var items = FilteredList
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize);

            foreach (var item in items)
                PagedList.Add(item);

            OnPropertyChanged(nameof(CurrentPage));
            OnPropertyChanged(nameof(TotalPages));
        }

        private void ChangePage(int delta)
        {
            CurrentPage += delta;
            UpdatePagedList();
        }

        private void OpenAddPopup()
        {
            var popup = new GeneralStockAddWindow(_service, _depo, _parca, _tedarikci);
            bool? result = popup.ShowDialog();

            if (result == true)
                _ = LoadListAsync();
        }

        private void ClearFilters()
        {
            FilterStartDate = null;
            FilterEndDate = null;
            SelectedPart = null;
            SelectedSupplier = null;

            OnPropertyChanged(nameof(FilterStartDate));
            OnPropertyChanged(nameof(FilterEndDate));
            OnPropertyChanged(nameof(SelectedPart));
            OnPropertyChanged(nameof(SelectedSupplier));

            ApplyFilters();
        }

    }

}
