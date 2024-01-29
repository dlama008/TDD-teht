using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using TDD_teht; // Olettaen, että tämä on nimiavaruus, jossa pääkoodisi sijaitsee

namespace TestProjectTDD
{
    public class ElectricityPriceComparerTests
    {
        private readonly ITestOutputHelper output; // ITestOutputHelper-injektio

        public ElectricityPriceComparerTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Test_JsonParsing()
        {
            // Testaa JSON-analyysin toimintaa
            string jsonInput = @"{
                ""prices"": [
                    {
                        ""price"": 13.494,
                        ""startDate"": ""2022-11-14T22:00:00.000Z"",
                        ""endDate"": ""2022-11-14T23:00:00.000Z""
                    },
                    {
                        ""price"": 17.62,
                        ""startDate"": ""2022-11-14T21:00:00.000Z"",
                        ""endDate"": ""2022-11-14T22:00:00.000Z""
                    }
                ]
            }";

            var comparer = new ElectricityPriceComparer();
            var prices = comparer.ParseJson(jsonInput);

            // Varmista, että tulokset ovat odotetut
            Assert.NotNull(prices);
            Assert.Equal(2, prices.Count);
            Assert.All(prices, price => Assert.Equal(ElectricityContractType.MarketPrice, price.CheaperContract));
        }

        [Fact]
        public void Test_PriceComparison_FixedPriceCheaper()
        {
            // Testaa kiinteän hinnan olevan halvempi kuin markkinahinta
            decimal fixedPriceValue = 10m;
            var marketPrices = new List<PriceDifference>
            {
                new PriceDifference(DateTime.Parse("2022-11-14T22:00:00.000Z"), DateTime.Parse("2022-11-14T23:00:00.000Z"), 13.494m, ElectricityContractType.MarketPrice),
                new PriceDifference(DateTime.Parse("2022-11-14T21:00:00.000Z"), DateTime.Parse("2022-11-14T22:00:00.000Z"), 17.62m, ElectricityContractType.MarketPrice)
            };

            var comparer = new ElectricityPriceComparer();
            var priceDifferences = comparer.ComparePrices(fixedPriceValue, marketPrices);

            // Varmista, että kaikki hintaerot ovat kiinteän hinnan hyväksi
            Assert.All(priceDifferences, pd => Assert.Equal(ElectricityContractType.FixedPrice, pd.CheaperContract));
        }

        [Fact]
        public void Test_PriceComparison_MarketPriceCheaper()
        {
            // Testaa markkinahinnan olevan halvempi kuin kiinteä hinta
            decimal fixedPriceValue = 18m;
            var marketPrices = new List<PriceDifference>
            {
                new PriceDifference(DateTime.Parse("2022-11-14T22:00:00.000Z"), DateTime.Parse("2022-11-14T23:00:00.000Z"), 13.494m, ElectricityContractType.MarketPrice),
                new PriceDifference(DateTime.Parse("2022-11-14T21:00:00.000Z"), DateTime.Parse("2022-11-14T22:00:00.000Z"), 17.62m, ElectricityContractType.MarketPrice)
            };

            var comparer = new ElectricityPriceComparer();
            var priceDifferences = comparer.ComparePrices(fixedPriceValue, marketPrices);

            // Varmista, että kaikki hintaerot ovat markkinahinnan hyväksi
            Assert.All(priceDifferences, pd => Assert.Equal(ElectricityContractType.MarketPrice, pd.CheaperContract));
        }

        [Fact]
        public void Test_PriceComparison_SamePrice()
        {
            // Testaa tilannetta, jossa hinnat ovat samat
            decimal fixedPriceValue = 13.494m;
            var marketPrices = new List<PriceDifference>
            {
                new PriceDifference(DateTime.Parse("2022-11-14T22:00:00.000Z"), DateTime.Parse("2022-11-14T23:00:00.000Z"), 13.494m, ElectricityContractType.MarketPrice)
            };

            var comparer = new ElectricityPriceComparer();
            var priceDifferences = comparer.ComparePrices(fixedPriceValue, marketPrices);

            // Tässä oletetaan, että tasapelin sattuessa kiinteä hinta on edullisempi
            Assert.All(priceDifferences, pd => Assert.Equal(ElectricityContractType.FixedPrice, pd.CheaperContract));
        }

        [Fact]
        public void Test_ConsoleOutput()
        {
            // Testaa tulostusta konsoliin
            decimal fixedPriceValue = 10m; // Oletetaan, että kiinteä hinta on 10c

            var marketPrices = new List<PriceDifference>
            {
                new PriceDifference(DateTime.Parse("2022-11-14T22:00:00.000Z"), DateTime.Parse("2022-11-14T23:00:00.000Z"), 13.494m, ElectricityContractType.MarketPrice),
                new PriceDifference(DateTime.Parse("2022-11-14T21:00:00.000Z"), DateTime.Parse("2022-11-14T22:00:00.000Z"), 17.62m, ElectricityContractType.MarketPrice)
            };

            var comparer = new ElectricityPriceComparer();
            var priceDifferences = comparer.ComparePrices(fixedPriceValue, marketPrices);

            // Tulosta tulokset konsoliin
            foreach (var priceDifference in priceDifferences)
            {
                output.WriteLine($"Pörssisähkö: {priceDifference.StartDate.ToString("O")} - {priceDifference.EndDate.ToString("O")} välinen hinta on {priceDifference.PriceDifferenceValue}c");
                output.WriteLine($"Kiinteäsähkö: {fixedPriceValue}c");
                output.WriteLine($"Edullisempi vaihtoehto: {priceDifference.CheaperContract}");
            }
        }
    }
}
