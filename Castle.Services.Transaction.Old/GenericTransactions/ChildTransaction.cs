#region License
//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
#endregion
namespace Castle.Services.Transaction
{
	/// <summary>
	/// Emulates a standalone transaction but in fact it 
	/// just propages a transaction. 
	/// </summary>
	public sealed class ChildTransaction : TransactionBase
	{
		private readonly ITransaction _Parent;

		public ChildTransaction(ITransaction parent) 
			: base(string.Format("Child-TX to \"{0}\"", parent.Name),
				   parent.TransactionMode, parent.IsolationMode)
		{
			_Parent = parent;
		}

		public override void Begin()
		{
		}

		protected override void InnerBegin()
		{
		}

		protected override void InnerCommit()
		{
		}

		public override void Rollback()
		{
			// Vote as rollback
			_Parent.SetRollbackOnly();
		}

		public override void SetRollbackOnly()
		{
			_Parent.SetRollbackOnly();
		}

		protected override void InnerRollback()
		{
		}

		public override void Commit()
		{
		}

		public override bool IsChildTransaction
		{
			get { return true; }
		}

		public override void Enlist(IResource resource)
		{
			_Parent.Enlist(resource);
		}

		public override void RegisterSynchronization(ISynchronization s)
		{
			_Parent.RegisterSynchronization(s);
		}

		public override bool IsAmbient { 
			get { return true; }
			protected set { } 
		}

		public override bool IsRollbackOnlySet
		{
			get { return _Parent.IsRollbackOnlySet; }
		}

        public override bool IsReadOnly
        {
            get { return _Parent.IsReadOnly; }
            protected set { }
        }
	}
}