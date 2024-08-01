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
    /// <summary>
    /// Provides utility functions for <see cref="Restraint"/>s.
    /// </summary>
    public static class FilterUtil
    {
        /// <summary>
        /// Constructs all alternative <see cref="FilterQuery"/>ies, that may be applied to a gw2 api response from the 
        /// given <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>The constructed alternative <see cref="FilterQuery"/>ies.</returns>
        public static FilterQuery[] QueryFromFilter(Restraint filter)
        {
            EndpointQueryBuilder queryBuilder = new EndpointQueryBuilder();
            BuildQueryBuilder(filter, ref queryBuilder);

            FilterQueryData coreFilterData = new FilterQueryData(queryBuilder.Build(), filter.GetValue(), filter.GetComparison());

            return QueryFromFilterCore(filter, coreFilterData);
        }

        private static void BuildQueryBuilder(Restraint filter, ref EndpointQueryBuilder queryBuilder)
        {
            if (queryBuilder == null)
            {
                queryBuilder = new EndpointQueryBuilder();
            }
            
            queryBuilder = queryBuilder
                .AddPart(
                    new EndpointQueryPartBuilder()
                        .WithEndpointName(filter.Key)
                        .Build()
                );

            if (filter is RestraintSubLevel filterSub)
            {
                BuildQueryBuilder(filterSub.Restraint, ref queryBuilder);
            }
        }

        private static FilterQuery[] QueryFromFilterCore(Restraint filter, FilterQueryData coreFilterData)
        {
            List<FilterQuery> alternativeFilterQueries = new List<FilterQuery>();

            FilterQuery[] andFilterQueries = Array.Empty<FilterQuery>();

            if (filter.AndRestraint != null)
            {
                andFilterQueries = QueryFromFilter(filter.AndRestraint);
            }
            else
            {
                alternativeFilterQueries.Add(new FilterQuery(coreFilterData));
            }

            foreach (FilterQuery andFilterQuery in andFilterQueries)
            {
                List<FilterQueryData> filterQueryData = andFilterQuery.Data.ToList();
                filterQueryData.Add(coreFilterData);

                alternativeFilterQueries.Add(new FilterQuery(filterQueryData));
            }

            if (filter.OrRestraint == null)
            {
                return alternativeFilterQueries.ToArray();
            }

            return alternativeFilterQueries.Concat(QueryFromFilter(filter.OrRestraint)).ToArray();
        }

        /// <summary>
        /// Filters the <paramref name="data"/> according to the <paramref name="alternativeFilters"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="alternativeFilters"></param>
        /// <returns>The <paramref name="data"/> objects, that fulfill any of the <paramref name="alternativeFilters"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="data"/> or <paramref name="alternativeFilters"/> 
        /// is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">If the provided key from any filter does not exists on any 
        /// <paramref name="data"/> object, or if any <see cref="FilterQueryData.Comparison"/> is an invalid choice 
        /// for the resulting value. Also, if the expected value from any filter can't be parsed to the 
        /// same <see cref="Type"/> as the retrieved value.</exception>
        /// <exception cref="NotImplementedException">If a <see cref="FilterQueryData.Comparison"/> is used, that has not been 
        /// implemented yet.</exception>
        public static async Task<object[]> ApplyAlternativeFiltersAsync(IEnumerable<object> data, IEnumerable<FilterQuery> alternativeFilters)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (alternativeFilters == null)
            {
                throw new ArgumentNullException(nameof(alternativeFilters));
            }

            List<object> result = new List<object>();
            List<object> remainingData = data.ToList();

            foreach (FilterQuery filter in alternativeFilters)
            {
                (object[] filtered, object[] discarded) = await filter.Apply(remainingData);

                result.AddRange(filtered);
                remainingData = discarded.ToList();

                if (!remainingData.Any())
                {
                    break;
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Applies the <paramref name="resultLayer"/> to the <paramref name="data"/> objects.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="resultLayer"></param>
        /// <returns>The <paramref name="data"/> shortened to the provided <paramref name="resultLayer"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="data"/> or <paramref name="resultLayer"/> is 
        /// <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">If the provided key from any <see cref="Layer"/> does not exists on any 
        /// <paramref name="data"/> object.</exception>
        public static async Task<object[]> ApplyResultLayerAsync(IEnumerable<object> data, Layer resultLayer)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (resultLayer == null)
            {
                throw new ArgumentNullException(nameof(resultLayer));
            }

            if (!data.Any())
            {
                return Array.Empty<object>();
            }

            EndpointQueryBuilder queryBuilder = new EndpointQueryBuilder()
                .AddPart(
                    new EndpointQueryPartBuilder()
                        .WithEndpointName(resultLayer.Key)
                        .Build()
                );

            Layer subLayer = resultLayer.SubLayer;

            while (subLayer != null)
            {
                queryBuilder.AddPart(
                    new EndpointQueryPartBuilder()
                        .WithEndpointName(subLayer.Key)
                        .Build()
                );

                subLayer = subLayer.SubLayer;
            }

            EndpointQuery query = queryBuilder.Build();

            List<object> result = new List<object>();

            foreach (object date in data)
            {
                object appliedLayer;

                try
                {
                    appliedLayer = await QueryUtil.ResolveSubQueryAsync(date, query, QuerySettings.Default);
                }
                catch (QueryResolveException ex)
                {
                    throw new InvalidOperationException($"Unable to apply filter to data, because a provided " +
                        $"key in query {query} does not exist.", ex);
                }

                result.Add(appliedLayer);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Retrieves the value for the given <paramref name="key"/> from the <paramref name="data"/> object.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns>The value of the given <paramref name="key"/> on the <paramref name="data"/> object.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="data"/> or <paramref name="key"/> 
        /// is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="key"/> is empty or whitespace.</exception>
        /// <exception cref="InvalidOperationException">>If the provided <paramref name="key"/> does not exists on the 
        /// <paramref name="data"/> object.</exception>
        public static async Task<object> GetValueAsync(object data, string key)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"{nameof(key)} can't be empty or whitespace.", nameof(key));
            }

            EndpointQueryBuilder queryBuilder = new EndpointQueryBuilder()
                .AddPart(
                    new EndpointQueryPartBuilder()
                        .WithEndpointName(key)
                        .Build()
                );

            EndpointQuery query = queryBuilder.Build();

            object result;

            try
            {
                result = await QueryUtil.ResolveSubQueryAsync(data, query, QuerySettings.Default);
            }
            catch (QueryResolveException ex)
            {
                throw new InvalidOperationException($"Unable to get value of data, because the provided " +
                    $"key {key} does not exist.", ex);
            }

            return result;
        }
    }
}
