using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Azure.KeyVault;
using System.Web.Configuration;
using System.Threading.Tasks;

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
            Task<SearchIndexClient> callTask = Task.Run(() => CreateSearchIndexClientAsync("docs-db-index"));
            callTask.Wait();
            var searchClient = callTask.Result;

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
            Task<SearchIndexClient> callTask = Task.Run(() => CreateSearchIndexClientAsync("docs-blob-index"));
            callTask.Wait();
            var searchClient = callTask.Result;

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

        private async Task<SearchIndexClient> CreateSearchIndexClientAsync(string indexName)
        {
            string searchServiceName = ConfigurationManager.AppSettings["ServiceName"];
            //string queryApiKey = ConfigurationManager.AppSettings["SearchApiKey"];
            var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(SimpleAzureSearch.Utils.TokenObtainer.GetToken));
            var queryApiKeySecret = await kv.GetSecretAsync(WebConfigurationManager.AppSettings["SecretUri"]);
            string queryApiKey = queryApiKeySecret.Value;

            SearchIndexClient indexClient = new SearchIndexClient(searchServiceName, indexName, new SearchCredentials(queryApiKey));
            return indexClient;
        }
    }
}