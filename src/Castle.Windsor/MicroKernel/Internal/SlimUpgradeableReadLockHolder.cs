// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.MicroKernel.Internal
{
	using System.Threading;

	internal class SlimUpgradeableReadLockHolder : IUpgradeableLockHolder
	{
		private readonly ReaderWriterLockSlim locker;
		private SlimWriteLockHolder writerLock;
		private readonly bool wasLockAlreadySelf;

		public SlimUpgradeableReadLockHolder(ReaderWriterLockSlim locker, bool waitForLock, bool wasLockAlreadySelf)
		{
			this.locker = locker;
			if (wasLockAlreadySelf)
			{
				LockAcquired = true;
				this.wasLockAlreadySelf = true;
				return;
			}

			if(waitForLock)
			{
				locker.EnterUpgradeableReadLock();
				LockAcquired = true;
				return;
			}

			LockAcquired = locker.TryEnterUpgradeableReadLock(0);
		}

		public void Dispose()
		{
			if (writerLock != null && writerLock.LockAcquired)
			{
				writerLock.Dispose();
				writerLock = null;
			}
			if (!LockAcquired) return;
			if (!wasLockAlreadySelf)
			{
				locker.ExitUpgradeableReadLock();
			}
			LockAcquired = false;
			
		}

		public ILockHolder Upgrade()
		{
			return Upgrade(true);
		}

		public ILockHolder Upgrade(bool waitForLock)
		{
			if(locker.IsWriteLockHeld)
			{
				return NoOpLock.Lock;
			}

			writerLock = new SlimWriteLockHolder(locker, waitForLock);
			return writerLock;
		}

		public bool LockAcquired { get; private set; }
	}
}