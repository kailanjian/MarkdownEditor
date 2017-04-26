using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using SystemFile = System.IO.File;
using System.Text;
using System.Security.Cryptography;

namespace PandocMarkdown2.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        // GET: document/5
        //[HttpGet("Edit/Document", Name = "Document")]
        public IActionResult EditDocument(string id)
        {
            string text = "failed to read";
            using (FileStream fs = SystemFile.OpenRead("Documents/" + GenerateHash(id) + ".md"))
            using (StreamReader sr = new StreamReader(fs))
            {
                text = sr.ReadToEnd();
            }
            string title = "Untitled";
            title = text.Substring(0, text.IndexOf('\n'));
            text = text.Substring(text.IndexOf('\n') + 1);
            ViewData["text"] = text;
            ViewData["document-title"] = title;
            ViewData["id"] = id;
            ViewData["perm"] = "write";
            return View("Index");
        }

        [HttpPost]
        public string SaveNewDocument([FromBody] string documentText)
        {
            // Save document
            string id = GenerateId();
            string hash = GenerateHash(id);
            string filename = hash + ".md";

            using (FileStream fs = SystemFile.Create("Documents/" + filename))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(documentText);
            }
            return id;
        }



        
        [HttpPost]
        public string SaveDocument([FromBody] string documentText)
        {
            // by arbitrary standard, first line is guid, and rest is body of text
            string id = documentText.Substring(0, documentText.IndexOf('\n'));
            // TODO check if id is valid incase someone tries to screw with the api
            string text = documentText.Substring(documentText.IndexOf('\n') + 1);

            string filename = "Documents/" + GenerateHash(id) + ".md";
            using (FileStream fs = SystemFile.OpenWrite(filename))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(text);
                
            }
            return id;
        }

        public IActionResult Error()
        {
            return View();
        }

        // TODO: move this stuff to model
        private string GenerateId()
        {
            // update this super complex algorithm to be more super complex and secure
            // TODO: use cryptographic RNG instead because apparently GUID is not very secure
            return Guid.NewGuid().ToString();
        }

        private string GenerateHash(string id)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(id);
            HMACSHA256 hashString = new HMACSHA256(new byte[] { 42 });
            byte[] hash = hashString.ComputeHash(bytes);
            string hashResult = "";
            foreach (byte x in hash)
            {
                hashResult += String.Format("{0:x2}", x);
            }
            return hashResult;

        }
    }
}
