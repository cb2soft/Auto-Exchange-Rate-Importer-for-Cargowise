using HtmlAgilityPack;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using ExchangeRateImporter;


namespace Winform_Client
{

    public partial class Program
    {
        string TCMB_Exchange_Link = "https://www.tcmb.gov.tr/kurlar/today.xml";
        string India_Exchange_Link = "https://application.axisbank.co.in/WebForms/corporatecardrate/index.aspx";
        string UAE_Exchange_Link = "https://api.globalexchangerates.org/v1/latest?provider=CBAE&currencies=EUR,GBP,JPY,SAR,SGD,USD&base&decimals&format=XML&header";
        //string AU1_Exchange_Link = "https://api.globalexchangerates.org/v1/latest?provider=RBAU&currencies=EUR,USD&base&decimals&format=XML&header&extended=false";
        string VN1_Exchange_Link = "https://www.vietcombank.com.vn/api/exchangerates?date=";
        // TST Server
        string uriText_TST = Environment.GetEnvironmentVariable("Cargowise_URI_TST");
        string Username_TST = Environment.GetEnvironmentVariable("Cargowise_User_TST");
        string password_TST = Environment.GetEnvironmentVariable("Cargowise_Password_TST");

        // PRD Server
        string uriText_PRD = Environment.GetEnvironmentVariable("Cargowise_URI_PRD");
        string Username_PRD = Environment.GetEnvironmentVariable("Cargowise_User_PRD");
        string password_PRD = Environment.GetEnvironmentVariable("Cargowise_Password_PRD");

        // Active server (set at runtime based on args[1])
        string activeUri;
        string activeUsername;
        string activePassword;

        string Global_Exchange_Rate_API_Key = Environment.GetEnvironmentVariable("Global_Exchange_Rates_API_Key");

        //TURKEY
        string sendMessage;
        string[] BuyRates = new string[3];
        string[] SellRates = new string[3];
        string[] exchangeType = new string[3];

        //INDIA
        string[] exchangeTypeINBOM = new string[17];
        string[] BuyRatesINBOM = new string[17];
        string[] SellRatesINBOM = new string[17];

        //UAE
        string[] exchangeTypeDXB = new string[6];
        string[] BuyRatesDXB = new string[6];
        string[] SellRatesDXB = new string[6];

        //AUSTRALIA
        string[] exchangeTypeAU1 = new string[50];
        string[] BuyRatesAU1 = new string[50];
        string[] SellRatesAU1 = new string[50];

        //VIETNAM
        string[] exchangeTypeVN1 = new string[50];
        string[] BuyRatesVN1 = new string[50];
        string[] SellRatesVN1 = new string[50];


        int sayi;
        DateTime tarihDateTime;
        string tarih;
        string bitisTarih;
        static string runTimestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
        int i;

        void SetActiveServer(string serverArg)
        {
            if (serverArg.Equals("TST", StringComparison.OrdinalIgnoreCase))
            {
                activeUri = uriText_TST;
                activeUsername = Username_TST;
                activePassword = password_TST;
                Console.WriteLine("Using TST (Test) server.");
            }
            else if (serverArg.Equals("PRD", StringComparison.OrdinalIgnoreCase))
            {
                activeUri = uriText_PRD;
                activeUsername = Username_PRD;
                activePassword = password_PRD;
                Console.WriteLine("Using PRD (Production) server.");
            }
            else
            {
                throw new ArgumentException($"Invalid server parameter: '{serverArg}'. Use 'TST' or 'PRD'.");
            }
        }

        static async Task Main(string[] args)
        {
            Program program = new Program();
            //Setting runtime directory
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            Directory.SetCurrentDirectory(dir);

            //INDIA PARAMETER
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ExchangeRateImporter <Country> <Server>");
                Console.WriteLine("  Country: Turkey, India, UAE, Australia, Vietnam");
                Console.WriteLine("  Server:  TST or PRD");
                Environment.Exit(1);
            }

            // Set active server from second argument
            try
            {
                program.SetActiveServer(args[1]);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
            }

