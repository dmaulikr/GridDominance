﻿using MonoSAMFramework.Portable.Screens.HUD.Operations;
using MonoSAMFramework.Portable.GameMath;
using MonoSAMFramework.Portable.GameMath.Geometry;
using MonoSAMFramework.Portable.Input;

namespace GridDominance.Shared.Screens.WorldMapScreen.HUD
{
	class HUDSettingsSlantedOpenOperation : HUDTimedElementOperation<SettingsButton>
	{
		private readonly int _index;
		private readonly int _posindex;
		private SubSettingButton _button;

		private float buttony;
		private float startX;
		private float endX;

		public HUDSettingsSlantedOpenOperation(int idx, int btnidx) : base(0.3f + 0.1f * btnidx)
		{
			_index = idx;
			_posindex = btnidx;
		}

		protected override void OnStart(SettingsButton button)
		{
			_button = button.SubButtons[_index];

			startX = _button.RelativePosition.X;
			endX = _button.TargetPosition.X;
			buttony = _button.RelativePosition.Y;
		}

		protected override void OnProgress(SettingsButton button, float progress, InputState istate)
		{
			var pp = FloatMath.FunctionEaseOutQuad(progress);

			var xx = startX + (endX - startX) * pp;
			var yy = buttony + FloatMath.Sin(pp*FloatMath.PI) * SubSettingButton.DIAMETER * (1 + _posindex*0.1f);

			_button.RelativePosition = new FPoint(xx, yy);
		}

		protected override void OnEnd(SettingsButton button)
		{
			button.OpeningProgress = 1f;
			button.RotationSpeed = 0.5f;

			_button.RelativePosition = new FPoint(endX, buttony);
		}

		public override string Name => "SettingsOpen";
	}
}
