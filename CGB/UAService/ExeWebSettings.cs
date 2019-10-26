using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGB.UAService
{
    public class ExeWebSettings
    {
        public int Id { get; set; }
        public string Bank { get; set; }
        public string Settings { get; set; }
        public bool? Active { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
