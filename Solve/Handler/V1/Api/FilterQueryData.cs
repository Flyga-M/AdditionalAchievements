using ApiParser.Endpoint;
using System;
using AchievementLib.Pack.V1.Models;

namespace Flyga.AdditionalAchievements.Solve.Handler.V1.Api
{
    public class FilterQueryData
    {
        public EndpointQuery Query { get; }

        public string ExpectedValue { get; }

        public Comparison Comparison { get; }

        public FilterQueryData(EndpointQuery query, string expectedValue, Comparison comparison)
        {
            Query = query ?? throw new ArgumentNullException(nameof(query));
            ExpectedValue = expectedValue ?? throw new ArgumentNullException(nameof(expectedValue));
            Comparison = comparison;

            if (string.IsNullOrWhiteSpace(expectedValue))
            {
                throw new ArgumentException($"{nameof(expectedValue)} can't be empty or whitespace.", nameof(expectedValue));
            }
        }
    }
}
