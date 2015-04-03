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
                                    if (CustomerHelper.ValidateCustomerRow(customerParameters))
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

        #region Run Simulation
        public static string RunSimulation()
        {
            try
            {
                // EACH MINUTE REMOVE CUSTOMERS WHOSE FINISH MINUTE MATCHES,
                // START NEW CUSTOMERS WHOSE START MINUTE MATCHES
                while (CurrentMinute >= 0)
                {
                    CustomerHelper.FinishCustomersPerMinute();  // remove finished customers before starting new ones
                    //FinishCustomersPerMinute();  // remove finished customers before starting new ones
                    CustomerHelper.StartCustomersPerMinute();

                    // IF THE CURRENT MINUTE IS NOT ZERO MINUTE AND THERE ARE NO CUSTOMERS LEFT, END SIMULATION
                    if (CustomerHelper.CountRemainingCustomers() == 0 && CurrentMinute > 0)
                    {
                        break;
                    }
                    // OTHERWISE, AUGMENT MINUTE
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

        #region Write Output
        public static void WriteOutput(string output)
        {
            Console.Write(output);
            string restart = Console.ReadLine();
            if (restart.ToUpper() == "R")
            {
                Main(null);
            }
            Console.ReadLine();
            Console.ReadKey();
        }
        #endregion
    }
}
