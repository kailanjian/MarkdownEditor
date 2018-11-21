using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Net.Http;
using System.Net;

namespace PandocMarkdown2.Controllers
{
    [Route("api/Convert")]
    public class ConvertController : Controller
    {
        // POST: api/Convert
        [HttpPost]
        public string Post([FromBody]string value)
        {
            Console.WriteLine(value);
            return Convert(value);
        }

        // used: http://stackoverflow.com/questions/1469764/run-command-prompt-commands#1469790
        // Method to take markdown and convert it to parsed output
        private string Convert(string source)
        {
            // Pandoc Process
            string processName = "pandoc";
            string args = "-r markdown -t html";

            // Configure the Process
            ProcessStartInfo psi = new ProcessStartInfo(processName, args);
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardInput = true;
            psi.StandardOutputEncoding = System.Text.Encoding.UTF8;

            // Run the Process
            Process p = new Process();
            p.StartInfo = psi;
            psi.UseShellExecute = false;
            p.Start();

            // Get output
            string outputString = "";
            byte[] inputBuffer = Encoding.UTF8.GetBytes(source);
            p.StandardInput.BaseStream.Write(inputBuffer, 0, inputBuffer.Length);
            p.StandardInput.Dispose();

            // Waiting for exit
            p.WaitForExit(5000);
            using (StreamReader sr = new StreamReader(p.StandardOutput.BaseStream, Encoding.UTF8))
            {
                outputString = sr.ReadToEnd();
            }

            return outputString;
        }
    }
}
