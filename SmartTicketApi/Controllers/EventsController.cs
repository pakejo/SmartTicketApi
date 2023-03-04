using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using SmartContracts.Contracts.SmarTicket;
using SmartContracts.Contracts.SmarTicket.ContractDefinition;
using SmartTicketApi.Data.DTO;
using SmartTicketApi.Data.Repository;
using SmartTicketApi.Models;
using SmartTicketApi.Utilities;
using System.Numerics;

namespace SmartTicketApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(UnauthorizedResult))]
    public class EventsController : ControllerBase
    {
        private readonly IEventRepository repository;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        private readonly UserManager<ApplicationUser> userManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="mapper"></param>
        /// <param name="configuration"></param>
        /// <param name="userManager"></param>
        public EventsController(IEventRepository repository,
                                IMapper mapper,
                                IConfiguration configuration,
                                UserManager<ApplicationUser> userManager)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.configuration = configuration;
            this.userManager = userManager;
        }

        /// <summary>
        /// Gets all events stored in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "GetEvents")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<EventDto>))]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents()
        {
            List<Event> events = await repository.GetAll();

            return Ok(mapper.Map<List<EventDto>>(events));
        }

        /// <summary>
        /// Searchs an event by its id
        /// </summary>
        /// <param name="id">Event id</param>
        /// <returns>Event info if found</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Event))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundResult))]
        public async Task<ActionResult<Event>> GetEvent(string id)
        {
            bool eventExists = await repository.Exists(e => e.Id == id);

            return !eventExists ? (ActionResult<Event>)NotFound() : (ActionResult<Event>)Ok(await repository.GetById(id));
        }

        /// <summary>
        /// Gets all planned event
        /// </summary>
        /// <returns>Event list</returns>
        [HttpGet("Upcoming")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Event>))]
        public async Task<ActionResult<IEnumerable<Event>>> GetFutureEvent()
        {
            return Ok(await repository.GetFutureEvents());
        }

        /// <summary>
        /// Creates a new event
        /// </summary>
        /// <remarks>
        /// ### Warning: this method makes a transaction to the blockchain
        /// ### Warning: only promoter users can use this endpoint
        /// </remarks>
        /// <param name="event">Event info</param>
        /// <returns>Created event info</returns>
        /// <exception cref="CustomException"></exception>
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer", Policy = "PromoterUser")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(EventCreationDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Event>> PostEvent(EventCreationDto @event)
        {
            bool eventExists = await repository.EventWithSameNameExists(@event.Name);

            if (eventExists)
            {
                return BadRequest("A event with the same name already exists");
            }

            ApplicationUser? applicationUser = await userManager.GetUserAsync(HttpContext.User) ?? throw new CustomException("Could not retreive current user");
            string? InfuraUrl = configuration["InfuraUrl"] ?? throw new CustomException("InfuraUrl not found");
            Event newEvent = mapper.Map<Event>(@event);

            Web3 web3 = new(new Account(@event.UserWalletPassword), InfuraUrl);
            SmarTicketDeployment deploymentMessage = new() { Price = Web3.Convert.ToWei(@event.TicketPrice) };

            TransactionReceipt receipt = await SmarTicketService
                .DeployContractAndWaitForReceiptAsync(web3, deploymentMessage);

            newEvent.ContractAddress = receipt.ContractAddress;
            newEvent.Promoter = applicationUser;
            newEvent.PromoterId = applicationUser.Id;

            repository.Create(newEvent);
            await repository.Save();

            EventDto eventDto = mapper.Map<EventDto>(newEvent);
            return CreatedAtRoute("GetEvents", new { id = newEvent.Id }, eventDto);
        }

        /// <summary>
        /// Withdraws all founds in the event contract to the promoter
        /// </summary>
        /// <remarks>
        /// ### Warning: this method makes a transaction to the blockchain
        /// ### Warning: only the user who created the event can use it
        /// </remarks>
        /// <param name="id">Event id</param>
        /// <param name="UserWalletPassword">User wallet password</param>
        /// <returns></returns>
        /// <exception cref="CustomException"></exception>
        [HttpPost("WithdrawFounds/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Policy = "PromoterUser")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TransactionReceipt))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<TransactionReceipt>> WithdrawFounds(string id, [FromBody] string UserWalletPassword)
        {
            bool existsEvent = await repository.Exists(x => x.Id == id);

            if (!existsEvent)
            {
                return BadRequest($"Event with id: {id} does not exist");
            }

            ApplicationUser? user = await userManager.GetUserAsync(HttpContext.User);
            Event? @event = await repository.GetById(id);

            if (user is null || @event is null)
            {
                return BadRequest("Current user or event could not be found");
            }
            else if (@event.Promoter.Id != user.Id)
            {
                return BadRequest("You are not the owner of the event");
            }

            string? InfuraUrl = configuration["InfuraUrl"] ?? throw new CustomException("InfuraUrl not found");
            Web3 web3 = new(new Account(UserWalletPassword), InfuraUrl);

            SmarTicketService smarTicketService = new(web3, @event.ContractAddress);

            return await smarTicketService.WithdrawRequestAndWaitForReceiptAsync();
        }

        /// <summary>
        /// Gets the total balance associated to the given event
        /// </summary>
        /// <remarks>
        /// ### Warning: this method makes a transaction to the blockchain
        /// </remarks>
        /// <param name="id">Event Id</param>
        /// <param name="UserWalletPassword">User wallet password</param>
        /// <returns>Smart contrat total balance</returns>
        /// <exception cref="CustomException"></exception>
        [HttpPost("GetEventBalance/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<EventBalanceResponseDto>> GetEventBalance(string id, [FromBody] string UserWalletPassword)
        {
            bool existsEvent = await repository.Exists(x => x.Id == id);

            if (!existsEvent)
            {
                return BadRequest($"Event with id: {id} does not exist");
            }

            ApplicationUser? user = await userManager.GetUserAsync(HttpContext.User);
            Event? @event = await repository.GetById(id);

            if (user is null || @event is null)
            {
                return BadRequest("Current user or event could not be found");
            }

            string? InfuraUrl = configuration["InfuraUrl"] ?? throw new CustomException("InfuraUrl not found");
            Web3 web3 = new(new Account(UserWalletPassword), InfuraUrl);
            SmarTicketService smarTicketService = new(web3, @event.ContractAddress);
            BigInteger balance = await smarTicketService.ContractBalanceQueryAsync();

            return Ok(new EventBalanceResponseDto()
            {
                Ether = Web3.Convert.FromWei(balance, Nethereum.Util.UnitConversion.EthUnit.Ether),
                Gwei = Web3.Convert.FromWei(balance, Nethereum.Util.UnitConversion.EthUnit.Gwei),
                Mwei = Web3.Convert.FromWei(balance, Nethereum.Util.UnitConversion.EthUnit.Mwei),
            });
        }

        /// <summary>
        /// Updates an event
        /// </summary>
        /// <param name="id">Event id</param>
        /// <param name="event">New value</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PutEvent(int id, Event @event)
        {
            if (id.ToString() != @event.Id)
            {
                return BadRequest("The given Id does not match the event id");
            }

            repository.Update(@event);
            await repository.Save();

            return NoContent();
        }
    }
}
