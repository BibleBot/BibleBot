/*
* Copyright (C) 2016-2026 Kerygma Digital Co.
*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this file,
* You can obtain one at https://mozilla.org/MPL/2.0/.
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Systemd;
using Serilog;

namespace BibleBot.Backend.Services
{
    public class SystemdWatchdogService : IHostedService, IDisposable
    {
        private Timer _timer;
        private SystemdNotifier _sdNotifier;

        public Task StartAsync(CancellationToken stoppingToken)
        {
            Log.Information("SystemdWatchdogService: Starting service...");

            _sdNotifier = new SystemdNotifier();
            _timer = new Timer(SendWatchdogNotify, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        private void SendWatchdogNotify(object state)
        {
            _sdNotifier.Notify(new ServiceState("WATCHDOG=1"));
            Log.Information("SystemdWatchdogService: WATCHDOG=1");
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            Log.Information("SystemdWatchdogService: Stopping service...");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _timer == null)
            {
                return;
            }

            _timer.Dispose();
            _timer = null;
        }
    }
}
