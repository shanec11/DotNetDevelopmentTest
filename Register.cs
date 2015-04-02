using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirstripCashRegister
{
    class Register
    {
        public int RegisterNumber { get; set; }
        public bool IsTraining { get; set; }
        public List<Customer> CurrentCustomers { get; set; }
    }
}
