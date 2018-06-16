using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SimpleGPIO.OS
{
    public class FileSystem : IFileSystem
    {
        private readonly Func<string, IFileInfoWrapper> _newFileInfo;

        public FileSystem(Func<string, IFileInfoWrapper> fileInfoFac = null)
            => _newFileInfo = fileInfoFac ?? (path => new FileInfoWrapper(path));

        public string Read(string path)
            => File.ReadAllText(path).Trim('\n');

        public void Write(string path, string content)
            => File.WriteAllText(path, content.Trim('\n'));

        public bool Exists(string path)
            => Directory.Exists(path) || File.Exists(path);

        public async Task WaitFor(string path, TimeSpan timeout)
            => await WaitFor(path, false, timeout);

        public async Task WaitForWriteable(string path, TimeSpan timeout)
            => await WaitFor(path, true, timeout);

        private async Task WaitFor(string path, bool writeable, TimeSpan timeout)
        {
            var time = Stopwatch.StartNew();
            var fileInfo = _newFileInfo(path);

            while ((!fileInfo.Exists || writeable && fileInfo.IsReadOnly) && time.Elapsed < timeout)
            {
                fileInfo.Refresh();
                await Task.Delay(1);
            }

            time.Stop();
            if (time.Elapsed > timeout)
                throw new TimeoutException();
        }
    }
}