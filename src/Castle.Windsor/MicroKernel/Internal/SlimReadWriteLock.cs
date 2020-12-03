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

	public class SlimReadWriteLock : Lock
	{
		private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

		public override IUpgradeableLockHolder ForReadingUpgradeable()
		{
			return ForReadingUpgradeable(true);
		}

		public override ILockHolder ForReading()
		{
			return ForReading(true);
		}

		public override ILockHolder ForWriting()
		{
			return ForWriting(true);
		}

		public override IUpgradeableLockHolder ForReadingUpgradeable(bool waitForLock)
		{
			return new SlimUpgradeableReadLockHolder(locker, waitForLock, locker.IsUpgradeableReadLockHeld || locker.IsWriteLockHeld);
		}

		public override ILockHolder ForReading(bool waitForLock)
		{
			if (locker.IsReadLockHeld || locker.IsUpgradeableReadLockHeld || locker.IsWriteLockHeld)
			{
				return NoOpLock.Lock;
			}

			return new SlimReadLockHolder(locker, waitForLock);
		}

		public override ILockHolder ForWriting(bool waitForLock)
		{
			return locker.IsWriteLockHeld ? NoOpLock.Lock : new SlimWriteLockHolder(locker, waitForLock);
		}

		public bool IsReadLockHeld => locker.IsReadLockHeld;

		public bool IsUpgradeableReadLockHeld => locker.IsUpgradeableReadLockHeld;

		public bool IsWriteLockHeld => locker.IsWriteLockHeld;
	}
}