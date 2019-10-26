using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGB.Models.Extensions
{
    public static class OrderExtensions
    {
        public static bool IsSameBank(this UAService.WithdrawOrder order)
        {
            if (order == null)
                throw new Exception("order is null");

            return String.IsNullOrEmpty(order.issuing_bank);
        }
    }
}
