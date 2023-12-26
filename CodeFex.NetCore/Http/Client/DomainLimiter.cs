using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace CodeFex.NetCore.Http.Client
{
    public class DomainLimiter
    {
        public static readonly DomainLimiter SiteCrowler = new DomainLimiter();

        protected ConcurrentDictionary<string, FixedWindowRateLimiter> Limits;

        protected static FixedWindowRateLimiterOptions DomainOptions;

        static DomainLimiter()
        {
            DomainOptions = new FixedWindowRateLimiterOptions()
            {
                AutoReplenishment = true,
                PermitLimit = 1,
                QueueLimit = 64,
#if RELEASE
                Window = TimeSpan.FromSeconds(10),
#else
                Window = TimeSpan.FromSeconds(10),
#endif
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            };
        }

        public DomainLimiter()
        {
            Limits = new ConcurrentDictionary<string, FixedWindowRateLimiter>();
        }

        public async Task<RateLimitLease> Acquire(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            FixedWindowRateLimiter limiter;

            while (!Limits.TryGetValue(uri.Host, out limiter))
            {
                limiter = new FixedWindowRateLimiter(DomainOptions);

                if (!Limits.TryAdd(uri.Host, limiter))
                {
                    await limiter.DisposeAsync().ConfigureAwait(false);
                }
            }

            return await limiter.AcquireAsync(1).ConfigureAwait(false);
        }

        public async Task<RateLimitLease> Acquire(Uri uri, CancellationToken cancellationToken)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            FixedWindowRateLimiter limiter;

            while (!Limits.TryGetValue(uri.Host, out limiter))
            {
                limiter = new FixedWindowRateLimiter(DomainOptions);

                if (!Limits.TryAdd(uri.Host, limiter))
                {
                    await limiter.DisposeAsync().ConfigureAwait(false);
                }
            }

            return await limiter.AcquireAsync(1, cancellationToken).ConfigureAwait(false);
        }
    }
}
