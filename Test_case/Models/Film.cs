using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Test_case.Models
{
    public class Film
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public String CreatorId { get; set; }

        public ApplicationUser Creator { get; set; }

        [Required]
        [MaxLength(300)]
        public String Name { get; set; }

        public String Description { get; set; }

        public Int32 Year { get; set; }

        [MaxLength(300)]
        public String Producer { get; set; }

        public String Path { get; set; }
    }
}