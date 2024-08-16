using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NBRMproject.Models;
using Microsoft.AspNetCore.Mvc;

namespace NBRMproject.Controllers
{
    public class exchangeController : Controller
    {
        private static readonly string BaseUrl = "https://www.nbrm.mk/KLServiceNOV/GetExchangeRate";

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null) startDate = DateTime.Today.AddDays(-5);
            if (endDate == null) endDate = DateTime.Today;

            var exchangeRates = await GetExchangeRatesAsync(startDate.Value, endDate.Value);

            var model = new HomeViewModel
            {
                ExchangeRates = exchangeRates,
                DateRange = new DateRangeModel
                {
                    StartDate = startDate.Value,
                    EndDate = endDate.Value
                }
            };

            return View(model);
        }

        private async Task<IEnumerable<ExchangeRate>> GetExchangeRatesAsync(DateTime startDate, DateTime endDate)
        {
            using (HttpClient client = new HttpClient())
            {
                var url = $"{BaseUrl}?StartDate={startDate:dd.MM.yyyy}&EndDate={endDate:dd.MM.yyyy}&format=json";

                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode(); 
                    string responseBody = await response.Content.ReadAsStringAsync();

                    JToken jsonToken = JToken.Parse(responseBody);

                    if (jsonToken.Type == JTokenType.Array)
                    {
                        JArray jsonArray = (JArray)jsonToken;
                        var exchangeRates = new List<ExchangeRate>();

                        foreach (var rate in jsonArray)
                        {
                            try
                            {
                                var exchangeRate = new ExchangeRate
                                {
                                    Date = DateTime.Parse(rate["datum"]?.ToString() ?? DateTime.Now.ToString()),
                                    Currency = rate["oznaka"]?.ToString() ?? "N/A",
                                    Rate = decimal.Parse(rate["sreden"]?.ToString() ?? "0")
                                };
                                exchangeRates.Add(exchangeRate);
                            }
                            catch (Exception)
                            {
                                
                            }
                        }

                        return exchangeRates;
                    }
                    else
                    {
                        return new List<ExchangeRate>();
                    }
                }
                catch (HttpRequestException)
                {
                    return new List<ExchangeRate>();
                }
                catch (Exception)
                {
                    return new List<ExchangeRate>();
                }
            }
        }
    }
}
