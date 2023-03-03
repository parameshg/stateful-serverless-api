using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Counter.Requests;

public class DeleteRequest
{
    [Required]
    [StringLength(32)]
    [FromRoute(Name = "name")]
    public string Name { get; set; }

    [FromBody]
    [Required]
    public bool Confirm { get; set; } = false;
}