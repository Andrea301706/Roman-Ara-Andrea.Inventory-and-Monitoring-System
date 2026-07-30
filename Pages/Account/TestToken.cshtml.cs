using Microsoft.AspNetCore.Mvc.RazorPages;
using Roman_Ara_Andrea.Inventory_and_Monitoring_System.Helpers;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Pages;

public class TestTokenModel : PageModel
{
    private readonly InviteTokenService _inviteTokenService;

    public string Token { get; set; } = "";

    public TestTokenModel(InviteTokenService inviteTokenService)
    {
        _inviteTokenService = inviteTokenService;
    }

    public void OnGet()
    {
        // Change 1 to your real UserId
        Token = _inviteTokenService.CreateInviteToken(1);
    }
}