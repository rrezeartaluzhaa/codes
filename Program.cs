using System;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

public class Program
{
    static Random random = new Random();

    static string generatePassword(int length)
    {
        const string uppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowercaseChars = "abcdefghijklmnopqrstuvwxyz";
        const string digitChars = "0123456789";
        const string specialChars = "!@#$%^&*()-_=+[{]}\\|;:'\",<.>/?";

        string password = "";

        // Ensure the password has at least one of each type of character
        password += uppercaseChars[random.Next(uppercaseChars.Length)];
        password += specialChars[random.Next(specialChars.Length)];
        password += digitChars[random.Next(digitChars.Length)];
        password += lowercaseChars[random.Next(lowercaseChars.Length)];

        // Fill the rest of the password with random characters
        for (int i = 4; i < length; i++)
        {
            int charType = random.Next(4); // 0 for uppercase, 1 for lowercase, 2 for digit, 3 for special
            switch (charType)
            {
                case 0:
                    password += uppercaseChars[random.Next(uppercaseChars.Length)];
                    break;
                case 1:
                    password += lowercaseChars[random.Next(lowercaseChars.Length)];
                    break;
                case 2:
                    password += digitChars[random.Next(digitChars.Length)];
                    break;
                case 3:
                    password += specialChars[random.Next(specialChars.Length)];
                    break;
                default:
                    break;
            }
        }

        // Shuffle the password characters randomly
        password = Shuffle(password);

        return password;
    }

    static string Shuffle(string str)
    {
        char[] array = str.ToCharArray();
        int n = array.Length;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            var value = array[k];
            array[k] = array[n];
            array[n] = value;
        }
        return new string(array);
    }

    public static void Main(string[] args)
    {
        /*if (args.Length != 1 || !int.TryParse(args[0], out int numberOfPasswords) || numberOfPasswords <= 0)
        {
            Console.WriteLine("Usage: detyre.exe <number>");
            Console.WriteLine("<number> should be a positive integer indicating the number of passwords to generate.");
            return;
        }*/

        Console.Write("gjatesia e passwords: ");
        if (!int.TryParse(Console.ReadLine(), out int passwordLength) || passwordLength < 8)
        {
            Console.WriteLine("Invalid: nr>8");
            return;
        }

        Console.Write("sa passwords: ");
        if (!int.TryParse(Console.ReadLine(), out int nrPasswords) || nrPasswords <= 0)
        {
            Console.WriteLine("Invalid: nr pozitiv!!");
            return;
        }

        string pdfFileName = "passwords.pdf";
        string pdfPath = Path.Combine(Directory.GetCurrentDirectory(), pdfFileName);

        using (FileStream fs = new FileStream(pdfPath, FileMode.Create))
        {
            using (PdfWriter writer = new PdfWriter(fs))
            {
                using (PdfDocument pdf = new PdfDocument(writer))
                {
                    Document document = new Document(pdf);

                    for (int i = 1; i <= nrPasswords; i++)
                    {
                        string password = generatePassword(passwordLength);
                        document.Add(new Paragraph($"Password {i}: {password}"));
                    }

                    document.Close();
                }
            }
        }

        Console.WriteLine($"PDF with {nrPasswords} passwords of length {passwordLength} generated successfully at: {pdfPath}");

    }
}

