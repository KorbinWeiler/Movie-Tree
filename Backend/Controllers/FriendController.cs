using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FriendController(AppDbContext db) : ControllerBase
{
    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    private FriendshipDto ToDto(Friendship f)
    {
        var userId = CurrentUserId;
        var isRequester = f.RequesterId == userId;
        var other = isRequester ? f.Addressee : f.Requester;
        return new FriendshipDto(f.Id, other.Id, other.UserName!, other.ProfilePictureUrl, f.Status, f.CreatedAt);
    }

    // GET /api/friend — list accepted friends
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var userId = CurrentUserId;
        var friends = await db.Friendships
            .Include(f => f.Requester)
            .Include(f => f.Addressee)
            .Where(f => f.Status == FriendshipStatus.Accepted &&
                        (f.RequesterId == userId || f.AddresseeId == userId))
            .ToListAsync();

        return Ok(friends.Select(ToDto));
    }

    // GET /api/friend/requests — incoming pending requests
    [HttpGet("requests")]
    public async Task<IActionResult> PendingRequests()
    {
        var userId = CurrentUserId;
        var requests = await db.Friendships
            .Include(f => f.Requester)
            .Include(f => f.Addressee)
            .Where(f => f.Status == FriendshipStatus.Pending && f.AddresseeId == userId)
            .ToListAsync();

        return Ok(requests.Select(ToDto));
    }

    // POST /api/friend/request — send a friend request
    [HttpPost("request")]
    public async Task<IActionResult> SendRequest([FromBody] FriendRequestDto req)
    {
        var userId = CurrentUserId;

        if (userId == req.AddresseeId)
            return BadRequest("Cannot send a friend request to yourself.");

        var addresseeExists = await db.Users.AnyAsync(u => u.Id == req.AddresseeId);
        if (!addresseeExists) return NotFound("User not found.");

        var existing = await db.Friendships.AnyAsync(f =>
            (f.RequesterId == userId && f.AddresseeId == req.AddresseeId) ||
            (f.RequesterId == req.AddresseeId && f.AddresseeId == userId));

        if (existing) return Conflict("A friendship or pending request already exists.");

        db.Friendships.Add(new Friendship
        {
            RequesterId = userId,
            AddresseeId = req.AddresseeId,
        });

        await db.SaveChangesAsync();
        return Ok();
    }

    // PUT /api/friend/request/{id}/accept
    [HttpPut("request/{id:int}/accept")]
    public async Task<IActionResult> Accept(int id)
    {
        var friendship = await db.Friendships
            .Include(f => f.Requester)
            .Include(f => f.Addressee)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (friendship is null) return NotFound();
        if (friendship.AddresseeId != CurrentUserId) return Forbid();
        if (friendship.Status != FriendshipStatus.Pending) return BadRequest("Request is no longer pending.");

        friendship.Status = FriendshipStatus.Accepted;
        friendship.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return Ok(ToDto(friendship));
    }

    // PUT /api/friend/request/{id}/decline
    [HttpPut("request/{id:int}/decline")]
    public async Task<IActionResult> Decline(int id)
    {
        var friendship = await db.Friendships
            .FirstOrDefaultAsync(f => f.Id == id);

        if (friendship is null) return NotFound();
        if (friendship.AddresseeId != CurrentUserId) return Forbid();
        if (friendship.Status != FriendshipStatus.Pending) return BadRequest("Request is no longer pending.");

        friendship.Status = FriendshipStatus.Declined;
        friendship.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/friend/{id} — remove an accepted friend
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remove(int id)
    {
        var userId = CurrentUserId;
        var friendship = await db.Friendships.FirstOrDefaultAsync(f =>
            f.Id == id && (f.RequesterId == userId || f.AddresseeId == userId));

        if (friendship is null) return NotFound();

        db.Friendships.Remove(friendship);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
