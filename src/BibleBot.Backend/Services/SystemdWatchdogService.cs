/*
* Copyright (C) 2016-2025 Kerygma Digital Co.
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
        private readonly ServiceState _watchdogServiceState = new("WATCHDOG=1");

        public Task StartAsync(CancellationToken stoppingToken)
        {
            Log.Information("SystemdWatchdogService: Starting service...");

            _timer = new Timer(SendWatchdogNotify, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            _sdNotifier = new SystemdNotifier();

            return Task.CompletedTask;
        }

        public void SendWatchdogNotify(object state)
        {
            _sdNotifier.Notify(_watchdogServiceState);
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
            if (disposing)
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }
        }
    }
}
