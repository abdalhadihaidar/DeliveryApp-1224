using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;

namespace DeliveryApp.Hubs
{
    public class OrderTrackingHub : Hub
    {
        private readonly ILogger<OrderTrackingHub> _logger;
        private static readonly ConcurrentDictionary<string, string> _userConnections = new();
        private static readonly ConcurrentDictionary<string, ConcurrentBag<string>> _orderSubscriptions = new();
        private static readonly Timer _cleanupTimer;
        private static readonly object _cleanupLock = new object();

        static OrderTrackingHub()
        {
            // Initialize cleanup timer to run every 10 minutes
            _cleanupTimer = new Timer(CleanupStaleConnectionsCallback, null, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));
        }

        public OrderTrackingHub(ILogger<OrderTrackingHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections.AddOrUpdate(userId, Context.ConnectionId, (key, oldValue) => Context.ConnectionId);
                _logger.LogInformation($"User {userId} connected with connection {Context.ConnectionId}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections.TryRemove(userId, out _);
                
                // Remove from order subscriptions
                var toRemove = new List<string>();
                foreach (var subscription in _orderSubscriptions)
                {
                    // Create a new bag without the current connection ID
                    var newBag = new ConcurrentBag<string>();
                    foreach (var connectionId in subscription.Value)
                    {
                        if (connectionId != Context.ConnectionId)
                        {
                            newBag.Add(connectionId);
                        }
                    }
                    
                    if (newBag.IsEmpty)
                    {
                        toRemove.Add(subscription.Key);
                    }
                    else
                    {
                        _orderSubscriptions.TryUpdate(subscription.Key, newBag, subscription.Value);
                    }
                }
                
                foreach (var key in toRemove)
                {
                    _orderSubscriptions.TryRemove(key, out _);
                }
                
                _logger.LogInformation($"User {userId} disconnected");
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinOrderGroup(string orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Order_{orderId}");
            
            _orderSubscriptions.AddOrUpdate(orderId, 
                new ConcurrentBag<string> { Context.ConnectionId },
                (key, existingBag) => 
                {
                    existingBag.Add(Context.ConnectionId);
                    return existingBag;
                });
            
            _logger.LogInformation($"Connection {Context.ConnectionId} joined order group {orderId}");
        }

        public async Task LeaveOrderGroup(string orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Order_{orderId}");
            
            if (_orderSubscriptions.TryGetValue(orderId, out var existingBag))
            {
                // Create a new bag without the current connection ID
                var newBag = new ConcurrentBag<string>();
                foreach (var connectionId in existingBag)
                {
                    if (connectionId != Context.ConnectionId)
                    {
                        newBag.Add(connectionId);
                    }
                }
                
                if (newBag.IsEmpty)
                {
                    _orderSubscriptions.TryRemove(orderId, out _);
                }
                else
                {
                    _orderSubscriptions.TryUpdate(orderId, newBag, existingBag);
                }
            }
            
            _logger.LogInformation($"Connection {Context.ConnectionId} left order group {orderId}");
        }

        public async Task JoinRestaurantGroup(string restaurantId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Restaurant_{restaurantId}");
            _logger.LogInformation($"Connection {Context.ConnectionId} joined restaurant group {restaurantId}");
        }

        public async Task JoinDeliveryGroup(string deliveryPersonId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Delivery_{deliveryPersonId}");
            _logger.LogInformation($"Connection {Context.ConnectionId} joined delivery group {deliveryPersonId}");
        }

        public async Task UpdateLocation(double latitude, double longitude)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                // Broadcast location update to relevant order groups
                await Clients.Groups($"Delivery_{userId}").SendAsync("LocationUpdate", new
                {
                    UserId = userId,
                    Latitude = latitude,
                    Longitude = longitude,
                    Timestamp = DateTime.UtcNow
                });
                
                _logger.LogInformation($"Location updated for user {userId}: {latitude}, {longitude}");
            }
        }

        private static void CleanupStaleConnectionsCallback(object state)
        {
            lock (_cleanupLock)
            {
                try
                {
                    // Clean up empty order subscriptions
                    var toRemove = new List<string>();
                    foreach (var subscription in _orderSubscriptions)
                    {
                        if (subscription.Value.IsEmpty)
                        {
                            toRemove.Add(subscription.Key);
                        }
                    }
                    
                    foreach (var key in toRemove)
                    {
                        _orderSubscriptions.TryRemove(key, out _);
                    }
                    
                    if (toRemove.Any())
                    {
                        Console.WriteLine($"Cleaned up {toRemove.Count} empty order subscriptions");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during SignalR cleanup: {ex.Message}");
                }
            }
        }
    }
}

