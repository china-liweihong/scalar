using Moq;
using NUnit.Framework;
using Scalar.Common;
using Scalar.Common.Maintenance;
using Scalar.Service;
using Scalar.UnitTests.Mock.Common;
using Scalar.UnitTests.Mock.FileSystem;
using System.IO;

namespace Scalar.UnitTests.Service.Mac
{
    [TestFixture]
    public class MacScalarVerbRunnerTests
    {
        private const int ExpectedActiveUserId = 502;
        private static readonly string ExpectedActiveRepoPath = Path.Combine(MockFileSystem.GetMockRoot(), "code", "repo2");

        private MockTracer tracer;
        private MockPlatform scalarPlatform;

        [SetUp]
        public void SetUp()
        {
            this.tracer = new MockTracer();
            this.scalarPlatform = (MockPlatform)ScalarPlatform.Instance;
            this.scalarPlatform.MockCurrentUser = ExpectedActiveUserId.ToString();
        }

        [TestCase]
        public void CallMaintenance_LaunchesVerbUsingCorrectArgs()
        {
            MaintenanceTasks.Task task = MaintenanceTasks.Task.FetchCommitsAndTrees;
            string taskVerbName = MaintenanceTasks.GetVerbTaskName(task);
            string scalarBinPath = Path.Combine(this.scalarPlatform.Constants.ScalarBinDirectoryPath, this.scalarPlatform.Constants.ScalarExecutableName);
            string expectedArgs =
                $"run {taskVerbName} \"{ExpectedActiveRepoPath}\" --{ScalarConstants.VerbParameters.InternalUseOnly} {new InternalVerbParameters(startedByService: true).ToJson()}";

            Mock<MacScalarVerbRunner.ScalarProcessLauncher> procLauncherMock = new Mock<MacScalarVerbRunner.ScalarProcessLauncher>(MockBehavior.Strict, this.tracer);
            procLauncherMock.Setup(mp => mp.LaunchProcess(
                scalarBinPath,
                expectedArgs,
                ExpectedActiveRepoPath))
                .Returns(new ProcessResult(output: string.Empty, errors: string.Empty, exitCode: 0));

            MacScalarVerbRunner verbProcess = new MacScalarVerbRunner(this.tracer, procLauncherMock.Object);
            verbProcess.CallMaintenance(task, ExpectedActiveRepoPath, ExpectedActiveUserId);

            procLauncherMock.VerifyAll();
        }
    }
}
