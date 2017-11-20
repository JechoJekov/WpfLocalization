using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace WpfLocalization
{
    /// <summary>
    /// Provides some helper methods for localization.
    /// </summary>
    public static class LocalizationHelper
    {
        /// <summary>
        /// Returns the list of available languages.
        /// </summary>
        /// <param name="defaultLanguageCultureName">The culture of the default resources (e.g. "en-US").</param>
        /// <returns></returns>
        /// <remarks>
        /// This method simply examines the names of all sub-directories in the application's directory that are valid culture names and returns
        /// the cultures sorted in alphabetical order.
        /// </remarks>
        public static List<CultureInfo> GetAvailableLanguages(string defaultLanguageCultureName = null)
        {
            return GetAvailableLanguages(defaultLanguageCultureName == null ? null : CultureInfo.GetCultureInfo(defaultLanguageCultureName));
        }

        /// <summary>
        /// Returns the list of available languages.
        /// </summary>
        /// <param name="defaultCulture">The culture of the default resources (e.g. "en-US").</param>
        /// <returns></returns>
        /// <remarks>
        /// This method simply examines the names of all sub-directories in the application's directory that are valid culture names and returns
        /// the cultures sorted in alphabetical order. The default culture is at the top.
        /// </remarks>
        public static List<CultureInfo> GetAvailableLanguages(CultureInfo defaultCulture = null)
        {
            var directoryPath = AppDomain.CurrentDomain.BaseDirectory;
            var subDirectoryNameSet = new HashSet<string>(Directory.GetDirectories(directoryPath).Select(x => Path.GetFileName(x)), StringComparer.OrdinalIgnoreCase);

            var list = CultureInfo
                .GetCultures(CultureTypes.NeutralCultures | CultureTypes.SpecificCultures)
                .Where(x => subDirectoryNameSet.Contains(x.Name))
                .OrderBy(x => x.NativeName)
                .ToList();

            if (defaultCulture != null)
            {
                list.Insert(0, defaultCulture);
            }

            return list;
        }
    }
}
