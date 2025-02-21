using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppManager.DataModels;

public record User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("user_id")]
    public int UserId { get; init; }

    [Column("user_name")]
    [MaxLength(50)]
    public string Email { get; init; }

    [Column("password_hash")] 
    public string PasswordHash { get; init; } = " ";

    [Column("password_salt")] 
    public string PasswordSalt { get; init; } = " ";
    
}