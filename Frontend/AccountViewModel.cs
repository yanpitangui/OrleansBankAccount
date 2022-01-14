using System.ComponentModel.DataAnnotations;

namespace Frontend;

public class AccountViewModel
{
    public AccountViewModel()
    {
        
    }
    public AccountViewModel(Guid key, string userName, decimal balance)
    {
        this.Key = key;
        this.UserName = userName;
        this.Balance = balance;
    }

    public Guid Key { get; set; }

    [Required(ErrorMessage = "Username is required")]
    public string UserName { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Balance { get; set; }

    public void Deconstruct(out Guid key, out string userName, out decimal balance)
    {
        key = this.Key;
        userName = this.UserName;
        balance = this.Balance;
    }
}