            if (args[0].Equals("India", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Collecting exchange rates for India...");
                try
                {
                    await program.ExchangeCollectorINBOM();
                    Console.WriteLine("Exchange rates collected successfully for India.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ExchangeCollector failed for India: " + ex.Message);
                    return; // Close if fails.
                }

                // Import to Cargowise
                try
                {
                    Console.WriteLine("Importing exchange rates to CargoWise for India...");
                    program.SendDataINBOM();
                    Console.WriteLine("Import completed successfully for India.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SendData failed for India: " + ex.Message);
                }

                // Closing program
                Console.WriteLine("Job finished for India. Program will now exit.");
            }
            //TURKEY PARAMETER
            else if (args[0].Equals("Turkey", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Collecting exchange rates for Turkey...");
                try
                {
                    program.ExchangeCollector();
                    Console.WriteLine("Exchange rates collected successfully for Turkey.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ExchangeCollector failed for Turkey: " + ex.Message);
                    return;
                }

                try
                {
                    Console.WriteLine("Importing exchange rates to CargoWise for Turkey...");
                    program.SendData();
                    Console.WriteLine("Import completed successfully for Turkey.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SendData failed for Turkey: " + ex.Message);
                }

                Console.WriteLine("Job finished for Turkey. Program will now exit.");
            }
            //UAE PARAMETER
            else if (args[0].Equals("UAE", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Collecting exchange rates for UAE...");
                try
                {
                    await program.ExchangeCollectorDXB();
                    Console.WriteLine("Exchange rates collected successfully for UAE.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ExchangeCollector failed for UAE: " + ex.Message);
                    return;
                }

                try
                {
                    Console.WriteLine("Importing exchange rates to CargoWise for UAE...");
                    program.SendDataDXB();
                    Console.WriteLine("Import completed successfully for UAE.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SendData failed for UAE: " + ex.Message);
                }

                Console.WriteLine("Job finished for UAE. Program will now exit.");
            }
            //AUSTRALIA PARAMETER
            else if (args[0].Equals("Australia", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Collecting exchange rates for Australia...");
                try
                {
                    await program.ExchangeCollectorAU1_v2();
                    Console.WriteLine("Exchange rates collected successfully for AU1.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ExchangeCollector failed AU1: " + ex.Message);
                    return;
                }

                try
                {
                    Console.WriteLine("Importing exchange rates to CargoWise for AU1...");
                    program.SendDataAU1();
                    Console.WriteLine("Import completed successfully for AU1.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SendData failed for AU1: " + ex.Message);
                }

                Console.WriteLine("Job finished. Program will now exit.");
            }
            //VIETNM PARAMETER
            else if (args[0].Equals("Vietnam", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Collecting exchange rates for Vietnam...");
                try
                {
                    await program.ExchangeCollectorVN();
                    Console.WriteLine("Exchange rates collected successfully for Vietnam.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ExchangeCollector failed for Vietnam: " + ex.Message);
                    return;
                }

                try
                {
                    Console.WriteLine("Importing exchange rates to CargoWise for Vietnam...");
                    program.SendDataVN();
                    Console.WriteLine("Import completed successfully for Vietnam.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SendData failed for Vietnam: " + ex.Message);
                }

                Console.WriteLine("Job finished. Program will now exit.");
            }
            Environment.Exit(0);
        }


        void SendData()
        {
            DateTime ertesiGunu = tarihDateTime.AddDays(1);
            if (ertesiGunu.DayOfWeek == DayOfWeek.Saturday)
                bitisTarih = tarihDateTime.AddDays(3).ToString("yyyy-MM-dd");
            else if (ertesiGunu.DayOfWeek == DayOfWeek.Sunday)
                bitisTarih = tarihDateTime.AddDays(2).ToString("yyyy-MM-dd");
            else
                bitisTarih = ertesiGunu.ToString("yyyy-MM-dd");

            for (i = 0; i < 3; i++)
            {
                sendMessage = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
    <Body>
        <CurrencyExchangeRate version=""2.0"">
            <RefExchangeRate Action=""MERGE"">
                <ExRateType>BUY</ExRateType>
                <StartDate>{bitisTarih}T00:00:00</StartDate>
                <ExpiryDate>{bitisTarih}T23:59:00</ExpiryDate>
                <SellRate>{BuyRates[i]}</SellRate>
                <IsSystem>False</IsSystem>
                <RefCurrency Action=""MERGE"">
                    <Code>{exchangeType[i]}</Code>
                </RefCurrency>
                <GlbCompany Action=""MERGE"">
                    <Code>TR1</Code>
                </GlbCompany>
            </RefExchangeRate>
        </CurrencyExchangeRate>
    </Body>
</Native>";
                sayi++;
                HTTPPostXMLMessage("Turkey", exchangeType[i], "BUY");

                sendMessage = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
    <Body>
        <CurrencyExchangeRate version=""2.0"">
            <RefExchangeRate Action=""MERGE"">
                <ExRateType>SEL</ExRateType>
                <StartDate>{bitisTarih}T00:00:00</StartDate>
                <ExpiryDate>{bitisTarih}T23:59:00</ExpiryDate>
                <SellRate>{SellRates[i]}</SellRate>
                <IsSystem>False</IsSystem>
                <RefCurrency Action=""MERGE"">
                    <Code>{exchangeType[i]}</Code>
                </RefCurrency>
                <GlbCompany Action=""MERGE"">
                    <Code>TR1</Code>
                </GlbCompany>
            </RefExchangeRate>
        </CurrencyExchangeRate>
    </Body>
</Native>";
                sayi++;
                HTTPPostXMLMessage("Turkey", exchangeType[i], "SEL");
            }
        }

        void Log(string text) => Console.WriteLine(text);

        XmlDocument xmlDoc = new XmlDocument();

        void ExchangeCollector()
        {
            xmlDoc.Load($"{TCMB_Exchange_Link}");

            XmlNode rootNode = xmlDoc.SelectSingleNode("/Tarih_Date");

            // EUR
            XmlNode eur = xmlDoc.SelectSingleNode("//Currency[@Kod='EUR']");
            BuyRates[0] = eur.SelectSingleNode("ForexBuying")?.InnerText;
            SellRates[0] = eur.SelectSingleNode("ForexSelling")?.InnerText;
            exchangeType[0] = "EUR";

            // GBP
            XmlNode gbp = xmlDoc.SelectSingleNode("//Currency[@Kod='GBP']");
            BuyRates[1] = gbp.SelectSingleNode("ForexBuying")?.InnerText;
            SellRates[1] = gbp.SelectSingleNode("ForexSelling")?.InnerText;
            exchangeType[1] = "GBP";

            // USD
            XmlNode usd = xmlDoc.SelectSingleNode("//Currency[@Kod='USD']");
            BuyRates[2] = usd.SelectSingleNode("ForexBuying")?.InnerText;
            SellRates[2] = usd.SelectSingleNode("ForexSelling")?.InnerText;
            exchangeType[2] = "USD";

            if (rootNode != null)
            {
                string tarihString = rootNode.Attributes["Tarih"]?.InnerText;
                tarihDateTime = DateTime.Parse(tarihString);
                tarih = tarihDateTime.ToString("yyyy-MM-dd");
            }
        }

        async Task ExchangeCollectorINBOM()
        {
            using var client = new HttpClient();
            var html = await client.GetStringAsync(India_Exchange_Link);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var rows = doc.DocumentNode.SelectNodes("//table//tr[position()>1]");
            if (rows == null)
                throw new Exception("No rows found in Axis Bank HTML response.");

            int index = 0;
            foreach (var tr in rows)
            {
                var cells = tr.SelectNodes("td");
                if (cells == null || cells.Count < 8) continue;

                exchangeTypeINBOM[index] = cells[1].InnerText.Trim();  // Currency code
                if (exchangeTypeINBOM[index] == "CNH") exchangeTypeINBOM[index] = "CNY"; // Fix for CargoWise ISO code
                BuyRatesINBOM[index] = cells[2].InnerText.Trim();      // TT Buy
                SellRatesINBOM[index] = cells[3].InnerText.Trim();     // TT Sell
                index++;
            }

            tarihDateTime = DateTime.Today;
            tarih = tarihDateTime.ToString("yyyy-MM-dd");
        }

        async Task ExchangeCollectorDXB()
        {
            if (string.IsNullOrEmpty(UAE_Exchange_Link))
            {
                Console.WriteLine("UAE_Exchange_Link is null or empty.");
                return;
            }

            HttpClient client = new HttpClient();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, UAE_Exchange_Link);
                request.Headers.Add("Subscription-Key", Global_Exchange_Rate_API_Key);

                HttpResponseMessage response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string xml = await response.Content.ReadAsStringAsync();

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);

                XmlNodeList nodes = doc.SelectNodes("//exchangeRate");
                if (nodes == null || nodes.Count == 0)
                {
                    throw new Exception("No exchangeRate nodes found in UAE API response.");
                }

                int index = 0;
                foreach (XmlNode node in nodes)
                {
                    if (index >= exchangeTypeDXB.Length) break;

                    string currency = node["currency"]?.InnerText;
                    string rateStr = node["rate"]?.InnerText;

                    if (!string.IsNullOrEmpty(currency) && !string.IsNullOrEmpty(rateStr))
                    {
                        if (decimal.TryParse(rateStr, System.Globalization.NumberStyles.Any,
                                             System.Globalization.CultureInfo.InvariantCulture, out decimal rateValue))
                        {
                            if (rateValue != 0)
                            {
                                decimal inverted = 1 / rateValue;

                                exchangeTypeDXB[index] = currency;
                                BuyRatesDXB[index] = inverted.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
                                SellRatesDXB[index] = inverted.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);

                                Console.WriteLine($"{currency} => {inverted} AED");
                            }
                            else
                            {
                                Console.WriteLine($"{currency} rate is zero, skipping...");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Failed to parse rate for {currency}: {rateStr}");
                        }
                    }
                    index++;
                }

                tarihDateTime = DateTime.Today;
                tarih = tarihDateTime.ToString("yyyy-MM-dd");
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP Error: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
            }
        }

        async Task ExchangeCollectorAU1_v2()
        {
            using var client = new HttpClient();

            // 1️. Anonymous token alma
            var anonTokenUrl = "https://customer.api.nab.com.au/v1/idp/oauth/token";

            var anonPayload = new
            {
                client_id = "69CC21FB-CDCA-027F-1BCF-475DF53D3C23",
                grant_type = "nab:anonymous",
                scope = "custoffer:edbcc:readsubmission custoffer:referencedata:read forms:form:get forms:submission forms:submission:create frauddetect:self-service"
            };

            var anonContent = new StringContent(JsonSerializer.Serialize(anonPayload), Encoding.UTF8, "application/json");
            var anonResponse = await client.PostAsync(anonTokenUrl, anonContent);
            anonResponse.EnsureSuccessStatusCode();

            var anonResult = await anonResponse.Content.ReadAsStringAsync();
            using var anonDoc = JsonDocument.Parse(anonResult);
            var anonAccessToken = anonDoc.RootElement.GetProperty("access_token").GetString();

            Console.WriteLine("Anonymous Access Token: " + anonAccessToken);

            // 2️. Token-exchange (actor_token = anonAccessToken)
            var exchangeUrl = "https://customer.api.nab.com.au/v1/idp/oauth/token";

            var exchangePayload = new
            {
                client_id = "69406D32-05CB-F258-064F-E7D6CA23F48E",
                grant_type = "urn:ietf:params:oauth:grant-type:token-exchange",
                requested_token_type = "urn:ietf:params:oauth:token-type:access_token",
                subject_token = "69406D32-05CB-F258-064F-E7D6CA23F48E",
                subject_token_type = "nab:oauth:token-type:client_id",
                actor_token = anonAccessToken,
                actor_token_type = "urn:ietf:params:oauth:token-type:access_token",
                scope = "content:fxcalculator:convert forms:form:get"
            };

            var exchangeContent = new StringContent(JsonSerializer.Serialize(exchangePayload), Encoding.UTF8, "application/json");
            var exchangeResponse = await client.PostAsync(exchangeUrl, exchangeContent);
            exchangeResponse.EnsureSuccessStatusCode();

            var exchangeResult = await exchangeResponse.Content.ReadAsStringAsync();
            using var exchangeDoc = JsonDocument.Parse(exchangeResult);
            var finalAccessToken = exchangeDoc.RootElement.GetProperty("access_token").GetString();

            Console.WriteLine("Final Access Token (for FX API): " + finalAccessToken);

            // 3️. FX Rates API çağrısı
            var fxUrl = "https://customer.api.nab.com.au/v1/content/nab-calculators-fx-bff";

            var fxPayload = new
            {
                operationName = "getRates",
                query = @"query getRates($input: String) {
  getRates(input: $input) {
    timestamp
    rates {
      currencyCode
      currencyName
      symbol
      direction
      buyRate
      sellRate
      __typename
    }
    __typename
  }
}",
                variables = new
                {
                    input = "IMT"
                }
            };

            var fxContent = new StringContent(JsonSerializer.Serialize(fxPayload), Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + finalAccessToken);

            var fxResponse = await client.PostAsync(fxUrl, fxContent);
            fxResponse.EnsureSuccessStatusCode();

            var fxResult = await fxResponse.Content.ReadAsStringAsync();
            Console.WriteLine("\nFX Rates Response:");
            Console.WriteLine(fxResult);

            // Örnek olarak tüm currencyCode + buy/sell rate bilgilerini yazdır
            using var fxDoc = JsonDocument.Parse(fxResult);
            var rates = fxDoc.RootElement
                             .GetProperty("data")
                             .GetProperty("getRates")
                             .GetProperty("rates");
            int index = 0;
            foreach (var rate in rates.EnumerateArray())
            {
                exchangeTypeAU1[index] = rate.GetProperty("currencyCode").GetString();
                BuyRatesAU1[index] = rate.GetProperty("buyRate").GetString();
                SellRatesAU1[index] = rate.GetProperty("sellRate").GetString();
                Console.WriteLine($"{exchangeTypeAU1[index]}: Buy={BuyRatesAU1[index]}, Sell={SellRatesAU1[index]}");
                index++;
            }
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TextFiles", $"FX_NAB_raw_{DateTime.Now:yyyyMMdd_HHmmss}.json"), fxResult);
        }

