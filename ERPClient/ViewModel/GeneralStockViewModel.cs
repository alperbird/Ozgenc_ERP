using EntityLayer.DTOs;
using BusinessLayer.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ERPClient.GenelModule;
using EntityLayer.Entities;
using ERPClient.Model;

namespace ERPClient.ViewModel
{
    public class GeneralStockViewModel : BaseViewModel
    {
        private readonly IStokService _service;
        private readonly IDepoService _depo;
        private readonly IParcalarService _parca;
        private readonly ITedarikciService _tedarikci;
        private readonly User _currentuser;

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
        public int PageSize { get; set; } = 25;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages => (int)Math.Ceiling((double)FilteredList.Count / PageSize);

        public ICommand LoadCommand { get; }
        public ICommand OpenAddPopupCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand ApplyFiltersCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        //public ICommand Update { get; set; }
        public ICommand Delete { get; set; }

        private ObservableCollection<F_GeneralPart> _allParts = new(); // combobox arama kısmı için kullanılacak
        private string _partSearchText;
        public string PartSearchText
        {
            get => _partSearchText;
            set
            {
                _partSearchText = value;
                OnPropertyChanged(nameof(PartSearchText));
                FilterParts();
            }
        }

        private ObservableCollection<F_Supplier> _allSuppliers = new();
        private string _supplierSearchText;
        public string SupplierSearchText
        {
            get => _supplierSearchText;
            set
            {
                _supplierSearchText = value;
                OnPropertyChanged(nameof(SupplierSearchText));
                FilterSuppliers();
            }
        }

        public GeneralStockViewModel(IStokService service, IDepoService depo, IParcalarService parca, ITedarikciService tedarikci, User user)
        {
            _service = service;
            _depo = depo;
            _parca = parca;
            _tedarikci = tedarikci;
            _currentuser = user;

            LoadCommand = new RelayCommand<object>(async _ => await LoadListAsync());
            OpenAddPopupCommand = new RelayCommand<object>(_ => OpenAddPopup());
            PreviousPageCommand = new RelayCommand<object>(_ => ChangePage(-1), _ => CurrentPage > 1);
            NextPageCommand = new RelayCommand<object>(_ => ChangePage(1), _ => CurrentPage < TotalPages);
            Delete = new RelayCommand<GeneralStockDto>(DeleteItem);
            

            ApplyFiltersCommand = new RelayCommand<object>(_ => ApplyFilters());
            ClearFiltersCommand = new RelayCommand<object>(_ => ClearFilters());

            _ = LoadListAsync();
        }

        private void FilterParts()
        {
            if (string.IsNullOrWhiteSpace(PartSearchText))
            {
                PartList = new ObservableCollection<F_GeneralPart>(_allParts);
            }
            else
            {
                var keyword = PartSearchText.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var filtered = _allParts
                    .Where(p => p.F_MalzemeAdi != null &&
                                 keyword.All(k => p.F_MalzemeAdi.ToLower().Contains(k)))
                    .ToList();

                PartList = new ObservableCollection<F_GeneralPart>(filtered);
            }
            OnPropertyChanged(nameof(PartList));
        }

        private void FilterSuppliers()
        {
            if (string.IsNullOrWhiteSpace(SupplierSearchText))
            {
                SupplierList = new ObservableCollection<F_Supplier>(_allSuppliers);
            }
            else
            {
                var keyword = SupplierSearchText.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var filtered = _allSuppliers
                    .Where(s => s.F_TedarikciAdi != null &&
                                 keyword.All(k => s.F_TedarikciAdi.ToLower().Contains(k)))
                    .ToList();
                SupplierList = new ObservableCollection<F_Supplier>(filtered);
            }
            OnPropertyChanged(nameof(SupplierList));

        }

        private async Task LoadListAsync()
        {
            try
            {
                PartList = new ObservableCollection<F_GeneralPart>(await _parca.GetAllParcalar());
                SupplierList = new ObservableCollection<F_Supplier>(await _tedarikci.GetAllTedarikcis());
                WarehouseList = new ObservableCollection<F_Warehouse>(await _depo.GetWarehouses());

                _allParts = new ObservableCollection<F_GeneralPart>(await _parca.GetAllParcalar()); // Arama için
                PartList = new ObservableCollection<F_GeneralPart>(_allParts);

                _allSuppliers = new ObservableCollection<F_Supplier>(await _tedarikci.GetAllTedarikcis()); // Arama için
                SupplierList = new ObservableCollection<F_Supplier>(_allSuppliers);

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
            var popup = new GeneralStockAddWindow(_service, _depo, _parca, _tedarikci, _currentuser);
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

        private async void DeleteItem(GeneralStockDto item)
        {
            if (item == null)
            {
                return;
            }

            var confirm = MessageBox.Show("Bu kaydı silmek istediğinize emin misiniz?", "Onay", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                await _service.DeleteStok(item.Id);
                MessageBox.Show("Kayıt başarıyla silindi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                LogHelper.LogAction(_currentuser.Id, $"{_currentuser.Name} {_currentuser.Surname} Genel Parça Stok Silme işlemi yaptı. Stok ID: {item.Id}, Parça Adı: {item.GeneralPartName}");
                _ = LoadListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kayıt silinirken hata oluştu:\n" + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

    }

}
