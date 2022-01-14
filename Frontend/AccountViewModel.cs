using System.ComponentModel.DataAnnotations;

namespace Frontend;

public class AccountViewModel
{
    public AccountViewModel()
    {
        
    }
    public AccountViewModel(Guid key, string userName, int balance)
    {
        this.Key = key;
        this.UserName = userName;
        this.Balance = balance;
    }

    public Guid Key { get; set; }

    [Required(ErrorMessage = "Username is required")]
    public string UserName { get; set; }

    [Range(0, int.MaxValue)]
    public int Balance { get; set; }

    public void Deconstruct(out Guid key, out string userName, out int balance)
    {
        key = this.Key;
        userName = this.UserName;
        balance = this.Balance;
    }
}