using System.ComponentModel.DataAnnotations;

namespace Roham.Services.Models;

public class PrintInputModel
{
    [Required]
    public string report_name { get; set; }

    /// <summary>
    /// pdf
    /// xls
    /// doc
    /// </summary>
    public string? print_type { get; set; } = "pdf";

    public string? data { get; set; }
}