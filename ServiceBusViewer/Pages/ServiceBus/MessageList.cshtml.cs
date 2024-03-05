using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ServiceBusViewer.Pages.ServiceBus;

public class MessageList : PageModel
{
    private readonly ServiceBus _serviceBus;
    
    public MessageInformation[] PeekedMessages { get; set; }
    public MessageInformation? ReceivedMessage { get; set; }

    public MessageList(ServiceBus serviceBus)
    {
        _serviceBus = serviceBus;
    }
    
    public async Task<IActionResult> OnGetAsync(string queueName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(queueName))
        {
            return NotFound();
        }

        var messages = await _serviceBus.PeekMessages(queueName, cancellationToken);

        if (messages is null)
        {
            return NotFound();
        }

        PeekedMessages = messages;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string queueName, string action, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(queueName))
        {
            return NotFound();
        }
        
        if (action != "ReceiveMessage")
        {
            return NotFound();
        }

        ReceivedMessage = await _serviceBus.ReceiveMessage(queueName, cancellationToken);
        
        var messages = await _serviceBus.PeekMessages(queueName, cancellationToken);

        if (messages is null)
        {
            return NotFound();
        }

        PeekedMessages = messages;

        return Page();
    }
}