        async Task ExchangeCollectorVN()
        {
            using var client = new HttpClient();
            try
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                string requestUrl = $"{VN1_Exchange_Link}{date}";

                Console.WriteLine($"Requesting Vietnam exchange rates from: {requestUrl}");

                HttpResponseMessage response = await client.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();

                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement dataArray = doc.RootElement.GetProperty("Data");

                // Temizle
                Array.Clear(exchangeTypeVN1, 0, exchangeTypeVN1.Length);
                Array.Clear(BuyRatesVN1, 0, BuyRatesVN1.Length);
                Array.Clear(SellRatesVN1, 0, SellRatesVN1.Length);

                string[] targetCurrencies = { "USD", "EUR", "GBP" };
                int index = 0;

                foreach (JsonElement item in dataArray.EnumerateArray())
                {
                    string currencyCode = item.GetProperty("currencyCode").GetString();

                    if (targetCurrencies.Contains(currencyCode))
                    {
                        string cashRate = item.GetProperty("cash").GetString();
                        string sellRate = item.GetProperty("sell").GetString();

                        if (string.IsNullOrEmpty(cashRate) || string.IsNullOrEmpty(sellRate))
                        {
                            Console.WriteLine($"Skipping {currencyCode}: missing rate values.");
                            continue;
                        }

                        // 6 hane ile normalize et
                        if (decimal.TryParse(cashRate, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal cashVal))
                            cashRate = Math.Round(cashVal, 6).ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
                        if (decimal.TryParse(sellRate, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal sellVal))
                            sellRate = Math.Round(sellVal, 6).ToString("F6", System.Globalization.CultureInfo.InvariantCulture);

                        exchangeTypeVN1[index] = currencyCode;
                        BuyRatesVN1[index] = cashRate;
                        SellRatesVN1[index] = sellRate;

                        Console.WriteLine($"{currencyCode} → BUY (cash): {cashRate} | SELL: {sellRate}");
                        index++;
                    }
                }

                if (index == 0)
                    throw new Exception("No matching currencies (USD, EUR, GBP) found in Vietnam API response.");

                tarihDateTime = DateTime.Today;
                tarih = tarihDateTime.ToString("yyyy-MM-dd");

                Console.WriteLine("Vietnam USD, EUR, GBP exchange rates collected successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error collecting Vietnam rates: {ex.Message}");
            }
        }

