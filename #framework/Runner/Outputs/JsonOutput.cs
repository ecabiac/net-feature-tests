﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using FeatureTests.Shared.ResultData;
using Newtonsoft.Json;
using NuGet;
using FeatureTests.Runner.Sources;
using FeatureTests.Shared;

namespace FeatureTests.Runner.Outputs {
    public class JsonOutput : IResultOutput {
        public void Write(DirectoryInfo outputDirectory, IReadOnlyCollection<ResultForAssembly> results, bool keepUpdatingIfTemplatesChange = false) {
            foreach (var result in results) {
                Write(outputDirectory, result);
            }
        }

        private void Write(DirectoryInfo outputDirectory, ResultForAssembly result) {
            var tableList = result.Tables.ToArray();

            var general = tableList.First(t => t.Key == MetadataKeys.GeneralInfoTable);
            var netFxVersions = tableList.First(t => t.Key == MetadataKeys.NetFxSupportTable);
            var data = tableList[0].Libraries.Select(l => new {
                name = l.Name,
                url = general[l, MetadataKeys.UrlFeature].DisplayUri,
                version = general[l, MetadataKeys.VersionFeature].DisplayValue,
                supports = GetNetFxVersions(l, netFxVersions),
                features = GetAllFeatureData(l, tableList)
            });

            var json = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore
            });
            File.WriteAllText(Path.Combine(outputDirectory.FullName, result.OutputNamePrefix + ".json"), json);
        }

        private string[] GetNetFxVersions(ILibrary library, FeatureTable table) {
            return table.Features.Where(f => table[library, f].State == FeatureState.Success)
                                 .Select(f => (IGrouping<string, FrameworkName>)f.Key)
                                 .Select(g => VersionUtility.GetShortFrameworkName(g.First()))
                                 .ToArray();
        }

        private IDictionary<string, object> GetAllFeatureData(ILibrary library, IEnumerable<FeatureTable> tables) {
            var featuresByName = tables.Where(t => t.Key != MetadataKeys.GeneralInfoTable && t.Key != MetadataKeys.NetFxSupportTable)
                                       .SelectMany(t => t.Features.Select(f => this.GetSingleFeatureData(f, t[library, f])))
                                       .GroupBy(p => p.Key)
                                       .ToArray();

            var duplicate = featuresByName.FirstOrDefault(g => g.Count() > 1);
            if (duplicate != null)
                throw new Exception("Feature name " + duplicate.Key + " was used more than once.");

            return featuresByName.ToDictionary(p => p.Key, p => p.Single().Value);
        }

        private KeyValuePair<string, object> GetSingleFeatureData(Feature feature, FeatureCell cell) {
            var key = this.GetFeatureName(feature);
            var value = new {
                result = cell.State.ToString().ToLowerInvariant(),
                comment = cell.Details,
                error = cell.RawError
            };
            
            return new KeyValuePair<string, object>(key, value);
        }

        private string GetFeatureName(Feature feature) {
            // HACK :)
            var method = feature.Key as MethodInfo;
            return method != null ? method.Name : feature.Name;
        }

        #region IDisposable Members

        public void Dispose() {
        }

        #endregion
    }
}
