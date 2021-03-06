﻿using System.Collections.Generic;
using System.Linq;
using GridDominance.Graphfileformat.Blueprint;
using GridDominance.Levelfileformat.Blueprint;
using GridDominance.Shared.Resources;
using Microsoft.Xna.Framework;
using MonoSAMFramework.Portable.GameMath.Geometry;
using MonoSAMFramework.Portable.LogProtocol;
using MonoSAMFramework.Portable.Screens.Entities;

namespace GridDominance.Shared.Screens.WorldMapScreen.Entities
{
	public class LevelGraph
	{
		private readonly GDWorldMapScreen screen;

		public readonly List<IWorldNode> Nodes = new List<IWorldNode>();

		public FRectangle BoundingRect;
		public FRectangle BoundingViewport;

		public RootNode InitialNode;

		public LevelGraph(GDWorldMapScreen s)
		{
			screen = s;
		}

		public void Init(GraphBlueprint g)
		{
			InitEntities(g);

			InitPipes(g);

			InitEnabled();

			InitViewport();
		}

		private void InitEntities(GraphBlueprint g)
		{
			foreach (var bpNode in g.LevelNodes)
			{
				LevelBlueprint f;
				if (Levels.LEVELS.TryGetValue(bpNode.LevelID, out f))
				{
					var data = MainGame.Inst.Profile.GetLevelData(f.UniqueID);
					var pos = new FPoint(bpNode.X, bpNode.Y);

					var node = new LevelNode(screen, pos, f, data);

					screen.Entities.AddEntity(node);
					Nodes.Add(node);
				}
				else
				{
					SAMLog.Error("LevelGraph::IE", $"Cannot find id {bpNode.LevelID:B} for graph");
				}
			}

			foreach (var bpNode in g.WarpNodes)
			{
				if (bpNode.TargetWorld == Levels.WORLD_ID_GAMEEND)
				{
					var node = new WarpGameEndNode(screen, bpNode);
					screen.Entities.AddEntity(node);
					Nodes.Add(node);
				}
				else
				{
					var node = new WarpNode(screen, bpNode);
					screen.Entities.AddEntity(node);
					Nodes.Add(node);
				}

			}

			InitialNode = new RootNode(screen, g.RootNode);
			screen.Entities.AddEntity(InitialNode);
			Nodes.Add(InitialNode);
		}

		private void InitPipes(GraphBlueprint g)
		{
			foreach (var pipe in g.RootNode.OutgoingPipes.OrderBy(p => p.Priority))
			{
				var sinknode = Nodes.FirstOrDefault(n => n.ConnectionID == pipe.Target);

				if (sinknode == null)
				{
					SAMLog.Error("LevelGraph::IP_1", $"Cannot find node with id {pipe.Target:B} in graph for pipe sink");
					continue;
				}

				InitialNode.CreatePipe(sinknode, pipe.PipeOrientation);
			}

			foreach (var bpNode in g.LevelNodes)
			{
				var sourcenode = Nodes.FirstOrDefault(n => n.ConnectionID == bpNode.LevelID);
				if (sourcenode == null)
				{
					SAMLog.Error("LevelGraph::IP_2", $"Cannot find node with id {bpNode.LevelID:B} in graph for pipe source");
					continue;
				}

				foreach (var pipe in bpNode.OutgoingPipes.OrderBy(p => p.Priority))
				{
					var sinknode = Nodes.FirstOrDefault(n => n.ConnectionID == pipe.Target);

					if (sinknode == null)
					{
						SAMLog.Error("LevelGraph::IP_3", $"Cannot find node with id {pipe.Target:B} in graph for pipe sink");
						continue;
					}

					sourcenode.CreatePipe(sinknode, pipe.PipeOrientation);
				}
			}
		}

		private void InitEnabled()
		{
			Stack<IWorldNode> nstack = new Stack<IWorldNode>();
			foreach (var nextnode in InitialNode.NextLinkedNodes)
			{
				nstack.Push(nextnode);
			}

			while (nstack.Any())
			{
				var n = nstack.Pop();

				n.NodeEnabled = true;
				if (n.HasAnyCompleted())
				{
					foreach (var nextnode in n.NextLinkedNodes)
					{
						if (!nextnode.NodeEnabled) nstack.Push(nextnode);
					}
				}
			}
		}

		private void InitViewport()
		{
			BoundingRect = FRectangle.CreateOuter(Nodes.Select(n => ((GameEntity)n).DrawingBoundingRect));

			BoundingViewport = BoundingRect
				.AsInflated(LevelNode.DIAMETER, LevelNode.DIAMETER)
				.SetRatioOverfitKeepCenter(GDConstants.VIEW_WIDTH * 1f / GDConstants.VIEW_HEIGHT);
		}

		public IWorldNode FindNode(INodeBlueprint bp)
		{
			return Nodes.FirstOrDefault(n => n.ConnectionID == bp.ConnectionID);
		}
	}
}
