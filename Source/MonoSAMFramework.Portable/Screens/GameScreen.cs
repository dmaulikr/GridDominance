﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoSAMFramework.Portable.BatchRenderer;
using MonoSAMFramework.Portable.DebugTools;
using MonoSAMFramework.Portable.GameMath;
using MonoSAMFramework.Portable.GameMath.Geometry;
using MonoSAMFramework.Portable.Input;
using MonoSAMFramework.Portable.Screens.Agents;
using MonoSAMFramework.Portable.Screens.Background;
using MonoSAMFramework.Portable.Screens.Entities;
using MonoSAMFramework.Portable.Screens.HUD;
using MonoSAMFramework.Portable.Screens.ViewportAdapters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoSAMFramework.Portable.Screens
{
	public abstract class GameScreen : Screen
	{
		private float _mapOffsetX = 0f;
		private float _mapOffsetY = 0f;

		public readonly GraphicsDeviceManager Graphics;
		public GraphicsDevice GraphicsDevice => Graphics.GraphicsDevice;
		public readonly MonoSAMGame Game;

#if DEBUG
		protected RealtimeAPSCounter FPSCounter;
		protected RealtimeAPSCounter UPSCounter;
		protected GCMonitor GCMonitor;
#endif

		public SAMViewportAdapter VAdapter;
		protected IDebugTextDisplay DebugDisp;

		public float MapOffsetX { get { return _mapOffsetX; } set { _mapOffsetX = value; TranslatedBatch.VirtualOffsetX = value; } }
		public float MapOffsetY { get { return _mapOffsetY; } set { _mapOffsetY = value; TranslatedBatch.VirtualOffsetY = value; } }
		public Vector2 MapOffset => new Vector2(_mapOffsetX, _mapOffsetY);
		public FRectangle GuaranteedMapViewport => new FRectangle(-MapOffsetX, -MapOffsetY, VAdapter.VirtualGuaranteedWidth, VAdapter.VirtualGuaranteedHeight);
		public FRectangle CompleteMapViewport => new FRectangle(-MapOffsetX - VAdapter.VirtualGuaranteedBoundingsOffsetX, -MapOffsetY - VAdapter.VirtualGuaranteedBoundingsOffsetY, VAdapter.VirtualTotalWidth, VAdapter.VirtualTotalHeight);
		public FRectangle MapFullBounds { get; private set; }

		protected InputStateManager InputStateMan;

		protected GameHUD GameHUD;
		protected EntityManager Entities;
		protected GameBackground Background;
		private List<GameScreenAgent> Agents;

#if DEBUG
		protected DebugMinimap DebugMap;
#endif

		protected SpriteBatch InternalBatch;

		protected IBatchRenderer FixedBatch;		          // no translation          (for HUD)
		protected ITranslateBatchRenderer TranslatedBatch;    // translated by MapOffset (for everything else)

#if DEBUG
		public int LastDebugRenderSpriteCount   => FixedBatch.LastDebugRenderSpriteCount   + TranslatedBatch.LastDebugRenderSpriteCount + DebugDisp.LastRenderSpriteCount;
		public int LastReleaseRenderSpriteCount => FixedBatch.LastReleaseRenderSpriteCount + TranslatedBatch.LastReleaseRenderSpriteCount;
		public int LastDebugRenderTextCount     => FixedBatch.LastDebugRenderTextCount     + TranslatedBatch.LastDebugRenderTextCount + DebugDisp.LastRenderTextCount;
		public int LastReleaseRenderTextCount   => FixedBatch.LastReleaseRenderTextCount   + TranslatedBatch.LastReleaseRenderTextCount;
#endif

		public float GameSpeed = 1f;

		protected GameScreen(MonoSAMGame game, GraphicsDeviceManager gdm)
		{
			Graphics = gdm;
			Game = game;
			
			Initialize();
		}

		private void Initialize()
		{
			MapFullBounds = CreateMapFullBounds();
			VAdapter = CreateViewport();

			InternalBatch   = new SpriteBatch(Graphics.GraphicsDevice);
			FixedBatch      = new StandardSpriteBatchWrapper(InternalBatch);
			TranslatedBatch = new TranslatingSpriteBatchWrapper(InternalBatch);

			InputStateMan = new InputStateManager(VAdapter, MapOffsetX, MapOffsetY);
			GameHUD = CreateHUD();
			Background = CreateBackground();

			Entities = CreateEntityManager();
			Agents = new List<GameScreenAgent>();

			DebugDisp = new DummyDebugTextDisplay();

#if DEBUG
			FPSCounter = new RealtimeAPSCounter();
			UPSCounter = new RealtimeAPSCounter();
			GCMonitor = new GCMonitor();

			DebugMap = CreateDebugMinimap();
#endif
		}

		protected override void OnRemove()
		{
			base.OnRemove();

			FixedBatch.Dispose();
			TranslatedBatch.Dispose();
		}

		public override void Update(GameTime gameTime)
		{
			var state = InputStateMan.GetNewState(MapOffsetX, MapOffsetY);

			if (state.IsExit()) Game.Exit();

#if DEBUG
			DebugSettings.Update(state);
			GCMonitor.Update(gameTime, state);
#endif

			GameHUD.Update(gameTime, state);
			DebugDisp.Update(gameTime, state);

			if (FloatMath.IsZero(GameSpeed))
			{
				return;
			}
			else if (FloatMath.IsOne(GameSpeed))
			{
				InternalUpdate(gameTime, state);
			}
			else if (GameSpeed < 1f)
			{
				var internalTime = new GameTime(gameTime.TotalGameTime, new TimeSpan((long) (gameTime.ElapsedGameTime.Ticks * GameSpeed)));

				InternalUpdate(internalTime, state);
			}
			else if (GameSpeed > 1f)
			{
				var totalTicks = gameTime.ElapsedGameTime.Ticks * GameSpeed;

				int runCount = (int) GameSpeed;

				long ticksPerRun = (long) (totalTicks / runCount);
				ticksPerRun += (long) ((totalTicks - ticksPerRun * runCount) / runCount);

				var time = gameTime.TotalGameTime;

				for (int i = 0; i < runCount; i++)
				{
					var span = new TimeSpan(ticksPerRun);

					InternalUpdate(new GameTime(time, span), state);

					time += span;
				}
			}
			else
			{
				// okay - dafuq
				throw new ArgumentException(nameof(GameSpeed) + " = " + GameSpeed, nameof(GameSpeed));
			}
		}

		private void InternalUpdate(GameTime gameTime, InputState state)
		{
#if DEBUG
			UPSCounter.Update(gameTime);
#endif

			InputStateMan.TriggerListener();
			
			Background.Update(gameTime, state);
			Entities.Update(gameTime, state);

			foreach (var agent in Agents) agent.Update(gameTime, state);

			OnUpdate(gameTime, state);
		}

		public override void Draw(GameTime gameTime)
		{
#if DEBUG
			FPSCounter.Update(gameTime);
#endif

			Graphics.GraphicsDevice.Clear(Color.Magenta);

			FixedBatch.OnBegin();
			TranslatedBatch.OnBegin();
			InternalBatch.Begin(transformMatrix: VAdapter.GetScaleMatrix());
			{
				Background.Draw(TranslatedBatch);

				Entities.Draw(TranslatedBatch);

				GameHUD.Draw(FixedBatch);

#if DEBUG
				using (FixedBatch.BeginDebugDraw()) DebugMap.Draw(FixedBatch);
#endif
			}
			InternalBatch.End();
			TranslatedBatch.OnEnd();
			FixedBatch.OnEnd();

			Entities.PostDraw();

#if DEBUG
			using (FixedBatch.BeginDebugDraw())
			using (TranslatedBatch.BeginDebugDraw())
			{
				Entities.DrawOuterDebug();
				DebugDisp.Draw();
			}
#endif
		}

		public override void Resize(int width, int height)
		{
			base.Resize(width, height);

			GameHUD.RecalculateAllElementPositions();
		}

		public void PushNotification(string text)
		{
			DebugDisp.AddDecayLine(text);
		}

		public void PushErrorNotification(string text)
		{
			DebugDisp.AddErrorDecayLine(text);
		}

		public void AddAgent(GameScreenAgent a)
		{
			Agents.Add(a);
		}

		public bool RemoveAgent(GameScreenAgent a)
		{
			return Agents.Remove(a);
		}

		public IEnumerable<T> GetEntities<T>()
		{
			return Entities.Enumerate().OfType<T>();
		}

		public IEnumerable<GameEntity> GetAllEntities()
		{
			return Entities.Enumerate();
		}

		protected abstract void OnUpdate(GameTime gameTime, InputState istate);

		protected abstract EntityManager CreateEntityManager();
		protected abstract GameHUD CreateHUD();
		protected abstract GameBackground CreateBackground();
		protected abstract SAMViewportAdapter CreateViewport();
		protected abstract DebugMinimap CreateDebugMinimap();
		protected abstract FRectangle CreateMapFullBounds();
	}
}
