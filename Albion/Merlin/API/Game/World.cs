﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using WorldMap;


namespace Merlin.API
{
	/* Internal Type: alb */
	public class World
	{
		#region Static

		public static World Instance
		{
			get 
			{ 
				var internalWorld = alb.a();

				if (internalWorld != null)
					return new World(internalWorld);

				return default(World);
			}
		}

		private static MethodInfo _getEntitiesCollection;
		private static FieldInfo _getWorldmapClusters;

		static World()
		{
			_getEntitiesCollection = typeof(alb).GetMethod("ai", BindingFlags.NonPublic | BindingFlags.Instance);
			_getWorldmapClusters = typeof(Worldmap).GetField("c", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		#endregion

		#region Fields

		private alb _internal;

		#endregion

		#region Properties and Events

		public WorldmapCluster CurrentCluster => GetCluster(_internal.u());

		#endregion

		#region Constructors and Cleanup

		protected World(alb world)
		{
			_internal = world;
		}

		#endregion

		#region Methods

		public Dictionary<long, ark> GetEntities() 
		{
			return _getEntitiesCollection.Invoke(_internal, new object[] { }) as Dictionary<long, ark>;
		}

		public Dictionary<string, WorldmapCluster> GetClusters()
		{
			return _getWorldmapClusters.GetValue(GameGui.Instance.WorldMap) as Dictionary<string, WorldmapCluster>;
		}

		public WorldmapCluster GetCluster(ake info)
		{
			var clusters = GetClusters();

			if (clusters.TryGetValue(info.ak(), out WorldmapCluster cluster))
				return cluster;

			return default(WorldmapCluster);
		}

		public WorldmapCluster GetCluster(string name)
		{
			var clusters = GetClusters();

			foreach (var cluster in clusters.Values)
			{
                //NOTE: Be sure about this (this is just a guess, because it worked in previous version), if correct, modify values in Clusters.tt
				if (cluster.Info.an().ToLower() == name.ToLower())
					return cluster;
			}

			return null;
		}

		#endregion
	}
}