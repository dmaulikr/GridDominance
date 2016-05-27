﻿using MonoGame.Extended;
using System;

namespace MonoSAMFramework.Portable.Screens.HUD
{
	public class HUDSeperator : HUDRectangle
	{
		private HUDOrientation _orientation;
		public HUDOrientation Orientation
		{
			get { return _orientation; }
			set { _orientation = value; InvalidatePosition(); }
		}

		public int SeperatorWidth = 1;

		public HUDSeperator(HUDOrientation orientation, int depth = 0)
			: base(depth)
		{
			_orientation = orientation;
		}

		protected override void OnBeforeRecalculatePosition()
		{
			switch (Orientation)
			{
				case HUDOrientation.Horizontal:
					Size = new Size(Size.Width, SeperatorWidth);
					break;
				case HUDOrientation.Vertical:
					Size = new Size(SeperatorWidth, Size.Height);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
