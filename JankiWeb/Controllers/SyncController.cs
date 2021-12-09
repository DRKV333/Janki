using JankiTransfer.ChangeDetection;
using JankiTransfer.DTO;
using JankiWebCards.Janki.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace JankiWeb.Controllers
{
    [ApiController]
    [Route("sync")]
    [Authorize]
    public class SyncController : ControllerBase
    {
        private readonly JankiWebContext context;
        private readonly ChangeDetector<JankiWebContext> changeDetector;

        public SyncController(JankiWebContext context, ChangeDetector<JankiWebContext> changeDetector)
        {
            this.context = context;
            this.changeDetector = changeDetector;
        }

        [HttpGet]
        public async Task<ActionResult<ChangeData>> GetSync(DateTime since)
        {
            return Ok(await changeDetector.DetectChanges(since, context));
        }

        [HttpPost]
        public async Task<ActionResult> PostSync(ChangeData data)
        {
            await changeDetector.ApplyChanges(data, HttpContext.User.GetBundleId(), context);
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}