        void SendDataVN()
        {
            tarihDateTime = DateTime.Today;
            tarih = tarihDateTime.ToString("yyyy-MM-dd");

            for (int j = 0; j < exchangeTypeVN1.Length; j++)
            {
                if (string.IsNullOrEmpty(exchangeTypeVN1[j]))
                    continue;

                // BUY
                sendMessage = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Body>
    <CurrencyExchangeRate version=""2.0"">
      <RefExchangeRate Action=""MERGE"">
        <ExRateType>BUY</ExRateType>
        <StartDate>{tarih}T00:00:00</StartDate>
        <ExpiryDate>{tarih}T23:59:00</ExpiryDate>
        <SellRate>{BuyRatesVN1[j]}</SellRate>
        <IsSystem>False</IsSystem>
        <RefCurrency Action=""MERGE"">
          <Code>{exchangeTypeVN1[j]}</Code>
        </RefCurrency>
        <GlbCompany Action=""MERGE"">
          <Code>VN1</Code>
        </GlbCompany>
      </RefExchangeRate>
    </CurrencyExchangeRate>
  </Body>
</Native>";
                HTTPPostXMLMessage("Vietnam", exchangeTypeVN1[j], "BUY");

                // SELL
                sendMessage = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Body>
    <CurrencyExchangeRate version=""2.0"">
      <RefExchangeRate Action=""MERGE"">
        <ExRateType>SEL</ExRateType>
        <StartDate>{tarih}T00:00:00</StartDate>
        <ExpiryDate>{tarih}T23:59:00</ExpiryDate>
        <SellRate>{SellRatesVN1[j]}</SellRate>
        <IsSystem>False</IsSystem>
        <RefCurrency Action=""MERGE"">
          <Code>{exchangeTypeVN1[j]}</Code>
        </RefCurrency>
        <GlbCompany Action=""MERGE"">
          <Code>VN1</Code>
        </GlbCompany>
      </RefExchangeRate>
    </CurrencyExchangeRate>
  </Body>
</Native>";
                HTTPPostXMLMessage("Vietnam", exchangeTypeVN1[j], "SEL");
            }

            Console.WriteLine("Vietnam USD, EUR, GBP rates successfully sent to CargoWise.");
        }

