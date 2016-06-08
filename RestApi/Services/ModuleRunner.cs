using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Parcs.Api.Dto;

namespace RestApi.Services
{
    public class ModuleRunner : IModuleRunner
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);
        private readonly string _matrixModuleFilePath = ConfigurationManager.AppSettings["matrixModuleFilePath"];
        private readonly string _matrixStorageFolder = ConfigurationManager.AppSettings["matrixStorageFolder"];

        public async Task<bool> TryRunMatrixModule(MatrixSize matrixSize, int pointCount, string serverIp, int priority)
        {
            int matrixFilePrefix = (int) matrixSize/1000;
            var username = Thread.CurrentPrincipal.Identity.Name;
            var userArg = string.IsNullOrEmpty(username) ? "" : $" --user {username}";
            var processStartInfo =
                new ProcessStartInfo(
                    $@"{_matrixModuleFilePath}",
                    $@"--m1 {_matrixStorageFolder}\{matrixFilePrefix}1.mtr --m2 {_matrixStorageFolder}\{matrixFilePrefix}2.mtr --p {pointCount} --serverip {serverIp} --priority {priority}{userArg}")
                {
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

            var process = new Process
            {
                StartInfo = processStartInfo
            };

            bool isError = false;

            process.OutputDataReceived += (sender, args) => OutputReceived(args.Data, ref isError);
            process.ErrorDataReceived += (sender, args) => Error(args.Data, out isError);

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await _semaphore.WaitAsync();
            return !isError;
        }

        private void OutputReceived(string data, ref bool isError)
        {
            if (string.IsNullOrEmpty(data))
            {
                return;                
            }
            if (data.ToUpper().Contains("ERROR"))
            {
                isError = true;
                _semaphore.Release();
                return;
            }

            if (data.StartsWith("connection to host", StringComparison.InvariantCultureIgnoreCase))
            {
                _semaphore.Release();
            }
        }

        private void Error(string data, out bool isError)
        {
            isError = true;
            _semaphore.Release();
        }
    }
}