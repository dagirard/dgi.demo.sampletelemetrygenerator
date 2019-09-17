//
// Copyright (c) Damien Girard. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//

namespace Dgi.Demo
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    /// <see cref="TelemetryItem" /> builder base class.
    /// </summary>
    public abstract class TelemetryItemBuilder
    {
        private static List<string> userIps = new List<string>
        {
            // Poland
            {"5.184.0.1"},
            {"5.184.0.2"},
            {"5.184.0.3"},
            // Luxembourg
            {"87.254.96.1"},
            {"87.254.96.2"},
            {"87.254.96.3"},
            // Denmark
            {"2.56.0.1"},
            {"2.56.0.2"},
            {"2.56.0.3"},
            // France
            {"5.135.0.1"},
            {"5.135.0.2"},
            {"5.135.0.3"}
        };

        protected string GetIpAddress(int userId)
        {
            return userIps[userId % userIps.Count];
        }

        protected string GetNodeId(int nodeId)
        {
            return "AS0001_"+ nodeId.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Build the telemetry items to be emitted.
        /// </summary>
        /// <param name="delay">Delay before event is emitted.</param>
        /// <param name="operationId">Operation identifier.</param>
        /// <param name="tenantId">Tenant identifier.</param>
        /// <param name="userId">User identifier.</param>
        /// <param name="nodeId">Node identifier.</param>
        /// <param name="operationDuration">Operation duration in ms.</param>
        /// <returns>List of <see cref="TelemetryItem" /> to emit.</returns>
        public abstract List<TelemetryItem> Build(int delay, int operationId, int tenantId, int userId, int nodeId, int operationDuration);
    }
}