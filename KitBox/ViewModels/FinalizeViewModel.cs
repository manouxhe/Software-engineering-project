using System.Collections.ObjectModel;
using System.Collections.Generic;
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

        private string _paymentMessage = "To finalize your purchase, please validate your cart.";
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

            }

            HasStockIssue = StockAlerts.Count > 0 || InfoMessages.Count > 0;

            if (HasStockIssue)
            {
                PaymentMessage = "Limited stock on some items: leave your email to receive a notification when they are back in stock.";
            }
            else
            {
                PaymentMessage = "All items are in stock. You can proceed with payment now.";
            }
        }

        private void OnPay()
        {
            // 1. Préparer un "méga dictionnaire" qui va contenir les pièces de TOUTES les armoires cumulées
            var totalUsedParts = new Dictionary<string, int>();

            foreach (var cabinet in Items)
            {
                var checkout = PartService.GetCheckoutDetails(cabinet);

                // On fusionne les pièces de cette armoire dans le dictionnaire global
                foreach (var part in checkout.UsedParts)
                {
                    if (!totalUsedParts.ContainsKey(part.Key))
                    {
                        totalUsedParts[part.Key] = 0;
                    }
                    totalUsedParts[part.Key] += part.Value;
                }
            }

            string nomClient = string.IsNullOrWhiteSpace(EmailAddress) ? "Client" : EmailAddress;

            // 2. On lance la TRANSACTION SQL UNE SEULE FOIS en envoyant la liste complète des armoires (Items)
            bool isComplete = !HasStockIssue; // Si y'a pas de problème de stock, la commande est Complète

            // On lance la TRANSACTION SQL UNE SEULE FOIS avec la liste complète des armoires (Items)
            bool success = OrderService.FinalizeOrder(nomClient, TotalPrice, isComplete, Items, totalUsedParts);

            // 3. Gestion de l'affichage
            if (success)
            {
                Items.Clear(); // On vide le panier

                if (HasStockIssue)
                {
                    PaymentMessage = $"Order saved! An email will be sent to {nomClient} as soon as the out-of-stock items arrive.";
                }
                else
                {
                    PaymentMessage = "Payment confirmed! Your order has been successfully recorded and the stock has been updated.";
                }
            }
            else
            {
                PaymentMessage = "Connection error. The order has been cancelled.";
            }
        }

        private static bool IsValidEmail(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return value.Contains('@') && value.Contains('.') && !value.EndsWith('.');
        }
    }
}
