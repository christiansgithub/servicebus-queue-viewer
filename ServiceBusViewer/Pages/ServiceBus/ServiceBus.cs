using Azure.Messaging.ServiceBus.Administration;
using System.Runtime.Caching;
using System.Text;
using Azure.Messaging.ServiceBus;

namespace ServiceBusViewer.Pages.ServiceBus;

public record QueueInformation(string Name);
public record MessageInformation(long SequenceNumber, string MessageId, DateTimeOffset EnqueuedTime, int DeliveryCount, string MessageText);

public class ServiceBus
{
    private readonly ServiceBusAdministrationClient _serviceBusAdministrationClient;
    private readonly ServiceBusClient _serviceBusClient;
    private readonly MemoryCache _cache;
    
    public ServiceBus(ServiceBusAdministrationClient serviceBusAdministrationClient, ServiceBusClient serviceBusClient)
    {
        _serviceBusAdministrationClient = serviceBusAdministrationClient;
        _serviceBusClient = serviceBusClient;
        _cache = MemoryCache.Default;
    }

    public async Task<QueueInformation[]> GetQueues(CancellationToken cancellationToken)
    {
        const string cacheItemKey = "ServiceBusQueues";

        if (_cache[cacheItemKey] is QueueInformation[] cachedQueues)
        {
            return cachedQueues;
        }
        
        var allQueues = await _serviceBusAdministrationClient.GetQueuesAsync(cancellationToken)
            .ToArrayAsync(cancellationToken);

        var queues = allQueues
            .Select(x => new QueueInformation(x.Name))
            .ToArray();
        
        _cache.Add(cacheItemKey, queues, new CacheItemPolicy());

        return queues;
    }

    public async Task<MessageInformation[]?> PeekMessages(string queueName, CancellationToken cancellationToken)
    {
        const int maxMessagesToPeek = 25;
        
        var queues = await GetQueues(cancellationToken);
        
        if (queues.Any(x => x.Name == queueName))
        {
            await using var receiver = _serviceBusClient.CreateReceiver(queueName);

            var messages = await receiver.PeekMessagesAsync(maxMessagesToPeek, cancellationToken: cancellationToken);

            return messages.Select(x => new MessageInformation(
                    x.SequenceNumber,
                    x.MessageId,
                    x.EnqueuedTime,
                    x.DeliveryCount,
                    Encoding.UTF8.GetString(x.Body.ToArray())))
                .ToArray();
        }

        return Array.Empty<MessageInformation>();
    }
    
    public async Task<MessageInformation?> ReceiveMessage(string queueName, CancellationToken cancellationToken)
    {
        var queues = await GetQueues(cancellationToken);
        
        if (queues.Any(x => x.Name == queueName))
        {
            await using var receiver = _serviceBusClient.CreateReceiver(queueName);

            var receivedMessage = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(2), cancellationToken);

            if (receivedMessage is null)
            {
                return null;
            }
            
            await receiver.CompleteMessageAsync(receivedMessage, cancellationToken);
            
            return new MessageInformation(
                receivedMessage.SequenceNumber,
                receivedMessage.MessageId,
                receivedMessage.EnqueuedTime,
                receivedMessage.DeliveryCount,
                Encoding.UTF8.GetString(receivedMessage.Body.ToArray()));

        }

        return null;
    }
}