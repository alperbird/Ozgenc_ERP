using BusinessLayer.Interfaces;
using DataAccessLayer.Interfaces;
using EntityLayer.Entities;
using ERPClient.GenelModule;
using ERPClient.Model;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ERPClient.ViewModel
{
    public class GeneralStockAddViewModel : BaseViewModel
    {
        private readonly IStokService _stokService;
        private readonly IDepoService _depoService;
        private readonly IParcalarService _parcaService;
        private readonly ITedarikciService _tedarikciService;

        private readonly User _currentUser;

        public ICommand AddPartCommand { get; set; }
        public ICommand AddSupplierCommand { get; set; }
        public ICommand AddWarehouseCommand { get; set; }

        public ObservableCollection<F_GeneralPart> PartList { get; set; } = new();
        public ObservableCollection<F_Supplier> SupplierList { get; set; } = new();
        public ObservableCollection<F_Warehouse> WarehouseList { get; set; } = new();

        public F_GeneralPart SelectedPart { get; set; } = new F_GeneralPart();
        public F_Supplier SelectedSupplier { get; set; } = new F_Supplier();
        public F_Warehouse SelectedWarehouse { get; set; } = new F_Warehouse();

        private ObservableCollection<F_GeneralPart> _allParts = new(); // combobox arama kısmı için kullanılacak
        private string _partSearchText;
        public string PartSearchText { 
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

        private ObservableCollection<F_Warehouse> _allWarehouses = new();
        private string _warehouseSearchText;
        public string WarehouseSearchText
        {
            get => _warehouseSearchText;
            set
            {
                _warehouseSearchText = value;
                OnPropertyChanged(nameof(WarehouseSearchText));
                FilterWarehouses();
            }
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

        private void FilterWarehouses()
        {
            if (string.IsNullOrWhiteSpace(WarehouseSearchText))
            {
                WarehouseList = new ObservableCollection<F_Warehouse>(_allWarehouses);
            }
            else
            {
                var filtered = _allWarehouses
                    .Where(w => w.DepoAdi != null &&
                                 w.DepoAdi.IndexOf(WarehouseSearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
                WarehouseList = new ObservableCollection<F_Warehouse>(filtered);
            }
            OnPropertyChanged(nameof(WarehouseList));
        }

        public DateTime? OrderDate { get; set; } = DateTime.Now;
        public DateTime? DeliveryDate { get; set; } = DateTime.Now;

        public int OrderedQuantity { get; set; }
        public int ReceivedQuantity { get; set; }

        public ICommand SaveCommand { get; }

        public GeneralStockAddViewModel(IStokService stok, IDepoService depo, IParcalarService parca, ITedarikciService tedarikci, User user)
        {
            _stokService = stok;
            _depoService = depo;
            _parcaService = parca;
            _tedarikciService = tedarikci;
            _currentUser = user;

            SaveCommand = new RelayCommand<object>(async _ => await SaveAsync());
            AddPartCommand = new RelayCommand<object>(_ => OpenAddPart());
            AddSupplierCommand = new RelayCommand<object>(_ => OpenAddSupplier());
            AddWarehouseCommand = new RelayCommand<object>(_ => OpenAddWarehouse());

            _ = LoadListsAsync();
        }

        private async Task LoadListsAsync()
        {
            PartList = new ObservableCollection<F_GeneralPart>(await _parcaService.GetAllParcalar());
            SupplierList = new ObservableCollection<F_Supplier>(await _tedarikciService.GetAllTedarikcis());
            WarehouseList = new ObservableCollection<F_Warehouse>(await _depoService.GetWarehouses());

            _allParts = new ObservableCollection<F_GeneralPart>(await _parcaService.GetAllParcalar()); // Arama için
            PartList = new ObservableCollection<F_GeneralPart>(_allParts);

            _allSuppliers = new ObservableCollection<F_Supplier>(await _tedarikciService.GetAllTedarikcis()); // Arama için
            SupplierList = new ObservableCollection<F_Supplier>(_allSuppliers);

            _allWarehouses = new ObservableCollection<F_Warehouse>(await _depoService.GetWarehouses()); // Arama için
            WarehouseList = new ObservableCollection<F_Warehouse>(_allWarehouses);

            OnPropertyChanged(nameof(PartList));
            OnPropertyChanged(nameof(SupplierList));
            OnPropertyChanged(nameof(WarehouseList));
        }

        private async void OpenAddPart()
        {
            var part = new GeneralPartAddWindow(_parcaService, _currentUser);

            if(part.ShowDialog() == true)
            {
                await LoadListsAsync(); // Yeni parça eklendikten sonra listeyi güncelle
            }
        }

        private async void OpenAddSupplier()
        {
            var supplier = new SupplierAddWindow(_tedarikciService, _currentUser);
            if (supplier.ShowDialog() == true)
            {
                await LoadListsAsync(); // Yeni tedarikçi eklendikten sonra listeyi güncelle
            }
        }

        private async void OpenAddWarehouse()
        {
            var warehouse = new WarehouseAddWindow(_depoService, _currentUser);
            if (warehouse.ShowDialog() == true)
            {
                await LoadListsAsync(); // Yeni depo eklendikten sonra listeyi güncelle
            }
        }

        private async Task<bool> SaveAsync()
        {
            if (SelectedPart == null || SelectedSupplier == null || SelectedWarehouse == null)
            {
                MessageBox.Show("Lütfen tüm alanları doldurun.");
                return false;
            }

            var entity = new GeneralStock
            {
                GeneralPartId = SelectedPart.Id,
                SupplierId = SelectedSupplier.Id,
                WarehouseId = SelectedWarehouse.Id,
                OrderDate = OrderDate ?? DateTime.Now,
                DeliveryDate = DeliveryDate ?? DateTime.Now,
                OrderQuantity = OrderedQuantity,
                ReceivedQuantity = ReceivedQuantity
            };

            await _stokService.AddStok(entity);
            MessageBox.Show("Stok başarıyla eklendi.");
            LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} yeni bir stok ekledi. Parça: {SelectedPart.F_MalzemeAdi},\n Tedarikçi: {SelectedSupplier.F_TedarikciAdi},\n Depo: {SelectedWarehouse.DepoAdi}");
            ClosePopup(true);
            return true;
        }

        private void ClosePopup(bool result)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is GeneralStockAddWindow)
                {
                    window.DialogResult = result;
                    window.Close();
                    LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} Genel Parça Stok Ekleme penceresini kapattı.");
                    break;
                }
            }
        }
    }
}
