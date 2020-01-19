using System;

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
    }
}