        void SendDataDXB()
        {
            tarihDateTime = DateTime.Today;
            tarih = tarihDateTime.ToString("yyyy-MM-dd");

            for (int j = 0; j < exchangeTypeDXB.Length; j++)
            {
                if (string.IsNullOrEmpty(exchangeTypeDXB[j]))
                    continue;

                // 6 basamak garantisi
                decimal buy = decimal.Parse(BuyRatesDXB[j], System.Globalization.CultureInfo.InvariantCulture);
                decimal sell = decimal.Parse(SellRatesDXB[j], System.Globalization.CultureInfo.InvariantCulture);
                BuyRatesDXB[j] = Math.Round(buy, 6).ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
                SellRatesDXB[j] = Math.Round(sell, 6).ToString("F6", System.Globalization.CultureInfo.InvariantCulture);

                sendMessage = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
    <Body>
        <CurrencyExchangeRate version=""2.0"">
            <RefExchangeRate Action=""MERGE"">
                <ExRateType>BUY</ExRateType>
                <StartDate>{tarih}T00:00:00</StartDate>
                <ExpiryDate>{tarih}T23:59:00</ExpiryDate>
                <SellRate>{BuyRatesDXB[j]}</SellRate>
                <IsSystem>False</IsSystem>
                <RefCurrency Action=""MERGE"">
                    <Code>{exchangeTypeDXB[j]}</Code>
                </RefCurrency>
                <GlbCompany Action=""MERGE"">
                    <Code>AE4</Code>
                </GlbCompany>
            </RefExchangeRate>
        </CurrencyExchangeRate>
    </Body>
</Native>";
                sayi++;
                HTTPPostXMLMessage("UAE", exchangeTypeDXB[j], "BUY");

                sendMessage = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
    <Body>
        <CurrencyExchangeRate version=""2.0"">
            <RefExchangeRate Action=""MERGE"">
                <ExRateType>SEL</ExRateType>
                <StartDate>{tarih}T00:00:00</StartDate>
                <ExpiryDate>{tarih}T23:59:00</ExpiryDate>
                <SellRate>{SellRatesDXB[j]}</SellRate>
                <IsSystem>False</IsSystem>
                <RefCurrency Action=""MERGE"">
                    <Code>{exchangeTypeDXB[j]}</Code>
                </RefCurrency>
                <GlbCompany Action=""MERGE"">
                    <Code>AE4</Code>
                </GlbCompany>
            </RefExchangeRate>
        </CurrencyExchangeRate>
    </Body>
</Native>";
                sayi++;
                HTTPPostXMLMessage("UAE", exchangeTypeDXB[j], "SEL");
            }
        }

