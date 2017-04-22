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
    //[Produces("application/json")]
    [Route("api/Convert")]
    public class ConvertController : Controller
    {
        // GET: api/Convert
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Convert/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Convert
        [HttpPost]
        public string Post([FromBody]string value)
        {
            Console.WriteLine(value);
            // http://stackoverflow.com/questions/1469764/run-command-prompt-commands#1469790
            return Convert(value);
        }

        private string Convert(string source)
        {
            string processName = "pandoc";
            string args = "-r markdown -t html";

            ProcessStartInfo psi = new ProcessStartInfo(processName, args);

            psi.RedirectStandardOutput = true;
            psi.RedirectStandardInput = true;
            psi.StandardOutputEncoding = System.Text.Encoding.UTF8;

            Process p = new Process();
            p.StartInfo = psi;
            psi.UseShellExecute = false;
            p.Start();

            string outputString = "";
            byte[] inputBuffer = Encoding.UTF8.GetBytes(source);
            p.StandardInput.BaseStream.Write(inputBuffer, 0, inputBuffer.Length);
            p.StandardInput.Dispose();

            p.WaitForExit(5000);
            using (StreamReader sr = new StreamReader(p.StandardOutput.BaseStream, Encoding.UTF8))
            {
                outputString = sr.ReadToEnd();
            }

            return outputString;
        }
        
        // PUT: api/Convert/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        // TODO: move to utility class
    }
}
