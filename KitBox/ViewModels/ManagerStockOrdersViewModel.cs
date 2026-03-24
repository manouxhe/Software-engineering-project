using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using KitBox.Models;
using KitBox.Services;
using System;

namespace KitBox.ViewModels
{
    public class ManagerStockOrdersViewModel : ViewModelBase
    {
        public ObservableCollection<StockOrder> StockOrders { get; } = new ObservableCollection<StockOrder>();
        public ReactiveCommand<StockOrder, Unit> ValidateStockOrderCommand { get; }

        public ManagerStockOrdersViewModel()
        {
            LoadStockOrders();

            MessageBus.Current.Listen<string>().Subscribe(message =>
            {
                if (message == "OrderCreated")
                {
                    LoadStockOrders(); // Fait apparaître la commande instantanément !
                }
            });

            ValidateStockOrderCommand = ReactiveCommand.Create<StockOrder>(order =>
            {
                if (order != null && order.Status == "In progress")
                {
                    bool success = StockOrderService.ReceiveStockOrder(order.Id, order.PartCode, order.Quantity);
                    if (success)
                    {
                        LoadStockOrders();

                        // 2. NOUVEAU : On prévient l'autre onglet que le stock physique a augmenté
                        MessageBus.Current.SendMessage("StockUpdated");
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