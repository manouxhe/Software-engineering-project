using System.Collections.ObjectModel;
using System.Reactive;
using KitBox.Models;
using KitBox.Services;
using ReactiveUI;

namespace KitBox.ViewModels
{
    public class ManagerOrdersViewModel : ViewModelBase
    {
        public ObservableCollection<Order> Orders { get; } = new ObservableCollection<Order>();
        public ReactiveCommand<Order, Unit> MarkAsCompleteCommand { get; }

        public ManagerOrdersViewModel()
        {
            LoadOrders();
            MarkAsCompleteCommand = ReactiveCommand.Create<Order>(OnMarkAsComplete);
        }

        public void LoadOrders()
        {
            Orders.Clear();

            var fetchedOrders = OrderService.GetAllOrders();

            foreach (var order in fetchedOrders)
            {
                Orders.Add(order);
            }
        }
        private void OnMarkAsComplete(Order order)
        {
            if (order != null && order.Status == "En attente")
            {
                bool success = OrderService.UpdateOrderStatusToComplete(order.Id);

                if (success)
                {
                    LoadOrders();
                }
            }
        }
    }
}