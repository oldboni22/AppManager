using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppManager.DataModels;

public record App
{

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("app_id")]
    public int AppId { get; init; }

    [Column("app_name")]
    [MaxLength(35)]
    public string Name { get; init; }

    [MaxLength(350)]
    [Column("add_description")] 
    public string Description { get; init; }

    [MaxLength(250)]
    [Column("App_secret")]
    public string Secret { get; init; } = " ";
}