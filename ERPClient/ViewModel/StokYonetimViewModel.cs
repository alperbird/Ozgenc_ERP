using ERPClient.DTOs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ERPClient.ViewModel
{
    public class StokYonetimViewModel : INotifyPropertyChanged
    {
        private const int PageSize = 10;

        public ObservableCollection<ProductStockDto> AllProducts { get; set; } = new();
        public ObservableCollection<ProductStockDto> PagedProducts { get; set; } = new();

        private ObservableCollection<ProductStockDto> _filteredProducts = new();
        public ObservableCollection<ProductStockDto> FilteredProducts
        {
            get => _filteredProducts;
            set
            {
                _filteredProducts = value;
                OnPropertyChanged(nameof(FilteredProducts));
            }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                ApplyFilter();
            }
        }

        private ProductStockDto? _selectedProduct;
        public ProductStockDto? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged(nameof(SelectedProduct));
            }
        }

        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
                UpdatePagedProducts();
            }
        }

        public string PageInfo => $"Sayfa {CurrentPage} / {TotalPages}";
        public int TotalPages => (int)Math.Ceiling((double)AllProducts.Count / PageSize);

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        public void LoadProducts(List<ProductStockDto> products)
        {
            AllProducts = new ObservableCollection<ProductStockDto>(products);
            FilteredProducts = new ObservableCollection<ProductStockDto>(AllProducts);
            CurrentPage = 1;
            UpdatePagedProducts();
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredProducts = new ObservableCollection<ProductStockDto>(AllProducts);
            }
            else
            {
                var filtered = AllProducts
                    .Where(p => p.ParcaAdi != null && p.ParcaAdi.ToLower().Contains(SearchText.ToLower()))
                    .ToList();

                FilteredProducts = new ObservableCollection<ProductStockDto>(filtered);
            }
        }

        public void UpdatePagedProducts()
        {
            PagedProducts.Clear();

            var items = AllProducts
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            foreach (var item in items)
                PagedProducts.Add(item);

            OnPropertyChanged(nameof(PageInfo));
        }

        public void NextPage()
        {
            if (CurrentPage < TotalPages)
                CurrentPage++;
        }

        public void PrevPage()
        {
            if (CurrentPage > 1)
                CurrentPage--;
        }
    }
}
