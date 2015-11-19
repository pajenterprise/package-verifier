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

namespace chocolatey.package.verifier.infrastructure.app.tasks
{
    using System;
    using System.Collections.Generic;
    using domain;
    using filesystem;
    using infrastructure.messaging;
    using infrastructure.tasks;
    using messaging;
    using registration;
    using results;
    using services;

    public class TestPackageTask : ITask
    {
        private readonly IVagrantService _vagrantService;
        private readonly IFileSystem _fileSystem;
        private IDisposable _subscription;

        public TestPackageTask(IVagrantService vagrantService, IFileSystem fileSystem)
        {
            _vagrantService = vagrantService;
            _fileSystem = fileSystem;
        }

        public void initialize()
        {
            _subscription = EventManager.subscribe<SubmitPackageMessage>(test_package, null, null);
            this.Log().Info(() => "{0} is now ready and waiting for SubmitPackageMessage".format_with(GetType().Name));
        }

        public void shutdown()
        {
            if (_subscription != null) _subscription.Dispose();
            _vagrantService.shutdown();
        }

        private void test_package(SubmitPackageMessage message)
        {
            this.Log().Info(() => "========== {0} v{1} ==========".format_with(message.PackageId, message.PackageVersion));
            this.Log().Info(() => "Testing Package: {0} Version: {1}".format_with(message.PackageId, message.PackageVersion));

            var prepSuccess = _vagrantService.prep();
            var resetSuccess = _vagrantService.reset();
            if (!prepSuccess || !resetSuccess)
            {
                Bootstrap.handle_exception(new ApplicationException("Unable to test package due to vagrant issues. See log for details"));
                return;
            }

            this.Log().Info(() => "Checking install.");
            var installResults = _vagrantService.run(
                "choco install {0} --version {1} -fdvy".format_with(
                    message.PackageId,
                    message.PackageVersion));

            this.Log().Debug(() => "Grabbing results files (.registry/.files) to include in report.");
            var registrySnapshot = string.Empty;
            var registrySnapshotFile = ".\\files\\{0}.{1}\\.registry".format_with(message.PackageId, message.PackageVersion);
            try
            {
                if (_fileSystem.file_exists(registrySnapshotFile)) registrySnapshot = _fileSystem.read_file(registrySnapshotFile);
            }
            catch (Exception ex)
            {
                Bootstrap.handle_exception(new ApplicationException("Unable to read file '{0}':{1} {2}".format_with(registrySnapshotFile,Environment.NewLine,ex.ToString()),ex));
            }
         

            var filesSnapshot = string.Empty;
            var filesSnapshotFile = ".\\files\\{0}.{1}\\.files".format_with(message.PackageId, message.PackageVersion);
            try { 
                if (_fileSystem.file_exists(filesSnapshotFile)) filesSnapshot = _fileSystem.read_file(filesSnapshotFile);
            }
            catch (Exception ex)
            {
                Bootstrap.handle_exception(new ApplicationException("Unable to read file '{0}':{1} {2}".format_with(filesSnapshotFile, Environment.NewLine, ex.ToString()), ex));
            }
            
            var success = installResults.Success && installResults.ExitCode == 0;
            this.Log().Info(() => "Install was '{0}'.".format_with(success ? "successful": "not successful"));

            var upgradeResults = new VagrantOutputResult();
            var uninstallResults = new VagrantOutputResult();
            if (success)
            {
                this.Log().Info(() => "Now checking uninstall.");
                // upgradeResults = _vagrantService.run("choco upgrade {0} --version {1} -fdvy".format_with(message.PackageId, message.PackageVersion));
                uninstallResults = _vagrantService.run("choco uninstall {0} --version {1} -dvy".format_with(message.PackageId, message.PackageVersion));
            }

            foreach (var subDirectory in _fileSystem.get_directories(".\\files").or_empty_list_if_null())
            {
                try
                {
                    _fileSystem.delete_directory_if_exists(subDirectory, recursive: true);
                }
                catch (Exception ex)
                {
                    Bootstrap.handle_exception(new ApplicationException("Unable to cleanup files directory (where .chocolatey files are put):{0} {1}".format_with(Environment.NewLine, ex.ToString()), ex));
                }
            }

            var logs = new List<PackageTestLog>();

            logs.Add(
                new PackageTestLog(
                    "_Summary.md",
                    "{0} v{1} - {2} - Package Tests Results{3} * Tested {4} UTC{3} * Tested against {5} ({6})".format_with(
                        message.PackageId,
                        message.PackageVersion,
                        success ? "Passed" : "Failed",
                        Environment.NewLine,
                        DateTime.UtcNow.ToLongDateString(),
                        "win2012r2x64",
                        "Windows Server 2012 R2 x64"
                        )));

            if (!string.IsNullOrWhiteSpace(installResults.Logs)) logs.Add(new PackageTestLog("Install.txt", installResults.Logs));
            if (!string.IsNullOrWhiteSpace(registrySnapshot)) logs.Add(new PackageTestLog("RegistrySnapshot.xml", registrySnapshot));
            if (!string.IsNullOrWhiteSpace(filesSnapshot)) logs.Add(new PackageTestLog("FilesSnapshot.xml", filesSnapshot));
            if (!string.IsNullOrWhiteSpace(upgradeResults.Logs)) logs.Add(new PackageTestLog("Upgrade.txt", upgradeResults.Logs));
            if (!string.IsNullOrWhiteSpace(uninstallResults.Logs)) logs.Add(new PackageTestLog("Uninstall.txt", uninstallResults.Logs));

            EventManager.publish(
                new PackageTestResultMessage(
                    message.PackageId,
                    message.PackageVersion,
                    "Windows2012R2 x64",
                    "win2012r2x64",
                    DateTime.UtcNow,
                    logs,
                    success: success
                    ));
        }
    }
}
