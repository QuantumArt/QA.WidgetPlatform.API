using Microsoft.AspNetCore.Mvc;
using QA.WidgetPlatform.Api.Models;

namespace QA.WidgetPlatform.Api.Services.Abstract
{
    public interface ISiteStructureService
    {
        void Warmup();

        /// <summary>
        /// Получение структуры страниц сайта
        /// </summary>
        /// <param name="dnsName">Доменное имя сайта</param>
        /// <param name="fields">Поля деталей к выдаче. Если пусто, то детали выдаваться не будут</param>
        /// <param name="deep">Глубина страуктуры, где 0 - это корневой элемент</param>
        /// <param name="fillDefinitionDetails">Заполнять дополнительные поля из дефинишена</param>
        /// <returns></returns>
        SiteNode Structure(string dnsName,
            [FromQuery] string[] fields,
            int? deep, bool fillDefinitionDetails = false);

        /// <summary>
        /// Получение массива нод, удовлетворяющих переданным фильтрам
        /// </summary>
        /// <param name="dnsName">Доменное имя сайта</param>
        /// <param name="fields">Поля деталей к выдаче. Если пусто, то будут выведены все детали</param>
        /// <returns></returns>
        IEnumerable<SimpleSiteNodeDetails> Details(string dnsName,
            string[] fields);

        /// <summary>
        /// Получение детальной информации по странице или виджету
        /// </summary>
        /// <param name="nodeId">id страницы или виджета</param>
        /// <returns></returns>
        SiteNodeDetails Node(int nodeId);

        /// <summary>
        /// Получение виджетов для страницы, сгруппированных по зонам
        /// </summary>
        /// <param name="abstractItemId">id страницы или виджета</param>
        /// <param name="zones">Список виджетных зон (если не передавать, то поиск виджетов не будет производиться для рекурсивных и глобальных зон)</param>
        /// <param name="fillDefinitionDetails">Заполнять дополнительные поля из дефинишена</param>
        /// <returns></returns>
        IDictionary<string, WidgetDetails[]> WidgetsForNode(int abstractItemId,
            string[] zones, bool fillDefinitionDetails = false);
    }
}