using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGB.Models.ConfigSettings
{
    public partial class WebSettings
    {
        public string ProgramName { get; set; }
        public bool FullAuto { get; set; }
        public long NoOfProcessWithdrawal { get; set; }
        public Url Url { get; set; }
        public Messages Messages { get; set; }
    }

    public partial class Messages
    {
        public string NoBankCards { get; set; }
        public string Relogin { get; set; }
        public string NoGhostKey { get; set; }
        public string BankCardDisabled { get; set; }
        public string NoteIsEmpty { get; set; }
    }

    public partial class Url
    {
        public string MainUrl { get; set; }
    }
}
