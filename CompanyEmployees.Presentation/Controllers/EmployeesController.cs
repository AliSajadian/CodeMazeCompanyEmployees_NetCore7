using Microsoft.AspNetCore.Mvc; 
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;

using Service.Contracts;
using Shared.DTO;
using CompanyEmployees.Presentation.Filters.ActionFilters;
using Shared.RequestFeatures;
using System.Text.Json;
using Entities.LinkModels;

namespace CompanyEmployees.Presentation.Controllers;


// [ApiVersion("1.0")]
[Route("api/companies/{companyId}/employees")] 
[ApiController] 
public class EmployeesController : ControllerBase 
{ 
    private readonly IServiceManager _service; 
    public EmployeesController(IServiceManager service) => 
        _service = service;

    /// <summary> 
    /// Gets the list of employees for specified company 
    /// </summary> 
    /// <returns>the list of employees for specified company</returns>
    [HttpGet] 
    [ServiceFilter(typeof(ValidateMediaTypeAttribute))]
    public async Task<IActionResult> GetEmployeesForCompany(Guid companyId, 
        [FromQuery] EmployeeParameters employeeParameters) 
    { 
        var linkParams = new LinkParameters(employeeParameters, HttpContext); 
        var result = await _service.EmployeeService.GetEmployeesAsync(companyId, linkParams, trackChanges: false); 
        Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(result.metaData));
        
        return result.linkResponse.HasLinks ? 
            Ok(result.linkResponse.LinkedEntities) : 
            Ok(result.linkResponse.ShapedEntities);
    }

    /// <summary> 
    /// Gets an employee for specified company 
    /// </summary> 
    /// <returns>An employee for specified company</returns>
    [HttpGet("{id:guid}", Name = "GetEmployeeForCompany")]
    public async Task<IActionResult> GetEmployeeForCompany(Guid companyId, Guid id) 
    { 
        var employee = await _service.EmployeeService.GetEmployeeAsync(companyId, id, trackChanges: false); 
        return Ok(employee);
    }

    /// <summary> 
    /// Create an employee for specified company 
    /// </summary> 
    /// <returns>An created employee for specified company</returns>
    [HttpPost] 
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDto employee) 
    { 
        var employeeToReturn = await _service.EmployeeService
                                       .CreateEmployeeForCompanyAsync(companyId, employee, trackChanges: false); 
        
        return CreatedAtRoute("GetEmployeeForCompany", 
                              new { companyId, id = employeeToReturn.Id }, 
                              employeeToReturn); 
    }
    
    /// <summary> 
    /// Delete an employee for specified company 
    /// </summary> 
    /// <returns>Nothing</returns>
    [HttpDelete("{id:guid}")] 
    public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid id) 
    { 
        await _service.EmployeeService.DeleteEmployeeForCompanyAsync(companyId, id, trackChanges: false); 
        return NoContent();
    }

    /// <summary> 
    /// Updates a employee 
    /// </summary> 
    /// <param name="companyId"></param>
    /// <param name="id"></param>
    /// <param name="employee"></param>
    /// <returns>An updated employee</returns> 
    /// <response code="200">Returns the updated item</response> 
    /// <response code="400">If the item is null</response> 
    /// <response code="422">If the model is invalid</response> 
    [HttpPut("{id:guid}")] 
    [ProducesResponseType(200)]
    [ProducesResponseType(400)] 
    [ProducesResponseType(422)]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] EmployeeForUpdateDto employee) 
    { 
        await _service.EmployeeService.UpdateEmployeeForCompanyAsync(companyId, id, employee, 
            compTrackChanges: false, empTrackChanges: true); 
            
        return NoContent(); 
    }

    /// <summary> 
    /// Updates a employee 
    /// </summary> 
    /// <param name="companyId"></param>
    /// <param name="id"></param>
    /// <param name="patchDoc"></param>
    /// <returns>An updated employee</returns> 
    /// <response code="200">Returns the updated item</response> 
    /// <response code="400">If the item is null</response> 
    /// <response code="422">If the model is invalid</response> 
    [HttpPatch("{id:guid}")] 
    [ProducesResponseType(200)]
    [ProducesResponseType(400)] 
    [ProducesResponseType(422)]
    public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(Guid companyId, Guid id, 
        [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDoc) 
    { 
        if (patchDoc is null)
            return BadRequest("patchDoc object sent from client is null."); 

        var result = await _service.EmployeeService.GetEmployeeForPatchAsync(companyId, id, 
            compTrackChanges: false, empTrackChanges: true); 
        
        patchDoc.ApplyTo(result.employeeToPatch, ModelState); 
        
        TryValidateModel(result.employeeToPatch);

        if (!ModelState.IsValid) 
            return UnprocessableEntity(ModelState);

        await _service.EmployeeService.SaveChangesForPatchAsync(result.employeeToPatch, result.employeeEntity); 
        return NoContent(); 
    }
}