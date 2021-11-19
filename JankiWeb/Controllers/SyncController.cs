﻿using JankiTransfer.ChangeDetection;
using JankiTransfer.DTO;
using JankiWeb.StartupExtentions;
using JankiWebCards.Janki.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
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
            await changeDetector.ApplyChanges(data, Guid.Parse(HttpContext.User.Claims.First(x => x.Type.Equals(IdentityServerConfig.BundleIdClaim, StringComparison.OrdinalIgnoreCase))?.Value), context);
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}