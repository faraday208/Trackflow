using Microsoft.AspNetCore.Mvc;
using Trackflow.Application.Services;
using Trackflow.Shared.DTOs;
using Trackflow.Shared.Enums;

namespace Trackflow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkOrdersController : ControllerBase
{
    private readonly WorkOrderService _workOrderService;
    private readonly AggregationService _aggregationService;

    public WorkOrdersController(WorkOrderService workOrderService, AggregationService aggregationService)
    {
        _workOrderService = workOrderService;
        _aggregationService = aggregationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<WorkOrderDto>>> GetAll()
    {
        var workOrders = await _workOrderService.GetAllAsync();
        return Ok(workOrders);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkOrderDto>> GetById(Guid id)
    {
        var workOrder = await _workOrderService.GetByIdAsync(id);
        if (workOrder == null)
            return NotFound();

        return Ok(workOrder);
    }

    [HttpGet("{id:guid}/detail")]
    public async Task<ActionResult<WorkOrderDetailDto>> GetDetail(Guid id)
    {
        var detail = await _workOrderService.GetDetailAsync(id);
        if (detail == null)
            return NotFound();

        return Ok(detail);
    }

    [HttpPost]
    public async Task<ActionResult<WorkOrderDto>> Create(CreateWorkOrderDto dto)
    {
        try
        {
            var workOrder = await _workOrderService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = workOrder.Id }, workOrder);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] WorkOrderStatus status)
    {
        var result = await _workOrderService.UpdateStatusAsync(id, status);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPost("{id:guid}/aggregate")]
    public async Task<ActionResult<AggregationResultDto>> Aggregate(Guid id)
    {
        try
        {
            var result = await _aggregationService.AggregateWorkOrderAsync(id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/boxes")]
    public async Task<ActionResult<PackingUnitDto>> CreateBox(Guid id, [FromBody] List<Guid> serialNumberIds)
    {
        try
        {
            var box = await _aggregationService.CreateBoxAsync(id, serialNumberIds);
            return Ok(box);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/pallets")]
    public async Task<ActionResult<PackingUnitDto>> CreatePallet(Guid id, [FromBody] List<Guid> boxIds)
    {
        try
        {
            var pallet = await _aggregationService.CreatePalletAsync(id, boxIds);
            return Ok(pallet);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _workOrderService.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPost("{id:guid}/reset")]
    public async Task<IActionResult> Reset(Guid id)
    {
        var result = await _workOrderService.ResetWorkOrderAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
