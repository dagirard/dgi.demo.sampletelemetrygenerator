//
// Copyright (c) Damien Girard. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//

namespace Dgi.Demo
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;

    /// <summary>
    /// Represent a telemetry event to send to Application Insight.
    /// </summary>
    public abstract class TelemetryItem : IComparable<TelemetryItem>
    {
        /// <summary>
        /// Delay before emitting the telemetry item.
        /// </summary>
        public int Delay { get; }

        /// <summary>
        /// The session ID.
        /// </summary>
        public string SessionId { get; private set; }

        /// <summary>
        /// The operation ID.
        /// </summary>
        public string OperationId { get; private set; }

        /// <summary>
        /// Tenant ID.
        /// </summary>
        public string TenantId { get; }

        /// <summary>
        /// User ID.
        /// </summary>
        public string UserId { get; }

        /// <summary>
        /// User IP.
        /// </summary>
        public string UserIp {get;}

        /// <summary>
        /// Node ID.
        /// </summary>
        public string NodeId { get; }

        /// <summary>
        /// Operation duration.
        /// </summary>
        public TimeSpan OperationDuration { get; }

        /// <summary>
        /// Instantiates a new <c>TelemetryPageViewItem</c> instance.
        /// </summary>
        /// <param name="delay">Delay before being run.</param>
        /// <param name="operationId">Operation ID.</param>
        /// <param name="sessionId">Session ID.</param>
        /// <param name="tenantId">Tenant ID.</param>
        /// <param name="userId">User ID.</param>
        /// <param name="userIp">User IP.</param>
        /// <param name="nodeId">Node ID.</param>
        /// <param name="operationDuration">Operation duration.</param>
        public TelemetryItem(int delay, string operationId, string sessionId, string tenantId, string userId, string userIp, string nodeId, TimeSpan operationDuration)
        {
            if (delay <= 0)
            {
                throw new ArgumentException(nameof(delay) + " must be > 0");
            }

            this.Delay = delay;
            this.OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            this.SessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            this.TenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
            this.UserId = userId ?? throw new ArgumentNullException(nameof(tenantId));
            this.NodeId = nodeId ?? throw new ArgumentNullException(nameof(nodeId));
            this.UserIp = userIp ?? throw new ArgumentNullException(nameof(userIp));

            if (operationDuration.TotalMilliseconds <= 0)
            {
                throw new ArgumentException(nameof(operationDuration) + " must be positive");
            }

            this.OperationDuration = operationDuration;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{OperationId} - {SessionId} {TenantId} - {UserId} - {NodeId} - {OperationDuration.TotalSeconds}";
        }

        /// <summary>
        /// Sends the telemetry item to Application Insight.
        /// </summary>
        /// <param name="client">Application insight telemetry client.</param>
        public abstract void Send(TelemetryClient client);

        /// <inheritdoc/>
        public int CompareTo(TelemetryItem other)
        {
            if (other == null)
            {
                return 1;
            }

            return this.Delay - other.Delay;
        }
    }
}