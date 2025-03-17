using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppManager.DataModels;

public record Login
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("login_id")]
    public int LoginId { get; init; }
    
    [Column("update_token")]
    public required string UpdateToken { get; init; }
    
    [Column("device_hash")]
    [MaxLength(16)]
    public required string HardwareHash { get; init; }
    
    [Column("device_salt")]
    public required string HardwareSalt { get; init; }
    
    [Column("user_id")]
    public int UserId { get; init; }
}