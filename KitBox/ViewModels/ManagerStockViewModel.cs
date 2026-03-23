using System.Collections.ObjectModel;
using System.Reactive;
using System.Linq;
using ReactiveUI;
using KitBox.Models;
using KitBox.Services;

namespace KitBox.ViewModels
{
    public class ManagerStockViewModel : ViewModelBase
    {
        public ObservableCollection<Part> Parts { get; } = new ObservableCollection<Part>();
        public ObservableCollection<SupplierOffer> AvailableOffers { get; } = new ObservableCollection<SupplierOffer>();

        // Gestion du Popup
        private bool _isPopupOpen;
        public bool IsPopupOpen { get => _isPopupOpen; set => this.RaiseAndSetIfChanged(ref _isPopupOpen, value); }

        private Part? _selectedPart;
        public Part? SelectedPart { get => _selectedPart; set => this.RaiseAndSetIfChanged(ref _selectedPart, value); }

        // Formulaire de commande
        private SupplierOffer? _selectedOffer;
        public SupplierOffer? SelectedOffer
        {
            get => _selectedOffer;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedOffer, value);
                this.RaisePropertyChanged(nameof(TotalPrice)); // Recalcule le prix
            }
        }

        private int _orderQuantity = 1;
        public int OrderQuantity
        {
            get => _orderQuantity;
            set
            {
                this.RaiseAndSetIfChanged(ref _orderQuantity, value);
                this.RaisePropertyChanged(nameof(TotalPrice)); // Recalcule le prix
            }
        }

        public decimal TotalPrice => (SelectedOffer?.PurchasePrice ?? 0) * OrderQuantity;

        // Commandes
        public ReactiveCommand<Part, Unit> OpenOrderPopupCommand { get; }
        public ReactiveCommand<Unit, Unit> ClosePopupCommand { get; }
        public ReactiveCommand<Unit, Unit> ValidateOrderCommand { get; }

        public ManagerStockViewModel()
        {
            LoadParts();

            OpenOrderPopupCommand = ReactiveCommand.Create<Part>(part =>
            {
                if (part == null) return;
                SelectedPart = part;

                // On suggère une quantité pour remonter le stock à 10 au-dessus du minimum
                OrderQuantity = part.MinStock >= part.Stock ? (part.MinStock - part.Stock) + 10 : 10;

                AvailableOffers.Clear();
                var offers = StockOrderService.GetOffersForPart(part.Code);
                foreach (var offer in offers) AvailableOffers.Add(offer);

                SelectedOffer = AvailableOffers.FirstOrDefault(); // Sélectionne le 1er fournisseur par défaut
                IsPopupOpen = true;
            });

            ClosePopupCommand = ReactiveCommand.Create(() =>
            {
                IsPopupOpen = false;
                SelectedPart = null;
            });

            // On active le bouton Valider uniquement si une offre est choisie et qté > 0
            var canValidate = this.WhenAnyValue(
                x => x.SelectedOffer, x => x.OrderQuantity,
                (offer, qty) => offer != null && qty > 0);

            ValidateOrderCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedPart != null && SelectedOffer != null)
                {
                    bool success = StockOrderService.CreateStockOrder(SelectedPart.Code, SelectedOffer.SupplierId, OrderQuantity, TotalPrice);
                    if (success)
                    {
                        IsPopupOpen = false;
                        // On recharge la liste pour voir les changements (optionnel car la livraison n'est pas immédiate en vrai)
                        LoadParts();
                    }
                }
            }, canValidate);
        }

        public void LoadParts()
        {
            Parts.Clear();
            var fetchedParts = PartService.GetAllParts();
            foreach (var part in fetchedParts) Parts.Add(part);
        }
    }
}