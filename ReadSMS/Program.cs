using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.IO;
using System.Data.SQLite;
using System.Net;
using System.Collections.Specialized;
using System.Net.Http;
using Newtonsoft.Json;

namespace ReadSMS
{
    class Program
    {
        class Message
        {            
            public long date;
            public string from;
            public string body;
        }


        static void Main(string[] args)
        {
            var adb = new ProcessStartInfo()
            {
                FileName = "C:\\Users\\Matin\\AppData\\Local\\Android\\android-sdk\\platform-tools\\adb.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            for (;;)
            {
                Fetch("mmssms.db", "sms", adb);

                System.Threading.Thread.Sleep(10 * 1000);
            }

            //Console.ReadKey();

        }

        private static void Fetch(string file, string table, ProcessStartInfo adb)
        {
            using (Process p = new Process())
            {
                adb.Arguments = "shell";
                p.StartInfo = adb;
                p.Start();

                p.StandardInput.WriteLine("su");
                p.StandardInput.WriteLine(string.Format("cat /data/data/com.android.providers.telephony/databases/{0} > /mnt/sdcard/{0}", file));                
                p.StandardInput.WriteLine("exit");
                p.StandardInput.WriteLine("exit");
                p.WaitForExit();

                if (p.ExitCode == 1)
                    return;

                Console.Write(p.StandardOutput.ReadToEnd());
            }

            using (Process p = new Process())
            {
                adb.Arguments = string.Format("pull /mnt/sdcard/{0} .", file);
                p.StartInfo = adb;
                p.Start();
                p.WaitForExit();
                Console.Write(p.StandardOutput.ReadToEnd());
            }

            Console.WriteLine();

            long lastID = 0;
            try { lastID = Convert.ToInt64(File.ReadAllText("data.txt")); } catch { }            

            var data = new List<Message>();

            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + file + ";Version=3;"))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand("select * from sms where date>"+ lastID, connection))
                {
                    var reader = command.ExecuteReader();                    
                    while(reader.Read())
                    {
                        Message m = new Message();                        
                        m.from = (string)reader["address"];
                        m.body = (string)reader["body"];
                        m.date = (long)reader["date"];
                        data.Add(m);

                        if (lastID < m.date)                        
                            lastID = m.date;                        
                    }
                }
            }
            Console.WriteLine("Messages Detected:\t" + data.Count);

            File.WriteAllText("data.txt", lastID.ToString());

            string json = JsonConvert.SerializeObject(data);


            if (data.Count > 0)
            {
                using (var client = new HttpClient())
                {
                    var res = client.PostAsync("http://lotfaliei.ir/sms/index.php",
                        new StringContent(json, Encoding.UTF8, "application/json"));
                    res.Wait();
                }
                Console.WriteLine("Messages Sent!");
            }
            Console.WriteLine();
        }

        private static long ReadCount(string report, string table, string Database)
        {
            long result = 0;
            if (File.Exists(Database))
                
            Console.WriteLine(string.Format("{0}:\t{1}", report, result));
            return result;
        }
    }
}
