using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirstripCashRegister
{
    class RegisterHelper
    {
        #region Select Customer Register
        // METHOD ASSOCIATES CUSTOMERS  WITH A REGISTER BASED ON THEIR CUSTOMER TYPE
        public static void SelectCustomerRegister(List<Customer> minuteCustomers, int totalCustomers)
        {
            try
            {
                int regNum = -1;
                bool regTraining = false;

                foreach (Customer c in minuteCustomers)
                {
                    switch (c.CustomerType.ToUpper())
                    {
                        case "A":
                            // TYPE "A" LOOKS FOR SHORTEST LINE
                            int shortestLineCount = -1;
                            foreach (Register reg in Program.registers)
                            {
                                // IF A REGISTER HAS THE LOWEST ASSESSED CUSTOMER COUNT, 
                                // ADD CUSTOMER TO REGISTER'S CUSTOMER LIST
                                if (reg.CurrentCustomers.Count() < shortestLineCount || shortestLineCount < 0)
                                {
                                    shortestLineCount = reg.CurrentCustomers.Count();
                                    regNum = reg.RegisterNumber;
                                    regTraining = reg.IsTraining;

                                    // IF THE LINE COUNT IS 0, END THE LOOP, ADD CUSTOMER TO THIS LINE
                                    if (shortestLineCount == 0)
                                    {
                                        break;
                                    }
                                }
                            }

                            // ADD CUSTOMER TO THE REGISTER'S CUSTOMERS LIST
                            AddCustomerToRegister(regNum, c);

                            // UPDATE CUSTOMER'S COMPLETION MINUTE BASED ON REGISTER SELECTION
                           CustomerHelper.UpdateCustomer(c.Id, regNum, regTraining);

                            break;

                        case "B":
                            // TYPE "B" LOOKS AT LAST CUSTOMER'S ITEM COUNT IN EACH LINE
                            int lowestItemCount = -1;

                            foreach (Register reg in Program.registers)
                            {
                                int lastCustomerIndex = 0;
                                int lastCustomerItemCount = 0;
                                // IF THERE ARE ANY CUSTOMERS AT THE REGISTER, SET LAST CUSTOMER ITEM COUNT
                                if (reg.CurrentCustomers.Count() >= 1)
                                {
                                    lastCustomerIndex = reg.CurrentCustomers.Count() - 1;   // get the array index of the last customer at a register
                                    lastCustomerItemCount = reg.CurrentCustomers[lastCustomerIndex].ItemCount;  //get that customer's item count
                                }

                                if (lastCustomerItemCount < lowestItemCount || lowestItemCount < 0)
                                {
                                    lowestItemCount = lastCustomerItemCount;
                                    regNum = reg.RegisterNumber;
                                    regTraining = reg.IsTraining;

                                    // IF LOWEST ITEM COUNT IS ZERO, END LOOP
                                    if (lowestItemCount == 0)
                                    {
                                        break;
                                    }
                                }
                            }

                            AddCustomerToRegister(regNum, c);
                            CustomerHelper.UpdateCustomer(c.Id, regNum, regTraining);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Program.WriteOutput("Error in SelectCustomerRegister(): " + ex.InnerException.Message);
            }
        }
        #endregion

        #region Add Customer To Register
        // METHOD ADDS A CUSTOMER TO A REGISTER'S CUSTOMERS LIST
        private static void AddCustomerToRegister(int regNum, Customer c)
        {
            try
            {
                var reg = (from r in Program.registers where r.RegisterNumber == regNum select r).First();
                reg.CurrentCustomers.Add(c);
            }
            catch (Exception ex)
            {
                Program.WriteOutput("Error in AddCustomerToRegister(): " + ex.InnerException.Message);
            }
        }
        #endregion
    }
}
