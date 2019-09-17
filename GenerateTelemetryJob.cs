//
// Copyright (c) Damien Girard. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//

namespace Dgi.Demo
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.Configuration;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Globalization;
    using System.Collections.Concurrent;

    /// <summary>
    /// Job that generates random performance data onto Application Insight.
    /// </summary>
    public static class GenerateTelemetryJob
    {
        private static TimeSpan FunctionRunDuration = TimeSpan.FromMinutes(3);

        private const int NumberOfNodes = 5;

        [FunctionName("GenerateTelemetryJob")]
        public static async Task Run([TimerTrigger("0 */7 * * * *", RunOnStartup = false /* Only set to true to test locally! */)]TimerInfo myTimer,
            ILogger log,
            Microsoft.Azure.WebJobs.ExecutionContext context,
            CancellationToken cancellationToken)
        {
            log.LogInformation($"GenerateTelemetryJob Timer trigger function executed at: {DateTime.UtcNow.ToTelemetryString()}");

            // Read the configuration
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Setting the target application insight
            TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
            configuration.InstrumentationKey = config["InstrumentationKey"];

            // Generating the list of telemetry events to emit
            SortedSet<TelemetryItem> items = new SortedSet<TelemetryItem>();
            List<TelemetryItemBuilder> builders = new List<TelemetryItemBuilder> {
                new TelemetryPageViewBuilder()
            };

            // Each tenants emits a random number of operations.
            // The number of users is TenantId * 2.
            // The telemetry item emitted is random as well.
            // A user hits always the same node.
            Random rand = new Random();
            int numberOfTenants = rand.Next(1, 10);
            int maxDelay = (int)FunctionRunDuration.TotalMilliseconds;
            for (int tenantId = 1; tenantId <= numberOfTenants; tenantId++)
            {
                int maxUserId = tenantId * 2;
                int numOfOperations = rand.Next(1, 50);
                for (int j = 1; j < numOfOperations; j++)
                {
                    int userId = rand.Next(1, maxUserId + 1);
                    builders[rand.Next(0, builders.Count)].Build(
                        rand.Next(1, maxDelay),
                        rand.Next(1, 100),
                        tenantId,
                        userId,
                        userId % NumberOfNodes,
                        rand.Next(1, maxDelay)
                        ).ForEach(t => items.Add(t));
                }
            }

            log.LogInformation($"About to emit {items.Count} telemetry events for {numberOfTenants} tenants.");

            // Emitting telemetry
            var telemetryClient = new TelemetryClient(configuration);
            telemetryClient.Context.InstrumentationKey = configuration.InstrumentationKey;

            int delay = 0;
            foreach (TelemetryItem item in items)
            {
                await Task.Delay(item.Delay - delay, cancellationToken);
                delay = item.Delay;
                log.LogDebug(new EventId(1), item.ToString());
                item.Send(telemetryClient);
            }

            log.LogInformation($"GenerateTelemetryJob Timer trigger function completed at: {DateTime.UtcNow.ToTelemetryString()}");
        }

        /// <summary>
        /// Returns the date in a standardized telemetry output format.
        /// </summary>
        /// <param name="dt">Datetime.</param>
        /// <returns>The UTC <see cref="dt" /> in a standard telemetry format.</returns>
        public static string ToTelemetryString(this DateTime dt)
        {
            return dt.ToUniversalTime().ToString("yyyy/MM/dd H:mm:ss UTC");
        }
    }
}
