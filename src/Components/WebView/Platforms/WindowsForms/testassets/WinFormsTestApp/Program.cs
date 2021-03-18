// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Windows.Forms;

namespace WinFormsTestApp
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(@"C:\git\dotnet\aspnetcore\artifacts\bin\WinFormsTestApp\Debug\net6.0-windows");

            foreach (var arg in args)
            {
                if (arg.StartsWith("--port=", StringComparison.OrdinalIgnoreCase))
                {
                    var port = arg.Substring(7);
                    Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", "--remote-debugging-port=" + port);
                }
            }

            AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
            {
                MessageBox.Show(text: error.ExceptionObject.ToString(), caption: "Error");
            };

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
