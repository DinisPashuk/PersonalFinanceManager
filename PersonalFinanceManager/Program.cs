
using System.Text.Json;


namespace PersonalFinanceManager
{
    class Program
    {
        
        static List<User> users = new List<User>();
        static User currentUser;
        static void Main(string[] args)
        {
            string userFilePath = "userData.json";
            
            currentUser = LoadDataFromFile<User>(userFilePath);
           
            bool exit = false;

            while (!exit)
            {
                if (currentUser == null)
                {
                    Console.WriteLine("1. Import danych użytkownika");
                    Console.WriteLine("2. Autoryzacja");
                    Console.WriteLine("3. Rejestracja");
                    Console.WriteLine("4. Wyjście");
                    var option = Console.ReadLine();
                    switch (option)
                    {
                        case "1":
                            currentUser = ImportData();
                            break;
                        case "2":
                            LoginUser();
                            break;
                        case "3":
                            RegisterUser();
                            break;
                        case "4":
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("Nieprawidłowy wybór. Proszę wybrać opcję od 1 do 4.");
                            break;
                    }
                }

                else
                {
                    Console.WriteLine("1. Eksport danych użytkownika");
                    Console.WriteLine("2. Dodaj transakcję");
                    Console.WriteLine("3. Pokaż transakcje");
                    Console.WriteLine("4. Ustal budżet");
                    Console.WriteLine("5. Dodaj kategorię wydatków");
                    Console.WriteLine("6. Dodaj przypomnienie");
                    Console.WriteLine("7. Przypomnienia");
                    Console.WriteLine("8. Statystyka");
                    Console.WriteLine("9. Statystyki według kategorii");
                    Console.WriteLine("10. Wyjście");                   
                    string choice = Console.ReadLine();
                    switch (choice)
                    {
                        case "1":
                            ExportData(currentUser);
                            break;
                        case "2":
                            AddTransaction(currentUser);
                            break;
                        case "3":
                            ShowTransactions(currentUser);
                            break;
                        case "4":
                            SetBudget(currentUser);
                            break;
                        case "5":
                            AddCategory(currentUser);
                            break;
                        case "6":
                            AddReminder(currentUser);
                            break;
                        case "7":
                            ShowReminders(currentUser);
                            break;
                        case "8":
                            ShowFinancialSummary(currentUser);
                            break;
                        case "9":
                            ShowExpensesByCategory(currentUser);
                            break;
                        case "10":
                            SaveDataToFile(userFilePath, currentUser);
                            currentUser = null;
                            break;
                        default:
                            Console.WriteLine("Nieprawidłowy wybór. Proszę wybrać opcję od 1 do 10.");
                            break;
                    }
                }
            }
        }

        static void AddTransaction(User currentUser)
        {
            var transaction = new Transaction();

            Console.WriteLine("Wybierz typ transakcji: 1 - Nadchodzenie, 2 - Wydatek");
            while (true)
            {
                string transactionType = Console.ReadLine();
                if (transactionType == "1")
                {
                    transaction.Amount = Math.Abs(GetAmountFromUser());
                    break;
                }
                else if (transactionType == "2")
                {
                    decimal amount = -Math.Abs(GetAmountFromUser());
                    if (CanAddExpense(currentUser, amount))
                    {
                        transaction.Amount = amount;
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Ta wydatek przekracza dostępny budżet. Operacja anulowana.");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("Nieprawidłowy wybór. Proszę wprowadzić 1 dla nadchodzeń lub 2 dla wydatków.");
                }
            }

            string currency;
            while (true)
            {
                Console.Write("Wprowadź walutę (USD, EUR, PLN): ");
                currency = Console.ReadLine().ToUpper();

                if (currency == "USD" || currency == "EUR" || currency == "PLN")
                {
                    if (currency != "PLN")
                    {
                        transaction.Amount = ConvertCurrency(transaction.Amount, currency, "PLN");
                        Console.WriteLine($"Kwota po przeliczeniu na PLN: {transaction.Amount} PLN");
                    }
                    break;
                }
                else
                {
                    Console.WriteLine("Nieobsługiwana waluta. Dostępne opcje to USD, EUR, PLN.");
                }
            }

            transaction.Category = GetCategoryFromUser(currentUser);
            transaction.Description = GetDescriptionFromUser();
            transaction.Date = GetDateFromUser();

            currentUser.Transactions.Add(transaction);
            Console.WriteLine("Dodano transakcję.");
            UpdateBudgetAfterTransaction(currentUser, transaction.Amount);
        }

        static decimal GetAmountFromUser()
        {
            decimal amount; 
            Console.Write("Wprowadź kwotę: ");
            while (!decimal.TryParse(Console.ReadLine(), out amount))
            {
                Console.WriteLine("Nieprawidłowa kwota, spróbuj ponownie.");
                Console.Write("Wprowadź kwotę: ");
            }
            return amount; 
        }

        static string GetCategoryFromUser(User currentUser)
        {
            string categoryInput;
            while (true)
            {
                Console.Write("Wprowadź kategorię: ");
                categoryInput = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(categoryInput))
                {
                    var existingCategory = currentUser.Categories.FirstOrDefault(c => c.Name.Equals(categoryInput, StringComparison.OrdinalIgnoreCase));
                    if (existingCategory == null)
                    {
                        currentUser.Categories.Add(new Category { Name = categoryInput });
                        Console.WriteLine($"Dodano nową kategorię: {categoryInput}");
                    }
                    break;
                }
                else
                {
                    Console.WriteLine("Pole kategorii jest obowiązkowe.");
                }
            }
            return categoryInput;
        }

