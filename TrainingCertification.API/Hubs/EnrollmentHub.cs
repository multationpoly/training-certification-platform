using Microsoft.AspNetCore.SignalR;

namespace TrainingCertification.API.Hubs;

public class EnrollmentHub : Hub
{
    public Task JoinSessionGroup(int sessionId) => Groups.AddToGroupAsync(Context.ConnectionId, $"session-{sessionId}");
    public Task LeaveSessionGroup(int sessionId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session-{sessionId}");

    public static Task UpdateEnrollmentCount(IHubContext<EnrollmentHub> hub, int sessionId, int remainingSpots)
    {
        return hub.Clients.Group($"session-{sessionId}").SendAsync("UpdateEnrollmentCount", sessionId, remainingSpots);
    }
}
