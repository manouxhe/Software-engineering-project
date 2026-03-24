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
        private Order? _selectedOrder;
        public Order? SelectedOrder
        {
            get => _selectedOrder;
            set => this.RaiseAndSetIfChanged(ref _selectedOrder, value);
        }

        private bool _isPopupVisible;
        public bool IsPopupVisible
        {
            get => _isPopupVisible;
            set => this.RaiseAndSetIfChanged(ref _isPopupVisible, value);
        }
        public ObservableCollection<string> OrderDetailsList { get; } = new ObservableCollection<string>();
        public ReactiveCommand<Order, Unit> MarkAsCompleteCommand { get; }
        public ReactiveCommand<Order, Unit> OpenDetailsCommand { get; }
        public ReactiveCommand<Unit, Unit> CloseDetailsCommand { get; }

        public ManagerOrdersViewModel()
        {
            LoadOrders();

            MarkAsCompleteCommand = ReactiveCommand.Create<Order>(OnMarkAsComplete);

            OpenDetailsCommand = ReactiveCommand.Create<Order>(OnOpenDetails);

            CloseDetailsCommand = ReactiveCommand.Create(() => { IsPopupVisible = false; });
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
            if (order != null && order.Status == "In progress")
            {
                bool success = OrderService.UpdateOrderStatusToComplete(order.Id);

                if (success)
                {
                    LoadOrders();
                }
            }
        }

        private void OnOpenDetails(Order order)
        {
            if (order == null) return;

            SelectedOrder = order;
            OrderDetailsList.Clear();

            var cabinet = OrderService.GetCabinetByOrderId(order.Id);

            if (cabinet != null)
            {
                var checkout = PartService.GetCheckoutDetails(cabinet);

                if (checkout.UsedParts.Count > 0)
                {
                    foreach (var part in checkout.UsedParts)
                    {
                        OrderDetailsList.Add($"{part.Value}x {part.Key}");
                    }
                }
                else
                {
                    OrderDetailsList.Add("Aucune pièce détectée.");
                }
            }
            else
            {
                OrderDetailsList.Add("Erreur : impossible de retrouver le meuble pour cette commande.");
            }
            IsPopupVisible = true;
        }
    }
}