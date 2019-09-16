//
// Copyright (c) Damien Girard. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//

namespace Dgi.Demo
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;

    public class TelemetryDependencyItem : TelemetryItem
    {
        public string DependencyName { get; private set; }

        public string DependencyData { get; private set; }

        public TelemetryDependencyItem(int delay, string dependencyName, string dependencyData, string operationId, string sessionId, string tenantId, string userId, string userIp, string nodeId, TimeSpan operationDuration)
            : base(delay, operationId, sessionId, tenantId, userId, userIp, nodeId, operationDuration)
        {
            this.DependencyName = dependencyName ?? throw new ArgumentNullException(nameof(dependencyName));
            this.DependencyData = dependencyData ?? throw new ArgumentNullException(nameof(dependencyData));
        }

        public TelemetryDependencyItem(int delay, string dependencyName, string dependencyData, TelemetryItem parentItem, TimeSpan operationDuration)
            : this(delay, dependencyName, dependencyData, parentItem.OperationId, parentItem.SessionId, parentItem.TenantId, parentItem.UserId, "127.0.0.1" /* Server is on same machine */, parentItem.NodeId, operationDuration)
        {
        }

        public override void Send(TelemetryClient client)
        {
            var dependencyTelemetry = new DependencyTelemetry(
                this.DependencyName,
                "Server",
                "Server",
                this.DependencyData,
                DateTimeOffset.Now,
                this.OperationDuration,
                "200",
                true)
            {
                Properties = { { "DeploymentType", "Prod" }, { "PlatformVersion", "1.0.0.0" }, { "TenantId", this.TenantId } },
                Context =
                    {
                        Cloud = { RoleInstance = this.NodeId },
                        InstrumentationKey = client.InstrumentationKey,
                        Session = { Id = this.SessionId },
                        Location = { Ip = this.UserIp },
                        User = { Id = this.UserId, AccountId = this.UserId },
                        Operation = { Id = Guid.NewGuid().ToString("D"), ParentId = this.OperationId }
                    }
            };

            client.TrackDependency(dependencyTelemetry);
        }

        
        public override string ToString()
        {
            return $"Dependency, {Delay} - {DependencyName} - {DependencyData} - " + base.ToString();
        }
    }
}