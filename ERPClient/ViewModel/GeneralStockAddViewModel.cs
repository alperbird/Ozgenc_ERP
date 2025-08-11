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
        private GeneralStock _existingStock;

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
        public string? Metraj { get; set; }
        public ICommand SaveCommand { get; }


        public int SelectedPartId
        {
            get => SelectedPart?.Id ?? 0;
            set
            {
                SelectedPart = PartList.FirstOrDefault(p => p.Id == value) ?? new F_GeneralPart();
                OnPropertyChanged(nameof(SelectedPartId));
                OnPropertyChanged(nameof(SelectedPart));
            }
        }


        public int SelectedSupplierId
        {
            get => SelectedSupplier?.Id ?? 0;
            set
            {
                SelectedSupplier = SupplierList.FirstOrDefault(s => s.Id == value) ?? new F_Supplier();
                OnPropertyChanged(nameof(SelectedSupplierId));
                OnPropertyChanged(nameof(SelectedSupplier));
            }
        }

        public int SelectedWarehouseId
        {
            get => SelectedWarehouse?.Id ?? 0;
            set
            {
                SelectedWarehouse = WarehouseList.FirstOrDefault(w => w.Id == value) ?? new F_Warehouse();
                OnPropertyChanged(nameof(SelectedWarehouseId));
                OnPropertyChanged(nameof(SelectedWarehouse));
            }
        }


        private readonly int? _selectedStockId;
        public GeneralStockAddViewModel(IStokService stok, IDepoService depo, IParcalarService parca, ITedarikciService tedarikci, User user, int? id = null)
        {
            _stokService = stok;
            _depoService = depo;
            _parcaService = parca;
            _tedarikciService = tedarikci;
            _currentUser = user;
            _selectedStockId = id;

            SaveCommand = new RelayCommand<object>(async _ => await SaveAsync());
            AddPartCommand = new RelayCommand<object>(_ => OpenAddPart());
            AddSupplierCommand = new RelayCommand<object>(_ => OpenAddSupplier());
            AddWarehouseCommand = new RelayCommand<object>(_ => OpenAddWarehouse());

            _ = LoadListsAsync();
        }

        
        private async Task LoadListsAsync()
        {
            // 1. Tüm liste verilerini önce çek
            var parts = await _parcaService.GetAllParcalar();
            var suppliers = await _tedarikciService.GetAllTedarikcis();
            var warehouses = await _depoService.GetWarehouses();

            // 2. Arama listelerine ata
            _allParts = new ObservableCollection<F_GeneralPart>(parts);
            _allSuppliers = new ObservableCollection<F_Supplier>(suppliers);
            _allWarehouses = new ObservableCollection<F_Warehouse>(warehouses);

            // 3. Görünen listelere ata
            PartList = new ObservableCollection<F_GeneralPart>(_allParts);
            SupplierList = new ObservableCollection<F_Supplier>(_allSuppliers);
            WarehouseList = new ObservableCollection<F_Warehouse>(_allWarehouses);

            OnPropertyChanged(nameof(PartList));
            OnPropertyChanged(nameof(SupplierList));
            OnPropertyChanged(nameof(WarehouseList));

            // 4. _selectedStockId varsa, değerleri ata (liste dolduktan sonra!)
            if (_selectedStockId.HasValue)
            {
                _existingStock = await _stokService.GetStokById(_selectedStockId.Value);
                if (_existingStock != null)
                {
                    SelectedPart = PartList.FirstOrDefault(p => p.Id == _existingStock.GeneralPartId);
                    SelectedSupplier = SupplierList.FirstOrDefault(s => s.Id == _existingStock.SupplierId);
                    SelectedWarehouse = WarehouseList.FirstOrDefault(w => w.Id == _existingStock.WarehouseId);
                    OrderDate = _existingStock.OrderDate;
                    DeliveryDate = _existingStock.DeliveryDate;
                    OrderedQuantity = _existingStock.OrderQuantity;
                    ReceivedQuantity = _existingStock.ReceivedQuantity;
                    Metraj = _existingStock.Metraj;

                    OnPropertyChanged(nameof(SelectedPart));
                    OnPropertyChanged(nameof(SelectedSupplier));
                    OnPropertyChanged(nameof(SelectedWarehouse));
                    OnPropertyChanged(nameof(OrderDate));
                    OnPropertyChanged(nameof(DeliveryDate));
                    OnPropertyChanged(nameof(OrderedQuantity));
                    OnPropertyChanged(nameof(ReceivedQuantity));
                    OnPropertyChanged(nameof(Metraj));
                }
            }
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

            if (_selectedStockId.HasValue)
            {
                //Id = _selectedStockId.Value,
                _existingStock.GeneralPartId = SelectedPart.Id;
                _existingStock.SupplierId = SelectedSupplier.Id;
                _existingStock.WarehouseId = SelectedWarehouse.Id;
                _existingStock.OrderDate = OrderDate ?? DateTime.Now;
                _existingStock.DeliveryDate = DeliveryDate ?? DateTime.Now;
                _existingStock.OrderQuantity = OrderedQuantity;
                _existingStock.ReceivedQuantity = ReceivedQuantity;
                _existingStock.Metraj = Metraj;


                await _stokService.UpdateStok(_existingStock);
                MessageBox.Show("Stok başarıyla güncellendi.");
                LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} stok güncellemesi yaptı. Parça: {SelectedPart.F_MalzemeAdi},\n Tedarikçi: {SelectedSupplier.F_TedarikciAdi},\n Depo: {SelectedWarehouse.DepoAdi}");
            }

            else
            {
                var entity = new GeneralStock
                {
                    GeneralPartId = SelectedPart.Id,
                    SupplierId = SelectedSupplier.Id,
                    WarehouseId = SelectedWarehouse.Id,
                    OrderDate = OrderDate ?? DateTime.Now,
                    DeliveryDate = DeliveryDate ?? DateTime.Now,
                    OrderQuantity = OrderedQuantity,
                    ReceivedQuantity = ReceivedQuantity,
                    Metraj = Metraj
                };

                await _stokService.AddStok(entity);
                MessageBox.Show("Stok başarıyla eklendi.");
                LogHelper.LogAction(_currentUser.Id, $"{_currentUser.Name} {_currentUser.Surname} yeni bir stok ekledi. Parça: {SelectedPart.F_MalzemeAdi},\n Tedarikçi: {SelectedSupplier.F_TedarikciAdi},\n Depo: {SelectedWarehouse.DepoAdi}");
            }

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
