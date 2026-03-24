# 📦 KitBox - Custom Cabinet & Inventory Management System

KitBox is a comprehensive desktop application built with **C#** and **Avalonia UI** following the **MVVM** (Model-View-ViewModel) architecture. It serves as both a point-of-sale system for customers to design custom modular cabinets and a powerful ERP/dashboard for store managers to handle inventory and suppliers.

## ✨ Features

### 🛒 Customer Interface (Point of Sale)

- **Visual Customization**: Customers can design multi-tier cabinets by selecting total dimensions and angle iron colors.
- **Locker Configuration**: Add, remove, and customize individual lockers (height, panel colors, door options).
- **Real-time Checkout**: Automatic calculation of required parts (panels, crossbars, cleats, etc.) and total price.
- **Stock Verification**: Checks part availability in real-time. If parts are missing, the order is placed on backorder pending supplier delivery.

### 🏢 Manager Dashboard (ERP)

- **Secure Authentication**: Dedicated login portal for store managers.
- **Inventory Tracking**: Real-time view of all parts with automated **Low Stock Alerts** when inventory drops below the safety threshold.
- **Supplier Management**: View supplier details and compare part prices/delivery times between multiple suppliers.
- **Restocking System**: Place supply orders directly through the app to restock missing parts.
- **Order Validation**:
  - Track and fulfill client orders.
  - Validate incoming supplier deliveries to automatically update the database inventory.

## 🛠️ Tech Stack

- **Language**: C# (.NET)
- **User Interface**: Avalonia UI (Cross-platform XAML framework) & ReactiveUI
- **Database**: MariaDB / MySQL
- **Data Access**: ADO.NET (`MySqlConnector`)
- **Architecture**: MVVM (Model - View - ViewModel)

## 📂 Project Structure

- `/Models`: Core business logic and database entities (Cabinet, Locker, Part, Order, Supplier...).
- `/ViewModels`: Presentation logic handling state, commands, and bridging Models to Views (ReactiveUI).
- `/Views`: XAML files defining the User Interface (Pages, UserControls, Popups).
- `/Services`: Database interaction layers handling SQL queries and transactions.

## 🚀 Getting Started

### Prerequisites

1. Install [.NET SDK](https://dotnet.microsoft.com/download) (Version 7.0 or 8.0 recommended).
2. A running instance of **MySQL** or **MariaDB**.
3. An IDE like Visual Studio, JetBrains Rider, or VS Code with C# Dev Kit.

### Installation & Setup

1. **Clone the repository**

   ```bash
   git clone [https://github.com/manouxhe/Software-engineering-project.git](https://github.com/manouxhe/Software-engineering-project.git)
   cd Software-engineering-project/KitBox
   ```

2. **Run the Application**

   ```bash
   dotnet build
   dotnet run
   ```
