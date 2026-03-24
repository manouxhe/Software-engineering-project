using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using KitBox.Models;
using KitBox.Services;

namespace KitBox.ViewModels
{
    public class ManagerStockOrdersViewModel : ViewModelBase
    {
        public ObservableCollection<StockOrder> StockOrders { get; } = new ObservableCollection<StockOrder>();
        public ReactiveCommand<StockOrder, Unit> ValidateStockOrderCommand { get; }

        public ManagerStockOrdersViewModel()
        {
            LoadStockOrders();

            ValidateStockOrderCommand = ReactiveCommand.Create<StockOrder>(order =>
            {
                if (order != null && order.Status == "In progress")
                {
                    bool success = StockOrderService.ReceiveStockOrder(order.Id, order.PartCode, order.Quantity);
                    if (success)
                    {
                        LoadStockOrders(); // Recharge la liste pour mettre à jour l'affichage
                    }
                }
            });
        }

        public void LoadStockOrders()
        {
            StockOrders.Clear();
            var fetchedStockOrders = StockOrderService.GetAllStockOrders();
            foreach (var order in fetchedStockOrders)
            {
                StockOrders.Add(order);
            }
        }
    }
}