//
// Copyright (c) Damien Girard. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//

namespace Dgi.Demo
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Builder for <see cref="TelemetryPageView" /> items.
    /// </summary>
    public class TelemetryPageViewBuilder : TelemetryItemBuilder
    {
        private static List<(string page, string type, int min, int max, double serverMin, double serverMax)> OpenPageRanges = new List<(string page, string type, int min, int max, double serverMin, double serverMax)>
        {
            { ("Sales order list", "List", 1, 4, 0.4, 0.8) },
            { ("Sales order card", "Card", 1, 2, 0.5, 0.6) },
            { ("Purchase order list", "List", 2, 6, 1.5, 1.7) },
            { ("Purchase order card", "Card", 1, 3, 0.5, 0.7) },
            { ("Item list", "List", 3, 8, 2.5, 2.9) },
            { ("Item card", "Card", 1, 2, 0.4, 0.7) }
        };

        /// <inheritdoc/>
        public override List<TelemetryItem> Build(int delay, int operationId, int tenantId, int userId, int nodeId, int operationDuration)
        {
            (string page, string type, int min, int max, double serverMin, double serverMax) page = OpenPageRanges[operationId % OpenPageRanges.Count];

            // In this software, the server is rendering
            int duration = Math.Max(page.min, operationDuration % page.max);
            double depDuration = Math.Max(page.serverMin, operationDuration % page.serverMax);

            Debug.Assert(duration > depDuration);

            int depDelay = delay + (int)((duration - depDuration) * 0.3 * 1000);
            Debug.Assert(depDelay + (depDuration * 1000) < delay + (duration * 1000));

            string operationGuid = Guid.NewGuid().ToString("D");
            string sessionGuid = Guid.NewGuid().ToString("D");

            TelemetryItem pageView = new TelemetryPageViewItem(
                delay,
                page.page,
                page.type,
                operationGuid,
                sessionGuid,
                "Tenant_"+ tenantId,
                "User_" + userId,
                this.GetIpAddress(userId),
                this.GetNodeId(nodeId),
                TimeSpan.FromSeconds(duration)
            );

            TelemetryItem dependency = new TelemetryDependencyItem(
                depDelay,
                "OpenForm",
                "Form: "+ page.page +" - Type: "+ page.type,
                pageView,
                TimeSpan.FromSeconds(depDuration)
            );

            return new List<TelemetryItem>(){
                pageView,
                dependency            
            };
        }
    }
}