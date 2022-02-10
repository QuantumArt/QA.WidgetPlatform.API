using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QA.DotNetCore.Engine.Abstractions;
using QA.DotNetCore.Engine.Abstractions.Targeting;
using QA.DotNetCore.Engine.Abstractions.Wildcard;

namespace QA.WidgetPlatform.Api
{
    /// <summary>
    /// Remove after update to QA.DotNetCore.Engine.QpData.Configuration to Version="1.5.15-beta7
    /// </summary>
    public static class RemoveMe
    {

        /// <summary>
        /// Создание нового фильтра из переданных
        /// </summary>
        /// <param name="filters"></param>
        /// <returns>Новый фильтр || null</returns>
        public static ITargetingFilter Combine(this IEnumerable<ITargetingFilter> filters)
        {
            ITargetingFilter result = null;

            foreach (var filter in filters.Where(x => x != null))
            {
                if (result == null)
                {
                    result = filter;
                    continue;
                }

                result = result.AddFilter(filter);
            }

            return result;
        }

        /// <summary>
        /// Получить коллекцию нод по фильтрам
        /// </summary>
        /// <param name="host">Фильтр домена</param>
        /// <param name="startPageFilter">Фильтр определения стартовой страницы</param>
        /// <param name="nodeFilter">Фильтр для плоской структуры нод</param>
        /// <returns>Коллекция нод || null</returns>
        public static T[] GetNodes<T>(
            AbstractItemStorage storage,
            string host, ITargetingFilter startPageFilter = null,
            ITargetingFilter nodeFilter = null)
            where T : class, IAbstractItem
        {
            var pattern = GetBindingPattern(storage.Root, host, startPageFilter);
            var startPage = pattern != null
                ? Value<Dictionary<string, IStartPage>>(storage, "_startPageByDnsPatternMappings")[pattern]
                : null;
            
            if (startPage == null)
                return null;
            
            var flatten = Flatten<T>(startPage);
            return flatten.Pipe(nodeFilter).ToArray();
        }
        
        private static T Value<T>(object obj, string name)
        {
            name ??= "_startPageByDnsPatternMappings";
            var field = obj.GetType().GetField(name,BindingFlags.NonPublic| BindingFlags.Instance);
            var value = field.GetValue(obj);
            return (T) value;
        }

        private static IEnumerable<T> Flatten<T>(IAbstractItem item)
            where T : class, IAbstractItem
        {
            if (item is T targetItem)
                yield return targetItem;

            var children = item.GetChildren();

            if (children == null)
                yield break;

            foreach (var child in children)
            {
                foreach (var childItem in Flatten<T>(child))
                {
                    yield return childItem;
                }
            }
        }


        private static string GetBindingPattern(IAbstractItem root, string host, ITargetingFilter filter)
        {
            var bindings = root
                .GetChildren(filter)
                .OfType<IStartPage>()
                .SelectMany(startPage => startPage.GetDNSBindings());

            var matcher = new WildcardMatcher(WildcardMatchingOption.FullMatch, bindings);
            return matcher.MatchLongest(host);
        }
    }
}