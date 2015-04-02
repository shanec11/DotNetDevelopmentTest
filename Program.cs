// THIS APPLICATION WAS WRITTEN BY SHANE M. COOKE ON 4/1/2015
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AirstripCashRegister
{
    class Program
    {
        public static List<Customer> customers = new List<Customer>();
        public static List<Register> registers = new List<Register>();
        public static int CurrentMinute = 0;

        #region Main
        static void Main(string[] args)
        {
            string strOutput = string.Empty;
            try
            {
                Console.Write("Please enter the local path to a Cash Register Application simulation file");  //Prompt user for file path input
                string strSimFilePath = Console.ReadLine();
                
                //If file path is valid, continue operation, else write error message to console
                if (File.Exists(strSimFilePath))
                {
                    System.IO.StreamReader file = new System.IO.StreamReader(strSimFilePath);
                    int counter = 0;                   // stores current index of the while loop
                    int customerId = 1;                // baseline id number for customer array members
                    string strLine = string.Empty;     // stores the data from a line of text in the simulation file
                    int registerCount;                 // stores the number of registers in the simulation
                    bool fileIsValid = true;           // assume input file is valid until proven otherwise

                        while ((strLine = file.ReadLine()) != null)
                        {
                            //IF THIS IS THE FIRST LINE OF THE INPUT FILE, POPULATE THE CASH REGISTERS LIST
                            if (counter == 0)
                            {
                                int number;
                                // VALIDATE FIRST LINE OF INPUT FILE IS A NUMBER, IF SO CREATE CASH REGISTERS LIST
                                if (Int32.TryParse(strLine, out number))
                                {
                                    registerCount = Convert.ToInt32(strLine);

                                    //ADD EACH REGISTER TO GLOBAL REGISTERS LIST, SET PROPERTIES
                                    for (int i = 1; i < registerCount + 1; i++)
                                    {
                                        Register thisRegister = new Register();
                                        thisRegister.CurrentCustomers = new List<Customer>();
                                        thisRegister.RegisterNumber = i;
                                        thisRegister.IsTraining = (i == registerCount) ? true : false;
                                        registers.Add(thisRegister);
                                    }
                                }
                                else
                                {
                                    strOutput = "Input file is not a valid format.";
                                    fileIsValid = false;
                                }
                            }
                            // OTHERWISE POPULATE THE CUSTOMERS LIST (IF INPUT FILE IS STILL VALID)
                            else
                            {
                                if(fileIsValid)
                                {
                                    string[] customerParameters = strLine.Split(' ');

                                    //CHECK IF THERE THERE IS CUSTOMER DATA IN INPUT FILE AND THAT IT'S VALID
                                    if (ValidateCustomerRow(customerParameters))
                                    {
                                        Customer thisCustomer = new Customer();
                                        thisCustomer.Id = customerId;
                                        thisCustomer.CustomerType = customerParameters[0].ToUpper();
                                        thisCustomer.ArrivalMinute = Convert.ToInt32(customerParameters[1]);
                                        thisCustomer.ItemCount = Convert.ToInt32(customerParameters[2]);
                                        customers.Add(thisCustomer);
                                        customerId++;
                                    }
                                    else
                                    {
                                        strOutput = "Input file is not a valid format.";
                                        fileIsValid = false;
                                    }
                                }                                            
                            }
                            counter++;
                        }
                        file.Close();

                    //COUNTER WILL REMAIN AT 1 IF NO CUSTOMER DATA WAS PROVIDED IN INPUT FILE, SO THROW ERROR, SKIP SIMULATION
                    if (counter == 1)
                    {
                       strOutput = "Input file did not contain customer data.";
                    }
                    //IF EXISTENT INPUT FILE IS VALID AFTER ALL CHECKS, FIRE THE METHOD THAT RUNS THE SIMULATION
                    else if(fileIsValid)
                    {
                        strOutput = RunSimulation();
                    }
                   
                }
                else  //THE INPUT FILE IS NOT FOUND
                {
                    strOutput = "Input file not found on the provided path.";
                }
                WriteOutput(strOutput);
            }
            catch (Exception ex)
            {
                strOutput = "An error was encountered: " + ex.InnerException.Message;
                WriteOutput(strOutput);
            }
        }
        #endregion

        #region Validate Customer Row
        private static bool ValidateCustomerRow(string[]cRow)
        {
            // ARE THERE THREE MEMBERS IN THE ARRAY?
            if(cRow.Count() != 3)
            {
                return false;
            }
            // FIRST MEMBER IS NOT A OR B
            if (cRow[0].ToString().ToUpper() != "A" && cRow[0].ToString().ToUpper() != "B")
            {
                return false;
            }
            //SECOND AND THIRD MEMBERS ARE NOT NUMBERS
            int number;
            if(Int32.TryParse(cRow[1], out number) == false || Int32.TryParse(cRow[2], out number) == false)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region Run Simulation
        private static string RunSimulation()
        {
            try
            {
                // EACH MINUTE REMOVE CUSTOMERS WHOSE FINISH MINUTE MATCHES,
                // START NEW CUSTOMERS WHOSE START MINUTE MATCHES
                while (CurrentMinute >= 0)
                {
                    FinishCustomersPerMinute();  // remove finished customers before starting new ones
                    StartCustomersPerMinute();
                    if (CountRemainingCustomers() == 0 && CurrentMinute > 0)
                    {
                        break;
                    }
                    CurrentMinute++;
                }

                // WHEN ALL CUSTOMERS ARE FINISHED, WRITE CURRENT MINUTE TO CONSOLE MESSAGE
                return ("Airstrip Cash Register simulation finished at t=" + CurrentMinute.ToString() + " minutes.");
            }
            catch(Exception ex)
            { 
                WriteOutput("Error in RunSimulation(): " + ex.InnerException.Message);
                return "";
            }
        }
        #endregion

        #region Finish Customers Per Minute
        //METHOD LOOPS THROUGH REGISTERS LIST, REMOVES CUSTOMERS WHOSE COMPLETION MINUTE IS THE CURRENT MINUTE
        private static void FinishCustomersPerMinute()
        {
            try
            {
                foreach (Register reg in registers)
                {
                    foreach (Customer cust in reg.CurrentCustomers.ToList<Customer>())
                    {
                        if (cust.RegCompletionMinute == CurrentMinute)
                        {
                            reg.CurrentCustomers.Remove(cust);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteOutput("Error in FinishCustomersPerMinute(): " + ex.InnerException.Message);
            }
        }
        #endregion

        #region Start Customers Per Minute
        private static void StartCustomersPerMinute()
        {
            try
            {
                // STORE CUSTOMERS WHO ARRIVE AT THE SAME MINUTE IN SEPERATE LIST
                List<Customer> CustomersByMinute = new List<Customer>();
                foreach (Customer c in customers)
                {
                    if (c.ArrivalMinute == CurrentMinute)
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
                    SelectCustomerRegister(CustomersByMinute, customers.Count);
                }
            }
            catch (Exception ex)
            {
                WriteOutput("Error in StartCustomersPerMinute(): " + ex.InnerException.Message);
            }
        }
        #endregion

        #region Select Customer Register
        // METHOD ASSOCIATES CUSTOMERS  WITH A REGISTER BASED ON THEIR CUSTOMER TYPE
        private static void SelectCustomerRegister(List<Customer> minuteCustomers, int totalCustomers)
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
                            foreach (Register reg in registers)
                            {
                                // IF A REGISTER HAS THE LOWEST ASSESSED CUSTOMER COUNT, 
                                // ADD CUSTOMER TO REGISTER'S CUSTOMER LIST
                                if (reg.CurrentCustomers.Count() < shortestLineCount || shortestLineCount < 0)
                                {
                                    shortestLineCount = reg.CurrentCustomers.Count();
                                    regNum = reg.RegisterNumber;
                                    regTraining = reg.IsTraining;

                                    // IF THE LINE COUNT IS 0, END THE LOOP, ADD CUSTOMER TO THIS LINE
                                    if (shortestLineCount == 0) {
                                        break;
                                    }
                                }
                            }

                            // ADD CUSTOMER TO THE REGISTER'S CUSTOMERS LIST
                            AddCustomerToRegister(regNum, c);

                            // UPDATE CUSTOMER'S COMPLETION MINUTE BASED ON REGISTER SELECTION
                            UpdateCustomer(c.Id, regNum, regTraining);

                            break;

                        case "B":
                            // TYPE "B" LOOKS AT LAST CUSTOMER'S ITEM COUNT IN EACH LINE
                            int lowestItemCount = -1;                            

                            foreach (Register reg in registers)
                            {
                                int lastCustomerIndex = 0;
                                int lastCustomerItemCount = 0;
                                // IF THERE ARE ANY CUSTOMERS AT THE REGISTER, SET LAST CUSTOMER ITEM COUNT
                                if(reg.CurrentCustomers.Count() >= 1)
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
                            UpdateCustomer(c.Id, regNum, regTraining);
                            break;
                    }
                }               
            }
            catch (Exception ex)
            {
                WriteOutput("Error in SelectCustomerRegister(): " + ex.InnerException.Message);
            }
        }
        #endregion

        #region Add Customer To Register
        // METHOD ADDS A CUSTOMER TO A REGISTER'S CUSTOMERS LIST
        private static void AddCustomerToRegister(int regNum, Customer c)
        {
            try
            {
                var reg = (from r in registers where r.RegisterNumber == regNum select r).First();
                reg.CurrentCustomers.Add(c);
            }
            catch (Exception ex){
                WriteOutput("Error in AddCustomerToRegister(): " + ex.InnerException.Message);
            }
        }
        #endregion

        #region Update Customer
        private static void UpdateCustomer(int id, int regNum, bool isTraining)
        {
            try
            {
                var cust = (from c in customers where c.Id == id select c).First();

                // CHANGE REGISTER NUMBER FOR CUSTOMER
                cust.RegisterNumber = regNum;
                // CHANGE COMPLETION MINUTE FOR CUSTOMER BASED ON WHETHER REGISTER IS TRAINING AND NUMBER OF ITEMS
                cust.RegCompletionMinute = (isTraining == true) ? cust.ArrivalMinute + (cust.ItemCount * 2) : cust.ArrivalMinute + cust.ItemCount;
                // AUGMENT COMPLETION MINUTE BASED ON CUSTOMERS IN LINE BEFORE THEM
                cust.RegCompletionMinute += CountMinutesBeforeCustomerBegins(regNum, cust.Id);
            }
            catch (Exception ex)
            {
                WriteOutput("Error in UpdateCustomer(): " + ex.InnerException.Message);
            }
        }
        #endregion

        #region Count Minutes Before Customer Begins
        // METHOD LOOKS AT CUSTOMERS IN LINE IN FRONT OF NEW CUSTOMER, 
        // TOTALS REMAINING MINUTES BEFORE NEW CUSTOMER BEGINS
        private static int CountMinutesBeforeCustomerBegins(int regNum, int id)
        {
           int mins = 0;
            try
            {
                var reg = (from r in registers where r.RegisterNumber == regNum select r).First();
                List<Customer> custs = reg.CurrentCustomers;

                // LOOP THROUGH CUSTOMERS AT NEW CUSTOMER'S REGISTER, TABULATE MINUTES OF WAIT
                foreach (Customer cust in custs)
                {
                    // IF CUSTOMER IS NOT THE NEW CUSTOMER, FACTOR THEIR MINUTES INTO NEW CUSTOMER'S FINISH MINUTE
                    if (cust.Id != id)
                    {
                        int custMinutesLeft = cust.RegCompletionMinute - CurrentMinute;
                        if (custMinutesLeft >= 1)
                        {
                            mins += cust.RegCompletionMinute - CurrentMinute;
                        }
                    }
                }
                return mins;
            }
            catch (Exception ex)
            {
                WriteOutput("Error in CountMinutesBeforeCustomerBegins(): " + ex.InnerException.Message);
                return mins;
            }
        }
        #endregion

        #region Count Remaining Customers
        private static int CountRemainingCustomers()
        {
            int rc = 0;
            foreach (Register register in registers)
            {
                foreach (Customer c in register.CurrentCustomers)
                {
                    rc++;
                }
            }
            return rc;
        }
        #endregion

        #region Write Output
        private static void WriteOutput(string output)
        {
            Console.Write(output);
            string restart = Console.ReadLine();
            if (restart.ToUpper() == "R")
            {
                Main(null);
            }
            Console.ReadKey();
            Console.ReadLine();
            Console.ReadKey();
        }
        #endregion
    }
}
