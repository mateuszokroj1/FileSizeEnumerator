using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace FileSizeEnumerator
{
    public sealed class FileEnumerator : IDisposable
    {
        #region Constructor

        /// <summary>
        /// Creates new instance of <see cref="FileEnumerator"/>.
        /// </summary>
        /// <param name="list">Writable collection for live adding searched <see cref="FileInfo"/>.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        public FileEnumerator(ICollection<FileInfo> list)
        {
            List = list ?? throw new ArgumentNullException(nameof(list));

            if (List.IsReadOnly)
                throw new ArgumentException("List is read-only.");

            IsWorkingObservable.OnNext(false);
        }

        #endregion

        #region Fields

        private Thread worker;
        private volatile bool isWorkingInTopDirectory = true;

        #endregion

        #region Properties

        public Subject<bool> IsWorkingObservable { get; } = new Subject<bool>();

        public bool IsCanceling { get; private set; } = false;

        public string StartingPath { get; set; }

        public ulong MinimumFileSize { get; set; } = 100_000_000;

        public ICollection<FileInfo> List { get; set; }

        public bool SkipOnExceptions { get; set; } = true;

        public SynchronizationContext Synchronization { get; set; } = SynchronizationContext.Current;

        #endregion

        #region Methods

        public void Cancel() => IsCanceling = true;

        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        public void Start()
        {
            if (StartingPath == null)
                throw new InvalidOperationException("StartingPath is null.");

            if (!Directory.Exists(StartingPath))
                throw new DirectoryNotFoundException();

            IsCanceling = false;

            this.isWorkingInTopDirectory = true;

            this.worker = new Thread(ThreadStart);
            this.worker.Start();
        }

        private void ThreadStart()
        {
            IsWorkingObservable.OnNext(true);
            EnumerateFiles(StartingPath);
            IsWorkingObservable.OnNext(false);
        }

        private void EnumerateFiles(string directoryPath)
        {
            IEnumerable<string> files = null;
            try
            {
                files = Directory.EnumerateFiles(directoryPath);
            }
            catch (Exception exc) when (exc is IOException || exc is UnauthorizedAccessException)
            {
                if (!SkipOnExceptions)
                    throw exc;

                if (Debugger.IsAttached)
                    Debug.WriteLine(exc.ToString());

                return;
            }
            catch (DirectoryNotFoundException) { return; }

            foreach (var path in files)
            {
                if (IsCanceling) return;

                FileInfo info;
                try
                {
                    info = new FileInfo(path);
                    if ((ulong)info.Length < MinimumFileSize) continue;
                }
                catch (Exception exc) when (exc is IOException || exc is UnauthorizedAccessException)
                {
                    if (!SkipOnExceptions)
                        throw exc;

                    if (Debugger.IsAttached)
                        Debug.WriteLine(exc.ToString());

                    continue;
                }

                Synchronization.Post(state => List.Add(info), null);
            }

            IEnumerable<string> directories = null;
            try
            {
                directories = Directory.EnumerateDirectories(directoryPath);
            }
            catch (Exception exc) when (exc is IOException || exc is UnauthorizedAccessException)
            {
                if (!SkipOnExceptions)
                    throw exc;

                if (Debugger.IsAttached)
                    Debug.WriteLine(exc.ToString());

                return;
            }
            catch (DirectoryNotFoundException) { return; }

            if (this.isWorkingInTopDirectory)
            {
                this.isWorkingInTopDirectory = false;
                Parallel.ForEach(directories, (directory, state) =>
                {
                    if (IsCanceling)
                        state.Stop();

                    if (state.IsStopped || state.IsExceptional)
                        return;

                    EnumerateFiles(directory);
                });
            }
            else
                foreach (var directory in directories)
                {
                    if (IsCanceling) return;

                    EnumerateFiles(directory);
                }
        }

        public void Dispose()
        {
            Cancel();
            if (this.worker?.ThreadState == System.Threading.ThreadState.Running)
                this.worker.Abort();

            IsWorkingObservable.OnCompleted();
        }

        #endregion
    }
}
