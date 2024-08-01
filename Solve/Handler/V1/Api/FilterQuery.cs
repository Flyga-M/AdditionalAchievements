using AchievementLib.Pack.V1.Models;
using ApiParser;
using ApiParser.Endpoint;
using ApiParser.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Flyga.AdditionalAchievements.Solve.Handler.V1.Api
{
    public class FilterQuery
    {
        public FilterQueryData[] Data { get; }

        public EndpointQuery[] Queries => Data.Select(date => date.Query).ToArray();
        public string[] ExpectedValues => Data.Select(date => date.ExpectedValue).ToArray();
        public Comparison[] Comparisons => Data.Select(date => date.Comparison).ToArray();

        /// <exception cref="ArgumentNullException">If <paramref name="data"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="data"/> is empty.</exception>
        public FilterQuery(IEnumerable<FilterQueryData> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (!data.Any())
            {
                throw new ArgumentException($"{nameof(data)} must have at least one element.", nameof(data));
            }

            Data = data.ToArray();
        }

        /// <exception cref="ArgumentNullException">If <paramref name="data"/> is <see langword="null"/>.</exception>
        public FilterQuery(FilterQueryData data) : this(new FilterQueryData[] { data }) { /** NOOP **/ }

        /// <summary>
        /// Filters the <paramref name="data"/> according to the <see cref="Queries"/> and their <see cref="ExpectedValues"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>The <paramref name="data"/> objects, that fulfill every filter and the discarded <paramref name="data"/> 
        /// objects.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="data"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">If the provided key from any filter does not exists on any 
        /// <paramref name="data"/> object, or if any <see cref="FilterQueryData.Comparison"/> is an invalid choice 
        /// for the resulting value. Also, if <see cref="FilterQueryData.ExpectedValue"/> can't be parsed to the 
        /// same <see cref="Type"/> as the retrieved value.</exception>
        /// <exception cref="NotImplementedException">If a <see cref="FilterQueryData.Comparison"/> is used, that has not been 
        /// implemented yet.</exception>
        public async Task<(object[] Filtered, object[] Discarded)> Apply(IEnumerable<object> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (!data.Any())
            {
                return (Array.Empty<object>(), Array.Empty<object>());
            }

            List<object> filteredData = new List<object>();
            List<object> discardedData = new List<object>();

            foreach (object date in data)
            {
                bool filterApplies = true;
                
                foreach (FilterQueryData filterData in Data)
                {
                    object result;
                    
                    try
                    {
                        result = await QueryUtil.ResolveSubQueryAsync(date, filterData.Query, QuerySettings.Default);
                    }
                    catch (QueryResolveException ex)
                    {
                        throw new InvalidOperationException($"Unable to apply filter to data, because a provided " +
                            $"key in query {filterData.Query} does not exist.", ex);
                    }

                    // let exceptions bubble up
                    bool comparisonResult = ComparisonUtil.Compare(result, filterData.ExpectedValue, filterData.Comparison);

                    if (!comparisonResult)
                    {
                        filterApplies = false;
                        break;
                    }
                }

                if (filterApplies)
                {
                    filteredData.Add(data);
                }
                else
                {
                    discardedData.Add(data);
                }
            }

            return (filteredData.ToArray(), discardedData.ToArray());
        }

        
    }
}
