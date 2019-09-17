//
// Copyright (c) Damien Girard. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//

using System;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Dgi.Demo
{
    /// <summary>
    /// Page view telemetry item.
    /// </summary>
    public class TelemetryPageViewItem : TelemetryItem
    {
        public string PageName { get; private set; }

        public string PageType { get; private set; }

        public TelemetryPageViewItem(int delay, string pageName, string pageType, string operationId, string sessionId, string tenantId, string userId, string userIp, string nodeId, TimeSpan operationDuration)
         : base(delay, operationId, sessionId, tenantId, userId, userIp, nodeId, operationDuration)
        {
            this.PageName = pageName ?? throw new ArgumentNullException(nameof(pageName));
            this.PageType = pageType ?? throw new ArgumentNullException(nameof(pageType));   
        }

        /// <inheritdoc/>
        public override void Send(TelemetryClient client)
        {
            var pageViewTelemetry = new PageViewTelemetry(this.PageName)
            {
                Timestamp = DateTimeOffset.Now,
                Url = new Uri("/GetPage", UriKind.Relative),
                Duration = this.OperationDuration,
                Properties = { { "Type", this.PageType }, { "DeploymentType", "Prod" }, { "PlatformVersion", "1.0.0.0" }, { "TenantId", this.TenantId } },
                Context =
                    {
                        Cloud = { RoleInstance = this.NodeId },
                        InstrumentationKey = client.InstrumentationKey,
                        Session = { Id = this.SessionId },
                        Location = { Ip = this.UserIp },
                        Operation = { Id = this.OperationId, Name = "OpenForm" },
                        User = { Id = this.UserId, AccountId = this.UserId }
                    }
            };

            client.TrackPageView(pageViewTelemetry);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Page, {Delay} - {PageName} - {PageType} - " + base.ToString();
        }
    }
}