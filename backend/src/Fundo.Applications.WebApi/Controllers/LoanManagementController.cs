using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Fundo.Applications.Apllication.Dtos;



using Fundo.Applications.Application.UseCases.LoansOperations.Commands;
using Fundo.Applications.Application.UseCases.LoansOperations.Queries;
using Fundo.Applications.Application.UseCases.LoansOperations.Queries.GetLoans;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Fundo.Applications.WebApi.Controllers
{
    [Route("api/loans")]
    [ApiController]
    [Authorize]
    public class LoanManagementController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LoanManagementController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllLoans([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _mediator.Send(new GetAllLoansQuery(new PaginationRequestDto(page, pageSize)));
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        
        [HttpGet("my")]
        public async Task<IActionResult> GetMyLoans([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _mediator.Send(new GetLoansByIdQuery(new PaginationRequestDto(page, pageSize)));
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLoanById(int id)
        {
            var result = await _mediator.Send(new GetLoanByIdQuery(id));
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateLoan([FromBody] LoanRequestDto requestDto)
        {
            var result = await _mediator.Send(new CreateLoanCommand(requestDto));
            
            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(GetLoanById), new { id = result.Data.Id }, result);
            }
            return BadRequest(result);
        }
        
        [HttpPost("{id}/payment")]
        public async Task<IActionResult> MakePayment([FromRoute] int id, [FromBody] LoanPaymentRequestDto requestDto)
        {
            requestDto = requestDto with { LoanId = id };
            var result = await _mediator.Send(new CreateLoanPaymentCommand(requestDto));
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}