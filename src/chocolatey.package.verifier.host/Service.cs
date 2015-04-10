﻿// Copyright © 2015 - Present RealDimensions Software, LLC
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// 
// You may obtain a copy of the License at
// 
// 	http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace chocolatey.package.verifier.Host
{
    using System;
    using System.ServiceProcess;
    using Console.Infrastructure.Registration;
    using SimpleInjector;
    using log4net;
    using verifier.Infrastructure.App;
    using verifier.Infrastructure.App.Registration;
    using verifier.Infrastructure.Tasks;

    /// <summary>
    ///   The service that registers tasks and schedules to run
    /// </summary>
    public partial class Service : ServiceBase
    {
        private readonly ILog _logger;
        private Container _container;

        /// <summary>
        ///   Initializes a new instance of the <see cref="Service" /> class.
        /// </summary>
        public Service()
        {
            InitializeComponent();
            Bootstrap.Initialize();
            _logger = LogManager.GetLogger(typeof (Service));
        }

        /// <summary>
        ///   When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            _logger.InfoFormat("Starting {0} service.", ApplicationParameters.Name);

            try
            {
                Bootstrap.Startup();
                //AutoMapperInitializer.Initialize();
                SimpleInjectorContainer.Start();
                _container = SimpleInjectorContainer.Container;

                var tasks = _container.GetAllInstances<ITask>();
                foreach (var task in tasks)
                {
                    task.initialize();
                }

                _logger.InfoFormat("{0} service is now operational.", ApplicationParameters.Name);

                if ((args.Length > 0) && (Array.IndexOf(args, "/console") != -1))
                {
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("{0} service had an error on {1} (with user {2}):{3}{4}",
                                    ApplicationParameters.Name,
                                    Environment.MachineName,
                                    Environment.UserName,
                                    Environment.NewLine,
                                    ex);
            }
        }

        /// <summary>
        ///   When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                _logger.InfoFormat("Stopping {0} service.", ApplicationParameters.Name);

                if (_container != null)
                {
                    var tasks = _container.GetAllInstances<ITask>();
                    foreach (var task in tasks.OrEmptyListIfNull())
                    {
                        task.shutdown();
                    }
                }

                Bootstrap.Shutdown();
                SimpleInjectorContainer.Stop();

                _logger.InfoFormat("{0} service has shut down.", ApplicationParameters.Name);
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("{0} service had an error on {1} (with user {2}):{3}{4}",
                                    ApplicationParameters.Name,
                                    Environment.MachineName,
                                    Environment.UserName,
                                    Environment.NewLine,
                                    ex
                    );
            }
        }

        /// <summary>
        ///   Runs as console.
        /// </summary>
        /// <param name="args">The args.</param>
        public void RunAsConsole(string[] args)
        {
            OnStart(args);
            OnStop();
        }
    }
}