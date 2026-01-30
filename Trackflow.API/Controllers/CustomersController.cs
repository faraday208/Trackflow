using Microsoft.AspNetCore.Mvc;
using Trackflow.Application.GS1;
using Trackflow.Application.Services;
using Trackflow.Shared.DTOs;

namespace Trackflow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly CustomerService _customerService;
    private readonly GS1Service _gs1Service;

    public CustomersController(CustomerService customerService, GS1Service gs1Service)
    {
        _customerService = customerService;
        _gs1Service = gs1Service;
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> GetAll()
    {
        var customers = await _customerService.GetAllAsync();
        return Ok(customers);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
            return NotFound();

        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(CreateCustomerDto dto)
    {
        var customer = await _customerService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _customerService.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> Update(Guid id, UpdateCustomerDto dto)
    {
        var customer = await _customerService.UpdateAsync(id, dto);
        if (customer == null)
            return NotFound();

        return Ok(customer);
    }

    [HttpGet("generate-gln")]
    public ActionResult<object> GenerateGLN()
    {
        var gln = _gs1Service.GenerateGLN();
        return Ok(new { gln });
    }
}
