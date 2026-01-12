using System.Net;
using System.Text;

namespace E_Commerce.Core.Shared.Settings;
public class MailData
{
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public ICredentials Password { get; set; } = null!;
    public Encoding Email { get; set; }
    public bool RequiresAuthentication { get; set; }
    public string Username { get; set; } = null!;
}
