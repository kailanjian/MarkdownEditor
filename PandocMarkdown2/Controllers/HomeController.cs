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
        //[HttpGet("{id}", Name = "GetDocument")]
        //[Route("{id}")]
        public IActionResult Document(string id)
        {
            string text = "failed to read";
            using (FileStream fs = SystemFile.OpenRead("Documents/" + id + ".md"))
            using (StreamReader sr = new StreamReader(fs))
            {
                text = sr.ReadToEnd();
            }
            ViewData["text"] = text;
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
            // Save document
            string id = Guid.NewGuid().ToString();
            string filename = id + ".md";
            using (FileStream fs = SystemFile.Create(filename))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(documentText);
            }
            return id;
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
