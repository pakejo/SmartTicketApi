using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SmartTicketApi.Data.DTO;
using SmartTicketApi.Models;
using SmartTicketApi.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartTicketApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IConfiguration configuration;
        private readonly IMapper mapper;

        public AccountsController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, IMapper mapper)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
            this.mapper = mapper;
        }

        /// <summary>
        /// Registers a new user in the system
        /// </summary>
        /// <param name="userCredentials">User credentials</param>
        /// <returns>User token</returns>
        [HttpPost("Register", Name = "SignIn")]
        public async Task<ActionResult<AuthenticationResponseDto>> SignInUser(UserCreationDto userCredentials)
        {
            ApplicationUser user = new()
            {
                UserName = userCredentials.Email,
                Email = userCredentials.Email,
                WalletAddress = userCredentials.WalletAddress
            };

            IdentityResult identityResult = await userManager.CreateAsync(user, userCredentials.Password);

            return identityResult.Succeeded
                ? (ActionResult<AuthenticationResponseDto>)await ConstruirToken(mapper.Map<UserCredentialsDto>(userCredentials))
                : (ActionResult<AuthenticationResponseDto>)BadRequest(identityResult.Errors);
        }

        /// <summary>
        /// Logs a user in the system
        /// </summary>
        /// <param name="userCredentials"></param>
        /// <returns>User token</returns>
        [HttpPost("Login", Name = "LogIn")]
        public async Task<ActionResult<AuthenticationResponseDto>> LogInUser(UserCredentialsDto userCredentials)
        {
            Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.PasswordSignInAsync(userCredentials.Email,
                                                                          userCredentials.Password,
                                                                          isPersistent: false,
                                                                          lockoutOnFailure: false);

            return result.Succeeded
                ? await ConstruirToken(userCredentials)
                : BadRequest("Credentials are invalid");
        }

        /// <summary>
        /// Converts a common user to a promoter
        /// </summary>
        /// <param name="email">User email</param>
        /// <returns></returns>
        [HttpPost("ToPromoter", Name = "toPromoter")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> ConvertUserToPromoter(string email)
        {
            try
            {
                await AddClaim(email, "IsPromoter");
            }
            catch (CustomException ex)
            {

                return BadRequest(ex.Message);
            }

            return NoContent();
        }

        /// <summary>
        /// Converts a user to staff
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost("ToStaff", Name = "toStaff")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> ConvertUserToStaff(string email)
        {
            try
            {
                await AddClaim(email, "IsStaff");
            }
            catch (CustomException ex)
            {

                return BadRequest(ex.Message);
            }

            return NoContent();
        }

        /// <summary>
        /// Renews a user token
        /// </summary>
        /// <returns></returns>
        [HttpGet("RenewToken", Name = "RenewToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthenticationResponseDto>> RenovarToken()
        {
            Claim EmailClaim = HttpContext.User.Claims.First(claim => claim.Type == "email");

            UserCredentialsDto userCredentials = new()
            {
                Email = EmailClaim.Value
            };

            return await ConstruirToken(userCredentials);
        }

        #region HELPERS

        private async Task AddClaim(string email, string claimName)
        {
            ApplicationUser? usuario = await userManager.FindByEmailAsync(email: email) ?? throw new CustomException($"Email not found while converting {email} to {claimName}");
            _ = await userManager.AddClaimAsync(usuario, new Claim(claimName, "1"));
        }

        private async Task<AuthenticationResponseDto> ConstruirToken(UserCredentialsDto userCredentials)
        {
            List<Claim> claims = new()
            {
                new Claim("email", userCredentials.Email)
            };

            ApplicationUser? user = await userManager.FindByNameAsync(userCredentials.Email) ?? throw new CustomException($"Could not find user with email {userCredentials.Email}");
            IList<Claim> claimsDb = await userManager.GetClaimsAsync(user);
            claims.AddRange(claimsDb);

            string? jwtKey = configuration["JwtKey"] ?? throw new CustomException("JWT key not found");

            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(jwtKey));
            SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);
            DateTime expiracion = DateTime.UtcNow.AddHours(1);

            JwtSecurityToken securityToken = new(claims: claims, expires: expiracion, signingCredentials: creds);

            return new AuthenticationResponseDto()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                ExpirationDate = expiracion,
            };
        }

        #endregion

    }
}
