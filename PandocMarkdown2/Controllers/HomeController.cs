﻿using System;
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

        // GET: /Home/EditDocument/:id
        public IActionResult EditDocument(string id)
        {
            string text = "failed to read";

            // open document based on hash of id.
            using (FileStream fs = SystemFile.OpenRead("Documents/" + GenerateHash(id) + ".md"))
            using (StreamReader sr = new StreamReader(fs))
            {
                text = sr.ReadToEnd();
            }

            string title = "Untitled";

            // get title and text from file.
            title = text.Substring(0, text.IndexOf('\n'));
            text = text.Substring(text.IndexOf('\n') + 1);

            // set view datas so view can get data from backend
            ViewData["text"] = text;
            ViewData["document-title"] = title;
            ViewData["id"] = id;
            ViewData["perm"] = "write";
            return View("Index");
        }

        // POST /Home/SaveNewDocument
        [HttpPost]
        public string SaveNewDocument([FromBody] string documentText)
        {
            // Save document
            string id = GenerateId();
            string hash = GenerateHash(id);
            string filename = hash + ".md";

            // Create a new document and write to it
            using (FileStream fs = SystemFile.Create("Documents/" + filename))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(documentText);
            }
            return id;
        }

        // POST /Home/SaveDocument
        [HttpPost]
        public string SaveDocument([FromBody] string documentText)
        {
            // by arbitrary standard, first line is guid, and rest is body of text
            string id = documentText.Substring(0, documentText.IndexOf('\n'));
            // TODO check if id is valid incase someone tries to mess with the api
            string text = documentText.Substring(documentText.IndexOf('\n') + 1);

            // write to the existing document
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

        // Black-box method to generate a page id
        private string GenerateId()
        {
            // update this super complex algorithm to be more super complex and secure
            // TODO: use built cryptographic RNG instead because GUID is not very secure
            return Guid.NewGuid().ToString();
        }

        // Black-box hashing function, will need to recompute all file names if this is changed!
        private string GenerateHash(string id)
        {
            // Convert to bytes
            byte[] bytes = Encoding.UTF8.GetBytes(id);
            
            // Use built in hashing function to get hash
            HMACSHA256 hashString = new HMACSHA256(new byte[] { 42 });
            byte[] hash = hashString.ComputeHash(bytes);
            string hashResult = "";
            
            // Combine hashed hex values into single string
            foreach (byte x in hash)
            {
                hashResult += String.Format("{0:x2}", x);
            }
            
            return hashResult;

        }
    }
}
