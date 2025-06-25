using DataAccessLayer.Repositories;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.SignalR;

namespace BusinessLayer.Services
{
    public class StockService //Bu yedek parçaların servisi. KARIŞTIRMAYIN!!!
    {
        private readonly ProductRepository _repo;
        private readonly IHubContext<StockHub> _hub;

        public StockService(ProductRepository repo, IHubContext<StockHub> hub)
        {
            _repo = repo;
            _hub = hub;
        }

        public async Task<bool> ChangeStockAsync(int productId, int amount)
        {
            var product = await _repo.GetByIdAsync(productId);
            if (product == null)
                return false;

            product.Stok += amount;
            await _repo.UpdateProduct(product);

            await _hub.Clients.All.SendAsync("ReceiveStockUpdate", new
            {
                Id = product.Id,
                ParcaAdi = product.ParcaAdi,
                Stok = product.Stok
            });

            return true;
        }
    }
}
