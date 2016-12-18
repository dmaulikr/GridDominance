﻿using GridDominance.Shared.Screens.ScreenGame.Entities;
using Microsoft.Xna.Framework;
using MonoSAMFramework.Portable.Input;
using GridDominance.Shared.Screens.ScreenGame.Fractions;
using MonoSAMFramework.Portable.Extensions;

namespace GridDominance.Shared.Screens.ScreenGame.FractionController
{
	abstract class AbstractFractionController
	{
		protected readonly GDGameScreen Owner;

		private readonly float updateInterval;
		private float timeSinceLastUpdate = 0;

		protected readonly Cannon Cannon;
		protected readonly Fraction Fraction;

		protected AbstractFractionController(float interval, GDGameScreen owner, Cannon cannon, Fraction fraction)
		{
			updateInterval = interval;
			Cannon = cannon;
			Fraction = fraction;
			Owner = owner;
		}

		public void Update(GameTime gameTime, InputState istate)
		{
			timeSinceLastUpdate -= gameTime.GetElapsedSeconds();
			if (timeSinceLastUpdate <= 0)
			{
				timeSinceLastUpdate = updateInterval;

				Calculate(istate);
			}
		}

		protected abstract void Calculate(InputState istate);
		public abstract bool DoBarrelRecharge();
	}
}
