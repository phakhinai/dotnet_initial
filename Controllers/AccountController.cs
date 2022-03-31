using System.Net;
using dotnet_hero.DTOs.Account;
using dotnet_hero.Entities;
using dotnet_hero.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_hero.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService accountService;
        public AccountController(IAccountService accountService) => this.accountService = accountService;

        [HttpPost("[action]")]
        public async Task<ActionResult> Register(RegisterRequest registerRequest)
        {
            var account = registerRequest.Adapt<Account>();
            await accountService.Register(account);
            return StatusCode((int)HttpStatusCode.Created);
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> Login(LoginRequest loginRequest)
        {
            var account = await accountService.Login(loginRequest.Username, loginRequest.Password);
            if (account == null)
            {
                return Unauthorized();
            }
            return Ok(new
            {
                token = accountService.GenerateToken(account)
            });
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> Info()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (accessToken == null)
            {
                return Unauthorized();
            }

            var account = accountService.GetInfo(accessToken);
            return Ok(new
            {
                username = account.Username,
                role = account.Role.Name
            });
        }

    }
}