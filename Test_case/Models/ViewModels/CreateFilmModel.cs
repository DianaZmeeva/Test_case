using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Test_case.Models.ViewModels
{
    public class CreateFilmModel
    {
        [Required]
        [MaxLength(300)]
        public String Name { get; set; }

        [DataType(DataType.MultilineText)]
        public String Description { get; set; }

        [Display(Name = "Year")]
        [Range(1700, 3000, ErrorMessage = "Incorrect year")]
        public Int32 Year { get; set; }

        [MaxLength(300)]
        public String Producer { get; set; }

        public HttpPostedFileBase ImageFile { get; set; }
    }
}