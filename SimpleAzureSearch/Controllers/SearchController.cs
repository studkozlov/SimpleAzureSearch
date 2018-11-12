using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace SimpleAzureSearch.Controllers
{
    public class SearchController : Controller
    {
        // GET: Search
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SearchInTable(string searchString)
        {
            var searchClient = CreateSearchIndexClient("docs-db-index");
            SearchParameters parameters = new SearchParameters()
            {
                Select = new[] { "Title" }
            };
            DocumentSearchResult results = searchClient.Documents.Search(searchString, parameters);
            String result = String.Empty;
            if (results.Results.Count > 0)
            {
                result = results.Results.Select(r => r.Document.FirstOrDefault())
                    .Select(d => $"<li>{d.Key}: {d.Value}</li>")
                    .Aggregate((current, next) => current + next);
                return Content($"<ul>{result}</ul>");
            }

            return Content("Data not found.");
        }

        public ActionResult SearchInBlob(string searchString)
        {
            var searchClient = CreateSearchIndexClient("docs-blob-index");
            SearchParameters parameters = new SearchParameters()
            {
                Select = new[] { "metadata_storage_name" }
            };
            DocumentSearchResult results = searchClient.Documents.Search(searchString, parameters);
            String result = String.Empty;
            if (results.Results.Count > 0)
            {
                result = results.Results.Select(r => r.Document.FirstOrDefault())
                    .Select(d => $"<li>File name: {d.Value}</li>")
                    .Aggregate((current, next) => current + "\n" + next);
                return Content($"<ul>{result}</ul>");
            }

            return Content("Data not found.");
        }

        private SearchIndexClient CreateSearchIndexClient(string indexName)
        {
            string searchServiceName = ConfigurationManager.AppSettings["ServiceName"];
            string queryApiKey = ConfigurationManager.AppSettings["SearchApiKey"];

            SearchIndexClient indexClient = new SearchIndexClient(searchServiceName, indexName, new SearchCredentials(queryApiKey));
            return indexClient;
        }
    }
}