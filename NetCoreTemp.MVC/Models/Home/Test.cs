using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace NetCoreTemp.MVC.Models.Home
{
    public partial class Test : WebApi.Models.BaseModel.BaseEntity
    {
        [Display(Name = "主键", Description = "主键")]
        [Required]
        public override Guid ID
        {
            get;
            set;
        } = Guid.NewGuid();

        [Display(Name = "a", Description = "", Order = 10)]
        public int a { get; set; }
        public string b { get; set; }
        [Required]
        public string c { get; set; }
    }

    [ModelMetadataType(typeof(TestMetadata))]
    public partial class Test { }

    public class TestMetadata
    {
        [Range(0, 9)]
        public int a { get; set; }

        [Required]
        public string b { get; set; }

        [MaxLength(10)]
        public string c { get; set; }

    }
}
