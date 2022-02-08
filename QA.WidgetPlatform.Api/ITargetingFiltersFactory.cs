using System.Collections.Generic;
using QA.DotNetCore.Engine.Abstractions.Targeting;

namespace QA.WidgetPlatform.Api
{
    /// <remarks>
    /// Необходимо назделение на <see cref="StructureFilter"/> и <see cref="FlattenNodesFilter"/> для того, 
    /// чтобы определять стартовую страницу, т.к. она может не попасть в выборку через <see cref="FlattenNodesFilter"/>
    /// </remarks>>
    public interface ITargetingFiltersFactory
    {
        /// <summary>
        /// Фильтр для применения к древовидной структуре (Parent-Children)
        /// </summary>
        /// <param name="targeting"></param>
        /// <returns></returns>
        ITargetingFilter StructureFilter(IDictionary<string, string> targeting);
        
        /// <summary>
        /// Фильтр для применения к плоской коллекции нод
        /// </summary>
        /// <param name="targeting"></param>
        /// <returns></returns>
        ITargetingFilter FlattenNodesFilter(IDictionary<string, string> targeting);
    }
}