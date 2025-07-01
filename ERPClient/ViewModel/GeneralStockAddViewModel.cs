using BusinessLayer.Interfaces;
using DataAccessLayer.Interfaces;
using EntityLayer.Entities;
using ERPClient.GenelModule;
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

        public ObservableCollection<F_GeneralPart> PartList { get; set; } = new();
        public ObservableCollection<F_Supplier> SupplierList { get; set; } = new();
        public ObservableCollection<F_Warehouse> WarehouseList { get; set; } = new();

        public F_GeneralPart SelectedPart { get; set; } = new F_GeneralPart();
        public F_Supplier SelectedSupplier { get; set; } = new F_Supplier();
        public F_Warehouse SelectedWarehouse { get; set; } = new F_Warehouse();

        public DateTime? OrderDate { get; set; } = DateTime.Now;
        public DateTime? DeliveryDate { get; set; } = DateTime.Now;

        public int OrderedQuantity { get; set; }
        public int ReceivedQuantity { get; set; }

        public ICommand SaveCommand { get; }

        public GeneralStockAddViewModel(IStokService stok, IDepoService depo, IParcalarService parca, ITedarikciService tedarikci)
        {
            _stokService = stok;
            _depoService = depo;
            _parcaService = parca;
            _tedarikciService = tedarikci;

            SaveCommand = new RelayCommand<object>(async _ => await SaveAsync());

            _ = LoadListsAsync();
        }

        private async Task LoadListsAsync()
        {
            PartList = new ObservableCollection<F_GeneralPart>(await _parcaService.GetAllParcalar());
            SupplierList = new ObservableCollection<F_Supplier>(await _tedarikciService.GetAllTedarikcis());
            WarehouseList = new ObservableCollection<F_Warehouse>(await _depoService.GetWarehouses());

            OnPropertyChanged(nameof(PartList));
            OnPropertyChanged(nameof(SupplierList));
            OnPropertyChanged(nameof(WarehouseList));
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
                    break;
                }
            }
        }
    }
}
