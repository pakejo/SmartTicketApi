using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using SmartContracts.Contracts.SmarTicket;
using SmartContracts.Contracts.SmarTicket.ContractDefinition;
using SmartTicketApi.Data.DTO;
using SmartTicketApi.Data.Repository;
using SmartTicketApi.Models;
using SmartTicketApi.Utilities;
using Event = SmartTicketApi.Models.Event;

namespace SmartTicketApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(UnauthorizedResult))]
    public class SalesController : ControllerBase
    {
        private readonly ISaleRepository saleRepository;
        private readonly IEventRepository eventRepository;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        private readonly UserManager<ApplicationUser> userManager;

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="saleRepository"></param>
        /// <param name="eventRepository"></param>
        /// <param name="mapper"></param>
        /// <param name="configuration"></param>
        /// <param name="userManager"></param>
        public SalesController(ISaleRepository saleRepository,
                               IEventRepository eventRepository,
                               IMapper mapper,
                               IConfiguration configuration,
                               UserManager<ApplicationUser> userManager)
        {
            this.saleRepository = saleRepository;
            this.eventRepository = eventRepository;
            this.mapper = mapper;
            this.configuration = configuration;
            this.userManager = userManager;
        }

        /// <summary>
        /// Gets all stored sales
        /// </summary>
        /// <returns>Sales list</returns>
        [HttpGet(Name = "GetSales")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SaleDto))]
        public async Task<ActionResult<List<SaleDto>>> GetSales()
        {
            return mapper.Map<List<SaleDto>>(await saleRepository.GetAll());
        }

        /// <summary>
        /// Get a sale by its id
        /// </summary>
        /// <param name="id">Sale id</param>
        /// <returns>Sale info</returns>
        [HttpGet("{id}", Name = "GetSale")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SaleDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundResult))]
        public async Task<ActionResult<SaleDto>> GetSale(string id)
        {
            bool saleExists = await saleRepository.Exists(e => e.Id == id);

            return !saleExists 
                ? (ActionResult<SaleDto>)NotFound() 
                : (ActionResult<SaleDto>)Ok(mapper.Map<SaleDto>(await saleRepository.GetById(id)));
        }

        /// <summary>
        /// Creates a new sale
        /// </summary>
        /// <remarks>
        /// ### Warning: this method makes a transaction to the blockchain
        /// </remarks>
        /// <param name="saleCreationDto">Sale creation data</param>
        /// <returns>New created sale</returns>
        /// <exception cref="CustomException"></exception>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SaleDto))]
        public async Task<ActionResult<Sale>> PostSale(SaleCreationDto saleCreationDto)
        {
            ApplicationUser? applicationUser = await userManager.GetUserAsync(HttpContext.User) ?? throw new CustomException("Could not retreive current user");
            Event? @event = await eventRepository.GetById(saleCreationDto.EventId) ?? throw new CustomException($"Could not found event with id {saleCreationDto.EventId}");
            Web3 web3 = new(new Account(saleCreationDto.CustomerWalletPassword), configuration["InfuraUrl"]);

            SmarTicketService smarTicketService = new(web3, @event.ContractAddress);

            SafeMintFunction safeMintFunction = new() { AmountToSend = Web3.Convert.ToWei(@event.TicketPrice) };

            _ = int.TryParse(await smarTicketService.SafeMintRequestAsync(safeMintFunction), out int tokenId);

            Sale sale = new()
            {
                CreationDate = DateTime.Now,
                UserId = applicationUser.Id,
                User = applicationUser,
                EventId = @event.Id,
                Event = @event,
                Token = tokenId
            };

            saleRepository.Create(sale);
            await saleRepository.Save();
            return CreatedAtRoute("GetSales", new { id = sale.Id }, mapper.Map<SaleDto>(sale));
        }

        /// <summary>
        /// Checks the current user has a ticket of the given event
        /// </summary>
        /// <remarks>
        /// ### Warning: this method makes a transaction to the blockchain
        /// </remarks>
        /// <param name="id">Sale ID</param>
        /// <param name="eventId">Event ID</param>
        /// <param name="userWalletPassword">User wallet password</param>
        /// <returns>True if the current user has a ticket of the given event, false otherwise</returns>
        [HttpPost("{id}/check/{eventId}", Name = "Check")]
        [Authorize(AuthenticationSchemes = "Bearer", Policy = "StaffUser")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(bool))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<bool>> Check(string id, string eventId, [FromBody] string userWalletPassword)
        {
            Event? @event = await eventRepository.GetById(eventId);

            if (@event is null)
            {
                return BadRequest($"Event {eventId} does not exists");
            }

            ApplicationUser? user = await userManager.GetUserAsync(HttpContext.User);

            if (user is null)
            {
                return BadRequest("Could not retreive current user");
            }

            Sale? sale = await saleRepository.GetById(id);

            if (sale is null)
            {
                return BadRequest("Could not retreive current sale");
            }

            Web3 web3 = new(new Account(userWalletPassword), configuration["InfuraUrl"]);
            SmarTicketService smarTicketService = new(web3, @event.ContractAddress);
            string tokenOwnerWallet = await smarTicketService.OwnerOfQueryAsync(sale.Token);

            return Ok(tokenOwnerWallet == user.WalletAddress);
        }
    }
}
