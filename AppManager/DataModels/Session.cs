using System.ComponentModel.DataAnnotations.Schema;

namespace AppManager.DataModels;

public record Session
{
    [Column("session_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SessionId { get; init; }
    
    [Column("app_id")]
    public int AppId { get; init; }

    public App App { get; init; } = new();
    
    [Column("aces_token")]
    public required string AcesToken { get; init; }
    
    [Column("refresh_token")]
    public required string RefreshToken { get; init; }

    [Column("hardware_fingerprint")] 
    public required string HardwareFingerprint { get; init; }
}