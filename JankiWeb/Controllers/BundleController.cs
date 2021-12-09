using JankiTransfer.DTO;
using JankiWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JankiWeb.Controllers
{
    [ApiController]
    [Route("bundle")]
    [Authorize]
    public class BundleController : ControllerBase
    {
        private readonly IBundleManagerService bundleManager;

        public BundleController(IBundleManagerService bundleManager)
        {
            this.bundleManager = bundleManager;
        }

        [Route("decks")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeckTreeModel>>> GetAllDecks()
        {
            return Ok(await bundleManager.GetAllDecks(HttpContext.User.GetBundleId()));
        }

        [Route("bundles")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BundleModel>>> GetPublicBundles()
        {
            return Ok(await bundleManager.GetPublicBundles());
        }

        [Route("publish")]
        [HttpPost]
        public async Task<ActionResult> PublishBundle(PublishData publishData)
        {
            await bundleManager.PublishBundle(publishData.DeckIds, publishData.Name);
            return NoContent();
        }

        [Route("import")]
        [HttpPost]
        public async Task<ActionResult> ImportBundle(ImportData bundle)
        {
            await bundleManager.ImportBundle(bundle.Id, HttpContext.User.GetBundleId());
            return NoContent();
        }
    }
}