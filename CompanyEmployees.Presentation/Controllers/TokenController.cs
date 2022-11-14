using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using Service.Contracts;
using Shared.DTO;
using CompanyEmployees.Presentation.Filters.ActionFilters;

namespace CompanyEmployees.Presentation.Controllers;


[Route("api/token")] 
[ApiController] 
public class TokenController : ControllerBase 
{ 
    private readonly IServiceManager _service; 
    public TokenController(IServiceManager service) => _service = service; 

    [HttpPost("refresh")] 
    [ServiceFilter(typeof(ValidationFilterAttribute))]
     public async Task<IActionResult> Refresh([FromBody]TokenDto tokenDto) 
     { 
        var tokenDtoToReturn = await _service.AuthenticationService.RefreshToken(tokenDto); 
        
        return Ok(tokenDtoToReturn); 
    }
}