﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace Merlin.API
{
	public class Landscape
	{
		#region Static

		public static Landscape Instance
		{
			get
			{
				var internalLandscape = a6o.s().z();

				if (internalLandscape != null)
					return new Landscape(internalLandscape);

				return default(Landscape);
			}
		} 

		#endregion

		#region Fields

		#endregion

		#region Properties and Events

		private a6l _landscape;

		#endregion

		#region Constructors and Cleanup

		protected Landscape(a6l landscape)
		{
			_landscape = landscape;
		}

		#endregion

		#region Methods

		public float GetLandscapeHeight(ajg position)
		{
			return _landscape.d(position);
		}

		#endregion
	}
}