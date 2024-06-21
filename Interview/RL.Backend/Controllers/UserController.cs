using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using RL.Backend.Commands;
using RL.Backend.Models;
using RL.Data;
using RL.Data.DataModels;

namespace RL.Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly RLContext _context;
    private readonly IMediator _mediator;

    public UsersController(ILogger<UsersController> logger, RLContext context, IMediator mediator)
    {
        _logger = logger;
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mediator = mediator;
    }

    [HttpGet]
    [EnableQuery]
    public IEnumerable<User> Get()
    {
        return _context.Users;
    }

    [HttpPost("/addUsers")]
    public async Task<IActionResult> AddUsersToPlanProcedure(UserModel user)
    {
        var command = new UserPlanProcedureCommand
        {
            PlanId = user.planId,
            ProcedureId = user.procedureId,
            UserIds = user.userIds
        };

        var response = await _mediator.Send(command);
        return response.ToActionResult();
    }

    [HttpGet("{planId}/procedures/{procedureId}/users")]
    [EnableQuery]
    public IEnumerable<User> GetUsersByPlanAndProcedure(int planId, int procedureId)
    {
        var query = from userPlanProcedure in _context.UserPlanProcedures
                    join user in _context.Users on userPlanProcedure.UserId equals user.UserId
                    where userPlanProcedure.PlanId == planId && userPlanProcedure.ProcedureId == procedureId
                    select user;

        return query;
    }
}
