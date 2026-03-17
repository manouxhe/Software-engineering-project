using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using KitBox.Models;
using KitBox.Services;
using System;

namespace KitBox.ViewModels
{
    public class FinalizeViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;
        public ObservableCollection<Cabinet> Items { get; }
        public ObservableCollection<string> StockAlerts { get; } = new();
        public ObservableCollection<string> InfoMessages { get; } = new();

        private bool _hasStockIssue;
        public bool HasStockIssue
        {
            get => _hasStockIssue;
            set => this.RaiseAndSetIfChanged(ref _hasStockIssue, value);
        }

        private string _emailAddress = string.Empty;
        public string EmailAddress
        {
            get => _emailAddress;
            set => this.RaiseAndSetIfChanged(ref _emailAddress, value);
        }

        private string _paymentMessage = "Confirmez votre panier svp pour finaliser l'achat.";
        public string PaymentMessage
        {
            get => _paymentMessage;
            set => this.RaiseAndSetIfChanged(ref _paymentMessage, value);
        }

        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get => _totalPrice;
            set => this.RaiseAndSetIfChanged(ref _totalPrice, value);
        }

        public ReactiveCommand<Unit, Unit> PayCommand { get; }
        public ReactiveCommand<Unit, Unit> GoHomeCommand { get; }

        public FinalizeViewModel(MainViewModel main, ObservableCollection<Cabinet> items)
        {
            _main = main;
            Items = items;

            RecomputeCheckout();

            var canPay = this.WhenAnyValue(
                x => x.HasStockIssue,
                x => x.EmailAddress,
                (hasIssue, email) => !hasIssue || IsValidEmail(email));

            PayCommand = ReactiveCommand.Create(OnPay, canPay);
            GoHomeCommand = ReactiveCommand.Create(() => _main.NavigateTo(new HomeViewModel(_main)));
        }

        private void RecomputeCheckout()
        {
            StockAlerts.Clear();
            InfoMessages.Clear();
            TotalPrice = 0;

            foreach (var cabinet in Items)
            {
                var checkout = PartService.GetCheckoutDetails(cabinet);
                TotalPrice += checkout.TotalPrice;

                foreach (var message in checkout.Messages)
                {
                    InfoMessages.Add(message);
                }

                foreach (var missing in checkout.MissingItems)
                {
                    StockAlerts.Add($"{missing.Label} : {missing.AvailableQuantity}/{missing.RequiredQuantity} en stock.");
                }
            }

            HasStockIssue = StockAlerts.Count > 0 || InfoMessages.Count > 0;

            if (HasStockIssue)
            {
                PaymentMessage = "Stock limité sur certains articles : laissez votre e-mail pour recevoir une notification de retour en stock.";
            }
            else
            {
                PaymentMessage = "Tous les articles sont en stock. Vous pouvez payer maintenant.";
            }
        }

        private void OnPay()
        {
            bool allSuccess = true;

            foreach (var cabinet in Items)
            {
                var checkout = PartService.GetCheckoutDetails(cabinet);

                // On ut l'email comme identifiant client
                string emailClient = EmailAddress;

                Console.WriteLine($"\n--- DEBUG SAUVEGARDE COMMANDE ---");
                Console.WriteLine($"Nombre de pièces différentes à déduire : {checkout.UsedParts.Count}");
                foreach (var p in checkout.UsedParts)
                {
                    Console.WriteLine($"ID: {p.Key} -> Quantité à déduire: {p.Value}");
                }
                Console.WriteLine($"---------------------------------\n");
                // --------------------------------

                bool success = OrderService.FinalizeOrder(emailClient, checkout.TotalPrice, checkout.MissingItems.Count == 0, cabinet, checkout.UsedParts);

                if (!success)
                {
                    allSuccess = false;
                }
            }

            if (allSuccess)
            {
                Items.Clear(); // On vide le panier car tout a été enregistré

                if (HasStockIssue)
                {
                    PaymentMessage = $"Commande enregistrée, Un e-mail sera envoyé à {EmailAddress} dès l'arrivée des pièces.";
                }
                else
                {
                    PaymentMessage = "Paiement validé ! Votre commande est enregistrée et le stock a été mis à jour.";
                }
            }
            else
            {
                PaymentMessage = "Erreur de connexion. La commande a été annulée.";
            }
        }

        private static bool IsValidEmail(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return value.Contains('@') && value.Contains('.') && !value.EndsWith('.');
        }
    }
}