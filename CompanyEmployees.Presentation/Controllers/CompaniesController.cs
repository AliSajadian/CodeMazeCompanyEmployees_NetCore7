using Microsoft.AspNetCore.Mvc; 
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Authorization;

using Service.Contracts;
using Shared.DTO;
using CompanyEmployees.Presentation.ModelBinders;
using CompanyEmployees.Presentation.Filters.ActionFilters;

namespace CompanyEmployees.Presentation.Controllers;

// [ApiVersion("1.0")]
[Route("api/companies")]
[ApiController] 
[ApiExplorerSettings(GroupName = "v1")]
public class CompaniesController : ControllerBase
{
    private readonly IServiceManager _service; 
    public CompaniesController(IServiceManager service) => 
        _service = service; 

    /// <summary> 
    /// Gets the list of all companies 
    /// </summary> 
    /// <returns>The companies list</returns>
    [HttpGet(Name = "GetCompanies")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> GetCompanies() 
    { 
        var companies = await _service.CompanyService.GetAllCompaniesAsync(trackChanges: false);

        return Ok(companies);
    }

    /// <summary> 
    /// Gets the list of a collection of companies 
    /// </summary> 
    /// <returns>The companies collection</returns>
    [HttpGet("collection/({ids})", Name = "CompanyCollection")] 
    public async Task<IActionResult> GetCompanyCollection (
        [ModelBinder(BinderType = typeof(ArrayModelBinder))]IEnumerable<Guid> ids) 
    { 
        var companies = await _service.CompanyService.GetByIdsAsync(ids, trackChanges: false); 
        return Ok(companies); 
    }

    /// <summary> 
    /// Gets the specified company 
    /// </summary> 
    /// <returns>The company</returns>
    [HttpGet("{id:guid}", Name = "CompanyById")]
    // [ResponseCache(CacheProfileName = "120SecondsDuration")]
    [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = 60)] 
    [HttpCacheValidation(MustRevalidate = false)]
    public async Task<IActionResult> GetCompany(Guid id) 
    { 
        var company = await _service.CompanyService.GetCompanyAsync(id, trackChanges: false);
        return Ok(company); 
    }

    /// <summary> 
    /// Creates a newly created company 
    /// </summary> 
    /// <param name="company"></param>
    /// <returns>A newly created company</returns> 
    /// <response code="201">Returns the newly created item</response> 
    /// <response code="400">If the item is null</response> 
    /// <response code="422">If the model is invalid</response> 
    [HttpPost(Name = "CreateCompany")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)] 
    [ProducesResponseType(422)]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> CreateCompany([FromBody] CompanyForCreationDto company)
    { 
        var createdCompany = await _service.CompanyService.CreateCompanyAsync(company); 

        return CreatedAtRoute("CompanyById", new { id = createdCompany.Id }, createdCompany); 
    }

    /// <summary> 
    /// Creates a collection of created companies 
    /// </summary> 
    /// <param name="companyCollection"></param>
    /// <returns>The collection of created companies</returns> 
    /// <response code="201">Returns collection of created item</response> 
    /// <response code="400">If the item is null</response> 
    /// <response code="422">If the model is invalid</response> 
    [HttpPost("collection")] 
    [ProducesResponseType(201)]
    [ProducesResponseType(400)] 
    [ProducesResponseType(422)]
    public async Task<IActionResult> CreateCompanyCollection (
        [FromBody] IEnumerable<CompanyForCreationDto> companyCollection) 
    { 
        var result = await _service.CompanyService.CreateCompanyCollectionAsync(companyCollection);
        return CreatedAtRoute("CompanyCollection", new { result.ids }, result.companies); 
    }

    /// <summary> 
    /// Delete the specified company 
    /// </summary> 
    /// <param name="id"></param>
    /// <returns>Nothing</returns> 
    /// <response code="200">Nothing</response> 
    /// <response code="400">If the id is null</response> 
    [HttpDelete("{id:guid}")] 
    [ProducesResponseType(200)]
    [ProducesResponseType(400)] 
    public async Task<IActionResult> DeleteCompany(Guid id) 
    { 
        await _service.CompanyService.DeleteCompanyAsync(id, trackChanges: false);
        return NoContent(); 
    }

    /// <summary> 
    /// Updates a company 
    /// </summary> 
    /// <param name="id"></param>
    /// <param name="company"></param>
    /// <returns>An updated company</returns> 
    /// <response code="200">Returns the updated item</response> 
    /// <response code="400">If the item is null</response> 
    /// <response code="422">If the model is invalid</response> 
    [HttpPut("{id:guid}")] 
    [ProducesResponseType(200)]
    [ProducesResponseType(400)] 
    [ProducesResponseType(422)]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] CompanyForUpdateDto company) 
    { 
        await _service.CompanyService.UpdateCompanyAsync(id, company, trackChanges: true);
        return NoContent(); 
    }

    /// <summary> 
    /// Gets the list of all options 
    /// </summary> 
    /// <returns>the list of all options</returns>
    [HttpOptions] public IActionResult GetCompaniesOptions()
    { 
        Response.Headers.Add("Allow", "GET, OPTIONS, POST, PUT, DELETE"); 
        return Ok(); 
    }
}