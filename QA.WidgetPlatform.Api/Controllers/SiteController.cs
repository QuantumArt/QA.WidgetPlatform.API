using Microsoft.AspNetCore.Mvc;
using QA.DotNetCore.Engine.Abstractions.Targeting;
using QA.WidgetPlatform.Api.Application;
using QA.WidgetPlatform.Api.Models;
using QA.WidgetPlatform.Api.Services.Abstract;
using System.ComponentModel.DataAnnotations;

namespace QA.WidgetPlatform.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SiteController : ControllerBase
    {
        private readonly ISiteStructureService _siteStructureService;
        private readonly ITargetingContextUpdater _updater;

        public SiteController(ISiteStructureService siteStructureService, ITargetingContextUpdater updater)
        {
            _siteStructureService = siteStructureService;
            _updater = updater;
        }

        [HttpGet("[action]")]
        public IActionResult Warmup()
        {
            _siteStructureService.Warmup();
            return Ok();
        }

        /// <summary>
        /// Получение структуры страниц сайта
        /// </summary>
        /// <param name="dnsName">Доменное имя сайта</param>
        /// <param name="targeting">Словарь значений таргетирования</param>
        /// <param name="fields">Поля деталей к выдаче. Если пусто, то детали выдаваться не будут</param>
        /// <param name="deep">Глубина страуктуры, где 0 - это корневой элемент</param>
        /// <param name="fillDefinitionDetails">Заполнять дополнительные поля из дефинишена</param>
        /// <returns></returns>
        [HttpGet("structure")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<SiteNode> Structure([Required][FromQuery] string dnsName,
            [Bind(Prefix = "t")][FromQuery] CaseInSensitiveDictionary<string> targeting, [FromQuery] string[] fields,
            int? deep, bool fillDefinitionDetails = false)
        {
            await _updater.Update(HttpContext, targeting);
            return _siteStructureService.Structure(dnsName, fields, deep, fillDefinitionDetails);
        }

        /// <summary>
        /// Получение массива нод, удовлетворяющих переданным фильтрам
        /// </summary>
        /// <param name="dnsName">Доменное имя сайта</param>
        /// <param name="targeting">Словарь значений таргетирования</param>
        /// <param name="fields">Поля деталей к выдаче. Если пусто, то будут выведены все детали</param>
        /// <returns></returns>
        [HttpGet("details")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IEnumerable<SimpleSiteNodeDetails>> Details([Required][FromQuery] string dnsName,
            [Bind(Prefix = "t")][FromQuery] CaseInSensitiveDictionary<string> targeting,
            [FromQuery] string[] fields)
        {
            await _updater.Update(HttpContext, targeting);
            return _siteStructureService.Details(dnsName, fields);
        }

        /// <summary>
        /// Получение детальной информации по странице или виджету
        /// </summary>
        /// <param name="nodeId">id страницы или виджета</param>
        /// <returns></returns>
        [HttpGet("node/{nodeId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public SiteNodeDetails Node(int nodeId)
            => _siteStructureService.Node(nodeId);

        /// <summary>
        /// Получение виджетов для страницы или виджета, сгруппированных по зонам
        /// </summary>
        /// <param name="abstractItemId">id страницы или виджета</param>
        /// <param name="targeting">Словарь значений таргетирования</param>
        /// <param name="zones">Список виджетных зон (если не передавать, то поиск виджетов не будет производиться для рекурсивных и глобальных зон)</param>
        /// <param name="fillDefinitionDetails">Заполнять дополнительные поля из дефинишена</param>
        /// <returns></returns>
        [HttpGet("widgets/{abstractItemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IDictionary<string, WidgetDetails[]>> WidgetsForNode(int abstractItemId,
            [Bind(Prefix = "t")][FromQuery] CaseInSensitiveDictionary<string> targeting, [FromQuery] string[] zones,
            bool fillDefinitionDetails = false)
        {
            await _updater.Update(HttpContext, targeting);
            return _siteStructureService.WidgetsForNode(abstractItemId, zones, fillDefinitionDetails);
        }
    }
}