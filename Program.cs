using System;
using System.ServiceModel;
using WcfClientApp.ServiceReference1; 

namespace WcfClientApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Service1Client client = new Service1Client();

            try
            {
                string embg = "1009003455050";
                string validationResult = client.ValidateEMBG(embg);
                Console.WriteLine("ValidateEMBG Result: " + validationResult);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }
            finally
            {
                if (client.State == CommunicationState.Faulted)
                {
                    client.Abort();
                }
                else
                {
                    client.Close();
                }
            }
        }
    }
}