        void SendDataAU1()
        {
            tarihDateTime = DateTime.Today;
            tarih = tarihDateTime.ToString("yyyy-MM-dd");

            for (int j = 0; j < exchangeTypeAU1.Length; j++)
            {
                if (string.IsNullOrEmpty(exchangeTypeAU1[j]))
                    continue;

                // 6 basamak garantisi
                //decimal buy = decimal.Parse(BuyRatesAU1[j], System.Globalization.CultureInfo.InvariantCulture);
                //decimal sell = decimal.Parse(SellRatesAU1[j], System.Globalization.CultureInfo.InvariantCulture);
                //BuyRatesAU1[j] = Math.Round(buy, 6).ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
                //SellRatesAU1[j] = Math.Round(sell, 6).ToString("F6", System.Globalization.CultureInfo.InvariantCulture);

                sendMessage = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
    <Body>
        <CurrencyExchangeRate version=""2.0"">
            <RefExchangeRate Action=""MERGE"">
                <ExRateType>BUY</ExRateType>
                <StartDate>{tarih}T00:00:00</StartDate>
                <ExpiryDate>{tarih}T23:59:00</ExpiryDate>
                <SellRate>{BuyRatesAU1[j]}</SellRate>
                <IsSystem>False</IsSystem>
                <RefCurrency Action=""MERGE"">
                    <Code>{exchangeTypeAU1[j]}</Code>
                </RefCurrency>
                <GlbCompany Action=""MERGE"">
                    <Code>AU1</Code>
                </GlbCompany>
            </RefExchangeRate>
        </CurrencyExchangeRate>
    </Body>
</Native>";
                sayi++;
                HTTPPostXMLMessage("Australia", exchangeTypeAU1[j], "BUY");

                sendMessage = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
    <Body>
        <CurrencyExchangeRate version=""2.0"">
            <RefExchangeRate Action=""MERGE"">
                <ExRateType>SEL</ExRateType>
                <StartDate>{tarih}T00:00:00</StartDate>
                <ExpiryDate>{tarih}T23:59:00</ExpiryDate>
                <SellRate>{SellRatesAU1[j]}</SellRate>
                <IsSystem>False</IsSystem>
                <RefCurrency Action=""MERGE"">
                    <Code>{exchangeTypeAU1[j]}</Code>
                </RefCurrency>
                <GlbCompany Action=""MERGE"">
                    <Code>AU1</Code>
                </GlbCompany>
            </RefExchangeRate>
        </CurrencyExchangeRate>
    </Body>
</Native>";
                sayi++;
                HTTPPostXMLMessage("Australia", exchangeTypeAU1[j], "SEL");
            }
        }

