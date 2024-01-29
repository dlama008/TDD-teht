using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TDD_teht
{
    public enum ElectricityContractType
    {
        FixedPrice,  // Kiinteä hintasopimus
        MarketPrice  // Markkinahintasopimus
    }

    public class PriceDifference
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal PriceDifferenceValue { get; set; }
        public ElectricityContractType CheaperContract { get; set; }

        public PriceDifference(DateTime startDate, DateTime endDate, decimal priceDifference, ElectricityContractType cheaperContract)
        {
            StartDate = startDate;
            EndDate = endDate;
            PriceDifferenceValue = priceDifference;
            CheaperContract = cheaperContract;
        }
    }

    public class ElectricityPriceComparer
    {
        public List<PriceDifference> ParseJson(string jsonInput)
        {
            var result = new List<PriceDifference>();

            // Parsi JSON-data JObject-muotoon
            JObject jsonData = JsonConvert.DeserializeObject<JObject>(jsonInput);

            // Tarkista, onko "prices" -avain olemassa ja että se on taulukko
            if (jsonData.TryGetValue("prices", out var pricesToken) && pricesToken is JArray pricesArray)
            {
                foreach (var priceObject in pricesArray)
                {
                    // Haetaan aloitus- ja lopetuspäivämäärä sekä hinta JSONista
                    var startDate = priceObject.Value<DateTime>("startDate");
                    var endDate = priceObject.Value<DateTime>("endDate");
                    var price = priceObject.Value<decimal>("price");

                    // Lisää PriceDifference-olio listaan markkinahintasopimuksella
                    result.Add(new PriceDifference(startDate, endDate, price, ElectricityContractType.MarketPrice));
                }
            }

            return result;
        }

        public List<PriceDifference> ComparePrices(decimal fixedPrice, List<PriceDifference> marketPrices)
        {
            var priceDifferences = new List<PriceDifference>();

            foreach (var marketPrice in marketPrices)
            {
                // Laske hintaero kiinteän hinnan ja markkinahinnan välillä
                var difference = marketPrice.PriceDifferenceValue - fixedPrice;

                // Määritä, kumpi sopimus on halvempi
                var cheaperContract = difference > 0 ? ElectricityContractType.FixedPrice : ElectricityContractType.MarketPrice;

                // Käsittely tasatilanteelle (hintaero on 0)
                if (difference == 0)
                {
                    cheaperContract = ElectricityContractType.FixedPrice; // Tai MarketPrice, riippuen siitä, miten haluat käsitellä tasatilanteita
                }

                // Lisää PriceDifference-olio listaan
                priceDifferences.Add(new PriceDifference(marketPrice.StartDate, marketPrice.EndDate, Math.Abs(Math.Round(difference, 3)), cheaperContract));
            }

            return priceDifferences;
        }
    }
}
