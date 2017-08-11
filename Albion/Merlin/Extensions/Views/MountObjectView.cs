﻿using System;
using System.Collections.Generic;

using UnityEngine;

namespace Merlin
{
	public static class MountObjectViewExtensions
	{
		public static bool InRange(this MountObjectView mount, Vector3 position)
		{
			var mountPosition = mount.transform.position;
			var distance = (position - mountPosition).magnitude;

			var desiredRange = mount.MountObject.sb().eg();

			if (distance > desiredRange)
				return false;

			return true;
		}
	}
}