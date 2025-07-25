using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microservices.Shared.Models
{
    public class EmailTemplateViewModel
    {
        public string? ResetPasswordUrl { get; set; }
        public string? Name { get; set; }
    }
}