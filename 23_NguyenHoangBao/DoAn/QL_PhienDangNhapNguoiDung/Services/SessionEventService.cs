using System;
using System.Diagnostics;
using System.Web.Script.Serialization;
using StackExchange.Redis;
using QL_PhienDangNhapNguoiDung.Models;

namespace QL_PhienDangNhapNguoiDung.Services
{
    /// <summary>
    /// Publishes and receives session events through Redis Pub/Sub.
    /// Pub/Sub is only a realtime notification channel; Redis/SQL remain the source of truth.
    /// </summary>
    public sealed class SessionEventService
    {
        private const string ChannelName = "session-events";
        private readonly ISubscriber _subscriber;
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

        public SessionEventService()
        {
            _subscriber = RedisHelper.Instance.Db.Multiplexer.GetSubscriber();
        }

        public void Publish(SessionEvent sessionEvent)
        {
            try
            {
                sessionEvent.Timestamp = sessionEvent.Timestamp == default(DateTime)
                    ? DateTime.Now
                    : sessionEvent.Timestamp;

                string payload = _serializer.Serialize(sessionEvent);
                _subscriber.Publish(RedisChannel.Literal(ChannelName), payload);
            }
            catch (Exception ex)
            {
                // A notification failure must not prevent login/logout from completing.
                Debug.WriteLine(ex);
            }
        }

        public IDisposable Subscribe(Action<SessionEvent> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            Action<RedisChannel, RedisValue> callback = (channel, value) =>
            {
                try
                {
                    SessionEvent? sessionEvent =
                        _serializer.Deserialize<SessionEvent>(value.ToString());
                    if (sessionEvent != null)
                        handler(sessionEvent);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            };

            RedisChannel channelName = RedisChannel.Literal(ChannelName);
            _subscriber.Subscribe(channelName, callback);
            return new Subscription(() => _subscriber.Unsubscribe(channelName, callback));
        }

        private sealed class Subscription : IDisposable
        {
            private readonly Action _unsubscribe;
            private bool _disposed;

            public Subscription(Action unsubscribe)
            {
                _unsubscribe = unsubscribe;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _unsubscribe();
            }
        }
    }
}
