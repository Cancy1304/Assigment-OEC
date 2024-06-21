using MediatR;
using RL.Backend.Models;
using RL.Data.DataModels;
using RL.Data;
using RL.Backend.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace RL.Backend.Commands.Handlers.Users;

public class UserPlanProcedureCommandHandler : IRequestHandler<UserPlanProcedureCommand, ApiResponse<Unit>>
{
    private readonly RLContext _context;

    public UserPlanProcedureCommandHandler(RLContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<Unit>> Handle(UserPlanProcedureCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // validation 
            if (request.PlanId < 1)
                return ApiResponse<Unit>.Fail(new BadRequestException("Invalid PlanId"));
            if (request.ProcedureId < 1)
                return ApiResponse<Unit>.Fail(new BadRequestException("Invalid ProcedureId"));
            if (request.UserIds == null || !request.UserIds.Any() || request.UserIds.Any(id => id < 1))
                return ApiResponse<Unit>.Fail(new BadRequestException("Invalid UserIds"));

            // check planand produre present in the context
            var plan = await _context.Plans
                .Include(p => p.PlanProcedures)
                .FirstOrDefaultAsync(p => p.PlanId == request.PlanId);
            var procedure = await _context.Procedures.FirstOrDefaultAsync(p => p.ProcedureId == request.ProcedureId);
           

            if (plan is null)
                return ApiResponse<Unit>.Fail(new NotFoundException($"PlanId: {request.PlanId} not found"));
            if (procedure is null)
                return ApiResponse<Unit>.Fail(new NotFoundException($"ProcedureId: {request.ProcedureId} not found"));

            // check user ids are present in the context
            var missingUserIds = request.UserIds
                .Where(userId => !_context.Users.Any(u => u.UserId == userId))
                .ToList();

            if (missingUserIds.Any())
                return ApiResponse<Unit>.Fail(new NotFoundException($"UserIds: {string.Join(", ", missingUserIds)} not found"));

            // remove existing entries
            var existingEntries = _context.UserPlanProcedures
            .Where(up => up.PlanId == request.PlanId && up.ProcedureId == request.ProcedureId)
            .ToList();

            _context.UserPlanProcedures.RemoveRange(existingEntries);

            // add entry
            var userPlanProcedures = request?.UserIds?.Select(userId => new UserPlanProcedure
            {
                UserId = userId,
                PlanId = request.PlanId,
                ProcedureId = request.ProcedureId,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            }).ToList();

            await _context.UserPlanProcedures.AddRangeAsync(userPlanProcedures, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<Unit>.Succeed(Unit.Value);
        }
        catch (Exception ex)
        {
            return ApiResponse<Unit>.Fail(ex);
        }
    }
}