        static string GetDescriptionFromUser()
        {
            Console.Write("Wprowadź opis (można pominąć): ");
            return Console.ReadLine();
        }

        static DateTime GetDateFromUser()
        {
            Console.Write("Wprowadź datę (format dd.MM.yyyy): ");
            DateTime date;
            while (!DateTime.TryParse(Console.ReadLine(), out date))
            {
                Console.WriteLine("Nieprawidłowa data, spróbuj ponownie używając formatu dd.MM.yyyy.");
                Console.Write("Wprowadź datę (format dd.MM.yyyy): ");
            }
            return date;
        }


        static void ShowTransactions(User currentUser)
        {
            Console.WriteLine("Twoje transakcje:");

            
            var incomes = currentUser.Transactions.Where(t => t.Amount > 0).OrderBy(t => t.Date);
            Console.WriteLine("Nadchodzenia:");
            foreach (var transaction in incomes)
            {
                Console.WriteLine($"Data: {transaction.Date.ToShortDateString()}, Kwota: {transaction.Amount} {transaction.Currency}, Kategoria: {transaction.Category}{(string.IsNullOrEmpty(transaction.Description) ? "" : ", Opis: " + transaction.Description)}");
            }

            
            var expenses = currentUser.Transactions.Where(t => t.Amount < 0).OrderBy(t => t.Date);
            Console.WriteLine("Wydatki:");
            foreach (var transaction in expenses)
            {
                Console.WriteLine($"Data: {transaction.Date.ToShortDateString()}, Kwota: {transaction.Amount} {transaction.Currency}, Kategoria: {transaction.Category}{(string.IsNullOrEmpty(transaction.Description) ? "" : ", Opis: " + transaction.Description)}");
            }
        }



        static void RegisterUser()
        {
            Console.Write("Wprowadź nazwę użytkownika: ");
            string username = Console.ReadLine();
            Console.Write("Wprowadź hasło: ");
            string password = Console.ReadLine(); 

            var user = new User { Username = username, Password = password, Budget = 0, Transactions = new List<Transaction>(), Categories = new List<Category>(), Reminders = new List<Reminder>() };
            users.Add(user);
            Console.WriteLine("Użytkownik jest zarejestrowany.");
        }

        static void LoginUser()
        {
            Console.Write("Wprowadź nazwę użytkownika: ");
            string username = Console.ReadLine();
            Console.Write("Wprowadź hasło: ");
            string password = Console.ReadLine();

            var user = users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user != null)
            {
                currentUser = user;
                Console.WriteLine("Autoryzacja zakończona powodzeniem.");
            }
            else
            {
                Console.WriteLine("Nieprawidłowa nazwa użytkownika lub hasło.");
            }
        }
        static void SetBudget(User currentUser)
        {
            Console.Write("Wprowadź swój nowy budżet: ");
            if (decimal.TryParse(Console.ReadLine(), out decimal newBudget))
            {
                currentUser.Budget = newBudget;
                Console.WriteLine("Budżet został zaktualizowany.");              
                var totalExpenses = currentUser.Transactions.Where(t => t.Amount < 0).Sum(t => t.Amount);
                var totalIncomes = currentUser.Transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
                var finalBalance = currentUser.Budget + totalIncomes + totalExpenses;

                Console.WriteLine($"Twój aktualny balans wynosi: {finalBalance}");
            }
            else
            {
                Console.WriteLine("Nieprawidłowa kwota, spróbuj ponownie.");
            }
        }

