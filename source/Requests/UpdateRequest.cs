using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Api.Requests
{
    public class UpdateRequest
    {
        [FromBody]
        [Required]
        [StringLength(32)]
        public string Name { get; set; }

        [FromBody]
        public int Value { get; set; } = 0;
    }
}