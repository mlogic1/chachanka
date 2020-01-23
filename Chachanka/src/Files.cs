using System;
using System.IO;

// TODO json config file

namespace Chachanka
{
    partial class Program
    {
        private const string TOKEN_FILE_NAME = "./token.txt";

        private string GetToken()
        {
            return System.IO.File.ReadAllText(TOKEN_FILE_NAME).Replace(Environment.NewLine, "");
        }

        private void WriteLog(string logText)
        {
            using (StreamWriter w = File.AppendText("chankalog.txt"))
            {
                w.WriteLine(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + logText);
            }
        }
    }
}