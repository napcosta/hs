using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HockeySlam.Class.Networking
{
	class OperationCompletedEventArgs : EventArgs
	{
		#region Properties

		public IAsyncResult AsynchResult
		{
			get { return _asyncResult; }
			set { _asyncResult = value; }
		}

		IAsyncResult _asyncResult;

		#endregion

		#region Initialization

		public OperationCompletedEventArgs(IAsyncResult asyncResult)
		{
			_asyncResult = asyncResult;
		}

		#endregion
	}
}
