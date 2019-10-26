using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGB.UAService
{
    public class CardLoginHistory
    {
        public int Id { get; set; }
        public int CardId { get; set; }
        public DateTime? LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public bool? IsLogin { get; set; }
    }
}
