

namespace PersonalFinanceManager
{
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
    public class Category
    {
        public string Name { get; set; }
        public TransactionType Type { get; set; } 
    }

    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public decimal Budget { get; set; }
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Reminder> Reminders { get; set; } = new List<Reminder>();
    }
    public class Reminder
    {
        public DateTime ReminderDate { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }


}
