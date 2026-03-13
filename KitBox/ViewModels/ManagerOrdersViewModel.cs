using System.Collections.ObjectModel;
using KitBox.Models;
using KitBox.Services;

namespace KitBox.ViewModels
{
    public class ManagerOrdersViewModel : ViewModelBase
    {
        public ObservableCollection<Order> Orders { get; } = new ObservableCollection<Order>();

        public ManagerOrdersViewModel()
        {
            LoadOrders();
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
    }
}