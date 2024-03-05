using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ServiceBusViewer.Pages.ServiceBus;

public class QueueList : PageModel
{
    private readonly ServiceBus _serviceBus;
    public QueueInformation[] Queues { get; set; }
    
    public QueueList(ServiceBus serviceBus)
    {
        _serviceBus = serviceBus;
    }
    
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Queues = await _serviceBus.GetQueues(cancellationToken);
    }
}