using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using SystemFile = System.IO.File;

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
            using (FileStream fs = SystemFile.OpenRead("Documents/" + id + ".md"))
            using (StreamReader sr = new StreamReader(fs))
            {
                text = sr.ReadToEnd();
            }
            ViewData["text"] = text;
            ViewData["id"] = id;
            ViewData["perm"] = "write";
            return View("Index");
        }

        [HttpPost]
        public string SaveNewDocument([FromBody] string documentText)
        {
            // Save document
            string id = Guid.NewGuid().ToString();
            string filename = id + ".md";

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
            string text = documentText.Substring(documentText.IndexOf('\n') + 1);

            string filename = "Documents/" + id + ".md";
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
    }
}
