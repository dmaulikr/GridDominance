﻿using FarseerPhysics;
using FarseerPhysics.DebugView;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using MonoSAMFramework.Portable.Input;
using MonoSAMFramework.Portable.Screens.Entities;
using GridDominance.Shared.Resources;
using GridDominance.Shared.Screens;
using GridDominance.Shared.Screens.ScreenGame;
using MonoSAMFramework.Portable.DebugTools;
using MonoSAMFramework.Portable.GameMath.Geometry;
using MonoSAMFramework.Portable.Screens;
using MonoSAMFramework.Portable.Screens.ViewportAdapters;

namespace GridDominance.Shared.NormalGameScreen
{
	public class GDEntityManager : EntityManager
	{
		public readonly World PhysicsWorld;
		public GDGameScreen GDOwner => (GDGameScreen) Owner;

#if DEBUG
		private readonly DebugViewXNA debugView;
#endif

		public GDEntityManager(GDGameScreen scrn) : base(scrn)
		{
			PhysicsWorld = new World(Vector2.Zero);

#if DEBUG
			debugView = new DebugViewXNA(PhysicsWorld);
			debugView.LoadContent(GDOwner.Graphics.GraphicsDevice, GDOwner.Game.Content, Textures.DebugFont);
			debugView.AppendFlags(DebugViewFlags.Shape);
			debugView.AppendFlags(DebugViewFlags.DebugPanel);
			debugView.AppendFlags(DebugViewFlags.PerformanceGraph);
			debugView.AppendFlags(DebugViewFlags.ContactPoints);
			debugView.AppendFlags(DebugViewFlags.ContactNormals);
			debugView.AppendFlags(DebugViewFlags.Controllers);
			debugView.TextColor = Color.Black;
			debugView.Enabled = false;
#endif
		}

		protected override FRectangle RecalculateBoundingBox()
		{
			return Owner.VAdapterGame.VirtualTotalBoundingBox.AsInflated(GDConstants.TILE_WIDTH, GDConstants.TILE_WIDTH);
		}

		protected override void OnBeforeUpdate(SAMTime gameTime, InputState state)
		{
#if DEBUG
			debugView.DebugPanelPosition = new Vector2(55, Owner.VAdapterGame.RealTotalHeight - 180);
			debugView.PerformancePanelBounds = new Rectangle(450, (int) (Owner.VAdapterGame.RealTotalHeight - 180), 200, 100);

			debugView.Enabled = DebugSettings.Get("PhysicsDebugView");
#endif
#if DEBUG
			DebugUtils.TIMING_PHYSICS.Start();
			PhysicsWorld.Step(gameTime.ElapsedSeconds);
			DebugUtils.TIMING_PHYSICS.Stop();
#else
			PhysicsWorld.Step(gameTime.ElapsedSeconds);
#endif
		}

		protected override void OnAfterUpdate(SAMTime gameTime, InputState state)
		{
			// NOP
		}

		public override void DrawOuterDebug()
		{
#if DEBUG
			
			
			var pMatrix = Matrix.CreateTranslation(GDOwner.MapOffsetX, GDOwner.MapOffsetY, 0) * GDOwner.VAdapterGame.GetFarseerDebugProjectionMatrix();
			var vMatrix = Matrix.CreateScale(GDConstants.PHYSICS_CONVERSION_FACTOR);
			debugView.RenderDebugData(ref pMatrix, ref vMatrix);
#endif
		}
	}
}
