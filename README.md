# 📟 HouseholdBudget

**HouseholdBudget** is a .NET-based application designed to help users manage their personal or household finances with clarity and control. It allows you to track income and expenses, categorize transactions, monitor financial trends, and generate monthly summaries.

---

## 🚀 Features

* 💸 **Transaction Management**
  Add and update income or expense entries, linked to specific categories and dates.

* 📁 **Custom Categories**
  Create, rename, and delete user-specific categories for better financial organization (e.g., Food, Transport, Rent).

* 📊 **Monthly Summaries**
  View comprehensive summaries of income and expenses for each calendar month.

* 📈 **Daily Trends Visualization**
  Analyze financial patterns over time with daily budget point data.

* 👥 **User Session Support**
  Each user's data is securely scoped to their own account context.

* 🌐 **Multi-Currency Support**
  Currency handling with exchange rate integration (customizable via exchange provider).

---

## 🏗️ Project Structure

The app follows a clean, layered architecture with separation of concerns:

```
HouseholdBudget/
├── Core/
│   ├── Models/              # Domain models (Category, Transaction, etc.)
│   ├── Services/            # Business logic implementations
│   ├── Services.Interfaces/ # Service contracts
│   └── UserData/            # Session and user context
├── Tests/                   # Unit tests for core services
└── DesktopApp               # Fully functional desktop app
```

---

## ⚙️ Technology Stack

* **Frontend:** Windows Forms (.NET)
* **Backend:** C# 12 with Entity Framework Core (SQLite)
* **UI Charts:** ScottPlot
* **Architecture:** Clean architecture with Dependency Injection and application bootstrapper
* **Testing:** xUnit, Moq, FluentAssertions

---

## 🧪 Unit Testing

The project includes unit tests under the `HouseholdBudget.Tests` project, following AAA (Arrange–Act–Assert) pattern.

Example:

```csharp
[Fact]
public async Task CreateCategoryAsync_ShouldAddAndReturnCategory()
{
    var service = new LocalCategoryService(_repoMock.Object, _sessionMock.Object);
    var category = await service.CreateCategoryAsync("Utilities");

    category.Name.Should().Be("Utilities");
    _repoMock.Verify(r => r.AddCategoryAsync(It.IsAny<Category>()), Times.Once);
}
```

## 📅 Planned Features

The following features are planned or currently under development:

* ✉️ Exporting financial data to formats such as CSV or Excel
* 🧠 AI-based financial insights and spending analysis
* 🗃️ OCR support to extract data from scanned receipts
* ⏰ Notification system for budget limits, due dates, etc.

---

## ⚠️ Limitations / Work in Progress

The system currently supports user registration and multiple accounts, but some user account features are incomplete:

* Cannot delete an existing user account
* Cannot change user email, default currency, or password after account creation

---

## 🛠️ Getting Started

### Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
* Visual Studio 2022+ or any .NET-compatible IDE

### Steps

1. **Clone the repository**

   ```bash
   git clone https://github.com/your-username/HouseholdBudget.git
   cd HouseholdBudget
   ```

2. **Apply database migrations**
   If you're using the local database provider (SQLite), navigate into the `.Core` folder:

   ```bash
   cd HouseholdBudget.Core
   ```

   Then run the following command to create the database:

   ```bash
   dotnet ef database update --startup-project ../HouseholdBudget.DesktopApp
   ```

3. **Build and run the app**

   ```bash
   cd ..
   dotnet build
   dotnet run --project HouseholdBudget.DesktopApp
   ```
