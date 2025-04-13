using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace ADAM.Bot;

public class MessageSender
{
    private readonly IBotFrameworkHttpAdapter _adapter;
    private readonly ITurnContext _turnContext;

    public MessageSender(IBotFrameworkHttpAdapter adapter, ITurnContext turnContext)
    {
        _adapter = adapter;
        _turnContext = turnContext;
    }

    // Send a message to a specific user by their ID
    public async Task SendMessageToUserAsync(string userId, string message,
        CancellationToken cancellationToken = default)
    {
        // TODO: Initiate OR continue in a DM
    }
}