        void SendDataINBOM()
        {
            for (i = 0; i < exchangeTypeINBOM.Length; i++)
            {
                if (string.IsNullOrEmpty(exchangeTypeINBOM[i])) continue;

                sendMessage = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Body>
    <CurrencyExchangeRate version=""2.0"">
      <RefExchangeRate Action=""MERGE"">
        <ExRateType>BUY</ExRateType>
        <StartDate>{tarih}T00:00:00</StartDate>
        <ExpiryDate>{tarih}T23:59:00</ExpiryDate>
        <SellRate>{BuyRatesINBOM[i]}</SellRate>
        <IsSystem>False</IsSystem>
        <RefCurrency Action=""MERGE"">
          <Code>{exchangeTypeINBOM[i]}</Code>
        </RefCurrency>
        <GlbCompany Action=""MERGE"">
          <Code>IND</Code>
        </GlbCompany>
      </RefExchangeRate>
    </CurrencyExchangeRate>
  </Body>
</Native>";
                sayi++;
                HTTPPostXMLMessage("India", exchangeTypeINBOM[i], "BUY");

                sendMessage = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Body>
    <CurrencyExchangeRate version=""2.0"">
      <RefExchangeRate Action=""MERGE"">
        <ExRateType>SEL</ExRateType>
        <StartDate>{tarih}T00:00:00</StartDate>
        <ExpiryDate>{tarih}T23:59:00</ExpiryDate>
        <SellRate>{SellRatesINBOM[i]}</SellRate>
        <IsSystem>False</IsSystem>
        <RefCurrency Action=""MERGE"">
          <Code>{exchangeTypeINBOM[i]}</Code>
        </RefCurrency>
        <GlbCompany Action=""MERGE"">
          <Code>IND</Code>
        </GlbCompany>
      </RefExchangeRate>
    </CurrencyExchangeRate>
  </Body>
</Native>";
                sayi++;
                HTTPPostXMLMessage("India", exchangeTypeINBOM[i], "SEL");
            }
        }