        static void AddCategory(User currentUser)
        {
            Console.Write("Wprowadź nazwę nowej kategorii wydatków: ");
            string categoryName = Console.ReadLine();

            var categoryExists = currentUser.Categories.Any(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
            if (categoryExists)
            {
                Console.WriteLine("Kategoria już istnieje.");
                return;
            }

            currentUser.Categories.Add(new Category { Name = categoryName });
            Console.WriteLine("Dodano kategorię.");
        }
        static void ShowFinancialSummary(User currentUser)
        {
            if (currentUser.Transactions.Count == 0)
            {
                Console.WriteLine("W tej chwili nie ma żadnych transakcji.");
                return;
            }

            var totalExpenses = currentUser.Transactions
                .Where(t => t.Amount < 0)
                .Sum(t => t.Amount);

            var totalIncomes = currentUser.Transactions
                .Where(t => t.Amount > 0)
                .Sum(t => t.Amount);

            var finalBalance = currentUser.Budget;

            Console.WriteLine("Sprawozdanie z działalności finansowej:");
            Console.WriteLine($"Wpływy ogółem: {totalIncomes}");
            Console.WriteLine($"Całkowite wydatki: {Math.Abs(totalExpenses)}");
            Console.WriteLine($"Balans końcowy: {finalBalance}");

            Console.Write("W jakiej walucie chcesz zobaczyć swój balans? (USD, EUR, PLN): ");
            string desiredCurrency = Console.ReadLine().ToUpper();

            
            if (desiredCurrency != "PLN" && desiredCurrency != "USD" && desiredCurrency != "EUR")
            {
                Console.WriteLine("Nieobsługiwana waluta. Dostępne opcje to USD, EUR, PLN.");
                return;
            }

            try
            {
                decimal balanceInDesiredCurrency = ConvertCurrency(finalBalance, "PLN", desiredCurrency);
                Console.WriteLine($"Twój balans w {desiredCurrency}: {balanceInDesiredCurrency}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Nie można przeliczyć waluty: {ex.Message}");
            }
        }
        static void ShowExpensesByCategory(User currentUser)
        {
            Console.WriteLine("Wydatki według kategorii:");
            foreach (var category in currentUser.Categories)
            {
                var totalAmount = currentUser.Transactions
                    .Where(t => t.Category.Equals(category.Name, StringComparison.OrdinalIgnoreCase) && t.Amount < 0)
                    .Sum(t => t.Amount);

                Console.WriteLine($"{category.Name}: {Math.Abs(totalAmount)}");
            }
        }
        static void AddReminder(User currentUser)
        {
            var reminder = new Reminder();

            Console.Write("Wprowadź datę przypomnienia (format dd.MM.yyyy): ");
            string dateString = Console.ReadLine();
            DateTime reminderDate;

            
            while (!DateTime.TryParseExact(dateString, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out reminderDate))
            {
                Console.WriteLine("Nieprawidłowy format daty, spróbuj ponownie (format dd.MM.yyyy): ");
                dateString = Console.ReadLine();
            }
            reminder.ReminderDate = reminderDate;

            Console.Write("Wprowadź opis płatności: ");
            reminder.Description = Console.ReadLine();

            Console.Write("Wprowadź kwotę płatności: ");
            string amountString = Console.ReadLine();
            decimal amount;

           
            while (!decimal.TryParse(amountString, out amount))
            {
                Console.WriteLine("Nieprawidłowa kwota, spróbuj ponownie: ");
                amountString = Console.ReadLine();
            }
            reminder.Amount = amount;

            currentUser.Reminders.Add(reminder);
            Console.WriteLine("Dodano przypomnienie.");
        }
        static void ShowReminders(User currentUser)
        {
            Console.WriteLine("Aktywne przypomnienia:");
            foreach (var reminder in currentUser.Reminders.OrderBy(r => r.ReminderDate))
            {
                Console.WriteLine($"Data: {reminder.ReminderDate.ToShortDateString()},Opis: {reminder.Description}, Kwota: {reminder.Amount}");
            }
        }
        static void ExportData(User currentUser)
        {
            
            var options = new JsonSerializerOptions { WriteIndented = true };

            
            string userDataJson = JsonSerializer.Serialize(currentUser, options);

            
            Console.Write("Wprowadź ścieżkę do zapisania pliku danych (np. C:\\Users\\Username\\Documents\\userData.json): ");
            string filePath = Console.ReadLine();

            
            try
            {
                File.WriteAllText(filePath, userDataJson);
                Console.WriteLine("Dane zostały pomyślnie wyeksportowane.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wystąpił błąd podczas eksportowania danych: {ex.Message}");
            }
        }
        static User ImportData()
        {
            Console.Write("Wprowadź ścieżkę do pliku danych do zaimportowania: ");
            string filePath = Console.ReadLine();

            try
            {
                string userDataJson = File.ReadAllText(filePath);
                var user = JsonSerializer.Deserialize<User>(userDataJson);
                Console.WriteLine("Dane zostały pomyślnie zaimportowane.");
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd importowania danych: {ex.Message}");
                return null;
            }
        }
        static decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency)
        {
            // Kursy wymiany walut
            const decimal usdToPlnRate = 4.0m; // 1 USD = 4.0 PLN
            const decimal eurToPlnRate = 4.5m; // 1 EUR = 4.5 PLN
            const decimal usdToEurRate = 0.9m; // 1 USD = 0.9 EUR

            if (fromCurrency == toCurrency)
            {
                return amount;
            }

            // Przeliczenie z PLN
            if (fromCurrency == "PLN" && toCurrency == "USD")
            {
                return amount / usdToPlnRate;
            }
            if (fromCurrency == "PLN" && toCurrency == "EUR")
            {
                return amount / eurToPlnRate;
            }

            // Przeliczenie в PLN
            if (fromCurrency == "USD" && toCurrency == "PLN")
            {
                return amount * usdToPlnRate;
            }
            if (fromCurrency == "EUR" && toCurrency == "PLN")
            {
                return amount * eurToPlnRate;
            }

            // Przeliczenie між USD та EUR
            if (fromCurrency == "USD" && toCurrency == "EUR")
            {
                return amount * usdToEurRate;
            }
            if (fromCurrency == "EUR" && toCurrency == "USD")
            {
                return amount / usdToEurRate;
            }

            throw new ArgumentException("Nieobsługiwana waluta");
        }
        static void UpdateBudgetAfterTransaction(User currentUser, decimal transactionAmount)
        {
            currentUser.Budget += transactionAmount;
            Console.WriteLine($"Bieżący budżet: {currentUser.Budget}");
        }
        static bool CanAddExpense(User currentUser, decimal expenseAmount)
        {
            if (currentUser.Budget + expenseAmount < 0) 
            {
                Console.WriteLine("Wydatek przekracza dostępny budżet. Operacja anulowana.");
                Console.WriteLine($"Całkowity budżet: {currentUser.Budget}");
                return false;
            }
            return true;
        }
        public static void SaveDataToFile<T>(string filePath, T data)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(data, options);
            File.WriteAllText(filePath, jsonString);
            Console.WriteLine("Zapisane dane użytkownika.");
        }
        
        public static T LoadDataFromFile<T>(string filePath) where T : class
        {
            if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
            {
                Console.WriteLine("Nie znaleziono pliku danych. Utworz nowy profil użytkownika.");
                return null; 
                
            }

            string jsonString = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(jsonString))

            {
                Console.WriteLine("Plik danych jest pusty lub zawiera nieprawidłowe dane. Utworz nowy profil użytkownika.");
                return null; 
            }

            try
            {
                Console.WriteLine("Dane użytkownika zostały pomyślnie przesłane.");
                T data = JsonSerializer.Deserialize<T>(jsonString);
                return data; 
            }
            catch
            {
                Console.WriteLine("Wystąpił błąd podczas deserializacji danych. Utworz nowy profil użytkownika.");
                return null; 
            }
        }

    }
}
