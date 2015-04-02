using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirstripCashRegister
{
    class Customer
    {
        public int Id { get; set; }
        public string CustomerType { get; set; }
        public int ItemCount { get; set; }
        public int RegisterNumber { get; set; }
        public int ArrivalMinute { get; set; }
        public int RegCompletionMinute { get; set; }
        public int TrainCompletionMinute { get; set; }
    }
}