        void HTTPPostXMLMessage(string countryCode, string currencyCode, string rateType)
        {
            var uri = new Uri(activeUri);
            var client = new HttpXmlClient(uri, true, activeUsername, activePassword);
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };

            using (var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(sendMessage)))
            {
                var response = client.Post(sourceStream); // Response is NOT closed here to allow reading stream
                var responseStatus = response.StatusCode;

                if (response.Content != null)
                {
                    var stream = response.Content.ReadAsStreamAsync().Result;
                    if (response.Content.Headers.ContentEncoding.Contains("gzip", StringComparer.InvariantCultureIgnoreCase))
                    {
                        stream = new GZipStream(stream, CompressionMode.Decompress);
                    }

                    using (XmlReader reader = XmlReader.Create(stream, settings))
                    {
                        // Load directly into XDocument instead of XmlDocument to keep it in memory easily
                        XDocument doc = XDocument.Load(reader);
                        IsPosted(doc, countryCode, currencyCode, rateType);
                    }
                }
            }
        }

        public static bool IsPosted(XDocument doc, string countryCode, string currencyCode, string rateType)
        {
            XNamespace ns = "http://www.cargowise.com/Schemas/Universal/2011/11";

            var processingLog = doc.Root.Element(ns + "ProcessingLog")?.Value;
            string statusMessage = "Unknown";
            bool isSuccess = false;

            if (!string.IsNullOrEmpty(processingLog))
            {
                if (processingLog.Contains("Information - RefExchangeRate - 1 inserts") ||
                    processingLog.Contains("Information - GlbCompany - 1 inserts"))
                {
                    statusMessage = "1 Insert";
                    isSuccess = true;
                    Console.WriteLine($"{countryCode} - {currencyCode} {rateType}: 1 Insert");
                }
                else if (processingLog.Contains("Information - RefExchangeRate - 0 inserts, 1 updates") ||
                         processingLog.Contains("Information - GlbCompany - 0 inserts, 1 updates"))
                {
                    statusMessage = "1 Update";
                    isSuccess = true;
                    Console.WriteLine($"{countryCode} - {currencyCode} {rateType}: 1 Update");
                }
                else if (processingLog.Contains("Information - RefExchangeRate - 0 inserts, 0 updates"))
                {
                    statusMessage = "NO UPDATES/INSERTS";
                    Console.WriteLine($"{countryCode} - {currencyCode} {rateType}: NO UPDATES (Review needed)");
                }
                else
                {
                    // Catch other cases, potentially errors or mixed results
                    statusMessage = "Other/Error";
                    Console.WriteLine($"{countryCode} - {currencyCode} {rateType}: {processingLog}");
                }
            }
            else
            {
                Console.WriteLine($"{countryCode} - {currencyCode} {rateType}: No ProcessingLog found.");
            }

            // --- FILE LOGGING ---
            try
            {
                string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TextFiles");
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                // string dateStr = DateTime.Now.ToString("yyyyMMdd");
                string logFileName = $"ImportLog_{countryCode}_{runTimestamp}.txt";
                string logPath = Path.Combine(logDir, logFileName);
                string timestamp = DateTime.Now.ToString("HH:mm:ss");

                string logLine = $"{timestamp} - {currencyCode} {rateType}: {statusMessage}";
                if (!isSuccess && !string.IsNullOrEmpty(processingLog))
                {
                    // If it wasnt a clear success pattern, log the full message for debugging
                    logLine += $" | Details: {processingLog}";
                }

                File.AppendAllText(logPath, logLine + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logging failed: {ex.Message}");
            }

            return isSuccess;
        }
    }
}