using System.ComponentModel.DataAnnotations.Schema;

namespace AppManager.DataModels;

public record Session
{
    [Column("session_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SessionId { get; init; }

    [Column("login_id")]
    public int LoginId { get; init; }
    public required Login Login { get; init; }

    [Column("access_token")]
    public required string AccessToken { get; init; }
}