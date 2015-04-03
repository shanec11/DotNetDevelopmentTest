using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirstripCashRegister
{
    class CustomerHelper
    {
        #region Validate Customer Row
        public static bool ValidateCustomerRow(string[] cRow)
        {
            // THERE ARE NOT THREE MEMBERS IN THE CUSTOMER ARRAY
            if (cRow.Count() != 3)
            {
                return false;
            }

            // FIRST MEMBER IS NOT "A" OR "B"
            if (cRow[0].ToString().ToUpper() != "A" && cRow[0].ToString().ToUpper() != "B")
            {
                return false;
            }

            //SECOND AND THIRD MEMBERS ARE NOT NUMBERS
            int number;
            if (Int32.TryParse(cRow[1], out number) == false || Int32.TryParse(cRow[2], out number) == false)
            {
                return false;
            }

            //SECOND AND THIRD MEMBERS ARE NEGATIVE NUMBERS
            if (Convert.ToInt32(cRow[1]) <= -1 || Convert.ToInt32(cRow[2]) <= -1)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region Finish Customers Per Minute
        //METHOD LOOPS THROUGH REGISTERS LIST, REMOVES CUSTOMERS WHOSE COMPLETION MINUTE IS THE CURRENT MINUTE
        public static void FinishCustomersPerMinute()
        {                        
            foreach (Register reg in Program.registers)
            {
                foreach (Customer cust in reg.CurrentCustomers.ToList<Customer>())
                {
                    if (cust.RegCompletionMinute == Program.CurrentMinute)
                    {
                        reg.CurrentCustomers.Remove(cust);
                    }
                }
            }
        }        
        #endregion

        #region Start Customers Per Minute
        public static void StartCustomersPerMinute()
        {
            try
            {
                // STORE CUSTOMERS WHO ARRIVE AT THE SAME MINUTE IN SEPERATE LIST
                List<Customer> CustomersByMinute = new List<Customer>();
                foreach (Customer c in Program.customers)
                {
                    if (c.ArrivalMinute == Program.CurrentMinute)
                    {
                        CustomersByMinute.Add(c);
                    }
                }

                // IF ANY CUSTOMERS ARRIVE DURING THIS MINUTE, BUILD A NEW LIST OF THESE CUSTOMERS, 
                // PASS THE LIST TO THE REGISTER SELECTION METHOD
                if (CustomersByMinute.Count > 0)
                {
                    // ORDER BY ITEM COUNT THEN CUSTOMER TYPE, ADHERING TO RULE REGARDING 
                    // SIMULTANEOUSLY ARRIVING CUSTOMERS
                    CustomersByMinute = CustomersByMinute.OrderBy(x => x.ItemCount).ThenBy(x => x.CustomerType).ToList();
                    RegisterHelper.SelectCustomerRegister(CustomersByMinute, Program.customers.Count);
                }
            }
            catch (Exception ex)
            {
                Program.WriteOutput("Error in StartCustomersPerMinute(): " + ex.InnerException.Message);
            }
        }
        #endregion

        #region Update Customer
        public static void UpdateCustomer(int id, int regNum, bool isTraining)
        {
            try
            {
                var cust = (from c in Program.customers where c.Id == id select c).First();

                // CHANGE REGISTER NUMBER FOR CUSTOMER
                cust.RegisterNumber = regNum;
                // CHANGE COMPLETION MINUTE FOR CUSTOMER BASED ON WHETHER REGISTER IS TRAINING AND NUMBER OF ITEMS
                cust.RegCompletionMinute = (isTraining == true) ? cust.ArrivalMinute + (cust.ItemCount * 2) : cust.ArrivalMinute + cust.ItemCount;
                // AUGMENT COMPLETION MINUTE BASED ON CUSTOMERS IN LINE BEFORE THEM
                cust.RegCompletionMinute += CountMinutesBeforeCustomerBegins(regNum, cust.Id);
            }
            catch (Exception ex)
            {
                Program.WriteOutput("Error in UpdateCustomer(): " + ex.InnerException.Message);
            }
        }
        #endregion

        #region Count Minutes Before Customer Begins
        // METHOD LOOKS AT CUSTOMERS IN LINE IN FRONT OF NEW CUSTOMER, 
        // TOTALS REMAINING MINUTES BEFORE NEW CUSTOMER BEGINS
        public static int CountMinutesBeforeCustomerBegins(int regNum, int id)
        {
            int mins = 0;
            try
            {
                var reg = (from r in Program.registers where r.RegisterNumber == regNum select r).First();
                List<Customer> custs = reg.CurrentCustomers;

                // LOOP THROUGH CUSTOMERS AT NEW CUSTOMER'S REGISTER, TABULATE MINUTES OF WAIT
                foreach (Customer cust in custs)
                {
                    // IF CUSTOMER IS NOT THE NEW CUSTOMER, FACTOR THEIR MINUTES INTO NEW CUSTOMER'S FINISH MINUTE
                    if (cust.Id != id)
                    {
                        int custMinutesLeft = cust.RegCompletionMinute - Program.CurrentMinute;
                        if (custMinutesLeft >= 1)
                        {
                            mins += custMinutesLeft;
                        }
                    }
                }
                return mins;
            }
            catch (Exception ex)
            {
                Program.WriteOutput("Error in CountMinutesBeforeCustomerBegins(): " + ex.InnerException.Message);
                return mins;
            }
        }
        #endregion

        #region Count Remaining Customers
        public static int CountRemainingCustomers()
        {
            int rc = 0;
            foreach (Register register in Program.registers)
            {
                foreach (Customer c in register.CurrentCustomers)
                {
                    rc++;
                }
            }
            return rc;
        }
        #endregion
    }
}
