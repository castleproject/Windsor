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

namespace Castle.Facilities.NHibernateIntegration.Tests.Common
{
	using NHibernate;

	/// <summary>
	/// An implementation of the <see cref="IInterceptor"/> interface for testing
	/// purposes.
	/// </summary>
	public class TestInterceptor : EmptyInterceptor
	{
		private bool onSaveCall;
		private bool instantiationCall;

		public bool ConfirmOnSaveCall()
		{
			return onSaveCall;
		}

		public bool ConfirmInstantiationCall()
		{
			return instantiationCall;
		}

		public void ResetState()
		{
			instantiationCall = false;
			onSaveCall = false;
		}

		#region IInterceptor Members

		public override int[] FindDirty(object entity, object id, object[] currentState, object[] previousState,
		                                string[] propertyNames, NHibernate.Type.IType[] types)
		{
			return null;
		}

		public override object Instantiate(string clazz, EntityMode entityMode, object id)
		{
			instantiationCall = true;
			return null;
		}

		public override bool OnFlushDirty(object entity, object id, object[] currentState, object[] previousState,
		                                  string[] propertyNames, NHibernate.Type.IType[] types)
		{
			return false;
		}

		public override bool OnLoad(object entity, object id, object[] state, string[] propertyNames,
		                            NHibernate.Type.IType[] types)
		{
			return false;
		}

		public override bool OnSave(object entity, object id, object[] state, string[] propertyNames,
		                            NHibernate.Type.IType[] types)
		{
			onSaveCall = true;
			return false;
		}

		public override void OnDelete(object entity, object id, object[] state, string[] propertyNames,
		                              NHibernate.Type.IType[] types)
		{
		}

		public override void PreFlush(System.Collections.ICollection entities)
		{
		}

		public override void PostFlush(System.Collections.ICollection entities)
		{
		}

		#endregion
	}
}