using JankiCards.Janki;
using JankiTransfer.DTO;
using JankiWebCards.Janki;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Undersea.Controllers
{
    [ApiController]
    [Route("account")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<JankiUser> userManager;

        public AccountController(UserManager<JankiUser> userManager)
        {
            this.userManager = userManager;
        }

        /*
        curl --request POST \
          --url 'http://localhost:5000/connect/token' \
          --header 'content-type: application/x-www-form-urlencoded' \
          --data grant_type=password \
          --data username=Vince \
          --data password=MyPass1* \
          --data client_id=rop \
          --data client_secret=SuperSecretClientSecret
        */

        [Route("signup")]
        [HttpPost]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Signup(Signup signup)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            JankiUser newUser = new JankiUser()
            {
                UserName = signup.UserName,
                Bundle = new Bundle()
            };

            IdentityResult result = await userManager.CreateAsync(newUser, signup.Password);

            if (result.Succeeded)
                return NoContent();
            else
                return Problem(result.Errors.FirstOrDefault()?.Description, statusCode: StatusCodes.Status400BadRequest);
        }
    }
}