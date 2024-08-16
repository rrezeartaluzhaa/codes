using System;
using System.Linq;  // Ensure this namespace is included for LINQ extension methods
using System.ServiceModel;

namespace WcfService1
{
    public class Service1 : IService1
    {
        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }

        public string ValidateEMBG(string embg)
        {
            if (string.IsNullOrEmpty(embg))
            {
                return "EMBG cannot be empty.";
            }

            if (embg.Length != 13)
            {
                return "Invalid EMBG length. It should be 13 digits.";
            }


            if (!embg.All(char.IsDigit))
            {
                return "Invalid EMBG. It should contain only numeric characters.";
            }

            int[] digits = embg.Select(z => int.Parse(z.ToString())).ToArray();

            int a = digits[0], b = digits[1], c = digits[2], d = digits[3];
            int e = digits[4], f = digits[5], g = digits[6], h = digits[7];
            int i = digits[8], j = digits[9], k = digits[10], l = digits[11];
            int m = 11 - ((7 * (a + g) + 6 * (b + h) + 5 * (c + i) + 4 * (d + j) + 3 * (e + k) + 2 * (f + l)) % 11);

            int checkDigit = (m == 10 || m == 11) ? 0 : m;

            if (checkDigit == digits[12])
            {
                return "Valid EMBG.";
            }
            else
            {
                return "Invalid EMBG.";
            }
        }
    }
}
