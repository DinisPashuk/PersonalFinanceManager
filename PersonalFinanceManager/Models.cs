

namespace PersonalFinanceManager
{
    //Definiuje klasę transakcji, zawierającą datę, kwotę, walutę, kategorię, opis i typ transakcji 
    public class Transaction
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } 
        public string Category { get; set; }
        public string Description { get; set; }
        public TransactionType Type { get; set; }
    }

    public enum TransactionType
    {
        Income,
        Expense
    }




    //Definiuje klasę kategorii, zawierającą nazwę i typ, który określa, czy kategoria dotyczy przychodów czy wydatków
    public class Category
    {
        public string Name { get; set; }
        public TransactionType Type { get; set; } 
    }





    //Definiuje klasę użytkownika, zawierającą nazwę użytkownika, hasło, budżet, listę transakcji, kategorii oraz przypomnień.
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public decimal Budget { get; set; }
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Reminder> Reminders { get; set; } = new List<Reminder>();
    }





    //Definiuje klasę przypomnienia, zawierającą datę przypomnienia, opis oraz kwotę przypomnienia.
    public class Reminder
    {
        public DateTime ReminderDate { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }


}
